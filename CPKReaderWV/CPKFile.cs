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
        public struct HeaderStruct 
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
        public FileInfo[] HashTable;
        public helper help;
        public int Reverse = -1;
        public uint nCurrentReadOffset = 64;
        //version 6
        //nCurrentReadOffset = 64; 
        //version 7
        //nCurrentReadOffset = 72; 


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

        public void IsByteReverse()
        {
            FileStream o = OpenCPKFile(CPKFilePath);
            BinaryReader reader = new BinaryReader(o);
            uint Magic = reader.ReadUInt32();
            CloseCPKFile(o);
            if(Magic.ToString("X8").Equals("A1B2C3D4"))
                Reverse = 1;
            if (Magic.ToString("X8").Equals("D4C3B2A1"))
                Reverse = 0;
            else
                Reverse = -1;
        }

        public void GetPackageVersion()
        {
            FileStream o = OpenCPKFile(CPKFilePath);
            Header.MagicNumber = help.RUInt32(o, Reverse); //should skip to next, not read
            Header.PackageVersion = help.RUInt32(o, Reverse);
            CloseCPKFile(o);
        }

        public HeaderStruct ReadHeader(Stream s)
        {
            FileStream o = OpenCPKFile(CPKFilePath);
            Header.MagicNumber = help.RUInt32(o, Reverse);
            Header.PackageVersion = help.RUInt32(o, Reverse);
            Header.DecompressedFileSize = help.RUInt64(s, Reverse);
            Header.Flags = help.RUInt32(s,Reverse);
            Header.FileCount = help.RUInt32(s, Reverse);
            Header.LocationCount = help.RUInt32(s, Reverse);
            Header.HeaderSector = help.RUInt32(s, Reverse);
            Header.FileSizeBitCount = help.RUInt32(s, Reverse);
            Header.FileLocationCountBitCount = help.RUInt32(s, Reverse);
            Header.FileLocationIndexBitCount = help.RUInt32(s, Reverse);
            Header.LocationBitCount = help.RUInt32(s, Reverse);
            Header.CompSectorToDecomOffsetBitCount = help.RUInt32(s, Reverse);
            Header.DecompSectorToCompSectorBitCount = help.RUInt32(s, Reverse);
            Header.CRC = help.RUInt32(s, Reverse);
            if (Header.PackageVersion == 6)
            {
                Header.unknown = help.ReadU32(s); //always 0
                Header.nReadSectorSize = 0x10000;
                Header.nCompSectorSize = 0x4000;
            }
            if (Header.PackageVersion == 7)
            {
                Header.nReadSectorSize = help.RInt32(o, Reverse);
                Header.nCompSectorSize = help.RInt32(o, Reverse);
            }
            CloseCPKFile(o);
            return Header;
        }

        public byte[] ReadHashTable(Stream s)
        {
            uint size = 64;
            size += Header.FileSizeBitCount;
            size += Header.FileLocationCountBitCount;
            size += Header.FileLocationIndexBitCount;
            size *= Header.FileCount;
            size += 7;
            size = size >> 3;
            byte[] BFileInfo = new byte[size];
            s.Read(BFileInfo, 0, (int)size);
            return BFileInfo;
        }

        public FileInfo[] GetHashTable()
        {
            FileInfo[] result = new FileInfo[Header.FileCount];
            FileStream o = OpenCPKFile(CPKFilePath);
            o.Position = 64;
            byte[] Htable = ReadHashTable(o);
            uint position = 0;
            for(int i=0;i< Header.FileCount;i++ )
            {
                result[i].dwHash = (uint)help.ReadBits(Htable, position, 64);
                position += 64;
                result[i].nSize =  (uint)help.ReadBits(Htable, position, Header.FileSizeBitCount);
                position += Header.FileSizeBitCount;
                result[i].nLocationCount = (uint)help.ReadBits(Htable, position, Header.FileLocationCountBitCount);
                position += Header.FileLocationCountBitCount;
                result[i].nLocationIndex = (uint)help.ReadBits(Htable, position, Header.FileLocationIndexBitCount);
                position += Header.FileLocationIndexBitCount;
                if(Reverse==1)
                {
                    result[i].dwHash = help.ReverseUInt64(result[i].dwHash);
                    result[i].nSize = help.ReverseUInt32(result[i].nSize);
                    result[i].nLocationCount = help.ReverseUInt32(result[i].nLocationCount);
                    result[i].nLocationIndex = help.ReverseUInt32(result[i].nLocationIndex);
                }
            }
            return result;
        }
    }
}
