namespace ipx.bimops.tracking;

using System;
using System.IO;

public class FileWatcher : IDisposable
{
    private readonly FileSystemWatcher _watcher;

    public event Action<string> OnFileChanged;

    public FileWatcher(string filePath)
    {
        var directoryPath = Path.GetDirectoryName(filePath);
        var fileName = Path.GetFileName(filePath);

        _watcher = new FileSystemWatcher(directoryPath, fileName)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
        };

        _watcher.Changed += (s, e) => OnFileChanged?.Invoke(e.FullPath);
        _watcher.EnableRaisingEvents = true;
    }

    public void WriteWithRetries()
    {
        // I start writing some data to the JSON

        // I get a FileLockedException

        // try - catch

        // On catching the Exception, 
        // add a delay to wait 500ms?
        // try exponential backoff, increase the delay time a bit more on each retry
        // Loop a certain amount of times to access the locked data
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
