// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DaySim.Framework.ChoiceModels {
    public class ChoiceModelSession {
        private readonly LazyConcurrentDictionary<Type, IChoiceModel> choiceModelObjectsDictionary = new LazyConcurrentDictionary<Type, IChoiceModel>();

        public TChoiceModel Get<TChoiceModel>() where TChoiceModel : IChoiceModel {
            Type requestedType = typeof(TChoiceModel);
            IChoiceModel choiceModelObject = choiceModelObjectsDictionary.GetOrAdd(requestedType, (key) => {
                //create the Singleton of type (or derived type) of TChoiceModel
                Type possiblyCustomizedType = Global.Configuration.getAssignableObjectType(requestedType);
                choiceModelObject = (IChoiceModel)Activator.CreateInstance(possiblyCustomizedType);

                choiceModelObject.RunInitialize();
                Global.PrintFile.WriteLine("CustomizationDll Get<TChoiceModel> for '" + requestedType + "' just created object of type '" + possiblyCustomizedType);
                return choiceModelObject;
            });

            return (TChoiceModel)choiceModelObject;
        }   //end Get<TChoiceModel>

    }   //end class
}   //end namespace