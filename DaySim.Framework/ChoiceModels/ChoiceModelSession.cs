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

namespace DaySim.Framework.ChoiceModels
{
    public class ChoiceModelSession
    {
        //could use a ConcurrentDictionary with GetOrAdd method but then the object is possible 
        //created and initialized multiple times depending on threading
        //See https://blogs.endjin.com/2015/10/using-lazy-and-concurrentdictionary-to-ensure-a-thread-safe-run-once-lazy-loaded-collection/
        private readonly Dictionary<Type, IChoiceModel> choiceModelObjectsDictionary = new Dictionary<Type, IChoiceModel>();

        public TChoiceModel Get<TChoiceModel>() where TChoiceModel : IChoiceModel
        {
            Type requestedType = typeof(TChoiceModel);
            IChoiceModel choiceModelObject;
            if (!choiceModelObjectsDictionary.TryGetValue(requestedType, out choiceModelObject))
            {
                lock (choiceModelObjectsDictionary)
                {
                    //after acquiring lock check that object still needs to be created
                    if(!choiceModelObjectsDictionary.TryGetValue(requestedType, out choiceModelObject))
                    {
                        //create the Singleton of type (or derived type) of TChoiceModel
                        Type possiblyCustomizedType = Global.Configuration.getAssignableObjectType(requestedType);
                        choiceModelObject = (IChoiceModel)Activator.CreateInstance(possiblyCustomizedType);

                        choiceModelObject.RunInitialize();
                        //add to dictionary so we have quick retrieval next time.
                        choiceModelObjectsDictionary.Add(requestedType, choiceModelObject);
                    } //end if still not in dictionary after getting lock
                }   //end lock
            }   //end if not in dictionary
            return (TChoiceModel) choiceModelObject;
        }   //end Get<TChoiceModel>
    }   //end class
}   //end namespace