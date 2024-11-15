using CsvHelper;
using CsvHelper.Configuration;


public class CsvUploader
{
    private static int _last_position = 0;
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
        var redisCachePath = "path/to/your/chunked/csv.csv"; // for each csv found, find in the redis cache if there' unuploaded data and upload it
        await ProcessCsvInChunks(filePath, 100, _cts.Token);
    }

    private static async Task ProcessCsvInChunks(string filePath, int chunkSize, CancellationToken cancellationToken)
    {
        var config = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<ModelerTrackingSchema>().Skip(_last_position); // Start from 0, adjust Skip dynamically in real scenarios

        var currentChunk = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var chunk = records.Skip(currentChunk + chunkSize).Take(chunkSize).ToList();

            if (!chunk.Any()) break;

            // await AirtableUploader.UploadChunkToAirtable(chunk);

            currentChunk++;
        }
    }
}

