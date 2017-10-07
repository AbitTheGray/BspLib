using System;
namespace BspLib.Bsp.GoldSource
{
    /// <summary>
    /// struct
    /// {
    ///    int32_t nOffset; // File offset to data
    ///    int32_t nLength; // Length of data
    /// }
    /// </summary>
    public struct Lump
    {
        public Lump(int offset, int size)
        {
            this.Offset = offset;
            this.Size = size;
        }

        /// <summary>
        /// File offset to data from begining of the file
        /// </summary>
        public int Offset;
        /// <summary>
        /// Size of the lump in bytes.
        /// </summary>
        public int Size;
    }
}