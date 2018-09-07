using System;
using System.Runtime.Serialization;

namespace DaySim.Framework.Exceptions {
  [Serializable]
  public class MissingInstallationException : Exception {
    public MissingInstallationException() : this("An installation is missing. Please ensure that appropriate installations are installed.") { }

    public MissingInstallationException(string message) : base(message) { }

    public MissingInstallationException(string message, Exception innerException) : base(message, innerException) { }

    protected MissingInstallationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
