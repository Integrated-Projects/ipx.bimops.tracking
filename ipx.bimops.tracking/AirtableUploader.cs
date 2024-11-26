using AirtableApiClient;
using ipx.bimops.core;
namespace ipx.bimops.tracking;

public static class AirtableUploader
{
    public static async Task UploadChunksToAirtable(IEnumerable<ModelerTrackingSchema> records, int chunkSize, AppSettings creds, CancellationToken cancellationToken)
    {
        var currentChunk = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var chunk = records.Skip(currentChunk * chunkSize).Take(chunkSize).ToList();

            if (!chunk.Any()) break;

            await AirtableUploader.UploadRecordsToAirtable(chunk, creds.AirtableAPIKey, creds.AirtableBaseTrackingId, creds.Environment == "prod" ? creds.AirtableTrackingTableId : creds.Environment == "test" ? creds.AirtableTrackingTableId_Testing : creds.AirtableTrackingTableId_Staging);

            currentChunk++;
        }
    }

    public static async Task UploadRecordsToAirtable(List<ModelerTrackingSchema> records, string airtableAPIKey, string baseId, string tableId)
    {
        using (AirtableBase airtableBase = new AirtableBase(airtableAPIKey, baseId))
        {
            foreach (ModelerTrackingSchema schema in records)
            {
                var recordFields = new Fields();
                recordFields.AddField("id_document", schema.id_document);
                recordFields.AddField("id_project", schema.id_project);
                recordFields.AddField("id_user", schema.id_user);
                recordFields.AddField("id_user_ip_address", schema.id_user_ip_address);
                recordFields.AddField("timestamp", schema.timestamp);
                recordFields.AddField("id_element", schema.id_element);
                recordFields.AddField("type_element", schema.type_element);
                recordFields.AddField("title_document", schema.title_document);
                recordFields.AddField("duration", schema.duration);
                recordFields.AddField("action_project", schema.action_project.ToString());
                recordFields.AddField("action_element", schema.action_element.ToString());

                var res = await airtableBase.CreateRecord(tableId, recordFields);

                if (res.Success)
                {
                    LoggingService.LogInfo($"Record {schema.id_document} uploaded successfully");
                }
                else
                {
                    LoggingService.LogInfo($"Record {schema.id_document} failed to upload");
                    LoggingService.LogInfo($"Error for record {schema.id_document} - {res.AirtableApiError.DetailedErrorMessage}");
                }
            }
        }
    }
}
