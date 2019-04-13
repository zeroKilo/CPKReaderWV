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
        public ulong Hash64(string name)
        {
            char[] v1 = name.ToCharArray();
            UInt64 result = 0xCBF29CE484222325L;
            int strlen = name.Length;
            if (strlen > 0)
            {
                for (int i = 0; i < strlen; i++)
                    result = 0x100000001B3L * (result ^ v1[i]);
            }
            return result;
        }

        public ulong Hash64More(string data, UInt64 previousHash)
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

        public ulong GetSubFileHash(UInt64 dwParentHash, string szFilename)
        {
            return Hash64More(szFilename.ToLower(), dwParentHash);
        }

        public ulong GetFileHash(string szFilename)
        {
            return Hash64(szFilename.ToLower());
        }
    }
}
