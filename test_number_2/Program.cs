using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Net.Http;
using System.Text.Json;

public class CsvUploader
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
        var records = csv.GetRecords<dynamic>().Skip(0); // Start from 0, adjust Skip dynamically in real scenarios

        var currentChunk = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var chunk = records.Skip(currentChunk + chunkSize).Take(chunkSize).ToList();

            if (!chunk.Any()) break;

            await UploadChunkToAirtable(chunk);

            currentChunk++;
        }
    }

    private static async Task UploadChunkToAirtable(dynamic chunk)
    {
        Console.WriteLine("This is working!");
        // var client = new HttpClient();
        // var airtableUrl = "https://api.airtable.com/v0/YOUR_APP_ID/YOUR_TABLE_NAME";
        // client.DefaultRequestHeaders.Add("Authorization", "Bearer YOUR_API_KEY");

        // var content = new
        // {
        //     records = chunk.Select(record => new
        //     {
        //         fields = record
        //     })
        // };

        // var json = JsonSerializer.Serialize(content);
        // var response = await client.PostAsync(airtableUrl, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

        // if (response.IsSuccessStatusCode)
        // {
        //     Console.WriteLine("Chunk uploaded successfully.");
        // }
        // else
        // {
        //     Console.WriteLine($"Failed to upload chunk: {response.ReasonPhrase}");
        // }
    }
}

public class ModelerTrackingSchemaData
{
    public required string id_project {get; set;}
    public required string id_document {get; set;}
    public required string id_user {get; set;}
    public required string id_user_ip_address { get; set;}
    public required int timestamp { get; set;}
    public required string? id_element {get; set;}
    public required string? type_element {get; set;}
    public required int? duration {get; set; }
    public enum action_project
    {
        OPEN,
        MODIFY,
        SUBMIT,
        CLOSE,
        IDLE_BEGIN,
        IDLE_END
    }    
    public enum action_element
    {
        CREATE,
        MODIFY,
        DELETE
    }
}
