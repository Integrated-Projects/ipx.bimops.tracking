using System.Text.Json;
using AirtableApiClient;
using CsvHelper.Expressions;

public static class AirtableUploader
{
    public static async Task UploadChunkToAirtable(List<ModelerTrackingSchema> records, string airtableAPIKey, string baseId, string tableId)
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