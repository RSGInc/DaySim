// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Concurrent;

namespace DaySim.Framework.ChoiceModels {
	public class ChoiceModelSession {
		private readonly ConcurrentDictionary<Type, IChoiceModel> _dictionary = new ConcurrentDictionary<Type, IChoiceModel>();

		public TChoiceModel Get<TChoiceModel>() where TChoiceModel : IChoiceModel {
			var type = typeof (TChoiceModel);

			var model = _dictionary.GetOrAdd(type, key => {
				var m = (IChoiceModel) Activator.CreateInstance(type);

				m.RunInitialize();

				return m;
			});

			return (TChoiceModel) model;
		}
	}
}