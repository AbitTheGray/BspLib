using System;

namespace BspLib.Wad.Exceptions
{
    public class WadVersionNotSupportedException : WadException
    {
        public WadVersionNotSupportedException(uint version) : this(version, "Unknown")
        {
        }

        public WadVersionNotSupportedException(uint version, string reason) : base(string.Format("Wad version {0} not supported: {1}", version, reason))
        {
            this.Version = version;
            this.Reason = reason;
        }

        public uint Version
        {
            get;
        }

        public string Reason
        {
            get;
        }
    }
}