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

    public static void SetSessionHandler(ISessionHandler sessionHandler)
    {
        _sessionHandler = sessionHandler;
    }

    public static void ValidateArgs(string[]? args)
    {
        PathCSV = args != null && args.Length > 0 ? args[0] : null;
        PathJSON = args != null && args.Length > 1 ? args[1] : null;

        if (PathCSV == null) throw new Exception("Can't find CSV location");
        if (PathJSON == null) throw new Exception("Can't find JSON location");
    }

    public static void OnSessionDataUpdated(string path)
    {
        Task.Run(() =>
        {
            if (PathJSON == null) return;
            var session = _sessionHandler.GetSessionInfoFromJSON(PathJSON);
            if (session == null) return;

            if ((bool)!session.SessionActive) IsSessionActive = false;

            if (_cursor == session.LastRead) return;

            _cursor = session.LastRead ?? 0;

            // If I reach the MAX, I should continue watching the JSON for changes until the session marks itself as `inactive`
            if (session.LastRead == session.LastWrite) ShouldUploadData = false;

            // But if the session is inactive AND I've already uploaded everything, the session is complete
            SessionUploadComplete = !IsSessionActive && !ShouldUploadData;
        });
    }

    public static async Task Main(string[]? args)
    {
        ValidateArgs(args);

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
                var chunkCount = chunks.Count(); // may have fewer than 100 chunks 
                await AirtableUploader.UploadChunksToAirtable(chunks, chunkCount, CredentialService.GetCreds("appsettings.json"), _cts.Token);
                _cursor += chunkCount; // update cursor with the amount of items uploaded
                _sessionHandler.WriteSessionInfoToJSON(PathJSON, null, null, _cursor);
            }

            // wait a second before restarting the loop to upload
            Console.WriteLine("Resting for 1 second");
            Thread.Sleep(1000);
        }
    }
}
