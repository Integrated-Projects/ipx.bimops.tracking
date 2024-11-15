using AirtableApiClient;

namespace ipx.bimops.tracking.tests;

public class ModelTrackingDataTests
{
    internal List<ModelerTrackingSchema>? test_data;
    internal string airtableAPIKey = "pat98Z02t5tSUtpfc.65ec65631c9b38c7d17d88b278ed4ec37f56ddb6c59aa9b270954f7ca0915177";
    internal string baseId = "appNGZd2un2U80HlE";
    internal string tableId = "tbly0YCApVX84yP83";

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
