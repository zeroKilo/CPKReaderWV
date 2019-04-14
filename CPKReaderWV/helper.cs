using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPKReaderWV
{
    public enum CPKArchive
    {
        CPK_FLAG_VALID = 0,
        CPK_FLAG_COMPRESSED = 1,
        CPK_FLAG_FROM_MEMORY = 2,
        CPK_FLAG_COMPACT_SECTORS = 3,
        CPK_FLAG_PENDING_WRITE = 4,
        CPK_FLAG_READ_AHEAD_VALID = 5,
        CPK_FLAG_CLOSING = 6,
        CPK_FLAG_BASE_ARCHIVE_LAST_FLAG = 5,
        CPK_FLAG_RUNTIME_FLAGS = 0x35,
    }

    public enum CPKArchiveSizes
    {
        CPK_COMP_SECTOR_SIZE = 0x4000,
        CPK_COMP_READ_CHUNK_SIZE = 0x4000,
        CPK_READ_SECTOR_SIZE = 0x10000,
        CPK_MAX_DECOMP_BUFFER_SIZE = 0x10000,
    }

    public enum CPKArchiveTypes
    {
        CPK_ARCHIVE_TYPE_STANDARD = 1,
        CPK_ARCHIVE_TYPE_CACHE = 2,
    }
    public enum Results
    {
        IORESULTS_SUCCESS = 0,
        IORESULTS_CANCELED = 1,
        IORESULTS_FILE_NOT_FOUND = 2,
        IORESULTS_FILE_IO_ERROR = 3,
        IORESULTS_WRONG_VERSION = 4,
        IORESULTS_INVALID_HEADER = 5,
        IORESULTS_COMPRESSION_ERROR = 6,
        IORESULTS_CRC_VALIDATION_FAILED = 7,
        IORESULTS_NOT_ENOUGHT_SPACE = 8,
        IORESULTS_FAILED = 9,
    }
    public class helper
    {
        public uint GetHighestBit(uint u)
        {
            uint result = 0;
            while (u != 0)
            {
                u = (u >> 1);
                result++;
            }
            return result;
        }

        public string ReadString(Stream s)
        {
            string result = "";
            char b;
            while ((b = (char)s.ReadByte()) != (char)0)
                result += b;
            return result;
        }

        public ushort ReadU16(Stream s)
        {
            ushort res = 0;
            res |= (byte)s.ReadByte();
            res = (ushort)((res << 8) | (byte)s.ReadByte());
            return res;
        }
        public uint ReadU32(Stream s)
        {
            uint res = 0;
            res |= (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            return res;
        }

        public ulong ReadU64(Stream s)
        {
            ulong res = 0;
            res |= (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            res = (res << 8) | (byte)s.ReadByte();
            return res;
        }

        public Int16 ReadBinaryInt16(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadInt16();
        }
        public UInt16 ReadBinaryUInt16(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadUInt16();
        }

        public Int32 ReadBinaryInt32(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadInt32();
        }

        public UInt32 ReadBinaryUInt32(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadUInt32();
        }

        public Int64 ReadBinaryInt64(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadInt64();
        }

        public UInt64 ReadBinaryUInt64(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            return reader.ReadUInt64();
        }

        public ulong ReadBits(byte[] buff, uint bitPos, uint bitCount)
        {
            ulong result = 0;
            for (uint i = 0; i < bitCount; i++)
            {
                uint pos = bitPos + i;
                uint bytePos = pos / 8;
                uint byteBit = 7 - pos % 8;
                result = result << 1;
                if ((buff[bytePos] & (1 << (int)byteBit)) != 0)
                    result |= 1;
            }
            return result;
        }
        public string ReverseString(string source)
        {
            char[] dest = source.ToArray();
            string result = "";
            for (int i = dest.Length -1; i >= 0; i--)
                result += dest[i];
            return result;
        }

        public Int16 ReverseInt16(Int16 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToInt16(bytes, 0);
            return x;
        }

        public UInt16 ReverseUInt16(UInt16 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToUInt16(bytes, 0);
            return x;
        }

        public Int32 ReverseInt32(Int32 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToInt32(bytes, 0);
            return x;
        }

        public UInt32 ReverseUInt32(UInt32 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToUInt32(bytes, 0);
            return x;
        }

        public Int64 ReverseInt64(Int64 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToInt64(bytes, 0);
            return x;
        }

        public UInt64 ReverseUInt64(UInt64 x)
        {
            byte[] bytes = BitConverter.GetBytes(x);
            Array.Reverse(bytes);
            x = BitConverter.ToUInt64(bytes, 0);
            return x;
        }

        public Int16 RInt16(Stream s, int r)
        {
            if (r == 0)
                return ReverseInt16(ReadBinaryInt16(s));
            else
                return ReadBinaryInt16(s);
        }

        public UInt16 RUInt16(Stream s, int r)
        {
            if (r == 0)
                return ReverseUInt16(ReadBinaryUInt16(s));
            else
                return ReadBinaryUInt16(s);
        }

        public Int32 RInt32(Stream s, int r)
        {
            if (r == 0)
                return ReverseInt32(ReadBinaryInt32(s));
            else
                return ReadBinaryInt32(s);
        }

        public UInt32 RUInt32(Stream s, int r)
        {
            if (r == 0)
                return ReverseUInt32(ReadBinaryUInt32(s));
            else
                return ReadBinaryUInt32(s);
        }

        public Int64 RInt64(Stream s, int r)
        {
            if (r == 0)
                return ReverseInt64(ReadBinaryInt64(s));
            else
                return ReadBinaryInt64(s);
        }

        public UInt64 RUInt64(Stream s, int r)
        {
            if (r == 0)
                return ReverseUInt64(ReadBinaryUInt64(s));
            else
                return ReadBinaryUInt64(s);
        }

        public string RString(Stream s, int r)
        {
            if (r == 0)
                return ReverseString(ReadString(s));
            else
                return ReadString(s);
        }

        public ulong Hash64(string name)
        {
            char[] v1 = name.ToCharArray();
            ulong result = 0xCBF29CE484222325L;
            int strlen = name.Length;
            if (strlen > 0)
            {
                for (int i = 0; i < strlen; i++)
                    result = 0x100000001B3L * (result ^ v1[i]);
            }
            return result;
        }

        public ulong Hash64More(string data, ulong previousHash)
        {
            char[] v1 = data.ToCharArray();
            int strlen = data.Length;
            if (strlen > 0)
            {
                for (int i = 0; i < strlen; i++)
                    previousHash = 0x100000001B3L * (v1[i] ^ previousHash);
            }
            return previousHash;
        }

        public ulong GetSubFileHash(ulong dwParentHash, string szFilename)
        {
            return Hash64More(szFilename.ToLower(), dwParentHash);
        }

        public ulong GetFileHash(string szFilename)
        {
            return Hash64(szFilename.ToLower());
        }

        public bool FindArchiveContaining(string szFilename, CPKFile cpk)
        {
            CPKFile.FileInfo[] info = cpk.GetHashTable();
            ulong hash = GetFileHash(szFilename); //or GetSubFileHash
            return SearchHashInArchive(hash, info);
        }

        public bool SearchHashInArchive(ulong dwHash, CPKFile.FileInfo[] ptFileInfo)
        {
            bool result = false;
            for(int i=0;i< ptFileInfo.Length; i++)
            {
                if (ptFileInfo[i].dwHash == dwHash)
                {
                    result = true;
                    break;
                }
            }
            return result; 
        }
                
    }
}
