using System;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.Factories;
using DaySim.PathTypeModels;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
//using DaySim.PathTypeModels.Default;

namespace DaySim.PathTypeModels {
  static class GenericPluginLoader<T> {
    public static T LoadPlugin(string dllFile) {
      T plugin = default(T);
      Type pluginType = typeof(T);

      bool fileExists = File.Exists(dllFile);
      if (fileExists) {
        AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
        Assembly assembly = Assembly.Load(an);

        if (assembly != null) {
          Type[] types = assembly.GetTypes();

          foreach (Type type in types) {
            if (pluginType.IsAssignableFrom(type)) {
              plugin = (T)Activator.CreateInstance(type);
              break;
            }
          } //end foreach types
        } //end if assembly not null
      } //end if file exists
      if (plugin == null) {
        throw new Exception("LoadPlugin: Could not load dll: " + dllFile + ". File exists?: " + fileExists);
      } else {
        Global.PrintFile.WriteLine("LoadPlugin: Successfully loaded dll: " + dllFile + " with type " + pluginType);
      }
      return plugin;
    } //end LoadPlugin
  } //end class GenericPluginLoader

  static class PathTypeModelFactory {
    public static Type ModelType { get; private set; }
    public static dynamic Model { get; private set; }

    static PathTypeModelFactory() {
      /*
			ChoiceModelRunner type;
			var config = Global.Configuration;

			if (Enum.TryParse(config.ChoiceModelRunner, out type)) {

				switch (type) {
					case ChoiceModelRunner.Default:
						ModelType = typeof (PathTypeModel);
						Create<PathTypeModel>();
						break;
					case ChoiceModelRunner.Actum:
						ModelType = typeof (Actum.PathTypeModel);
						Create<Actum.PathTypeModel>();
						break;
					case ChoiceModelRunner.H:
						ModelType = typeof (H.PathTypeModel);
						Create<H.PathTypeModel>();
						break;
					default:
						throw new ApplicationException(string.Format("PathTypeModel '{0}' cannot be created", type));
				}
				return;
			}

			var values =
				Enum
					.GetValues(typeof(ChoiceModelRunner))
					.Cast<ChoiceModelRunner>()
					.ToList();

			throw new Exception(string.Format("Unable to determine type. The choice model runner set to \"{0}\" is not valid. Valid values are {1}", config.ChoiceModelRunner, string.Join(", ", values)));
		  */


      if (!string.IsNullOrWhiteSpace(Global.Configuration.CustomizationDll)) {
        string pluginsPath = Global.GetInputPath(Global.Configuration.CustomizationDll);
        Model = GenericPluginLoader<IPathTypeModel>.LoadPlugin(pluginsPath);
      } else {
        switch (Global.PathTypeModel) {
          case "PathTypeModel":
            ModelType = typeof(PathTypeModel);
            Create<PathTypeModel>();
            break;
          //				case "PathTypeModel_Actum":
          //					ModelType = typeof (PathTypeModel_Actum);
          //					Create<PathTypeModel_Actum>();
          //					break;
          //				case "PathTypeModel_alternate":
          //					ModelType = typeof (PathTypeModel_alternate);
          //					Create<PathTypeModel_alternate>();
          //					break;
          case "PathTypeModel_override":
            ModelType = typeof(PathTypeModel_override);
            Create<PathTypeModel_override>();
            break;
          default:
            throw new ApplicationException(string.Format("Path type model class '{0}' cannot be found in PathTypeModels", Global.PathTypeModel));
        } //end switch
      } //end if not using a plugin
    } //end PathTypeModelFactory()

    private static void Create<T>() {
      Model = (IPathTypeModel)Activator.CreateInstance(typeof(T));
    }
  }
}
