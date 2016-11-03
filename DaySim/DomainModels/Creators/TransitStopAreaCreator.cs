// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Creators;
using DaySim.Framework.DomainModels.Models;
using DaySim.Framework.DomainModels.Wrappers;
using DaySim.Framework.Factories;
using System;

namespace DaySim.DomainModels.Creators {
    [UsedImplicitly]
    [Factory(Factory.WrapperFactory, Category = Category.Creator)]
    public class TransitStopAreaCreator<TWrapper, TModel> : ITransitStopAreaCreator where TWrapper : ITransitStopAreaWrapper where TModel : ITransitStopArea, new() {
        ITransitStopArea ITransitStopAreaCreator.CreateModel() {
            return CreateModel();
        }

        private static TModel CreateModel() {
            return new TModel();
        }

        ITransitStopAreaWrapper ITransitStopAreaCreator.CreateWrapper(ITransitStopArea transitStopArea) {
            return CreateWrapper(transitStopArea);
        }

        private static TWrapper CreateWrapper(ITransitStopArea transitStopArea) {
            var type = typeof(TWrapper);
            var instance = Activator.CreateInstance(type, transitStopArea);

            return (TWrapper)instance;
        }
    }
}