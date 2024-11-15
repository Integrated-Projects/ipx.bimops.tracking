using System;
using System.Collections.Generic;

public static class ModelTrackingDataCreator
{
    public static List<ModelerTrackingSchema> Create()
    {
        var random = new Random();
        var fakeData = new List<ModelerTrackingSchema>();

        for (int i = 0; i < 10; i++)
        {
            fakeData.Add(new ModelerTrackingSchema
            {
                id_project = Guid.NewGuid().ToString(),
                id_document = Guid.NewGuid().ToString(),
                id_user = Guid.NewGuid().ToString(),
                id_user_ip_address = $"{random.Next(192, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}.{random.Next(0, 255)}",
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                id_element = i % 2 == 0 ? Guid.NewGuid().ToString() : null,
                type_element = i % 2 == 0 ? "Wall" : null,
                duration = i % 2 == 0 ? random.Next(10, 300) : null
            });
        }

        return fakeData;
    }

    public static void PrintData(List<ModelerTrackingSchema> data)
    {
        foreach (var record in data)
        {
            Console.WriteLine($"Project ID: {record.id_project}, Document ID: {record.id_document}, User ID: {record.id_user}, " +
                $"IP Address: {record.id_user_ip_address}, Timestamp: {record.timestamp}, " +
                $"Element ID: {record.id_element}, Element Type: {record.type_element}, Duration: {record.duration}");
        }
    }
}
