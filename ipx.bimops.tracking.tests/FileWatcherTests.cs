using NUnit.Framework;
using System;
using System.IO;
using System.Threading;

namespace ipx.bimops.tracking.tests
{
    [TestFixture]
    public class FileWatcherTests
    {
        private string _tempFilePath;
        private FileWatcher? _fileWatcher;

        [SetUp]
        public void Setup()
        {
            // Create a temporary file
            _tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
            File.WriteAllText(_tempFilePath, "Line 1\nLine 2\nLine 3\n");
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up the temporary file and stop the watcher
            _fileWatcher?.Dispose();
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }

        [Test]
        public void FileWatcher_Should_Trigger_OnFileChanged_When_File_Is_Updated()
        {
            // Arrange
            var fileChanged = new ManualResetEvent(false); // Used to wait for the event
            int initialLineCount = File.ReadLines(_tempFilePath).Count();
            int newLineCount = 0;

            _fileWatcher = new FileWatcher(_tempFilePath);
            _fileWatcher.OnFileChanged += path =>
            {
                // Ensure we read after the file is fully updated
                Thread.Sleep(100); // Slight delay to allow for the update to propagate
                newLineCount = File.ReadLines(path).Count();
                fileChanged.Set(); // Signal that the event was triggered
            };

            // Act
            AppendNewLines(_tempFilePath, new[] { "Line 4", "Line 5" });
            var eventTriggered = fileChanged.WaitOne(6000); // Wait up to 2 seconds for the event

            // Assert
            Assert.That(eventTriggered, Is.True, "OnFileChanged event was not triggered.");
            Assert.That(newLineCount, Is.EqualTo(initialLineCount + 2), "The line count is not what it should be");
        }
        private void AppendNewLines(string filePath, string[] newLines)
        {
            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                foreach (var line in newLines)
                {
                    writer.WriteLine(line);
                }
                writer.Flush(); // Ensure content is flushed to disk
            }
        }
    }
}
