// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.


using System;
using System.Runtime.Serialization;

namespace DaySim.Framework.Exceptions {
  [Serializable]
  public class InvalidParkAndRideZoneIdException : Exception {
    public InvalidParkAndRideZoneIdException() : this("A park-and-ride zone has an ID of zero. Zero is a special case for skims meaning none or empty.") { }

    public InvalidParkAndRideZoneIdException(string message) : base(message) { }

    public InvalidParkAndRideZoneIdException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidParkAndRideZoneIdException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}