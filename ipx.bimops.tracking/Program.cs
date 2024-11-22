using System.Reflection.Metadata;

namespace ipx.bimops.tracking;

public class Program
{
    private static ISessionHandler _sessionHandler = new SessionHandler();
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    private static string? PathCSV = null;
    private static string? PathJSON = null;
    private static bool IsSessionActive = true; // Tracks session state
    private static bool ShouldUploadData = true; // Tracks session state
    private static bool SessionUploadComplete = true;
    private static int _cursor = 0;
    private static int _chunkSize = 100;
    private static FileWatcher? watcherCSV;
    private static bool ShouldGenerateFakeData = false;
    private static bool ShouldSimulateRandomData = false;

    public static void SetSessionHandler(ISessionHandler sessionHandler)
    {
        _sessionHandler = sessionHandler;
    }

    /**
        * Validates the arguments passed to the program
        * Arguments should be able to be passed in any order
        * Valid arguments are:
        * 1. --generateFakeData (no value necessary)
        * 2. --pathToCSV="path/to/csv"
        * 3. --pathToJSON="path/to/json"
        * 4. --simulateRandomData (no value necessary)
        * @param args
    */
    public static void ValidateArgs(string[]? args)
    {
        Console.WriteLine("Validating arguments...");
        ShouldGenerateFakeData = args != null && args.Length > 0 && args.Any(arg => arg == "--generateFakeData");
        ShouldSimulateRandomData = args != null && args.Length > 0 && args.Any(arg => arg == "--simulateRandomData");

        if (ShouldGenerateFakeData)
        {
            GenerateFakeData();
        }
        else
        {
            var csvIndex = args.Any(arg => arg.Contains("--pathToCSV"));
            var jsonIndex = args.Any(arg => arg.Contains("--pathToJSON"));

            if (csvIndex)
            {
                PathCSV = args != null && args.Length > 0 ? args.First(arg => arg.Contains("--pathToCSV")).Split("=")[1] : null;
            }

            if (jsonIndex)
            {
                PathJSON = args != null && args.Length > 0 ? args.First(arg => arg.Contains("--pathToJSON")).Split("=")[1] : null;
            }
        }

        if (PathCSV == null) throw new Exception("Can't find CSV location");
        if (PathJSON == null) throw new Exception("Can't find JSON location");

        Console.WriteLine($"Will be watching the CSV at {PathCSV}");
        Console.WriteLine($"Will be watching the JSON at {PathJSON}");
    }

    public static void GenerateFakeData()
    {
        Console.WriteLine($"Generating fake data and setting the CSV & JSON paths accordingly");
        // if generate fake data, use the modeldata creator to create some data
        var data = ModelTrackingDataCreator.Create(Math.Max(1, new Random().Next(1, _chunkSize)));
        var id = Guid.NewGuid();

        // set PATHCSV & PATHJSON
        PathCSV = Path.Combine(Path.GetTempPath(), $"{id}.csv");
        PathJSON = Path.Combine(Path.GetTempPath(), $"{id}.json");
        string csvData = string.Concat($"{ModelerTrackingSchema.GetCSVHeader()}\n", string.Join("\n", data.Select(d => d.ToCSV())));

        // write this to a CSV close to the application
        File.WriteAllText(PathCSV, csvData);
        var lineCount = File.ReadLines(PathCSV).Count() - 1;

        // write this to a JSON close to the application
        _sessionHandler.WriteSessionInfoToJSON(PathJSON, id.ToString(), lineCount, 0, true);
    }

    public static void SetSessionData()
    {
        if (PathJSON == null) return;

        Console.WriteLine($"Getting session data from {PathJSON}");
        var session = _sessionHandler.GetSessionInfoFromJSON(PathJSON);
        if (session == null)
        {
            Console.WriteLine($"The session is NULL!");
            return;
        }

        Console.WriteLine($"Session is available and has {session.LastWrite} writes and {session.LastRead} reads");

        if ((bool)!session.SessionActive) IsSessionActive = false;
        Console.WriteLine((session.SessionActive ?? false) ? $"The session is currently active" : "The session is no longer active");

        Console.WriteLine($"Setting cursor to last read state");
        _cursor = session.LastRead ?? 0;

        // If I reach the MAX, I should continue watching the JSON for changes until the session marks itself as `inactive`
        // if (_cursor == session.LastWrite)
        // {
        //     Console.WriteLine($"The Program has completed writing all relevant data at the moment");
        //     ShouldUploadData = false;
        // }

        // But if the session is inactive AND I've already uploaded everything, the session is complete
        SessionUploadComplete = !IsSessionActive && !ShouldUploadData;
        Console.WriteLine($"The session has completed the uploads");
    }

    public static void OnSessionDataUpdated(string path)
    {
        Task.Run(SetSessionData);
    }

    public static async Task Main(string[]? args)
    {
        Console.WriteLine("Starting program...");
        ValidateArgs(args);

        Console.WriteLine("Setting session data.");
        SetSessionData();
        Console.WriteLine("Session data is set - beginning file watch.");

        // what I want to happen is that a watcher watches the CSV
        watcherCSV = new FileWatcher(PathCSV);
        watcherCSV.OnFileChanged += OnSessionDataUpdated;

        // Keep the program running
        while (!SessionUploadComplete)
        {
            Console.WriteLine("Watching for new data to upload");

            if (ShouldUploadData)
            {
                Console.WriteLine("Parsing & uploading data...");
                var session = _sessionHandler.GetSessionInfoFromJSON(PathJSON);
                // I should send off chunks of the CSV 100 at at time until I have reached the MAX
                var chunks = CSVParser.ProcessCsvInChunks(PathCSV, _chunkSize, _cts.Token, _cursor);
                var chunkCount = chunks.Count(); // get the amount of chunks
                Console.WriteLine($"Uploading {chunkCount} chunks to Airtable");
                await AirtableUploader.UploadChunksToAirtable(chunks, chunkCount, AppSettingsService.GetCreds("appsettings.json"), _cts.Token);
                _cursor += chunkCount; // update cursor with the amount of items uploaded
                _sessionHandler.WriteSessionInfoToJSON(PathJSON, null, null, _cursor);
            }

            // wait a second before restarting the loop to upload
            Console.WriteLine("Resting for 1 second");
            Thread.Sleep(1000);

            // if simulateRandomData is true, I should generate some random data
            if (ShouldSimulateRandomData)
            {
                Console.WriteLine("Simulating random data...");
                var randomData = ModelTrackingDataCreator.Create(Math.Max(1, new Random().Next(1, _chunkSize)));
                var lineCount = File.ReadLines(PathCSV).Count() - 1;
                var csvData = $"\n{string.Join("\n", randomData.Select(d => d.ToCSV()))}";
                File.AppendAllText(PathCSV, csvData);
                _sessionHandler.WriteSessionInfoToJSON(PathJSON, null, lineCount, null, true);
            }
        }
        Console.WriteLine("Program complete!");
    }
}
