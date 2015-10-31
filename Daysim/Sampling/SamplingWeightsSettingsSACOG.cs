// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using Daysim.Framework.Core;
using Daysim.Framework.Exceptions;

namespace Daysim.Sampling {
	public class SamplingWeightsSettingsSACOG : ISamplingWeightsSettings {

		public SamplingWeightsSettingsSACOG()
		{
			SizeFactors = new[]
					{
						/*     EDU   food  GOV   IND   MED   OFC   RSC   RET   SVC EMPTOT  HOUSE  K8    UNI LUSE19 LU19Q OPENSP HighSchool*/
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -04D, -06D, -06D, -30D, -30D, -30D, -06D},  /* 0  7 :motorized WBTour or IntStop --work"             */
						new[] {-03D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -07D, -06D, -02D, -02D, -30D, -30D, -30D, -02D},  /* 1  8 :motorized WBTour or IntStop --school"           */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -04D, -30D, -30D, -30D, -03D},  /*	2	9 :motorized WBTour or IntStop --escort"           */
						new[] {-30D, -30D, -30D, -30D, -01D, -30D, -30D, -30D, -30D, -01D, -1.5D, -02D, -03D, -30D, -30D, -30D, -02D}, /*	3	10:motorized WBTour or IntStop --pers bus."        */
						new[] {-30D, -30D, -30D, -30D, -30D, -03D, -30D, -00D, -03D, -04D, -04D, -06D, -06D, -30D, -30D, -30D, -06D},  /*	4	11:motorized WBTour or IntStop --shop"             */
						new[] {-30D, -00D, -30D, -30D, -30D, -03D, -30D, -02D, -03D, -04D, -03D, -07D, -07D, -30D, -30D, -30D, -07D},  /*	5	12:motorized WBTour or IntStop --meal"             */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -03D, -30D, -30D, -01D, -03D},  /*	6	13:motorized WBTour or IntStop --social"           */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -1.5D, -03D, -03D, -03D, -01D, -30D, -03D, -03D}, /*	7	14:motorized WBTour or IntStop --rec"              */
						new[] {-30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -07D, -30D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	8	15:motorized WBTour or IntStop --medical"          */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	9	16:motorized HBTour --work"                        */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -00D},  /*	10	17:motorized HBTour --school"                      */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -04D, -30D, -30D, -30D, -03D},  /*	11	18:motorized HBTour --escort"                      */
						new[] {-30D, -30D, -30D, -30D, -01D, -30D, -30D, -30D, -30D, -01D, -1.5D, -02D, -03D, -30D, -30D, -30D, -02D}, /*	12	19:motorized HBTour --pers bus."                   */
						new[] {-30D, -30D, -30D, -30D, -30D, -03D, -30D, -00D, -03D, -04D, -30D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	13	20:motorized HBTour --shop"                        */
						new[] {-30D, -00D, -30D, -30D, -30D, -03D, -30D, -02D, -03D, -04D, -03D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	14	21:motorized HBTour --meal"                        */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -03D, -30D, -30D, -01D, -03D},  /*	15	22:motorized HBTour --social"                      */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -07D, -04D, -04D, -02D, -30D, -03D, -04D},  /*	16	23:motorized HBTour --rec"                         */
						new[] {-30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -07D, -30D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	17	24:motorized HBTour --medical"                     */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	18	25:walk --work"                                    */
						new[] {-03D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -07D, -04D, -02D, -02D, -30D, -30D, -30D, -02D},  /* 19 26:walk --school"                                  */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -04D, -30D, -30D, -30D, -03D},  /*	20	27:walk --escort"                                  */
						new[] {-30D, -30D, -30D, -30D, -01D, -30D, -30D, -30D, -30D, -01D, -1.5D, -02D, -03D, -30D, -30D, -30D, -02D}, /*	21	28:walk --pers bus."                               */
						new[] {-30D, -30D, -30D, -30D, -30D, -03D, -30D, -00D, -03D, -04D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	22	29:walk --shop"                                    */
						new[] {-30D, -00D, -30D, -30D, -30D, -03D, -30D, -02D, -03D, -04D, -03D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	23	30:walk --meal"                                    */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -03D, -30D, -30D, -01D, -03D},  /*	24	31:walk --social"                                  */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -04D, -04D, -04D, -02D, -30D, -03D, -04D},  /*	25	32:walk --rec"                                     */
						new[] {-30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -07D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	26	33:walk --medical"                                 */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	27	34:bike --work"                                    */
						new[] {-03D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -07D, -04D, -02D, -02D, -30D, -30D, -30D, -02D},  /* 28 35:bike --school"                                  */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -04D, -30D, -30D, -30D, -03D},  /*	29	36:bike --escort"                                  */
						new[] {-30D, -30D, -30D, -30D, -01D, -30D, -30D, -30D, -30D, -01D, -1.5D, -02D, -03D, -30D, -30D, -30D, -02D}, /*	30	37:bike --pers bus."                               */
						new[] {-30D, -30D, -30D, -30D, -30D, -03D, -30D, -00D, -03D, -04D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	31	38:bike --shop"                                    */
						new[] {-30D, -00D, -30D, -30D, -30D, -03D, -30D, -02D, -03D, -04D, -03D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	32	39:bike --meal"                                    */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -01D, -03D, -03D, -30D, -30D, -01D, -03D},  /*	33	40:bike --social"                                  */
						new[] {-30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -02D, -04D, -04D, -04D, -02D, -30D, -03D, -04D},  /*	34	41:bike --rec"                                     */
						new[] {-30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D, -07D, -04D, -30D, -30D, -30D, -30D, -30D, -30D},  /*	35	42:bike --medical"                                 */
						new[] {-02D, -30D, -30D, -30D, -30D, -02D, -30D, -30D, -02D, -02D, -01D, -30D, -00D, -30D, -30D, -30D, -03D}   /* 36   :ptypes 1-5 --usu school                         */
						//new[] {-03D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -07D, -30D, -02D, -02D, -30D, -30D, -30D, -02D},	10	17:motorized HBTour --school"                        
						//new[] {-04D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -30D, -00D, -30D, -30D, -30D, -30D}    36   :ptypes 1-5 --usu school                             
					};

			WeightFactors = new[] {
				/*  0     7:motorized WBTour or IntStop --work"          */new[] {1455D}, //{1455}, /*values on the right are temporary values to synchronize with delphi for testing only*/
				/*  1     8:motorized WBTour or IntStop --school"        */new[] {1111D}, //{1111},  
				/*  2     9:motorized WBTour or IntStop --escort"        */new[] {1216D}, //{1216},  
				/*  3    10:motorized WBTour or IntStop --pers bus."     */new[] {1150D}, //{1150},  
				/*  4    11:motorized WBTour or IntStop --shop"          */new[] {1077D}, //{1077},  
				/*  5    12:motorized WBTour or IntStop --meal"          */new[] {1096.5D}, //{1096.5},
				/*  6    13:motorized WBTour or IntStop --social"        */new[] {1225.5D}, //{1225.5},
				/*  7    14:motorized WBTour or IntStop --rec"           */new[] {1292D}, //{1292},  
				/*  8    15:motorized WBTour or IntStop --medical"       */new[] {1329D}, //{1329},  
				/*  9    16:motorized HBTour --work"                     */new[] {2877.5D}, //{1455},  
				/* 10    17:motorized HBTour --school"                   */new[] {1608D}, //{1111},  
				/* 11    18:motorized HBTour --escort"                   */new[] {1519D}, //{1216},  
				/* 12    19:motorized HBTour --pers bus."                */new[] {1642.5D}, //{1150},  
				/* 13    20:motorized HBTour --shop"                     */new[] {1391D}, //{1077},  
				/* 14    21:motorized HBTour --meal"                     */new[] {1492D}, //{1096.5},
				/* 15    22:motorized HBTour --social"                   */new[] {1571D}, //{1225.5},
				/* 16    23:motorized HBTour --rec"                      */new[] {1556D}, //{1292},  
				/* 17    24:motorized HBTour --medical"                  */new[] {1900D}, //{1329},  
				/* 18    25:walk --work"                                 */new[] {400D}, //{1455},
				/* 19    26:walk --school"                               */new[] {400D}, //{1111},
				/* 20    27:walk --escort"                               */new[] {400D}, //{1216},
				/* 21    28:walk --pers bus."                            */new[] {400D}, //{1150},
				/* 22    29:walk --shop"                                 */new[] {400D}, //{1077},
				/* 23    30:walk --meal"                                 */new[] {400D}, //{1096.5},
				/* 24    31:walk --social"                               */new[] {400D}, //{1225.5},
				/* 25    32:walk --rec"                                  */new[] {400D}, //{1292},
				/* 26    33:walk --medical"                              */new[] {400D}, //{1329},
				/* 27    34:bike --work"                                 */new[] {700D}, //{1455},
				/* 28    35:bike --school"                               */new[] {700D}, //{1111},
				/* 29    36:bike --escort"                               */new[] {700D}, //{1216},
				/* 30    37:bike --pers bus."                            */new[] {700D}, //{1150},
				/* 31    38:bike --shop"                                 */new[] {700D}, //{1077},
				/* 32    39:bike --meal"                                 */new[] {700D}, //{1096.5},
				/* 33    40:bike --social"                               */new[] {700D}, //{1225.5},
				/* 34    41:bike --rec"                                  */new[] {700D}, //{1292},
				/* 35    42:bike --medical"                              */new[] {700D}, //{1329}
				/* 36      :ptypes 1-5 usual school"                     */new[] {1608D} //   
			};
		}

		#region size factors

		public double[][] SizeFactors
		{
			get; private set;
		}

		#endregion

		#region weight factors

		public double[][] WeightFactors { get; private set; }

		#endregion

		public int GetTourDestinationSegment(int purpose, int priority, int mode, int personType)
		{
			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business) &&
			    priority == Global.Settings.TourPriorities.WorkBasedTour && mode >= Global.Settings.Modes.Sov)
			{
				return 0;
			}


			if (purpose == Global.Settings.Purposes.School && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 1;
			}

			if (purpose == Global.Settings.Purposes.Escort && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 2;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 3;
			}

			if (purpose == Global.Settings.Purposes.Shopping && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 4;
			}

			if (purpose == Global.Settings.Purposes.Meal && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 5;
			}

			if (purpose == Global.Settings.Purposes.Social && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 6;
			}

			if (purpose == Global.Settings.Purposes.Recreation && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 7;
			}

			if (purpose == Global.Settings.Purposes.Medical && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 8;
			}

			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business) && mode >= Global.Settings.Modes.Sov)
			{
				return 9;
			}

			if (purpose == Global.Settings.Purposes.School && mode >= Global.Settings.Modes.Sov &&
			    priority == Global.Settings.TourPriorities.UsualLocation && personType <= Global.Settings.PersonTypes.UniversityStudent)
			{
				return 36;
			}

			if (purpose == Global.Settings.Purposes.School && mode >= Global.Settings.Modes.Sov)
			{
				return 10;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode >= Global.Settings.Modes.Sov)
			{
				return 11;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode >= Global.Settings.Modes.Sov)
			{
				return 12;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode >= Global.Settings.Modes.Sov)
			{
				return 13;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode >= Global.Settings.Modes.Sov)
			{
				return 14;
			}

			if (purpose == Global.Settings.Purposes.Social && mode >= Global.Settings.Modes.Sov)
			{
				return 15;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode >= Global.Settings.Modes.Sov)
			{
				return 16;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode >= Global.Settings.Modes.Sov)
			{
				return 17;
			}

			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business) && mode == Global.Settings.Modes.Walk)
			{
				return 18;
			}

			if (purpose == Global.Settings.Purposes.School && mode == Global.Settings.Modes.Walk)
			{
				return 19;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode == Global.Settings.Modes.Walk)
			{
				return 20;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode == Global.Settings.Modes.Walk)
			{
				return 21;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode == Global.Settings.Modes.Walk)
			{
				return 22;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode == Global.Settings.Modes.Walk)
			{
				return 23;
			}

			if (purpose == Global.Settings.Purposes.Social && mode == Global.Settings.Modes.Walk)
			{
				return 24;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode == Global.Settings.Modes.Walk)
			{
				return 25;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode == Global.Settings.Modes.Walk)
			{
				return 26;
			}

			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business) && mode == Global.Settings.Modes.Bike)
			{
				return 27;
			}

			if (purpose == Global.Settings.Purposes.School && mode == Global.Settings.Modes.Bike)
			{
				return 28;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode == Global.Settings.Modes.Bike)
			{
				return 29;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode == Global.Settings.Modes.Bike)
			{
				return 30;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode == Global.Settings.Modes.Bike)
			{
				return 31;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode == Global.Settings.Modes.Bike)
			{
				return 32;
			}

			if (purpose == Global.Settings.Purposes.Social && mode == Global.Settings.Modes.Bike)
			{
				return 33;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode == Global.Settings.Modes.Bike)
			{
				return 34;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode == Global.Settings.Modes.Bike)
			{
				return 35;
			}

			if (purpose == Global.Settings.Purposes.Business && priority == Global.Settings.TourPriorities.WorkBasedTour &&
			    mode >= Global.Settings.Modes.Sov)
			{
				return 0;
			}

			throw new SegmentRemainsUnassignedException();
		}

		public int GetIntermediateStopSegment(int purpose, int mode) {
			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business)  && mode >= Global.Settings.Modes.Sov) {
				return 0;
			}

			if (purpose == Global.Settings.Purposes.School && mode >= Global.Settings.Modes.Sov) {
				return 1;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode >= Global.Settings.Modes.Sov) {
				return 2;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode >= Global.Settings.Modes.Sov) {
				return 3;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode >= Global.Settings.Modes.Sov) {
				return 4;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode >= Global.Settings.Modes.Sov) {
				return 5;
			}

			if (purpose == Global.Settings.Purposes.Social && mode >= Global.Settings.Modes.Sov) {
				return 6;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode >= Global.Settings.Modes.Sov) {
				return 7;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode >= Global.Settings.Modes.Sov) {
				return 8;
			}

			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business)  && mode == Global.Settings.Modes.Walk) {
				return 18;
			}

			if (purpose == Global.Settings.Purposes.School && mode == Global.Settings.Modes.Walk) {
				return 19;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode == Global.Settings.Modes.Walk) {
				return 20;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode == Global.Settings.Modes.Walk) {
				return 21;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode == Global.Settings.Modes.Walk) {
				return 22;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode == Global.Settings.Modes.Walk) {
				return 23;
			}

			if (purpose == Global.Settings.Purposes.Social && mode == Global.Settings.Modes.Walk) {
				return 24;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode == Global.Settings.Modes.Walk) {
				return 25;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode == Global.Settings.Modes.Walk) {
				return 26;
			}

			if ((purpose == Global.Settings.Purposes.Work || purpose == Global.Settings.Purposes.Business)  && mode == Global.Settings.Modes.Bike) {
				return 27;
			}

			if (purpose == Global.Settings.Purposes.School && mode == Global.Settings.Modes.Bike) {
				return 28;
			}

			if (purpose == Global.Settings.Purposes.Escort && mode == Global.Settings.Modes.Bike) {
				return 29;
			}

			if (purpose == Global.Settings.Purposes.PersonalBusiness && mode == Global.Settings.Modes.Bike) {
				return 30;
			}

			if (purpose == Global.Settings.Purposes.Shopping && mode == Global.Settings.Modes.Bike) {
				return 31;
			}

			if (purpose == Global.Settings.Purposes.Meal && mode == Global.Settings.Modes.Bike) {
				return 32;
			}

			if (purpose == Global.Settings.Purposes.Social && mode == Global.Settings.Modes.Bike) {
				return 33;
			}

			if (purpose == Global.Settings.Purposes.Recreation && mode == Global.Settings.Modes.Bike) {
				return 34;
			}

			if (purpose == Global.Settings.Purposes.Medical && mode == Global.Settings.Modes.Bike) {
				return 35;
			}

			throw new SegmentRemainsUnassignedException();
		}
	}
}