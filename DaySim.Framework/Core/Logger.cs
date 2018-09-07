// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Collections.Generic;
using System.IO;

namespace DaySim.Framework.Core {
  public sealed class Logger : IDisposable {
    private readonly StreamWriter _writer;
    private readonly List<string> _buffer = new List<string>();
    private bool _condition;
    private bool _isNested;
    private bool _logging;

    public Logger(string path) {
      FileInfo file = new FileInfo(path);

      _writer = new StreamWriter(file.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    public void SetCondition(bool condition) {
      _condition = condition;
    }

    public void StartLogging(string title = null) {
      if (!_condition) {
        return;
      }

      _logging = true;

      _buffer.Clear();

      if (!string.IsNullOrEmpty(title)) {
        WriteHeader('*', title);
      }
    }

    public void StopLogging() {
      if (!_condition) {
        return;
      }

      _writer.Flush();

      _logging = false;
    }

    public void StartNestedLogging(string title = null) {
      if (!_logging) {
        return;
      }

      _isNested = true;

      if (!string.IsNullOrEmpty(title)) {
        WriteHeader('-', title);
      }
    }

    public void StopNestedLogging() {
      if (!_logging) {
        return;
      }

      WriteHeader('-');

      _isNested = false;
    }

    private void WriteHeader(char c, string line = null) {
      if (!_logging) {
        return;
      }

      if (_isNested) {
        line = GetLine(line, c, 78);

        AddToBuffer(line);
      } else {
        line = GetLine(line, c, 80);

        _writer.WriteLine(line);
      }
    }

    private static string GetLine(string line, char c, int columns) {
      if (line == null) {
        return new string(c, columns);
      }

      line = string.Format(" {0} ", line);

      int length = columns - line.Length;
      int paddingLeft = length / 2;
      int paddingRight = length - paddingLeft;

      return string.Format("{0}{1}{2}", new string(c, paddingLeft), line, new string(c, paddingRight));
    }

    public void WriteLine(string line) {
      if (!_logging) {
        return;
      }

      _writer.WriteLine(line);
    }

    public void AddToBuffer(string line) {
      if (!_logging) {
        return;
      }

      if (_isNested) {
        _buffer.Add("> " + line);
      } else {
        _buffer.Add(line);
      }
    }

    public void WriteBuffer() {
      if (!_logging || _buffer.Count == 0) {
        return;
      }

      WriteHeader('*');

      foreach (string line in _buffer) {
        _writer.WriteLine(line);
      }
    }

    public void Dispose() {
      _writer.Dispose();
    }
  }
}