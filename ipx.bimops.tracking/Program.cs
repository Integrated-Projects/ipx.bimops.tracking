using System.Reflection.Metadata;

namespace ipx.bimops.tracking;

public class Program
{
    private static CancellationTokenSource _cts = new CancellationTokenSource();
    private static string? PathCSV = null;
    private static string? PathJSON = null;
    private static bool IsSessionActive = true; // Tracks session state
    private static bool ShouldUploadData = true; // Tracks session state
    private static bool SessionUploadComplete = true;
    private static FileWatcher? watcherCSV;
    private static int _cursor = 0;
    private static int _chunkSize = 100;

    public static async Task Main(string[] args)
    {
        PathCSV = args[0];
        PathJSON = args[1];

        if (PathCSV == null) throw new Exception("Can't find CSV location");
        if (PathJSON == null) throw new Exception("Can't find JSON location");

        void OnSessionDataUpdated(string path)
        {
            Task.Run(() =>
            {
                if (PathJSON == null) return;
                var session = SessionHandler.GetSessionInfoFromJSON(PathJSON);
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
                var session = SessionHandler.GetSessionInfoFromJSON(PathJSON);
                // I should send off chunks of the CSV 100 at at time until I have reached the MAX
                var chunks = CSVParser.ProcessCsvInChunks(PathCSV, _chunkSize, _cts.Token, _cursor);
                var chunkCount = chunks.Count(); // may have fewer than 100 chunks 
                await AirtableUploader.UploadChunksToAirtable(chunks, chunkCount, CredentialService.GetCreds("appsettings.json"), _cts.Token);
                _cursor += chunkCount; // update cursor with the amount of items uploaded
                SessionHandler.WriteSessionInfoToJSON(PathJSON, null, null, _cursor);
            }

            // wait a second before restarting the loop to upload
            Console.WriteLine("Resting for 1 second");
            Thread.Sleep(1000);
        }
    }
}
