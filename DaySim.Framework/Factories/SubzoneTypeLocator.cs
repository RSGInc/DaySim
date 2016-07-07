using System;
using System.Collections.Generic;
using System.Linq;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Factories {
	public class SubzoneTypeLocator : TypeLocator {
		public SubzoneTypeLocator(Configuration configuration) : base(configuration) {}

		public Type GetSubzoneType() {
			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies) {
				types
					.AddRange(
						assembly
							.GetTypes()
							.Where(type => Attribute.IsDefined(type, typeof (FactoryAttribute)) && typeof (ISubzone).IsAssignableFrom(type)));
			}

			foreach (var type in types) {
				var attribute =
					type
						.GetCustomAttributes(typeof (FactoryAttribute), false)
						.Cast<FactoryAttribute>()
						.FirstOrDefault(x => x.Factory == Factory.SubzoneFactory && x.DataType == DataType);

				if (attribute != null) {
					return type;
				}
			}

			throw new Exception(string.Format("Unable to determine type. The combination using {0} and {1} for type {2} was not found.", Factory.SubzoneFactory, DataType, typeof (ISubzone).Name));
		}
	}
}