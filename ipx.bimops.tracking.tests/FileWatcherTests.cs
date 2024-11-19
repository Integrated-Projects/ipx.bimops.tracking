using NUnit.Framework;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ipx.bimops.tracking.tests;

public class FileWatcherTests
{
    private string _tempFilePath;

    [SetUp]
    public void Setup()
    {
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.csv");
        File.WriteAllText(_tempFilePath, "Initial content");
    }

    [TearDown]
    public void Teardown()
    {
        if (File.Exists(_tempFilePath))
        {
            File.Delete(_tempFilePath);
        }
    }

    [Test]
    public async Task FileWatcher_ShouldTriggerOnFileChanged()
    {
        var tcs = new TaskCompletionSource<string>();

        using (var watcher = new FileWatcher(_tempFilePath))
        {
            int lineCount = 0;
            void OnChangedHandler(string path)
            {
                tcs.TrySetResult(path); // Signal the task that the event has fired
                lineCount = File.ReadAllLines(path).Length;
                Assert.That(lineCount, Is.GreaterThan(1));
            }

            Assert.That(lineCount, Is.EqualTo(1));

            watcher.OnFileChanged += OnChangedHandler;

            // Modify the file to trigger the OnChanged event
            File.AppendAllText(_tempFilePath, "New content");
            lineCount = File.ReadAllLines(_tempFilePath).Length;


            // Wait for the event to be triggered or timeout
            var filePath = await Task.WhenAny(tcs.Task, Task.Delay(2000)) == tcs.Task
                ? tcs.Task.Result
                : null;

            // watcher.OnFileChanged -= OnChangedHandler; // Unsubscribe
            // watcher.Stop(); // Stop the watcher
        }
    }
}
