using Microsoft.Extensions.Configuration;

namespace ipx.bimops.tracking;

public class CredentialService
{
    public static Credentials GetCreds(string settingsFilePath = "")
    {
        if(settingsFilePath == "")
            throw new Exception("Must have a valid appsettings.json loaded");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsFilePath)
            .Build();

        string? clientId = configuration["ClientId"];
        Console.WriteLine($"Client ID: {clientId}");

        var creds = new Credentials
        {
            AirtableAPIKey = configuration["AirtableAPIKey"],
            AirtableBaseTrackingId = configuration["AirtableBaseTrackingId"],
            AirtableTrackingTableId = configuration["AirtableTrackingTableId"],
            AirtableTrackingTableId_Testing = configuration["AirtableTrackingTableId_Testing"]
        };

        return creds;
    }
}

public class Credentials
{
    public string? AirtableAPIKey = "";
    public string? AirtableBaseTrackingId = "";
    public string? AirtableTrackingTableId = "";
    public string? AirtableTrackingTableId_Testing = "";
}
