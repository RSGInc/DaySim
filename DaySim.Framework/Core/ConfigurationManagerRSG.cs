// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DaySim.Framework.Core {
  public class ConfigurationManagerRSG {
    public const string DEFAULT_CONFIGURATION_NAME = "Configuration.xml";

    private readonly FileInfo _file;
    private readonly string _extension;

    public ConfigurationManagerRSG(string path) {
      if (string.IsNullOrEmpty(path)) {
        string directoryName = GetExecutingAssemblyLocation();

        path =
            directoryName == null
                ? DEFAULT_CONFIGURATION_NAME
                : Path.Combine(directoryName, DEFAULT_CONFIGURATION_NAME);
      }

      _file = new FileInfo(path);

      _extension =
          Path
              .GetExtension(_file.Name)
              .ToLower();
    }

    public static string GetExecutingAssemblyLocation() {
      string location = Assembly.GetExecutingAssembly().Location;
      string directoryName = Path.GetDirectoryName(location);
      return directoryName;
    }

    public class Group {
      public string GroupName;
    }


    private void Serializer_UnknownAttribute(object sender, XmlAttributeEventArgs e) {
      Console.Error.WriteLine("WARNING - Unknown attribute: \t" + e.Attr.Name + " " + e.Attr.InnerXml + "\t LineNumber: " + e.LineNumber + "\t LinePosition: " + e.LinePosition);
    }

    public Configuration Open() {
      using (FileStream stream = _file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
        if (_extension == ".xml") {
          XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
          // Add a delegate to handle unknown element events.
          serializer.UnknownAttribute += new XmlAttributeEventHandler(Serializer_UnknownAttribute);
          return (Configuration)serializer.Deserialize(stream);
        }

        if (_extension == ".properties") {
          return Deserialize(stream);
        }
      }

      return null;
    }

    public void Write(Configuration configuration, PrintFile printFile) {
      PropertyInfo[] properties = typeof(Configuration).GetProperties(BindingFlags.Public | BindingFlags.Instance);

      if (_extension == ".xml") {
        WriteFromXml(printFile, properties);
      }

      if (_extension == ".properties") {
        WriteFromProperties(printFile, properties);
      }

      var list =
          properties
              .Select(property => {
                object value = property.GetValue(configuration, null);

                MetadataAttribute metadata =
                      property
                          .GetCustomAttributes(typeof(MetadataAttribute), true)
                          .Cast<MetadataAttribute>()
                          .SingleOrDefault();

                string description =
                      metadata == null || string.IsNullOrEmpty(metadata.Value)
                          ? property.Name.ToSentenceCase()
                          : metadata.Value;

                string format;

                if (value == null) {
                  format = string.Empty;
                } else {
                  if (value is char) {
                    byte b = (byte)(char)value;

                    format = string.Format("{0} - {1}", b, AsciiTable.GetDescription(b));
                  } else {
                    format = value.ToString().Trim();
                  }
                }

                return new {
                  property.Name,
                  Value = format,
                  Description = description
                };
              })
              .ToList();

      int maxKeyLength =
                list
                    .Select(x => x.Name.Length)
                    .Max();

      int maxValueLength =
                list
                    .Select(x => x.Value.Length)
                    .Max();

      foreach (var property in list) {
        printFile
            .WriteLine("{0}> {1} // {2}.",
                property.Name.PadLeft(maxKeyLength),
                property.Value.PadRight(maxValueLength),
                property.Description);
      }

      printFile.WriteLine();
    }

    private void WriteFromXml(PrintFile printFile, PropertyInfo[] properties) {
      using (FileStream stream = _file.Open(FileMode.Open, FileAccess.Read, FileShare.Read)) {
        XDocument document = XDocument.Load(stream);

        List<string> attributes =
                    document.Root == null
                        ? new List<string>()
                        : document
                            .Root
                            .Attributes()
                            .Select(x => x.Name.LocalName)
                            .ToList();

        WriteInvalidAttributes(printFile, properties, attributes);
        WriteUnusedProperties(printFile, properties, attributes);
      }
    }

    private void WriteFromProperties(PrintFile printFile, PropertyInfo[] properties) { }

    private static void WriteUnusedProperties(PrintFile printFile, IEnumerable<PropertyInfo> properties, IEnumerable<string> attributes) {
      List<string> list =
                properties
                    .Where(property => attributes.All(x => x != property.Name))
                    .Select(property => property.Name)
                    .ToList();

      if (list.Count == 0) {
        return;
      }

      printFile.WriteLine("The following properties in the configuration file were not set:");
      printFile.IncrementIndent();

      foreach (string item in list) {
        printFile.WriteLine("* {0}", item);
      }

      printFile.DecrementIndent();
      printFile.WriteLine();
    }

    private static void WriteInvalidAttributes(PrintFile printFile, IEnumerable<PropertyInfo> properties, IEnumerable<string> attributes) {
      List<string> list =
                attributes
                    .Where(attribute => attribute != "xsd" && attribute != "xsi" && properties.All(x => x.Name != attribute))
                    .ToList();

      if (list.Count == 0) {
        return;
      }

      printFile.WriteLine("The following attributes in the configuration file are invalid:");
      printFile.IncrementIndent();

      foreach (string item in list) {
        printFile.WriteLine("* {0}", item);
      }

      printFile.DecrementIndent();
      printFile.WriteLine();
    }

    private static Configuration Deserialize(Stream stream) {
      Configuration configuration = new Configuration();
      Type type1 = configuration.GetType();
      List<string> keys = new List<string>();
      int number = 0;

      using (CountingReader reader = new CountingReader(stream)) {
        string line;

        while ((line = reader.ReadLine()) != null) {
          line = line.Trim();
          number++;

          if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) {
            continue;
          }

          string key = line.Split('=')[0].Trim();
          string value = string.Join("=", line.Split('=').Skip(1).ToArray()).Trim();

          if (!CodeGenerator.IsValidLanguageIndependentIdentifier(key)) {
            StringBuilder builder = new StringBuilder();

            builder
                .AppendFormat("Error reading configuration file on line {0}.", number).AppendLine()
                .AppendFormat("The indentifer \"{0}\" is invalid.", key).AppendLine()
                .AppendLine("Please correct or remove the invalid identifer.");

            throw new Exception(builder.ToString());
          }

          if (keys.Contains(key)) {
            StringBuilder builder = new StringBuilder();

            builder
                .AppendFormat("Error reading configuration file on line {0}.", number).AppendLine()
                .AppendFormat("The file contains a duplicate entry for \"{0}\".", key).AppendLine()
                .AppendLine("Please ensure that there are no duplicate entries inside of the configuration file.");

            throw new Exception(builder.ToString());
          }

          keys.Add(key);

          PropertyInfo property = type1.GetProperty(key, BindingFlags.Public | BindingFlags.Instance);

          if (property == null) {
            continue;
          }

          Type type2 = property.PropertyType;

          try {
            if (type2 == typeof(char)) {
              object b = Convert.ChangeType(value, typeof(byte));

              property.SetValue(configuration, Convert.ChangeType(b, type2), null);
            } else if (type2.IsEnum) {
              if (type2 == typeof(Configuration.NodeDistanceReaderTypes)) {
                property.SetValue(configuration, (Configuration.NodeDistanceReaderTypes)Enum.Parse(typeof(Configuration.NodeDistanceReaderTypes), value), null);
              } else {
                throw new Exception("Unhandled enum type in configuration parsing '" + key + "' of type '" + type2.FullName + "'.");
              }
            } else {  //some other type?
              property.SetValue(configuration, Convert.ChangeType(value, type2), null);
            }
          } catch {
            StringBuilder builder = new StringBuilder();

            builder
                .AppendFormat("Error reading configuration file on line {0}.", number).AppendLine()
                .AppendFormat("Cannot convert the value of \"{0}\" to the type of {1}.", value, type2.Name).AppendLine()
                .AppendLine("Please ensure that the value is in the correct format for the given type.");

            throw new Exception(builder.ToString());
          }
        }
      }

      return configuration;
    }

    public Configuration OverrideConfiguration(Configuration configuration, string overrides) {
      //read possible overrides
      string[] nameValuePairs = overrides.Trim().Split(',');
      // if (nameValuePairs.Length)
      if (nameValuePairs.Length > 0 && nameValuePairs[0].Trim().Length > 0) {
        Dictionary<string, string> keyValuePairs = nameValuePairs
       .Select(value => value.Split('='))
       .ToDictionary(pair => pair[0].Trim(), pair => pair[1].Trim());

        Type type1 = configuration.GetType();
        foreach (KeyValuePair<string, string> entry in keyValuePairs) {
          PropertyInfo property = type1.GetProperty(entry.Key, BindingFlags.Public | BindingFlags.Instance);

          if (property == null) {
            Console.WriteLine("WARNING: override key value pair ignored because key not found!: " + entry);
            continue;
          }

          Type type2 = property.PropertyType;

          try {
            if (type2 == typeof(char)) {
              object b = Convert.ChangeType(entry.Value, typeof(byte));

              property.SetValue(configuration, Convert.ChangeType(b, type2), null);
            } else {
              property.SetValue(configuration, Convert.ChangeType(entry.Value, type2), null);
            }
            Console.WriteLine("Configuration override applied: " + entry);
          } catch {
            StringBuilder builder = new StringBuilder();

            builder
                .AppendFormat("Error overriding configuration file for entry {0}.", entry).AppendLine()
                .AppendFormat("Cannot convert the value of \"{0}\" to the type of {1}.", entry.Value, type2.Name).AppendLine()
                .AppendLine("Please ensure that the value is in the correct format for the given type.");

            throw new Exception(builder.ToString());
          }
        }
      }
      return (configuration);
    }

    public Configuration ProcessPath(Configuration configuration, string configurationPath) {

      if (string.IsNullOrWhiteSpace(configuration.BasePath)) {
        //issue #52 use configuration file folder as default basepath rather than arbitrary current working directory.
        configuration.BasePath = Path.GetDirectoryName(Path.GetFullPath(configurationPath));
      }

      //copy the configuration file into the output so we can tell if configuration changed before regression test called.
      string archiveConfigurationFilePath = Global.GetOutputPath("archive_" + Path.GetFileName(configurationPath));
      archiveConfigurationFilePath.CreateDirectory(); //create output directory if needed
      File.Copy(configurationPath, archiveConfigurationFilePath, /* overwrite */ true);

      return (configuration);
    }

    public PrintFile ProcessPrintPath(PrintFile printFile, string printFilePath) {

      if (string.IsNullOrWhiteSpace(printFilePath)) {
        printFilePath = Global.GetOutputPath(PrintFile.DEFAULT_PRINT_FILENAME);
      }

      if (string.IsNullOrWhiteSpace(printFilePath)) {
        printFilePath = Global.GetOutputPath(PrintFile.DEFAULT_PRINT_FILENAME);
      }

      printFilePath.CreateDirectory(); //create printfile directory if needed
      printFile = new PrintFile(printFilePath, Global.Configuration);
      Write(Global.Configuration, printFile);

      return (printFile);
    }
  }
}