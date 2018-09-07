// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using DaySim.Framework.ShadowPricing;

namespace DaySim.ShadowPricing {
  public sealed class ShadowPriceParcel : IShadowPriceParcel {
    /// <summary>
    /// Gets or sets the parcel id.
    /// </summary>
    /// <value>
    /// The parcel id.
    /// </value>
    public int ParcelId { get; set; }

    /// <summary>
    /// Gets or sets the employment difference.
    /// </summary>
    /// <value>
    /// The employment difference.
    /// </value>
    public double EmploymentDifference { get; set; }

    /// <summary>
    /// Gets or sets the shadow price employment.
    /// </summary>
    /// <value>
    /// The shadow price employment.
    /// </value>
    public double ShadowPriceForEmployment { get; set; }

    /// <summary>
    /// Gets or sets the students K12 difference.
    /// </summary>
    /// <value>
    /// The students K12 difference.
    /// </value>
    public double StudentsK12Difference { get; set; }

    /// <summary>
    /// Gets or sets the shadow price students K12.
    /// </summary>
    /// <value>
    /// The shadow price students K12.
    /// </value>
    public double ShadowPriceForStudentsK12 { get; set; }

    /// <summary>
    /// Gets or sets the students university difference.
    /// </summary>
    /// <value>
    /// The students university difference.
    /// </value>
    public double StudentsUniversityDifference { get; set; }

    /// <summary>
    /// Gets or sets the shadow price students university.
    /// </summary>
    /// <value>
    /// The shadow price students university.
    /// </value>
    public double ShadowPriceForStudentsUniversity { get; set; }
  }
}