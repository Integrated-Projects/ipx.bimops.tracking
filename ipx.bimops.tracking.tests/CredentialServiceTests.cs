using NUnit.Framework;

namespace ipx.bimops.tracking.tests;

public class CredentialServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ShouldThrowErrorMessageIfTryingToRetrieveCredentialsWithoutDataInResources()
    {
        Assert.Throws<Exception>(() => CredentialService.GetResourceCredentials());
    }
    
    // [Test]
    // public void ShouldThrowErrorMessageIfTryingToRetrieveBearerTokenWithoutCredentials()
    // {
    //     Assert.ThrowsAsync<Exception>(async () => await CredentialService.GetBearerToken());
    // }
}
