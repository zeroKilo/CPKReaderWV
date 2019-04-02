using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPKReaderWV
{
    public class CPKFile
    {
        public struct HeaderStruct
        {
            public uint MagicNumber;
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
        }
        public uint fileSize;
        public HeaderStruct header;
        public byte[] block1;
        public byte[] block2;
        public byte[] block3;
        public byte[] block4;
        public uint[] block5;
        public string[] fileNames;
        public Dictionary<uint, uint> fileOffsets;
        public string cpkpath;

        public CPKFile(string path)
        {
            cpkpath = path;
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.End);
            fileSize = (uint)fs.Position;
            fs.Seek(0, 0);
            ReadHeader(fs);
            ReadBlock1(fs);
            ReadBlock2(fs);
            ReadBlock3(fs);
            ReadBlock4(fs);
            ReadBlock5(fs);
            ReadFileNames(fs);
            ReadFiles(fs);
            fs.Close();
        }

        public void ReadFiles(Stream s)
        {
            uint pos = (uint)s.Position & 0xFFFF0000;
            if ((s.Position % 0x10000) != 0)
                pos += 0x10000;
            fileOffsets = new Dictionary<uint, uint>();
            s.Seek(pos, 0);
            while(true)
            {
                pos = (uint)s.Position;
                ushort unk1 = ReadU16(s);
                ushort unk2 = ReadU16(s);
                ushort size = ReadU16(s);
                if (size == 0) break;
                fileOffsets.Add(pos, size);
                s.Seek(size, SeekOrigin.Current);
            }
        }

        public void ReadHeader(Stream s)
        {
            header = new HeaderStruct();
            header.MagicNumber = ReadU32(s);
            header.PackageVersion = ReadU32(s);
            header.DecompressedFileSize = ReadU64(s);
            header.Flags = ReadU32(s);
            header.FileCount = ReadU32(s);
            header.LocationCount = ReadU32(s);
            header.HeaderSector = ReadU32(s);
            header.FileSizeBitCount = ReadU32(s);
            header.FileLocationCountBitCount = ReadU32(s);
            header.FileLocationIndexBitCount = ReadU32(s);
            header.LocationBitCount = ReadU32(s);
            header.CompSectorToDecomOffsetBitCount = ReadU32(s);
            header.DecompSectorToCompSectorBitCount = ReadU32(s);
            header.CRC = ReadU32(s);
            ReadU32(s);
        }

        public string PrintHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Magic    : 0x" + header.MagicNumber.ToString("X8"));
            sb.AppendLine("Version  : 0x" + header.PackageVersion.ToString("X8"));
            sb.AppendLine("Header08 : 0x" + header.DecompressedFileSize.ToString("X16"));
            sb.AppendLine("Header10 : 0x" + header.Flags.ToString("X8"));
            sb.AppendLine("Header14 : 0x" + header.FileCount.ToString("X8"));
            sb.AppendLine("Header18 : 0x" + header.LocationCount.ToString("X8"));
            sb.AppendLine("Header1C : 0x" + header.HeaderSector.ToString("X8"));
            sb.AppendLine("Header20 : 0x" + header.FileSizeBitCount.ToString("X8"));
            sb.AppendLine("Header24 : 0x" + header.FileLocationCountBitCount.ToString("X8"));
            sb.AppendLine("Header28 : 0x" + header.FileLocationIndexBitCount.ToString("X8"));
            sb.AppendLine("Header2C : 0x" + header.LocationBitCount.ToString("X8"));
            sb.AppendLine("Header30 : 0x" + header.CompSectorToDecomOffsetBitCount.ToString("X8"));
            sb.AppendLine("Header34 : 0x" + header.DecompSectorToCompSectorBitCount.ToString("X8"));
            sb.AppendLine("Header38 : 0x" + header.CRC.ToString("X8"));
            return sb.ToString();
        }

        public void ReadBlock1(Stream s)
        {
            uint size = 0x40;
            size += header.FileSizeBitCount;
            size += header.FileLocationCountBitCount;
            size += header.FileLocationIndexBitCount;
            size *= header.FileCount;
            size += 7;
            size = size >> 3;
            block1 = new byte[size];
            s.Read(block1, 0, (int)size);
        }

        public string PrintBlock1()
        {
            StringBuilder sb = new StringBuilder();
            uint pos = 0;
            for (int i = 0; i < header.FileCount; i++)
            {
                sb.Append(i.ToString("d6") + " : ");
                ulong u1 = ReadBits(block1, pos, 0x40);
                pos += 0x40;
                ulong u2 = ReadBits(block1, pos, header.FileSizeBitCount);
                pos += header.FileSizeBitCount;
                ulong u3 = ReadBits(block1, pos, header.FileLocationCountBitCount);
                pos += header.FileLocationCountBitCount;
                ulong u4 = ReadBits(block1, pos, header.FileLocationIndexBitCount);
                pos += header.FileLocationIndexBitCount;
                sb.Append("0x" + u1.ToString("X16") + " 0x" + u2.ToString("X16") + " 0x" + u3.ToString("X16") + " 0x" + u4.ToString("X16"));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadBlock2(Stream s)
        {
            uint size = header.LocationBitCount * header.LocationCount;
            size += 7;
            size = size >> 3;
            block2 = new byte[size];
            s.Read(block2, 0, (int)size);
        }

        public string PrintBlock2()
        {
            StringBuilder sb = new StringBuilder();
            uint pos = 0;
            for (int i = 0; i < header.LocationCount; i++)
            {
                sb.Append(i.ToString("d6") + " : ");
                ulong u1 = ReadBits(block2, pos, header.LocationBitCount);
                pos += header.LocationBitCount;
                sb.Append("0x" + u1.ToString("X8"));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadBlock3(Stream s)
        {
            uint a = 0x10000;
            uint b = a * header.HeaderSector;
            b = fileSize - b;
            b += 0x3FFF;
            uint c = (b >> 0xD);
            c = (c >> 50);
            b += c;
            b = (b >> 0xE);
            uint size = b * header.LocationBitCount;
            size += 7;
            size = (size >> 3);
            block3 = new byte[size];
            s.Read(block3, 0, (int)size);
        }

        public string PrintBlock3()
        {
            StringBuilder sb = new StringBuilder();
            uint pos = 0;
            uint count = (uint)(block3.Length * 8) / header.LocationBitCount;
            for (int i = 0; i < count; i++)
            {
                sb.Append(i.ToString("d6") + " : ");
                ulong u1 = ReadBits(block3, pos, header.LocationBitCount);
                pos += header.LocationBitCount;
                sb.Append("0x" + u1.ToString("X8"));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadBlock4(Stream s)
        {
            uint a = 0x10000;
            uint b = a * header.HeaderSector;
            b = fileSize - b;
            b += 0x3FFF;
            uint c = (b >> 0xD);
            c = (c >> 50);
            b += c;
            b = (b >> 0xE);
            uint d = (uint)header.DecompressedFileSize + 0x3FFF;
            uint e = (d >> 0xD);
            e = (e >> 50);
            d += e;
            uint f = (d >> 0xE);
            uint g = GetHighestBit(b);
            uint size = f * g;
            size += 7;
            size = (size >> 3);
            block4 = new byte[size];
            s.Read(block4, 0, (int)size);
        }

        public void ReadBlock5(Stream s)
        {
            block5 = new uint[header.FileCount];
            for (int i = 0; i < header.FileCount; i++)
                block5[i] = ReadU32(s);
        }

        public void ReadFileNames(Stream s)
        {
            long pos = s.Position;
            fileNames = new string[header.FileCount];
            for (int i = 0; i < header.FileCount; i++)
            {
                s.Seek(pos + block5[i], 0);
                fileNames[i] = ReadString(s);
            }
        }

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
    }
}
