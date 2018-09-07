// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System.Runtime.InteropServices;

namespace DaySim.Framework.Roster {
  [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
  public struct SkimValue {
    /// <summary>
    /// Gets or sets the value of the skim variable. The variable may be blended based on some business rules.
    /// </summary>
    /// <value>
    /// The variable.
    /// </value>
    public double Variable { get; set; }

    /// <summary>
    /// Gets or sets the blended variable.
    /// </summary>
    /// <value>
    /// The blended variable.
    /// </value>
    public double BlendVariable { get; set; }
  }
}