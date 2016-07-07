// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DaySim.Framework.Core;

namespace DaySim.Framework.Factories {
	public class SettingsTypeLocator : TypeLocator {
		private readonly string _settings;

		public SettingsTypeLocator(Configuration configuration) : base(configuration) {
			if (string.IsNullOrWhiteSpace(configuration.Settings)) {
				var builder = new StringBuilder();

				builder
					.AppendLine("Please configure the desired settings file to be used before continuing.")
					.AppendLine()
					.AppendLine("Here's an example of how that would look inside of the configuration file:")
					.AppendLine()
					.AppendLine("...")
					.AppendLine("Settings=DefaultSettings")
					.AppendLine("...");

				throw new Exception(builder.ToString());
			}

			_settings = configuration.Settings;
		}

		public Type GetSettingsType() {
			var types = new List<Type>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (var assembly in assemblies) {
				types
					.AddRange(
						assembly
							.GetTypes()
							.Where(type => Attribute.IsDefined(type, typeof (FactoryAttribute)) && typeof (ISettings).IsAssignableFrom(type)));
			}

			foreach (var type in types) {
				var attribute =
					type
						.GetCustomAttributes(typeof (FactoryAttribute), false)
						.Cast<FactoryAttribute>()
						.FirstOrDefault(x => x.Factory == Factory.SettingsFactory && type.Name == _settings);

				if (attribute != null) {
					return type;
				}
			}

			throw new Exception(string.Format("Unable to determine type. The combination using {0} and {1} for type {2} was not found.", Factory.SettingsFactory, _settings, typeof (ISettings).Name));
		}
	}
}