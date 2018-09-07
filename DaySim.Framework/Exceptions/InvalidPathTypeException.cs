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
  public class InvalidPathTypeException : Exception {
    public InvalidPathTypeException() : this("The value used for path type is invalid. Please adjust the roster accordingly.") { }

    public InvalidPathTypeException(string message) : base(message) { }

    public InvalidPathTypeException(string message, Exception innerException) : base(message, innerException) { }

    protected InvalidPathTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}