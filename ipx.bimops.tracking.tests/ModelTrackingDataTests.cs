namespace ipx.bimops.tracking.tests;

public class ModelTrackingDataTests
{
    internal List<ModelerTrackingSchemaData>? test_data;

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
    public void ShouldCreateTenPiecesOfData()
    {
        Assert.That(test_data?.Count == 10);
    }

    [Test]
    public void ShouldBeAbleToReadCSVFromPosition()
    {
        
    }
}
