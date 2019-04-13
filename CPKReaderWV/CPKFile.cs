using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPKReaderWV
{
    public class CPKFile
    {
        //version 6
        //nCurrentReadOffset = 64; // (Int64)
        //nReadSectorSize = 0x10000;
        //nCompSectorSize = 0x4000;
        //version 7
        //nReadSectorSize_var = read from HeaderStruct;
        //nCompSectorSize = read from HeaderStruct;
        //nCurrentReadOffset = 72; // (Int64)
        public struct HeaderStruct // using CPK_VERSION = 6,
        {
            public uint MagicNumber; //always CPK_MAGIC_NUMBER = A1B2C3D4 
            public uint PackageVersion;
            public ulong DecompressedFileSize;
            public uint Flags;
            public uint FileCount;
            public uint LocationCount;
            public uint HeaderSector;
            public uint FileSizeBitCount;
            public uint FileLocationCountBitCount;
            public uint FileLocationIndexBitCount;
            public uint LocationBitCount;
            public uint CompSectorToDecomOffsetBitCount;
            public uint DecompSectorToCompSectorBitCount;
            public uint CRC;
            public uint unknown;
            //public Int32 nReadSectorSize;
            //public Int32 nCompSectorSize;
        }
        public struct FileInfo //sizeof = 0x18 , align = 0x8 => HashTable info
        {
            public ulong dwHash;
            public uint nSize;
            public uint nLocationCount;
            public uint nLocationIndex;
            public uint nLocationIndexOverride;
        }


}
}
