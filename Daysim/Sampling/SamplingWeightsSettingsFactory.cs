// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using Daysim.Framework.Core;

namespace Daysim.Sampling {
	public class SamplingWeightsSettingsFactory 
	{
		private readonly string _key = Global.SamplingWeightsSettingsType;

		public ISamplingWeightsSettings SamplingWeightsSettings { get; private set; }

		private readonly Dictionary<String, ISamplingWeightsSettings> _settings = new Dictionary<string, ISamplingWeightsSettings>(); 

		public void Register(String key, ISamplingWeightsSettings value)
		{
			_settings.Add(key, value);
		}

		public void Initialize()
		{
			SamplingWeightsSettings = _settings[_key];
		}
	}
}
