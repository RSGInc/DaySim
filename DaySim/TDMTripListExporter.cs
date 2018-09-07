// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.IO;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Wrappers;

namespace DaySim {
  public sealed class TDMTripListExporter : IDisposable {
    private int _current;
    private readonly char _delimiter;
    private readonly StreamWriter _writer;

    public TDMTripListExporter(string outputPath, char delimiter) {
      FileInfo outputFile = new FileInfo(outputPath);

      _writer = new StreamWriter(outputFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read)) { AutoFlush = false };
      _delimiter = delimiter;

      WriteHeader();
    }

    private void WriteHeader() {
      if (Global.Configuration.UseCustomTDMTripListFormat) {
        //add custom code to write header here
        _writer.Write("HHOLD");
        _writer.Write(_delimiter);

        _writer.Write("PERSON");
        _writer.Write(_delimiter);

        _writer.Write("TOUR");
        _writer.Write(_delimiter);

        _writer.Write("TRIP");
        _writer.Write(_delimiter);

        _writer.Write("START");
        _writer.Write(_delimiter);

        _writer.Write("END");
        _writer.Write(_delimiter);

        _writer.Write("ACTEND");
        _writer.Write(_delimiter);

        _writer.Write("ORIGIN");
        _writer.Write(_delimiter);

        _writer.Write("DESTINATION");
        _writer.Write(_delimiter);

        _writer.Write("PURPOSE");
        _writer.Write(_delimiter);

        _writer.Write("MODE");
        _writer.Write(_delimiter);

        _writer.Write("DORP");
        _writer.Write(_delimiter);

        _writer.Write("PARKNODE");
        _writer.Write(_delimiter);

        _writer.Write("PARKTYPE");
        _writer.Write(_delimiter);

        _writer.Write("PARKCOST");
        _writer.Write(_delimiter);

        _writer.Write("PARKWALK");
        _writer.Write(_delimiter);

        _writer.Write("COSTCOEF");
        _writer.Write(_delimiter);

        _writer.Write("TIMECOEF");
        _writer.Write(_delimiter);

        _writer.Write("FREEPARK");
        _writer.Write(_delimiter);

        _writer.WriteLine("EXPFAC");
      } else if (Global.Configuration.UseTransimsTDMTripListFormat) {
        _writer.Write("HHOLD");
        _writer.Write(_delimiter);

        _writer.Write("PERSON");
        _writer.Write(_delimiter);

        _writer.Write("TOUR");
        _writer.Write(_delimiter);

        _writer.Write("TRIP");
        _writer.Write(_delimiter);

        _writer.Write("START");
        _writer.Write(_delimiter);

        _writer.Write("END");
        _writer.Write(_delimiter);

        _writer.Write("DURATION");
        _writer.Write(_delimiter);

        _writer.Write("ORIGIN");
        _writer.Write(_delimiter);

        _writer.Write("DESTINATION");
        _writer.Write(_delimiter);

        _writer.Write("PURPOSE");
        _writer.Write(_delimiter);

        _writer.Write("MODE");
        _writer.Write(_delimiter);

        _writer.Write("CONSTRAINT");
        _writer.Write(_delimiter);

        _writer.Write("PRIORITY");
        _writer.Write(_delimiter);

        _writer.Write("VEHICLE");
        _writer.Write(_delimiter);

        _writer.Write("PASSENGERS");
        _writer.Write(_delimiter);

        _writer.WriteLine("TYPE");
      } else {
        _writer.Write(";id");
        _writer.Write(_delimiter);

        _writer.Write("otaz");
        _writer.Write(_delimiter);

        _writer.Write("dtaz");
        _writer.Write(_delimiter);

        _writer.Write("mode");
        _writer.Write(_delimiter);

        _writer.Write("deptm");
        _writer.Write(_delimiter);

        _writer.Write("arrtm");
        _writer.Write(_delimiter);

        _writer.Write("duration");
        _writer.Write(_delimiter);

        _writer.Write("dpurp");
        _writer.Write(_delimiter);

        _writer.WriteLine("vot");
      }
    }

