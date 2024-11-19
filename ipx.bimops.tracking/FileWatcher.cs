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

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}
