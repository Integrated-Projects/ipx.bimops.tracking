using AirtableApiClient;

namespace ipx.bimops.tracking.tests;

public class ModelTrackingDataTests
{
    internal List<ModelerTrackingSchema>? test_data;
    internal string airtableAPIKey = "";
    internal string baseId = "";
    internal string tableId = "";

    [SetUp]
    public void Setup()
    {
        test_data = ModelTrackingDataCreator.Create();
        var creds = CredentialService.GetCreds("appsettings.json");
        airtableAPIKey = creds.AirtableAPIKey;
        baseId = creds.AirtableBaseTrackingId;
        tableId = creds.AirtableTrackingTableId_Testing;
    }

    [Test]
    public void ShouldNotBeNull()
    {
        Assert.That(test_data != null);
    }

    [Test]
    public void TestCreatorShouldCreateTenPiecesOfData()
    {
        Assert.That(test_data?.Count, Is.EqualTo(10));
    }

    [Test]
    public void ShouldBeAbleToProcessCsvInChunks()
    {
        var filePath = "./fake_modeler_tracking_data.csv";
        var records = CSVParser.ProcessCsvInChunks(filePath, 100, new CancellationToken()).ToList();

        Assert.That(records.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task UploaderShouldBeAbleToUploadCSVToAirtable()
    {
        await AirtableUploader.UploadRecordsToAirtable(test_data!, airtableAPIKey, baseId, tableId);
        var records = await GetRecordsFromAirtable();

        Assert.That(records.Count(), Is.EqualTo(10));
    }

    [TearDown]
    public async Task ClearTestingData()
    {
        using(AirtableBase ab = new(airtableAPIKey, baseId))
        {
            IEnumerable<AirtableRecord> records = (await ab.ListRecords(tableId)).Records;

            if(!records.Any()) return; // if there are no records, return

            foreach(AirtableRecord r in records)
            {
                await ab.DeleteRecord(tableId, r.Id);
            }
        }
    }

    private async Task<IEnumerable<AirtableRecord>> GetRecordsFromAirtable()
    {
        using AirtableBase ab = new(airtableAPIKey, baseId);
        IEnumerable<AirtableRecord> records = (await ab.ListRecords(tableId)).Records;

        return records;
    }
}
