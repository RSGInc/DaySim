using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DaySim.Framework.Core {
  public static class UtilityFunctions {
    /**
     * from http://www.developer.com/net/csharp/article.php/3713886/NET-Tip-Display-All-Fields-and-Properties-of-an-Object.htm
     * 
     */
    public static string DisplayObjectInfo(object o) {
      StringBuilder sb = new StringBuilder();

      // Include the type of the object
      System.Type type = o.GetType();
      sb.Append("Type: " + type.Name);

      // Include information for each Field
      sb.Append("\r\n\r\nFields:");
      System.Reflection.FieldInfo[] fi = type.GetFields(
                   BindingFlags.NonPublic |
                   BindingFlags.Instance);
      if (fi.Length > 0) {
        foreach (FieldInfo f in fi) {
          sb.Append("\r\n " + f.ToString() + " = " +
                    f.GetValue(o));
        }
      } else {
        sb.Append("\r\n None");
      }

      // Include information for each Property
      sb.Append("\r\n\r\nProperties:");
      System.Reflection.PropertyInfo[] pi = type.GetProperties(
                   BindingFlags.NonPublic |
                   BindingFlags.Instance);
      if (pi.Length > 0) {
        foreach (PropertyInfo p in pi) {
          sb.Append("\r\n " + p.ToString() + " = " +
                    p.GetValue(o, null));
        }
      } else {
        sb.Append("\r\n None");
      }

      return sb.ToString();
    }
  }   //end DisplayObjectInfo

  //https://github.com/mcshaz/BlowTrial/blob/master/GenericToDataFile/ObjectDumper.cs
  //http://stackoverflow.com/questions/852181/c-printing-all-properties-of-an-object
  //PCA added Binding.NonPublic
  public class ObjectDumper {
    private int _currentIndent;
    private readonly int _indentSize;
    private readonly int _maxEnumeratedItems;
    private readonly StringBuilder _stringBuilder;
    private readonly Dictionary<object, int> _hashListOfFoundElements;
    private readonly char _indentChar;
    private readonly int _depth;
    private int _currentLine;

    private ObjectDumper(int depth, int maxEnumeratedItems, int indentSize, char indentChar) {
      _depth = depth;
      _maxEnumeratedItems = maxEnumeratedItems;
      _indentSize = indentSize;
      _indentChar = indentChar;
      _stringBuilder = new StringBuilder();
      _hashListOfFoundElements = new Dictionary<object, int>();
    }

    public static string Dump(object element, int depth = 4, int maxEnumeratedItems = 10,
    int indentSize = 2, char indentChar = ' ') {
      ObjectDumper instance = new ObjectDumper(depth, maxEnumeratedItems, indentSize, indentChar);
      string dumpedObjectString;
      try {
        dumpedObjectString = instance.DumpElement(element, true);
      } catch (Exception ex) {
        dumpedObjectString = "Caught exception while trying to Dump object of type: " + element.GetType() + " ToString: " + element.ToString() + " Exception: " + ex.ToString();
        Debug.WriteLine(dumpedObjectString);
      }
      return dumpedObjectString;
    }

    private string DumpElement(object element, bool isTopOfTree = false) {
      if (_currentIndent > _depth) { return null; }
      if (element == null || element is string) {
        Write(FormatValue(element));
      } else if (element is ValueType) {
        Type objectType = element.GetType();
        bool isWritten = false;
        if (objectType.IsGenericType) {
          Type baseType = objectType.GetGenericTypeDefinition();
          if (baseType == typeof(KeyValuePair<,>)) {
            isWritten = true;
            Write("Key:");
            _currentIndent++;
            DumpElement(objectType.GetProperty("Key").GetValue(element, null));
            _currentIndent--;
            Write("Value:");
            _currentIndent++;
            DumpElement(objectType.GetProperty("Value").GetValue(element, null));
            _currentIndent--;
          }
        }
        if (!isWritten) {
          Write(FormatValue(element));
        }
      } else {
        IEnumerable enumerableElement = element as IEnumerable;
        if (enumerableElement != null) {
          try {
            int enumeratedCount = 0;
            foreach (object item in enumerableElement) {
              if (item is IEnumerable && !(item is string)) {
                _currentIndent++;
                DumpElement(item);
                _currentIndent--;
              } else {
                DumpElement(item);
              }
              ++enumeratedCount;
              if (_maxEnumeratedItems > 0 && enumeratedCount >= _maxEnumeratedItems) {
                Write("...stopping after " + enumeratedCount + " items");
                break;
              }
            }
          } catch (Exception e) {
            Write("Caught exception with enumerating object of type " + enumerableElement.GetType() + ". Exception: " + e.ToString());
          }
        } else {
          Type objectType = element.GetType();
          Write("{{{0}(HashCode:{1})}}", objectType.FullName, element.GetHashCode());
          if (!AlreadyDumped(element)) {
            _currentIndent++;
            MemberInfo[] members = objectType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (MemberInfo memberInfo in members) {
              FieldInfo fieldInfo = memberInfo as FieldInfo;
              PropertyInfo propertyInfo = memberInfo as PropertyInfo;

              if (fieldInfo == null && (propertyInfo == null || !propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length > 0)) {
                continue;
              }

              Type type = fieldInfo != null ? fieldInfo.FieldType : propertyInfo.PropertyType;
              object value;
              try {
                value = fieldInfo != null
                                   ? fieldInfo.GetValue(element)
                                   : propertyInfo.GetValue(element, null);
              } catch (Exception e) {
                Write("{0} failed with:{1}", memberInfo.Name, (e.GetBaseException() ?? e).Message);
                continue;
              }

              if (type.IsValueType || type == typeof(string)) {
                Write("{0}: {1}", memberInfo.Name, FormatValue(value));
              } else {
                bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
                Write("{0}: {1}", memberInfo.Name, isEnumerable ? "..." : "{ }");

                _currentIndent++;
                DumpElement(value);
                _currentIndent--;
              }
            }
            _currentIndent--;
          }
        }
      }

      return isTopOfTree ? _stringBuilder.ToString() : null;
    }

    private bool AlreadyDumped(object value) {
      if (value == null) {
        return false;
      }

      if (_hashListOfFoundElements.TryGetValue(value, out int lineNo)) {
        Write("(reference already dumped - line:{0})", lineNo);
        return true;
      }
      _hashListOfFoundElements.Add(value, _currentLine);
      return false;
    }

    private void Write(string value, params object[] args) {
      string space = new string(_indentChar, _currentIndent * _indentSize);

      if (args != null) {
        value = string.Format(value, args);
      }

      _stringBuilder.AppendLine(space + value);
      _currentLine++;
    }

    private string FormatValue(object o) {
      if (o == null) {
        return ("null");
      }

      if (o is DateTime) {
        return (((DateTime)o).ToShortDateString());
      }

      if (o is string) {
        return "\"" + (string)o + "\"";
      }

      if (o is char) {
        if (o.Equals('\0')) {
          return "''";
        } else {
          return "'" + (char)o + "'";
        }
      }

      if (o is ValueType) {
        return (o.ToString());
      }

      if (o is IEnumerable) {
        return ("...");
      }

      return ("{ }");
    }

  }   //end class ObjectDumper
}   //end namespace
