using System;

namespace BspLib.Bsp.Exceptions
{
    public class BspVersionNotSupportedException : BspException
    {
        public BspVersionNotSupportedException(uint version) : this(version, "Unknown")
        {
        }

        public BspVersionNotSupportedException(uint version, string reason) : base(string.Format("BSP version {0} not supported: {1}", version, reason))
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