using Microsoft.Extensions.Configuration;

namespace ipx.bimops.tracking;

public class CredentialService
{
    public static string GetCreds(string settingsFilePath = "")
    {
        if(settingsFilePath == "")
            throw new Exception("Must have a valid appsettings.json loaded");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsFilePath)
            .Build();

        string? clientId = configuration["ClientId"];
        Console.WriteLine($"Client ID: {clientId}");

        return clientId;
    }
}
