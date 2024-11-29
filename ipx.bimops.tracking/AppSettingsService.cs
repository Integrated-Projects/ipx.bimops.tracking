using ipx.bimops.core;
using Microsoft.Extensions.Configuration;

namespace ipx.bimops.tracking;

public class AppSettingsService
{
    public static AppSettings GetCreds(string settingsFilePath = "")
    {
        if (settingsFilePath == "")
        {
            Exception ex =  new Exception("Must have a valid appsettings.json loaded");
            LoggingService.LogError($"An error occurred: {ex.Message}", ex);
        }
            
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(settingsFilePath)
            .Build();

        var creds = new AppSettings
        {
            Environment = configuration["Environment"],
            AirtableAPIKey = configuration["AirtableAPIKey"],
            AirtableBaseTrackingId = configuration["AirtableBaseTrackingId"],
            AirtableTrackingTableId = configuration["AirtableTrackingTableId"],
            AirtableTrackingTableId_Testing = configuration["AirtableTrackingTableId_Testing"],
            AirtableTrackingTableId_Staging = configuration["AirtableTrackingTableId_Staging"]
        };

        return creds;
    }
}

public class AppSettings
{
    public string? Environment = "";
    public string? AirtableAPIKey = "";
    public string? AirtableBaseTrackingId = "";
    public string? AirtableTrackingTableId = "";
    public string? AirtableTrackingTableId_Testing = "";
    public string? AirtableTrackingTableId_Staging = "";
}
