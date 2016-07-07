// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;

namespace DaySim.Framework.Factories {
	public class FactoryHelper {
		public FactoryHelper(Configuration configuration) {
			Module = new ModuleTypeLocator(configuration);
			Persistence = new PersistenceTypeLocator(configuration);
			Wrapper = new WrapperTypeLocator(configuration);
			Subzone = new SubzoneTypeLocator(configuration);
			Settings = new SettingsTypeLocator(configuration);
			ChoiceModelRunner = new ChoiceModelRunnerTypeLocator(configuration);
		}

		public ModuleTypeLocator Module { get; private set; }

		public PersistenceTypeLocator Persistence { get; private set; }

		public WrapperTypeLocator Wrapper { get; private set; }

		public SubzoneTypeLocator Subzone { get; private set; }

		public SettingsTypeLocator Settings { get; private set; }

		public ChoiceModelRunnerTypeLocator ChoiceModelRunner { get; private set; }
	}
}