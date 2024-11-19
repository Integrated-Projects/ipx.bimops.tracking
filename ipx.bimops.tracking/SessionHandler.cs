using System;
using System.Text.Json;

namespace ipx.bimops.tracking;

public static class SessionHandler
{
    public static Session? GetSessionInfoFromJSON(string filePath)
    {
        Session? session = null;

        try
        {
            string jsonString = File.ReadAllText(filePath);
            session = JsonSerializer.Deserialize<Session>(jsonString);

            if (session == null)
                Console.WriteLine("Failed to deserialize session data.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return session;
    }

    public static bool WriteSessionInfoToJSON(string filePath, string? sessionId = null, int? sessionLastWrite = null, int? sessionLastRead = null, bool? sessionActive = null)
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
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }
}
