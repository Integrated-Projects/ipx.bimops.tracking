using System.Runtime;
using NUnit.Framework;

namespace ipx.bimops.tracking.tests;

public class CredentialServiceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ShouldBeAbleToGetCreds()
    {  
        Assert.That(CredentialService.GetCreds("appsettings.sample.json"), Is.EqualTo("your-client-id"));
    }

    [Test]
    public void ShouldThrowAnExceptionIfNoCredentialFileIsGivem()
    {  
        Assert.Throws<Exception>(() => CredentialService.GetCreds());
    }
}