    public void Export(ITripWrapper trip) {
      if (trip == null) {
        throw new ArgumentNullException("trip");
      }
      if (Global.Configuration.UseCustomTDMTripListFormat) {
        //add custom code to write data items corresponding to header here
        _writer.Write(trip.Household.Id);
        _writer.Write(_delimiter);

        _writer.Write(trip.Person.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(trip.Tour.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(trip.IsHalfTourFromOrigin ? trip.Sequence : trip.Tour.HalfTour1Trips + trip.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(trip.DepartureTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.ArrivalTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.ActivityEndTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.OriginParcel.Id);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParcel.Id);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationPurpose);
        _writer.Write(_delimiter);

        _writer.Write(trip.Mode);
        _writer.Write(_delimiter);

        _writer.Write(trip.DriverType);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParkingNodeId);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParkingType);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParkingCost);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParkingWalkTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.Tour.CostCoefficient);
        _writer.Write(_delimiter);

        _writer.Write(trip.Tour.TimeCoefficient);
        _writer.Write(_delimiter);

        _writer.Write(trip.Person.PaidParkingAtWorkplace);
        _writer.Write(_delimiter);

        _writer.WriteLine(trip.ExpansionFactor);
      } else if (Global.Configuration.UseTransimsTDMTripListFormat) {
        _writer.Write(trip.Household.Id);
        _writer.Write(_delimiter);

        _writer.Write(trip.Person.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(trip.Tour.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(trip.IsHalfTourFromOrigin ? trip.Sequence : trip.Tour.HalfTour1Trips + trip.Sequence);
        _writer.Write(_delimiter);

        int departureTime24Hour = trip.DepartureTime.ToMinutesAfterMidnight();
        if (departureTime24Hour < 180) { departureTime24Hour += 1440; } // range becomes 180-1619 instead of 0-1439 

        _writer.Write(departureTime24Hour);
        _writer.Write(_delimiter);

        int arrivalTime24Hour = trip.ArrivalTime.ToMinutesAfterMidnight();
        if (arrivalTime24Hour < 180) { arrivalTime24Hour += 1440; } // range becomes 180-1619 instead of 0-1439

        _writer.Write(arrivalTime24Hour);
        _writer.Write(_delimiter);

        _writer.Write(trip.ActivityEndTime - trip.ArrivalTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.OriginParcel.LandUseCode);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationParcel.LandUseCode);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationPurpose);
        _writer.Write(_delimiter);

        int tsMode = trip.Mode == Global.Settings.Modes.Walk
                                 ? 1
                                 : trip.Mode == Global.Settings.Modes.Bike
                                       ? 2
                                       : trip.Mode >= Global.Settings.Modes.Sov && trip.Mode <= Global.Settings.Modes.Hov3 && trip.DriverType == Global.Settings.DriverTypes.Driver
                                             ? 3
                                             : trip.Mode >= Global.Settings.Modes.Sov && trip.Mode <= Global.Settings.Modes.Hov3 && trip.DriverType != Global.Settings.DriverTypes.Driver
                                                   ? 4
                                                   : trip.Mode == Global.Settings.Modes.Transit
                                                         ? 5
                                                         : trip.Mode == Global.Settings.Modes.SchoolBus
                                                               ? 11
                                                               : 6;

        int tsPass = trip.Mode == Global.Settings.Modes.Hov2
                                 ? 1
                                 : trip.Mode == Global.Settings.Modes.Hov3
                                       ? 2
                                       : 0;

        _writer.Write(tsMode);
        _writer.Write(_delimiter);

        const int zero = 0;
        _writer.Write(zero);
        _writer.Write(_delimiter);
        _writer.Write(zero);
        _writer.Write(_delimiter);

        _writer.Write(trip.Household.Id * 100 + trip.Person.Sequence);
        _writer.Write(_delimiter);

        _writer.Write(tsPass);
        _writer.Write(_delimiter);

        double tripVOT = trip.ValueOfTime;
        int votType = (trip.Mode < Global.Settings.Modes.Sov || trip.Mode > Global.Settings.Modes.Hov3)
                                  ? 0
                                  : ((tripVOT < 30
                                          ? 1 + (int)tripVOT
                                          : tripVOT < 100
                                                ? 31 + (int)((tripVOT - 30) / 5)
                                                : 45)
                                     + (trip.PathType == Global.Settings.PathTypes.NoTolls ? 0 : 50));

        _writer.WriteLine(votType);
      } else {
        _writer.Write(trip.Id);
        _writer.Write(_delimiter);

        _writer.Write(trip.OriginZoneKey);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationZoneKey);
        _writer.Write(_delimiter);

        _writer.Write(trip.Mode);
        _writer.Write(_delimiter);

        int departureTime24Hour = trip.DepartureTime.ToMinutesAfterMidnight();

        _writer.Write(departureTime24Hour / 60D);
        _writer.Write(_delimiter);

        int arrivalTime24Hour = trip.ArrivalTime.ToMinutesAfterMidnight();

        _writer.Write(arrivalTime24Hour / 60D);
        _writer.Write(_delimiter);

        _writer.Write(trip.ArrivalTime - trip.DepartureTime);
        _writer.Write(_delimiter);

        _writer.Write(trip.DestinationPurpose);
        _writer.Write(_delimiter);

        int valueOfTime =
                    trip.Household.Income < (trip.Tour.DestinationPurpose == Global.Settings.Purposes.Work ? 20000 : 40000)
                        ? Global.Settings.ValueOfTimes.Low
                        : trip.Household.Income < (trip.Tour.DestinationPurpose == Global.Settings.Purposes.Work ? 45000 : 110000)
                              ? Global.Settings.ValueOfTimes.Medium
                              : Global.Settings.ValueOfTimes.High;

        _writer.WriteLine(valueOfTime);
      }

      _current++;

      if (_current % 1000 == 0) {
        _writer.Flush();
      }
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing) {
      if (disposing) {
        _writer.Dispose();
      }
    }
  }
}