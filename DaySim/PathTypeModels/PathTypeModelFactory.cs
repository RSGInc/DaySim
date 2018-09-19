using System;
using DaySim.Framework.Core;
//using DaySim.PathTypeModels.Default;

namespace DaySim.PathTypeModels {
  internal static class PathTypeModelFactory {
    private static Type ModelType { get; set; }
    public static IPathTypeModel Singleton { get; private set; }

    public static IPathTypeModel New(object[] args) {
      //this version of CreateInstance will call the constructor that best matches the signature of the args
      return (IPathTypeModel)Activator.CreateInstance(ModelType, args);
    }

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


      ModelType = Global.Configuration.getAssignableObjectType(typeof(IPathTypeModel));

      if (ModelType == null || ModelType.IsInterface) {
        ModelType = typeof(PathTypeModel);
      }
      Singleton = New(new object[] { });
    }   //end PathTypeModelFactory
  }   //end class
}   //end namespace
