using System;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Globalization;

namespace QAR_Reader
{
    public partial class FormMain : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(string lpFileName, Int32 dwDesiredAccess,
            Int32 dwShareMode, Int32 lpSecurityAttributes, Int32 dwCreationDisposition,
            Int32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll")]
        static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer,
           uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, uint lpOverlapped);

        //[DllImport("kernel32.dll")]
        //extern static int SetFilePointer(IntPtr hFile, int lDistanceToMove, int lpDistanceToMoveHigh, uint dwMoveMethod);

        [DllImport("kernel32.dll")]
        extern static bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        [DllImport("kernel32.dll")]
        extern static Boolean CloseHandle(IntPtr hObject);

        static int FILE_FLAG_NO_BUFFERING = 536870912;

        private static int startPointer = 282624;//4227072;//80000;
        private static int clustNum = 512 * 64;

        private List<string[]> listAddrFlightsFound = new List<string[]>();
        //private List<int> listAddrFlightsFound = new List<int>();
        private String globalFileName = null;
        private String globalNewFileName = null;


        public FormMain()
        {
            InitializeComponent();
            export.Enabled = false;
            worker.WorkerReportsProgress = true;
            dataGridView.AllowUserToAddRows = false;
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            // Set filter options and filter index.
            openFileDialog.Filter = "Data files |*.dat|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.FileName = "CLU_0003.dat";
            openFileDialog.ShowDialog();
        }

        bool removeble = false;
        string[] removableLettersArray = new string[] { "E:", "F:", "G:", "H:", "I:", "J:", "K:", "L:", "M:", "N:", "O:" };
        static object clustProc = new object();

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            export.Enabled = true;
            globalFileName = openFileDialog.FileName;
            string fileName = openFileDialog.FileName;
            FileInfo f = new FileInfo(fileName);
            long fileLength = f.Length;
            long clustCount = fileLength / clustNum;
            List<string> fileInfo = new List<string>();
            fileInfo.Add(fileName);
            fileInfo.Add(Convert.ToString(clustCount));
            dataGridView.Rows.Clear();

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            clustProc = 0;

            string[] splitedPath = fileName.Split('\\');
            if (splitedPath.Length < 3) // if drive leter and name then check drive letter 
            {
                for (int i = 0; i < removableLettersArray.Length; i++)
                {
                    if (splitedPath[0] == removableLettersArray[i])
                    {
                        removeble = true;
                        i = removableLettersArray.Length;
                    }
                }

                if (removeble)
                {
                    if (worker.IsBusy != true)
                    {
                        fileInfo.Add(Convert.ToString(0));
                        worker.RunWorkerAsync(fileInfo);
                    }
                }
                else
                {
                    if (worker.IsBusy != true)
                    {
                        fileInfo.Add(Convert.ToString(1));
                        worker.RunWorkerAsync(fileInfo);
                    }
                }

            }
            else
            {
                if (worker.IsBusy != true)
                {
                    fileInfo.Add(Convert.ToString(1));
                    worker.RunWorkerAsync(fileInfo);
                }
            }

        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> fileInfo = (List<string>)e.Argument;
            string fileName = fileInfo[0];
            long clustCount = (long)Convert.ToInt32(fileInfo[1]);
            string fileType = fileInfo[2];

            string[] splitedPath = fileName.Split('\\');
            string driveHandle = @"\\.\" + splitedPath[0];
            double percent = 100 / (double)clustCount;

            byte[] b = new byte[512];
            uint read = 0;

