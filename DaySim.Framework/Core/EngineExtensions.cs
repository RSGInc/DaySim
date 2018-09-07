// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
using System.IO;

namespace DaySim.Framework.Core {
  public static class EngineExtensions {
    public static void CreateDirectory(this string path) {
      CreateDirectory(new FileInfo(path));
    }

    public static void CreateDirectory(this FileInfo file) {
      if (file == null) {
        return;
      }

      CreateDirectory(file.Directory);
    }

    public static void CreateDirectory(this DirectoryInfo directory) {
      if (directory == null) {
        return;
      }

      if (directory.Exists) {
        return;
      }

      directory.Create();

      if (Global.PrintFile != null) {
        Global.PrintFile.WriteLine(@"The directory ""{0}"" did not exist and has been created.", directory.FullName);
      }
    }

    public static string ToIndexedPath(this string path, int index) {
      if (string.IsNullOrEmpty(path)) {
        return null;
      }

      string extension = "." + index + Path.GetExtension(path);

      return Path.GetDirectoryName(path) + @"\" + Path.GetFileNameWithoutExtension(path) + extension;
    }
  }
}
