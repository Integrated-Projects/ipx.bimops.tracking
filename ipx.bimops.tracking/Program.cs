using CsvHelper;
using CsvHelper.Configuration;
using ipx.bimops.tracking;

public class Program
{
    private static CancellationTokenSource _cts = new CancellationTokenSource();

    public static async Task Main(string[] args)
    {
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Shutting down...");
            _cts.Cancel();
            e.Cancel = true;
        };

        var filePath = "path/to/your/chunked/csv.csv";
        // var redisCachePath = "path/to/your/chunked/csv.csv"; // for each csv found, find in the redis cache if there' unuploaded data and upload it
        var chunks = CSVParser.ProcessCsvInChunks(filePath, 10, _cts.Token);
        await AirtableUploader.UploadChunksToAirtable(chunks, 10, CredentialService.GetCreds("appsettings.json"), _cts.Token);
    }
}