            if (fileType == Convert.ToString(0))
            {
                long newFilePointer = 0;

                IntPtr ptr = CreateFile(driveHandle,
                    (int)FileAccess.Read,
                    (int)FileShare.Read,
                    0,
                    3,
                    (int)FileAttributes.Normal | FILE_FLAG_NO_BUFFERING,
                    IntPtr.Zero);

                if (ptr != (IntPtr)(-1)) //if can`t open file
                {
                    SetFilePointerEx(ptr, startPointer, out newFilePointer, (uint)SeekOrigin.Current);
                    for (int i = 0; i < clustCount; i++)
                    {
                        ReadFile(ptr, b, (uint)b.Length, out read, 0);
                        SetFilePointerEx(ptr, (long)clustNum - b.Length, out newFilePointer, (uint)SeekOrigin.Current);
                        if (((char)b[0] == 'M') && ((char)b[1] == 'O') && ((char)b[2] == 'N') && ((char)b[3] == 'S') && ((char)b[4] == 'T'))
                        {
                            int year = 2000 + b[38];
                            int month = b[40];
                            if (month > 12)
                            { month = 12; }

                            int day = b[42];
                            if (day > 31)
                            { day = 31; }

                            int hour = b[44];
                            if (hour > 24)
                            { hour = 24; }

                            int minute = b[46];
                            if (minute > 60)
                            { minute = 60; }

                            int second = b[48];
                            if (second > 60)
                            { second = 60; }

                            uint counter = BitConverter.ToUInt32(b, 112);

                            DateTime dt = new DateTime(year, month, day,
                                hour, minute, second, DateTimeKind.Utc);
                            dt.AddSeconds((double)counter);

                            string printDate = dt.ToShortDateString() + " " + dt.ToShortTimeString();

                            string[] flightFoundInfo = new string[3];
                            flightFoundInfo[0] = Convert.ToString(i * clustNum);
                            flightFoundInfo[1] = Convert.ToString(printDate);
                            flightFoundInfo[2] = Convert.ToString(printDate);
                            listAddrFlightsFound.Add(flightFoundInfo);
                        }
                        worker.ReportProgress((int)(percent * i));
                    }
                }
                CloseHandle(ptr);
            }
            else if (fileType == Convert.ToString(1))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    for (int i = 0; i < clustCount; i++)
                    {
                        b = reader.ReadBytes(b.Length);
                        reader.BaseStream.Seek(clustNum - b.Length, SeekOrigin.Current);
                        if (((char)b[0] == 'M') && ((char)b[1] == 'O') && ((char)b[2] == 'N') && ((char)b[3] == 'S') && ((char)b[4] == 'T'))
                        {
                            if ((b[38] < 100) && (b[40] < 100) && (b[42] < 100) && (b[44] < 100) && (b[46] < 100) && (b[48] < 100))
                            {
                                int year = 2000 + Int32.Parse(b[38].ToString("X"), NumberStyles.Number);
                                int month = Int32.Parse(b[40].ToString("X"), NumberStyles.Number);
                                if (month > 12)
                                { month = 12; }

                                int day = Int32.Parse(b[42].ToString("X"), NumberStyles.Number);
                                if (day > 31)
                                { day = 31; }

                                int hour = Int32.Parse(b[44].ToString("X"), NumberStyles.Number);
                                if (hour > 23)
                                { hour = 23; }

                                int minute = Int32.Parse(b[46].ToString("X"), NumberStyles.Number);
                                if (minute > 59)
                                { minute = 59; }

                                int second = Int32.Parse(b[48].ToString("X"), NumberStyles.Number);
                                if (second > 59)
                                { second = 59; }

                                UInt32 counter = BitConverter.ToUInt32(b, 116);
                                double seconds = Math.Round((double)counter / 10000000);

                                DateTime dt = new DateTime(year, month, day,
                                    hour, minute, second, DateTimeKind.Utc);
                                dt = dt.AddSeconds(seconds);

                                string printDate = dt.ToShortDateString() + " " + dt.ToShortTimeString();

                                string[] flightFoundInfo = new string[3];
                                flightFoundInfo[0] = Convert.ToString(i * clustNum);
                                flightFoundInfo[1] = Convert.ToString(printDate);
                                flightFoundInfo[2] = Convert.ToString(printDate);
                                listAddrFlightsFound.Add(flightFoundInfo);
                            }
                        }
                        worker.ReportProgress((int)(percent * i));
                    }
                }
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void export_Click(object sender, EventArgs e)
        {
            int selected = dataGridView.CurrentCell.RowIndex;
            List<string[]> l = listAddrFlightsFound;
            int found = dataGridView.RowCount - 1;
            string fileName = globalFileName;
            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;
            int startCopyInd = 0;
            int endCopyInd = 0;
            byte by;

            if (selected == found)
            {
                startCopyInd = Convert.ToInt32(l[selected][0]);
                endCopyInd = fileLength;
            }
            else
            {
                startCopyInd = Convert.ToInt32(l[selected][0]);
                endCopyInd = Convert.ToInt32(l[selected + 1][0]);
            }


            int emptyByteCounter = 0;
            string newFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + selected + "_CLU_0003.DAT";
            globalNewFileName = newFile;

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create));
                reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);
                for (int i = startCopyInd; i < endCopyInd; i++)
                {
                    by = reader.ReadByte();
                    bwr.Write(by);

                    if (by == 255)
                    {
                        emptyByteCounter++;
                        if (emptyByteCounter > 16)
                        {
                            i = endCopyInd;
                        }
                    }
                    else
                    {
                        emptyByteCounter = 0;
                    }
                }
                bwr.Close();
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 100;
            foreach (string[] i in listAddrFlightsFound)
            {
                this.dataGridView.Rows.Add(i[0], i[1], i[2]);
            }
        }
    }
}
