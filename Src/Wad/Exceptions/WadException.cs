using System;

namespace BspLib.Wad.Exceptions
{
    public class WadException : Exception
    {
        public WadException(string message) : base(message)
        {
        }
    }
}