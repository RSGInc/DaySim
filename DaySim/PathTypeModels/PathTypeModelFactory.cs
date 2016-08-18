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


            ModelType = typeof(PathTypeModel);
            Model = Global.Configuration.getCustomizationType(ModelType);

            if (Model == null) {
                Model = (IPathTypeModel)Activator.CreateInstance(ModelType);
            }
        }   //end PathTypeModelFactory
    }   //end class
}   //end namespace
