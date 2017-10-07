using System;

namespace BspLib.Bsp.Exceptions
{
    public class BspException : Exception
    {
        public BspException(string message) : base(message)
        {
        }
    }
}