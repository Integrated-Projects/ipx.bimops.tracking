using AirtableApiClient;
using ipx.bimops.tracking;

public static class AirtableUploader
{
    public static async Task UploadChunksToAirtable(IEnumerable<ModelerTrackingSchema> records, int chunkSize, Credentials creds, CancellationToken cancellationToken)
    {
        var currentChunk = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var chunk = records.Skip(currentChunk * chunkSize).Take(chunkSize).ToList();

            if (!chunk.Any()) break;

            await AirtableUploader.UploadRecordsToAirtable(chunk, creds.AirtableAPIKey, creds.AirtableBaseTrackingId, creds.AirtableTrackingTableId);

            currentChunk++;
        }
    }

    public static async Task UploadRecordsToAirtable(List<ModelerTrackingSchema> records, string airtableAPIKey, string baseId, string tableId)
    {
        using (AirtableBase airtableBase = new AirtableBase(airtableAPIKey, baseId))
        {
            foreach(ModelerTrackingSchema schema in records)
            {
                var recordFields = new Fields();
                recordFields.AddField("id_document", schema.id_document);


                await airtableBase.CreateRecord(tableId, recordFields, true);
            }
        }
    }
}
