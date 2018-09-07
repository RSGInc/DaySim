// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;

namespace DaySim.Framework.Core {
  public sealed class FileSizeFormatProvider : IFormatProvider, ICustomFormatter {
    private const string FILE_SIZE_FORMAT = "fs";
    private const decimal ONE_KILOBYTE = 1024M;
    private const decimal ONE_MEGABYTE = ONE_KILOBYTE * 1024M;
    private const decimal ONE_GIGABYTE = ONE_MEGABYTE * 1024M;

    public object GetFormat(Type formatType) {
      return formatType == typeof(ICustomFormatter) ? this : null;
    }

    public string Format(string format, object arg, IFormatProvider formatProvider) {
      if (format == null || !format.StartsWith(FILE_SIZE_FORMAT)) {
        return DefaultFormat(format, arg, formatProvider);
      }

      if (arg is string) {
        return DefaultFormat(format, arg, formatProvider);
      }

      decimal size;

      try {
        size = Convert.ToDecimal(arg);
      } catch (InvalidCastException) {
        return DefaultFormat(format, arg, formatProvider);
      }

      string suffix;

      if (size > ONE_GIGABYTE) {
        size /= ONE_GIGABYTE;
        suffix = "GB";
      } else if (size > ONE_MEGABYTE) {
        size /= ONE_MEGABYTE;
        suffix = "MB";
      } else if (size > ONE_KILOBYTE) {
        size /= ONE_KILOBYTE;
        suffix = "kB";
      } else {
        suffix = " B";
      }

      string precision = format.Substring(2);

      if (string.IsNullOrEmpty(precision)) {
        precision = "2";
      }

      return string.Format("{0:N" + precision + "}{1}", size, suffix);
    }

    private static string DefaultFormat(string format, object arg, IFormatProvider formatProvider) {
      IFormattable formattableArg = arg as IFormattable;

      return formattableArg != null ? formattableArg.ToString(format, formatProvider) : arg.ToString();
    }
  }
}