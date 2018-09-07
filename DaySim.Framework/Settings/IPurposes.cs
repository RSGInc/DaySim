// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

namespace DaySim.Framework.Settings {
  public interface IPurposes {
    int TotalPurposes { get; }

    int NoneOrHome { get; }

    int Work { get; }

    int HomeBasedComposite { get; }

    int School { get; }

    int WorkBased { get; }

    int Escort { get; }

    int PersonalBusiness { get; }

    int Shopping { get; }

    int Meal { get; }

    int Social { get; }

    int Recreation { get; }

    int Medical { get; }

    int ChangeMode { get; }

    int Business { get; }
  }
}