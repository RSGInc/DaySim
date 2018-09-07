using System;
using System.IO;
using System.Reflection;

namespace DaySim.Framework.Core {
  public class ErrorFile : IDisposable {
    public const string DEFAULT_ERROR_FILENAME = "errors.log";
    private readonly StreamWriter _writer;
    private int _indent;

    public ErrorFile(string path = null) {
      if (string.IsNullOrEmpty(path)) {
        string location = Assembly.GetExecutingAssembly().Location;
        string directoryName = Path.GetDirectoryName(location);

        path = directoryName == null ? DEFAULT_ERROR_FILENAME : Path.Combine(directoryName, DEFAULT_ERROR_FILENAME);
      }

      FileInfo file = new FileInfo(path);

      try {
        _writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) { AutoFlush = false };
      } catch (Exception) {
        Console.WriteLine("The path to the error file, {0}, is invalid. Please enter a valid path.", file.FullName);
        Console.WriteLine("Please press any key to exit");
        Console.ReadKey();
        Environment.Exit(2);
      }
    }

    public void IncrementIndent() {
      _indent += 2;
    }

    public void DecrementIndent() {
      _indent -= 2;
    }

    public virtual void WriteLine(string value = null) {
      if (string.IsNullOrEmpty(value)) {
        _writer.WriteLine();
      } else {
        _writer.WriteLine(new string(' ', _indent) + value);
      }

      _writer.Flush();
    }

    public void WriteLine(string format, params object[] values) {
      WriteLine(string.Format(format, values));
    }

    public void Dispose() {
      if (_writer != null) {
        _writer.Dispose();
      }
    }

  }
}