// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


namespace DaySim.ShadowPricing {
  public sealed class ShadowPriceZone {
    /// <summary>
    /// EMPTOTZW
    /// </summary>
    public double ExternalEmploymentTotal { get; set; }

    /// <summary>
    /// EMPPRD_Z
    /// </summary>
    public double EmploymentPrediction { get; set; }

    /// <summary>
    /// stuk12zw
    /// </summary>
    public double ExternalStudentsK12 { get; set; }

    /// <summary>
    /// K12PRD_Z
    /// </summary>
    public double StudentsK12Prediction { get; set; }

    /// <summary>
    /// stuunizw
    /// </summary>
    public double ExternalUniversityStudents { get; set; }

    /// <summary>
    /// UNIPRD_Z
    /// </summary>
    public double StudentsUniversityPrediction { get; set; }
  }
}