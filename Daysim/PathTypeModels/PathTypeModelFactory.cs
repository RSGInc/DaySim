using System;
using System.Linq;
using Daysim.Framework.Core;
using Daysim.Framework.Factories;
using Daysim.PathTypeModels;
//using Daysim.PathTypeModels.Default;

namespace Daysim.PathTypeModels {
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
						throw new ApplicationException(string.Format("PathTypeModel '{0}' can not be created", type));
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
			switch (Global.PathTypeModel) {
				case "PathTypeModel":
					ModelType = typeof (PathTypeModel_Actum);
					Create<PathTypeModel_Actum>();
					break;
				case "PathTypeModel_Actum":
					ModelType = typeof (PathTypeModel_Actum);
					Create<PathTypeModel_Actum>();
					break;
//				case "PathTypeModel_alternate":
//					ModelType = typeof (PathTypeModel_alternate);
//					Create<PathTypeModel_alternate>();
//					break;
				case "PathTypeModel_override":
					ModelType = typeof (PathTypeModel_override);
					Create<PathTypeModel_override>();
					break;
				default:
					throw new ApplicationException(string.Format("Path type model class '{0}' cannot be found in PathTypeModels", Global.PathTypeModel));
			}
		}

		private static void Create<T>() {
			Model = (IPathTypeModel) Activator.CreateInstance(typeof (T));
		}
	}
}
