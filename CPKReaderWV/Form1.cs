using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace CPKReaderWV
{
    public partial class Form1 : Form
    {
        public CPKFile cpk;

        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.cpk|*.cpk";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                status.Text = d.FileName;
                cpk = new CPKFile(d.FileName);
                RefreshAll();
            }
        }

        private void RefreshAll()
        {
            rtb1.Text = cpk.PrintHeader();
            rtb2.Text = cpk.PrintBlock1();
            rtb3.Text = cpk.PrintLocation();
            rtb4.Text = cpk.PrintBlock3();
            hb3.ByteProvider = new DynamicByteProvider(cpk.block4);
            listBox1.Items.Clear();
            foreach (uint u in cpk.block5)
                listBox1.Items.Add(u.ToString("X8"));
            listBox2.Items.Clear();
            int count = 0;
            foreach (string file in cpk.fileNames)
                listBox2.Items.Add((count++) + ":" + file);
            listBox3.Items.Clear();
            count = 0;
            foreach (KeyValuePair<uint, uint> pair in cpk.fileOffsets)
                listBox3.Items.Add((count++) + ": Offset=0x" + pair.Key.ToString("X8") + " Size=0x" + pair.Value.ToString("X8"));
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1) return;
            KeyValuePair<uint, uint> pair = cpk.fileOffsets.ToArray()[n];
            FileStream fs = new FileStream(cpk.cpkpath, FileMode.Open, FileAccess.Read);
            fs.Seek(pair.Key + 6, 0);
            byte[] buff = new byte[pair.Value];
            fs.Read(buff, 0, (int)pair.Value);
            fs.Close();
            try
            {
                buff = DecompressZlib(buff);
            }
            catch { }
            hb1.ByteProvider = new DynamicByteProvider(buff);
        }


        public static byte[] DecompressZlib(byte[] input)
        {
            MemoryStream source = new MemoryStream(input);
            byte[] result = null;
            using (MemoryStream outStream = new MemoryStream())
            {
                using (InflaterInputStream inf = new InflaterInputStream(source))
                {
                    inf.CopyTo(outStream);
                }
                result = outStream.ToArray();
            }
            return result;
        }
    }
}
