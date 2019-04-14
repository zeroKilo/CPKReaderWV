using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPKReaderWV
{
    public class CPKReader
    {
        public string cpkpath;
        public helper help;
        public CPKFile cpkfile;
        public uint fileSize;
        public CPKFile.HeaderStruct header;
        public CPKFile.FileInfo[] fileinfo;
        public byte[] BFileInfo;
        public byte[] block2;
        public byte[] block3;
        public byte[] block4;
        public uint[] block5;
        public string[] fileNames;
        public Dictionary<uint, uint> fileOffsets;
        

        public CPKReader(string path)
        {
            help = new helper();
            cpkfile = new CPKFile();
            cpkfile.CPKFilePath = path;
            //Console.WriteLine("Reverse: " + cpkfile.ByteReverse() +"\t Version: "+ cpkfile.GetPackageVersion());
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.End);
            fileSize = (uint)fs.Position;
            fs.Seek(0, 0);
            ReadHeader(fs);
            ReadBlock1(fs);
            ReadLocation(fs);
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
            while (true)
            {
                pos = (uint)s.Position;
                ushort unk1 = help.ReadU16(s);
                ushort unk2 = help.ReadU16(s);
                ushort size = help.ReadU16(s);
                if (size == 0) break;
                fileOffsets.Add(pos, size);
                s.Seek(size, SeekOrigin.Current);
            }
        }

        public void ReadHeader(Stream s)
        {
            header = new CPKFile.HeaderStruct();
            header.MagicNumber = help.ReadU32(s);
            header.PackageVersion = help.ReadU32(s);
            header.DecompressedFileSize = help.ReadU64(s);
            header.Flags = help.ReadU32(s);
            header.FileCount = help.ReadU32(s);
            header.LocationCount = help.ReadU32(s);
            header.HeaderSector = help.ReadU32(s);
            header.FileSizeBitCount = help.ReadU32(s);
            header.FileLocationCountBitCount = help.ReadU32(s);
            header.FileLocationIndexBitCount = help.ReadU32(s);
            header.LocationBitCount = help.ReadU32(s);
            header.CompSectorToDecomOffsetBitCount = help.ReadU32(s);
            header.DecompSectorToCompSectorBitCount = help.ReadU32(s);
            header.CRC = help.ReadU32(s);
            header.unknown = help.ReadU32(s); //always 0
        }

        public string PrintHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MagicNumber    : " + header.MagicNumber.ToString("X8"));
            sb.AppendLine("PackageVersion  : " + header.PackageVersion.ToString());
            sb.AppendLine("DecompressedFileSize : " + header.DecompressedFileSize.ToString());
            sb.AppendLine("Flags : " + header.Flags.ToString());
            sb.AppendLine("FileCount : " + header.FileCount.ToString());
            sb.AppendLine("LocationCount : " + header.LocationCount.ToString());
            sb.AppendLine("HeaderSector : " + header.HeaderSector.ToString());
            sb.AppendLine("FileSizeBitCount : " + header.FileSizeBitCount.ToString());
            sb.AppendLine("FileLocationCountBitCount : " + header.FileLocationCountBitCount.ToString());
            sb.AppendLine("FileLocationIndexBitCount : " + header.FileLocationIndexBitCount.ToString());
            sb.AppendLine("LocationBitCount : " + header.LocationBitCount.ToString());
            sb.AppendLine("CompSectorToDecomOffsetBitCount : " + header.CompSectorToDecomOffsetBitCount.ToString());
            sb.AppendLine("DecompSectorToCompSectorBitCount : " + header.DecompSectorToCompSectorBitCount.ToString());
            sb.AppendLine("CRC : " + header.CRC.ToString(""));
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
            BFileInfo = new byte[size];
            s.Read(BFileInfo, 0, (int)size);
        }

        public string PrintBlock1()
        {
            fileinfo = new CPKFile.FileInfo[header.FileCount];
            StringBuilder sb = new StringBuilder();
            uint pos = 0;
            for (int i = 0; i < header.FileCount; i++)
            {
                sb.Append(i.ToString("d6") + " : ");
                ulong u1 = help.ReadBits(BFileInfo, pos, 0x40);
                fileinfo[i].dwHash = u1;
                pos += 0x40;
                ulong u2 = help.ReadBits(BFileInfo, pos, header.FileSizeBitCount);
                fileinfo[i].nSize = (uint)u2;
                pos += header.FileSizeBitCount;
                ulong u3 = help.ReadBits(BFileInfo, pos, header.FileLocationCountBitCount);
                fileinfo[i].nLocationCount = (uint)u3;
                pos += header.FileLocationCountBitCount;
                ulong u4 = help.ReadBits(BFileInfo, pos, header.FileLocationIndexBitCount);
                fileinfo[i].nLocationIndex = (uint)u4;
                pos += header.FileLocationIndexBitCount;
                sb.Append("Hash: " + u1.ToString("X16") + " Size: " + u2.ToString() + " LocationCount: " + u3.ToString() + " LocationIndex: " + u4.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadLocation(Stream s)
        {
            uint size = header.LocationBitCount * header.LocationCount;
            size += 7;
            size = size >> 3;
            block2 = new byte[size];
            s.Read(block2, 0, (int)size);
        }

        public string PrintLocation()
        {
            StringBuilder sb = new StringBuilder();
            uint pos = 0;
            for (int i = 0; i < header.LocationCount; i++)
            {
                sb.Append(i.ToString("d6") + " : ");
                ulong u1 = help.ReadBits(block2, pos, header.LocationBitCount);
                pos += header.LocationBitCount;
                sb.Append("0x" + u1.ToString("X8"));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadBlock3(Stream s)
        {
            uint HeaderSize = (uint)CPKArchiveSizes.CPK_READ_SECTOR_SIZE * header.HeaderSector;
            HeaderSize = fileSize - HeaderSize;
            HeaderSize += 0x3FFF;
            uint c = (HeaderSize >> 0xD);
            c = (c >> 50);
            HeaderSize += c;
            HeaderSize = (HeaderSize >> 0xE);
            uint size = HeaderSize * header.LocationBitCount;
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
                ulong u1 = help.ReadBits(block3, pos, header.LocationBitCount);
                pos += header.LocationBitCount;
                sb.Append("0x" + u1.ToString("X8"));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public void ReadBlock4(Stream s)
        {
            uint HeaderSize = (uint)CPKArchiveSizes.CPK_READ_SECTOR_SIZE * header.HeaderSector;
            HeaderSize = fileSize - HeaderSize;
            HeaderSize += 0x3FFF;
            uint c = (HeaderSize >> 0xD);
            c = (c >> 50);
            HeaderSize += c;
            HeaderSize = (HeaderSize >> 0xE);
            uint d = (uint)header.DecompressedFileSize + 0x3FFF;
            uint e = (d >> 0xD);
            e = (e >> 50);
            d += e;
            uint f = (d >> 0xE);
            uint g = help.GetHighestBit(HeaderSize);
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
                block5[i] = help.ReadU32(s);
        }

        public void ReadFileNames(Stream s)
        {
            long pos = s.Position;
            fileNames = new string[header.FileCount];
            for (int i = 0; i < header.FileCount; i++)
            {
                s.Seek(pos + block5[i], 0);
                fileNames[i] = help.ReadString(s);
            }
        }
    }
}
