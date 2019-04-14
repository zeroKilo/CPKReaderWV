using System;
using System.Collections.Generic;
using System.IO;
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
            public int nReadSectorSize;
            public int nCompSectorSize;
        }
        public struct FileInfo //sizeof = 0x18 , align = 0x8 => HashTable info
        {
            public ulong dwHash;
            public uint nSize;
            public uint nLocationCount;
            public uint nLocationIndex;
            public uint nLocationIndexOverride;
        }

        public string CPKFilePath;
        public HeaderStruct Header;
        public FileInfo HashTable;


        public FileStream OpenCPKFile(string Path)
        {
            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, 0);
            return fs;
        }

        public void CloseCPKFile(FileStream fs)
        {
           fs.Close();

        }

        public int ByteReverse()
        {
            FileStream o = OpenCPKFile(CPKFilePath);
            BinaryReader reader = new BinaryReader(o);
            uint Magic = reader.ReadUInt32();
            CloseCPKFile(o);
            if(Magic.ToString("X8").Equals("A1B2C3D4"))
                return 1;
            if (Magic.ToString("X8").Equals("D4C3B2A1"))
                return 0;
            else
                return -1;
        }

        public uint GetPackageVersion()
        {
            int r = ByteReverse();
            uint version = 0;
            FileStream o = OpenCPKFile(CPKFilePath);
            BinaryReader reader = new BinaryReader(o);
            reader.ReadUInt32();
            if(r==0)
                version = reader.ReadUInt32();
                byte[] bytes = BitConverter.GetBytes(version);
                Array.Reverse(bytes);
                version = BitConverter.ToUInt32(bytes, 0);
            if (r==1)
                version = reader.ReadUInt32();
            CloseCPKFile(o);
            return version;
        }

        public FileInfo[] GetHashTable()
        {
            FileInfo[] result = new FileInfo[Header.FileCount];
            // add values
            return result;
        }
    }
}
