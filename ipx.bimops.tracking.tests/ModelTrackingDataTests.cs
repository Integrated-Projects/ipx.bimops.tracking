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

    // [Test]
    // public void ShouldBeAbleToReadCSVFromPosition()
    // {
    //     Assert.Fail();
    // }

    [Test]
    public async Task UploaderShouldBeAbleToUploadCSVToAirtable()
    {
        await AirtableUploader.UploadChunkToAirtable(test_data!, airtableAPIKey, baseId, tableId);
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
