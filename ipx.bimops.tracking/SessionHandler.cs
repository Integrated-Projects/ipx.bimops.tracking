using System;
using System.Text.Json;
using ipx.bimops.core;

namespace ipx.bimops.tracking;

public interface ISessionHandler
{
    Session? GetSessionInfoFromJSON(string filePath);
    bool WriteSessionInfoToJSON(string filePath, string? sessionId = null, int? sessionLastWrite = null, int? sessionLastRead = null, bool? sessionActive = null, int retryAttempt = 0);
}

public class SessionHandler : ISessionHandler
{
    public Session? GetSessionInfoFromJSON(string filePath)
    {
        Session? session = null;

        try
        {
            string jsonString = File.ReadAllText(filePath);
            session = JsonSerializer.Deserialize<Session>(jsonString);

            if (session == null)
                LoggingService.LogInfo("Failed to deserialize session data.");
        }
        catch (Exception ex)
        {
            LoggingService.LogError($"An error occurred: {ex.Message}", ex);
        }

        return session;
    }

    public bool WriteSessionInfoToJSON(string filePath, string? sessionId = null, int? sessionLastWrite = null, int? sessionLastRead = null, bool? sessionActive = null, int retryAttempt = 0)
    {
        var sessionPrev = GetSessionInfoFromJSON(filePath);

        try
        {
            var writeable = new Session
            {
                SessionId = sessionId ?? sessionPrev?.SessionId,
                LastWrite = sessionLastWrite ?? sessionPrev?.LastWrite,
                LastRead = sessionLastRead ?? sessionPrev?.LastRead,
                SessionActive = sessionActive ?? sessionPrev?.SessionActive
            };

            var jsonString = JsonSerializer.Serialize(writeable);
            File.WriteAllText(filePath, jsonString);
            return true;
        }

        // On catching the FileLockedException, 
        // add a delay to wait 500ms?
        // try exponential backoff, increase the delay time a bit more on each retry
        // Loop a certain amount of times to access the locked data
        catch (IOException ex)
        {
            if (retryAttempt < 10)
            {
                LoggingService.LogInfo($"The file is locked: {ex.Message}");
                var iter = retryAttempt + 1;
                Thread.Sleep(50 * iter);
                WriteSessionInfoToJSON(filePath, sessionId ?? sessionPrev?.SessionId, sessionLastWrite ?? sessionPrev?.LastWrite, sessionLastRead ?? sessionPrev?.LastRead, sessionActive ?? sessionPrev?.SessionActive, iter);
            }

            LoggingService.LogError($"An error occurred: {ex.Message}", ex);

            return false;
        }
    }
}
