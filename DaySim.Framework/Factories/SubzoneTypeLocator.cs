using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Factories {
  public class SubzoneTypeLocator : TypeLocator {
    public SubzoneTypeLocator(Configuration configuration) : base(configuration) { }

    public Type GetSubzoneType() {
      List<Type> types = new List<Type>();
      System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

      foreach (System.Reflection.Assembly assembly in assemblies) {
        types
            .AddRange(
                assembly
                    .GetTypes()
                    .Where(type => Attribute.IsDefined(type, typeof(FactoryAttribute)) && typeof(ISubzone).IsAssignableFrom(type)));
      }

      foreach (Type type in types) {
        FactoryAttribute attribute =
                    type
                        .GetCustomAttributes(typeof(FactoryAttribute), false)
                        .Cast<FactoryAttribute>()
                        .FirstOrDefault(x => x.Factory == Factory.SubzoneFactory && x.DataType == DataType);

        if (attribute != null) {
          return type;
        }
      }

      throw new Exception(string.Format("Unable to determine type. The combination using {0} and {1} for type {2} was not found.", Factory.SubzoneFactory, DataType, typeof(ISubzone).Name));
    }
  }
}