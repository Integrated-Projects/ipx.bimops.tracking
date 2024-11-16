using System;
using System.IO;

public static class FileWatcher
{
    public static void Watch(string filePath)
    {
        string directoryPath = Path.GetDirectoryName(filePath);
        string fileName = Path.GetFileName(filePath);

        using var watcher = new FileSystemWatcher(directoryPath, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };

        watcher.Changed += OnChanged;

        watcher.EnableRaisingEvents = true;

        Console.WriteLine($"Watching for changes in {filePath}. Press Enter to exit.");
        Console.ReadLine(); // Keep the program running
    }

    private static void OnChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"File {e.FullPath} has been changed.");
        try
        {
            var lines = File.ReadAllLines(e.FullPath);
            Console.WriteLine($"File now contains {lines.Length} lines.");
        }
        catch (IOException)
        {
            Console.WriteLine("File is in use, retrying later...");
        }
    }
}
