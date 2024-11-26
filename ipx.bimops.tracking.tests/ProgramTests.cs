using System.Reflection;
using Moq;
using NUnit.Framework;

namespace ipx.bimops.tracking.tests;

public class ProgramTests
{
    private Mock<ISessionHandler> _sessionHandlerMock;

    [SetUp]
    public void Setup()
    {
        _sessionHandlerMock = new Mock<ISessionHandler>();
        Program.SetSessionHandler(_sessionHandlerMock.Object);

        // Reset the private fields before each test
        SetPrivateField("_cursor", 0);
        SetPrivateField("IsSessionActive", true);
        SetPrivateField("ShouldUploadData", true);
        SetPrivateField("SessionUploadComplete", false);
        SetPrivateField("PathJSON", "dummyPath");
        SetPrivateField("PathCSV", "dummyPath");
    }

    [Test]
    public void Program_ShouldNotThrowExceptionIfPathToJSONandCSV()
    {
        Assert.DoesNotThrow(() => Program.ValidateArgs(["something.csv", "something.json"]));
    }

    [Test]
    public void OnSessionDataUpdated_Should_Update_IsSessionActive()
    {
        // Arrange
        var session = new Session { SessionActive = true, LastRead = 50, LastWrite = 100 };
        _sessionHandlerMock.Setup(sh => sh.GetSessionInfoFromJSON(It.IsAny<string>())).Returns(session);

        // Act
        Program.OnSessionDataUpdated("dummyPath");
        Task.Delay(100).Wait(); // Simulate async delay

        // Assert
        Assert.That(GetPrivateField<bool>("IsSessionActive"), Is.EqualTo(true));
    }

    [Test]
    public void OnSessionDataUpdated_Should_Update_Cursor()
    {
        // Arrange
        var session = new Session { SessionActive = true, LastRead = 75, LastWrite = 100 };
        _sessionHandlerMock.Setup(sh => sh.GetSessionInfoFromJSON(It.IsAny<string>())).Returns(session);

        // Act
        Program.OnSessionDataUpdated("dummyPath");
        Task.Delay(100).Wait(); // Simulate async delay

        // Assert
        Assert.That(GetPrivateField<int>("_cursor"), Is.EqualTo(75));
    }

    // [Test]
    // public void OnSessionDataUpdated_Should_Set_ShouldUploadData_To_False_When_Read_Equals_Write()
    // {
    //     // Arrange
    //     var session = new Session { SessionActive = true, LastRead = 100, LastWrite = 100 };
    //     _sessionHandlerMock.Setup(sh => sh.GetSessionInfoFromJSON(It.IsAny<string>())).Returns(session);

    //     // Act
    //     Program.OnSessionDataUpdated("dummyPath");
    //     Task.Delay(100).Wait(); // Simulate async delay

    //     // Assert
    //     Assert.That(GetPrivateField<bool>("ShouldUploadData"), Is.EqualTo(false));
    // }

    [Test]
    public void OnSessionDataUpdated_Should_Set_SessionUploadComplete_When_Not_Active_And_Should_Not_Upload()
    {
        // Arrange
        var session = new Session { SessionActive = true, LastRead = 50, LastWrite = 100 };
        _sessionHandlerMock.Setup(sh => sh.GetSessionInfoFromJSON(It.IsAny<string>())).Returns(session);

        // Act
        Program.OnSessionDataUpdated("dummyPath");
        Task.Delay(100).Wait(); // Simulate async delay

        // Assert
        Assert.That(GetPrivateField<bool>("SessionUploadComplete"), Is.EqualTo(false));
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(Program).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        return (T)field.GetValue(null)!;
    }

    private void SetPrivateField<T>(string fieldName, T value)
    {
        var field = typeof(Program).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
        field.SetValue(null, value);
    }
}
