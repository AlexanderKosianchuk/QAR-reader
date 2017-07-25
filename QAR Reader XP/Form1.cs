using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace QAR_Reader_XP
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

        [DllImport("kernel32.dll")]
        static extern bool WriteFile(IntPtr hFile, byte[] lpBuffer,
           uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, uint lpOverlapped);

        //[DllImport("kernel32.dll")]
        //extern static int SetFilePointer(IntPtr hFile, int lDistanceToMove, int lpDistanceToMoveHigh, uint dwMoveMethod);

        [DllImport("kernel32.dll")]
        extern static bool SetFilePointerEx(IntPtr hFile, long liDistanceToMove, out long lpNewFilePointer, uint dwMoveMethod);

        [DllImport("kernel32.dll")]
        extern static Boolean CloseHandle(IntPtr hObject);

        static int FILE_FLAG_NO_BUFFERING = 536870912;

        private const long startPointer = 249856;//8388608;//282624;//4227072;//80000;
        private static int clustNum = 512 * 64;
        private static int headerLength = 128;

        private static int dimBitOrByte = 0;
        private static int channelLength = 0;
        private static int channelsCountInFrame = 0;
        private static int frameFreq = 0;
        private static int clusterTimeLength = 0;
        private static int qarType = 0;
        private string chosenDriveHandle = String.Empty;

        private static string[] qarTypesNames = new string[256];

        private List<string[]> listAddrFlightsFound = new List<string[]>();
        //private List<int> listAddrFlightsFound = new List<int>();
        private string globalFileName = string.Empty;

        public FormMain()
        {
            InitializeComponent();
            export.Enabled = false;
            worker.WorkerReportsProgress = true;
            exportWorker.WorkerReportsProgress = true;
            exportPartitionWorker.WorkerReportsProgress = true;

            dataGridView.AllowUserToAddRows = false;

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Value = 0;

            qarTypesNames[0] = "ЭБН-12"; 
            qarTypesNames[1] = "ЭБН-64"; 
            qarTypesNames[5] = "ЭБН-Т-М"; 
            qarTypesNames[6] = "ЭБН-Т-Л"; 
            qarTypesNames[10] = "ЭБН-Б-1"; 
            qarTypesNames[11] = "ЭБН-Б-3"; 
            qarTypesNames[14] = "ЭБН-Т-2"; 
            qarTypesNames[21] = "CFDR-42"; 
            qarTypesNames[22] = "ЭБН САРПП";
            qarTypesNames[23] = "4700";
            qarTypesNames[254] = "VDR";
            qarTypesNames[255] = "ЭБН-Р";

            labelAction.Text = "waiting for user actions";
        }

#region toolStrip events

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Set filter options and filter index.
            openFileDialog.Filter = "EBN files |*.dat|Partition files |*.par|All Files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.FileName = "CLU_0003.dat";
            openFileDialog.ShowDialog();
        }

        bool removeble = false;
        string[] removableLettersArray = new string[] { "E:", "F:", "G:", "H:", "I:", "J:", "K:", "L:", "M:", "N:", "O:" };
        static object clustProc = new object();

        private void openFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            dataGridView.Rows.Clear();
            globalFileName = openFileDialog.FileName;
            if ((Path.GetExtension(globalFileName) == ".dat") || 
                (Path.GetExtension(globalFileName) == ".DAT"))
            {
                fileButt.Enabled = false;
                readFile(openFileDialog.FileName);
            }
            else if ((Path.GetExtension(globalFileName) == ".par") || 
                (Path.GetExtension(globalFileName) == ".PAR"))
            {
                //fileButt.Enabled = false;
                readPartition(openFileDialog.FileName);
            }
            else
            {
                MessageBox.Show("Incorrect input file", "Warning!");
            }
        }

        private void noForciblyStripMenuItem_Click(object sender, EventArgs e)
        {
            noForciblyStripMenuItem.Checked = true;
            rawToolStripMenuItem.Checked = false;
            A320BitAppendWithRotationToolStripMenuItem.Checked = false;
            a320CFToolStripMenuItem.Checked = false;
            saab340ToolStripMenuItem.Checked = false;
        }

        private void rawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noForciblyStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = true;
            A320BitAppendWithRotationToolStripMenuItem.Checked = false;
            a320CFToolStripMenuItem.Checked = false;
            saab340ToolStripMenuItem.Checked = false;
        }

        private void A320BitAppendWithRotationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noForciblyStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            A320BitAppendWithRotationToolStripMenuItem.Checked = true;
            a320CFToolStripMenuItem.Checked = false;
            saab340ToolStripMenuItem.Checked = false;
        }

        private void a320CFToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noForciblyStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            A320BitAppendWithRotationToolStripMenuItem.Checked = false;
            a320CFToolStripMenuItem.Checked = true;
            saab340ToolStripMenuItem.Checked = false;
        }

        private void saab340ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            noForciblyStripMenuItem.Checked = false;
            rawToolStripMenuItem.Checked = false;
            A320BitAppendWithRotationToolStripMenuItem.Checked = false;
            a320CFToolStripMenuItem.Checked = false;
            saab340ToolStripMenuItem.Checked = true;
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows[dataGridView.CurrentCell.RowIndex].Selected = true;
            }
        }

        private void exportFlightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveFileDialog.Filter = "Exported flight (*.inf)|*.inf|All Files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;

            int selected = dataGridView.CurrentCell.RowIndex;
            List<string[]> l = listAddrFlightsFound;
            string flightDate = l[selected][2];
            string flightLength = l[selected][4];

            flightLength = flightLength.Replace(':', '_');

            DateTime saveNow = DateTime.Parse(flightDate);
            saveFileDialog.FileName = flightLength + "-";

            if (saveNow.Hour.ToString().Length < 2)
            { saveFileDialog.FileName += "0" + saveNow.Hour.ToString(); }
            else { saveFileDialog.FileName += saveNow.Hour.ToString(); }

            if (saveNow.Minute.ToString().Length < 2)
            { saveFileDialog.FileName += "0" + saveNow.Minute.ToString(); }
            else { saveFileDialog.FileName += saveNow.Minute.ToString(); }

            if (saveNow.Second.ToString().Length < 2)
            { saveFileDialog.FileName += "0" + saveNow.Second.ToString(); }
            else { saveFileDialog.FileName += saveNow.Second.ToString(); }

            if (saveNow.Day.ToString().Length < 2)
            { saveFileDialog.FileName += "_0" + saveNow.Day.ToString(); }
            else { saveFileDialog.FileName += "_" + saveNow.Day.ToString(); }

            if (saveNow.Month.ToString().Length < 2)
            { saveFileDialog.FileName += "0" + saveNow.Month.ToString(); }
            else { saveFileDialog.FileName += saveNow.Month.ToString(); }

            if (saveNow.Year.ToString().Length < 2)
            { saveFileDialog.FileName += "0" + saveNow.Year.ToString(); }
            else { saveFileDialog.FileName += saveNow.Year.ToString(); }

            saveFileDialog.FileName += ".inf";

            saveFileDialog.ShowDialog();
        }

        private void saveFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            String saveFileName = saveFileDialog.FileName;

            export.Enabled = false;
            exportPartition.Enabled = false;
            labelAction.Text = "exporting flight";

            int selected = dataGridView.CurrentCell.RowIndex;
            List<string[]> l = listAddrFlightsFound;
            int startCopyInd = Convert.ToInt32(l[selected][0]);
            int endCopyInd = Convert.ToInt32(l[selected][1]);
            string flightDate = l[selected][2];

            if (exportWorker.IsBusy != true)
            {
                List<object> exportOptions = new List<object>();
                exportOptions.Add(saveFileName);
                exportOptions.Add(qarTypesNames[qarType]);
                exportOptions.Add(startCopyInd);
                exportOptions.Add(endCopyInd);
                exportOptions.Add(selected + 1);
                exportOptions.Add(flightDate);
                exportOptions.Add(globalFileName);
                exportWorker.RunWorkerAsync(exportOptions);
            }
            else
            {
                MessageBox.Show("Cant export file because of previous action not complete", "Fail!");
            }

        }

        private void exportPartitionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                fileButt.Enabled = false;
                export.Enabled = false;
                exportPartition.Enabled = false;

                string folderName = folderBrowserDialog.SelectedPath;

                if (exportPartitionWorker.IsBusy != true)
                {
                    exportPartitionWorker.RunWorkerAsync(folderName);
                }

                labelAction.Text = "exporting and archiving partition";
                return;
            }
        }

#endregion

        private void readFile(string selectedFileName)
        {
            export.Enabled = true;
            FileInfo f = new FileInfo(selectedFileName);
            long fileLength = f.Length;
            long clustCount = fileLength / clustNum;
            List<string> fileInfo = new List<string>();
            fileInfo.Add(selectedFileName);
            fileInfo.Add(Convert.ToString(clustCount));
            dataGridView.Rows.Clear();
            listAddrFlightsFound.Clear();

            clustProc = 0;

            string[] splitedPath = selectedFileName.Split('\\');
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

            labelAction.Text = "reading QAR file";
        }

        DateTime dt = new DateTime();
        DateTime dtFlightLength = new DateTime();
        DateTime endDt = new DateTime();

        private void searchFlights(byte[] b, long position)
        {
            if (((char)b[0] == 'M') && ((char)b[1] == 'O') && ((char)b[2] == 'N') && ((char)b[3] == 'S') && ((char)b[4] == 'T'))
            {
                if ((b[38] < 100) && (b[40] < 100) && (b[42] < 100) && (b[44] < 100) && (b[46] < 100) && (b[48] < 100))
                {
                    int num = 0;
                    int year = 2000;
                    bool isNum = int.TryParse(b[38].ToString("X"), out num);
                    if (isNum)
                    {
                        year += num;
                    }
                    else
                    {
                        year += Int32.Parse(b[38].ToString("D"), NumberStyles.Number);
                    }

                    int month = 0;
                    isNum = int.TryParse(b[40].ToString("X"), out num);
                    if (isNum)
                    {
                        month += num;
                    }
                    else
                    {
                        month += Int32.Parse(b[40].ToString("D"), NumberStyles.Number);
                    }
                    if (month > 12)
                    { month = 12; }


                    int day = 0;
                    isNum = int.TryParse(b[42].ToString("X"), out num);
                    if (isNum)
                    {
                        day += num;
                    }
                    else
                    {
                        day += Int32.Parse(b[42].ToString("D"), NumberStyles.Number);
                    }
                    if (day > 31)
                    { day = 31; }

                    int hour = 0;
                    isNum = int.TryParse(b[44].ToString("X"), out num);
                    if (isNum)
                    {
                        hour += num;
                    }
                    else
                    {
                        hour += Int32.Parse(b[44].ToString("D"), NumberStyles.Number);
                    }
                    if (hour > 23)
                    { hour = 23; }

                    int minute = 0;
                    isNum = int.TryParse(b[46].ToString("X"), out num);
                    if (isNum)
                    {
                        minute += num;
                    }
                    else
                    {
                        minute += Int32.Parse(b[46].ToString("D"), NumberStyles.Number);
                    }
                    if (minute > 59)
                    { minute = 59; }

                    int second = 0;
                    isNum = int.TryParse(b[48].ToString("X"), out num);
                    if (isNum)
                    {
                        second += num;
                    }
                    else
                    {
                        second += Int32.Parse(b[48].ToString("D"), NumberStyles.Number);
                    }
                    if (second > 59)
                    { second = 59; }

                    string s = b[115].ToString("X");
                    if (b[114].ToString("X").Length == 1)
                    {
                        s += "0" + b[114].ToString("X");
                    }
                    else
                    {
                        s += b[114].ToString("X");
                    }

                    if (b[113].ToString("X").Length == 1)
                    {
                        s += "0" + b[113].ToString("X");
                    }
                    else
                    {
                        s += b[113].ToString("X");
                    }

                    if (b[112].ToString("X").Length == 1)
                    {
                        s += "0" + b[112].ToString("X");
                    }
                    else
                    {
                        s += b[112].ToString("X");
                    }

                    UInt32 initializationCounterValue = UInt32.Parse(s, NumberStyles.AllowHexSpecifier);

                    s = b[123].ToString("X");
                    if (b[122].ToString("X").Length == 1)
                    {
                        s += "0" + b[122].ToString("X");
                    }
                    else
                    {
                        s += b[122].ToString("X");
                    }

                    if (b[121].ToString("X").Length == 1)
                    {
                        s += "0" + b[121].ToString("X");
                    }
                    else
                    {
                        s += b[121].ToString("X");
                    }

                    if (b[120].ToString("X").Length == 1)
                    {
                        s += "0" + b[120].ToString("X");
                    }
                    else
                    {
                        s += b[120].ToString("X");
                    }

                    UInt32 counterValue = UInt32.Parse(s, NumberStyles.AllowHexSpecifier);

                    double counter = 0;
                    if (initializationCounterValue > counterValue)
                    {
                        counter = 4294967295 - counterValue + initializationCounterValue;
                    }
                    else
                    {
                        counter = counterValue - initializationCounterValue;
                    }
                    double seconds = Math.Round((double)counter / 256);

                    dt = new DateTime(year, month, day,
                        hour, minute, second, DateTimeKind.Utc);
                    dt = dt.AddSeconds(seconds);
                    dtFlightLength = new DateTime(year, month, day,
                        0, 0, 0, DateTimeKind.Utc);
                    string printDate = dt.ToShortDateString() + " " + dt.ToLongTimeString();

                    string[] flightFoundInfo = new string[5];
                    flightFoundInfo[0] = Convert.ToString(position);
                    flightFoundInfo[1] = Convert.ToString(position);
                    flightFoundInfo[2] = Convert.ToString(printDate);
                    flightFoundInfo[3] = Convert.ToString(0);
                    flightFoundInfo[4] = Convert.ToString(0);
                    listAddrFlightsFound.Add(flightFoundInfo);

                    dimBitOrByte = b[11];
                    byte[] dimInfo = new byte[2];

                    if (dimBitOrByte == 0)
                    {
                        Array.Copy(b, 12, dimInfo, 0, 2);
                        channelLength = BitConverter.ToInt16(dimInfo, 0);
                        Array.Copy(b, 14, dimInfo, 0, 2);
                        channelsCountInFrame = BitConverter.ToInt16(dimInfo, 0);
                        Array.Copy(b, 16, dimInfo, 0, 2);
                        frameFreq = BitConverter.ToInt16(dimInfo, 0);

                        clusterTimeLength = clustNum / (channelsCountInFrame * channelLength) / frameFreq;
                    }
                    else if (dimBitOrByte == 1)
                    {
                        Array.Copy(b, 12, dimInfo, 0, 2);
                        channelLength = BitConverter.ToInt16(dimInfo, 0) * 8;
                        Array.Copy(b, 14, dimInfo, 0, 2);
                        channelsCountInFrame = BitConverter.ToInt16(dimInfo, 0) * 8;
                        Array.Copy(b, 16, dimInfo, 0, 2);
                        frameFreq = BitConverter.ToInt16(dimInfo, 0) * 8;

                        clusterTimeLength = clustNum / (channelsCountInFrame * channelLength) / frameFreq;
                    }

                    int endSector = Int32.Parse(listAddrFlightsFound[listAddrFlightsFound.Count - 1][1]);
                    endSector += clustNum;
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][1] = Convert.ToString(endSector);

                    dtFlightLength = dtFlightLength.AddSeconds(clusterTimeLength);
                    string printLenght = dtFlightLength.ToLongTimeString();
                    endDt = dt.AddSeconds(clusterTimeLength);
                    string printEndFlight = endDt.ToShortDateString() + " " + endDt.ToLongTimeString();
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][3] = Convert.ToString(printEndFlight);
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][4] = Convert.ToString(printLenght);

                    qarType = b[10];
                }
            }
            else if ((b[0] != 0) || (b[1] != 0) || (b[2] != 0) || (b[3] != 0) || (b[4] != 0) || (b[6] != 0) || (b[7] != 0) ||
                (b[8] != 0) || (b[9] != 0) || (b[10] != 0) || (b[11] != 0) || (b[12] != 0) || (b[13] != 0) || (b[14] != 0) || (b[15] != 0) &&
                (b[0] != 255) || (b[1] != 255) || (b[2] != 255) || (b[3] != 255) || (b[4] != 255) || (b[6] != 255) || (b[7] != 255) ||
                (b[8] != 255) || (b[9] != 255) || (b[10] != 255) || (b[11] != 255) || (b[12] != 255) || (b[13] != 255) || (b[14] != 255) || (b[15] != 255))
            {
                if (listAddrFlightsFound.Count > 0)
                {
                    int endSector = Int32.Parse(listAddrFlightsFound[listAddrFlightsFound.Count - 1][1]);
                    endSector += clustNum;
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][1] = Convert.ToString(endSector);

                    dtFlightLength = dtFlightLength.AddSeconds(clusterTimeLength);
                    string printLenght = dtFlightLength.ToLongTimeString();
                    endDt = endDt.AddSeconds(clusterTimeLength);
                    string printEndFlight = endDt.ToShortDateString() + " " + endDt.ToLongTimeString();
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][3] = Convert.ToString(printEndFlight);
                    listAddrFlightsFound[listAddrFlightsFound.Count - 1][4] = Convert.ToString(printLenght);
                }
            }
        }

        public void readPartition(string fileName)
        {
            export.Enabled = true;
            FileInfo f = new FileInfo(fileName);
            long fileLength = f.Length;
            List<string> fileInfo = new List<string>();
            dataGridView.Rows.Clear();
            int frameLenth = 768;
            int headerInsertLenth = 32;
            long prevFoudFlightLength = 0;
            DateTime prevFoundStartDate;
            string[] flightFoundInfo = new string[5];
            int emptyByteCounter = 0;
            int emptyByteCounterMaxCount = frameLenth * 1000;
            long curPos = 0;

            TimeSpan t;
            string flightFormatedDuration;
            DateTime saveNow = DateTime.Now;
            flightFoundInfo[0] = Convert.ToString(curPos);
            flightFoundInfo[1] = Convert.ToString(curPos);
            flightFoundInfo[2] = Convert.ToString(saveNow.ToShortDateString() + " " + saveNow.ToLongTimeString());
            flightFoundInfo[3] = Convert.ToString(saveNow.ToShortDateString() + " " + saveNow.ToLongTimeString());
            flightFoundInfo[4] = Convert.ToString(0);
            listAddrFlightsFound.Add(flightFoundInfo);

            byte[] b = new byte[frameLenth];
            byte[] headerInsert = new byte[headerInsertLenth];
            byte oneByte = new byte();

            using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                while ((br.Read(b, 0, frameLenth) > 0) && (emptyByteCounter < emptyByteCounterMaxCount))
                {
                    if (Array.TrueForAll(b, EqualZero) || Array.TrueForAll(b, EqualOne))
                    {
                        listAddrFlightsFound[listAddrFlightsFound.Count - 1][1] = 
                            (br.BaseStream.Position - (long)frameLenth).ToString();
                    }

                    //we found flight end
                    if (listAddrFlightsFound[listAddrFlightsFound.Count - 1][0] != 
                            listAddrFlightsFound[listAddrFlightsFound.Count - 1][1])
                    {
                        //we need to search next flight begining
                        oneByte = br.ReadByte();
                        emptyByteCounter = 0;
                        while ((oneByte == 0) && 
                            (br.BaseStream.Position < br.BaseStream.Length) &&
                            (emptyByteCounter < emptyByteCounterMaxCount))
                        {
                            oneByte = br.ReadByte();
                            emptyByteCounter++;
                        }

                        if (emptyByteCounter < emptyByteCounterMaxCount)
                        {
                            br.BaseStream.Seek(-1, SeekOrigin.Current);
                            curPos = br.BaseStream.Position;

                            br.Read(headerInsert, 0, headerInsertLenth);
                            br.BaseStream.Seek(-headerInsertLenth, SeekOrigin.Current);

                            int year = 2000;
                            year += Int32.Parse(headerInsert[15].ToString("X"), NumberStyles.Number);

                            int month = 0;
                            month += Int32.Parse(headerInsert[14].ToString("X"), NumberStyles.Number);
                            if (month > 12)
                            { month = 12; }
                            else if (month < 1)
                            { month = 1; }

                            int day = 0;
                            day += Int32.Parse(headerInsert[13].ToString("X"), NumberStyles.Number);

                            if (day > 31)
                            { day = 31; }
                            else if (day < 1)
                            { day = 1; }

                            int hour = 0;
                            hour += Int32.Parse(headerInsert[11].ToString("X"), NumberStyles.Number);
                            if (hour > 23)
                            { hour = 23; }

                            int minute = 0;
                            minute += Int32.Parse(headerInsert[10].ToString("X"), NumberStyles.Number);
                            if (minute > 59)
                            { minute = 59; }

                            int second = 0;
                            second += Int32.Parse(headerInsert[9].ToString("X"), NumberStyles.Number);
                            if (second > 59)
                            { second = 59; }

                            dt = new DateTime(year, month, day,
                                hour, minute, second, DateTimeKind.Utc);

                            flightFoundInfo = new string[5];
                            saveNow = DateTime.Now;

                            flightFoundInfo[0] = Convert.ToString(curPos);
                            flightFoundInfo[1] = Convert.ToString(curPos);
                            flightFoundInfo[2] = Convert.ToString(dt.ToShortDateString() + " " + dt.ToLongTimeString());
                            flightFoundInfo[3] = Convert.ToString(saveNow.ToShortDateString() + " " + saveNow.ToLongTimeString());
                            flightFoundInfo[4] = Convert.ToString("");
                            listAddrFlightsFound.Add(flightFoundInfo);

                            prevFoudFlightLength = (Convert.ToInt64(listAddrFlightsFound[listAddrFlightsFound.Count - 2][1]) -
                            Convert.ToInt64(listAddrFlightsFound[listAddrFlightsFound.Count - 2][0])) / frameLenth * 2;

                            prevFoundStartDate = Convert.ToDateTime(listAddrFlightsFound[listAddrFlightsFound.Count - 2][2]);
                            prevFoundStartDate = prevFoundStartDate.AddSeconds(prevFoudFlightLength);
                            t = TimeSpan.FromSeconds(prevFoudFlightLength);

                            flightFormatedDuration = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                            t.Hours,
                                            t.Minutes,
                                            t.Seconds);

                            listAddrFlightsFound[listAddrFlightsFound.Count - 2][3] =
                                Convert.ToString(prevFoundStartDate.ToShortDateString() + " " + prevFoundStartDate.ToLongTimeString()); ;
                            listAddrFlightsFound[listAddrFlightsFound.Count - 2][4] = flightFormatedDuration;
                        }
                    }
                }
            }

            /*if (Array.TrueForAll(b, EqualZero))
            {
                listAddrFlightsFound.RemoveAt(listAddrFlightsFound.Count - 1);
            }*/

            prevFoudFlightLength = (fileLength -
                        Convert.ToInt64(listAddrFlightsFound[listAddrFlightsFound.Count - 1][0])) / frameLenth * 2;

            t = TimeSpan.FromSeconds(prevFoudFlightLength);

            flightFormatedDuration = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds);

            listAddrFlightsFound[listAddrFlightsFound.Count - 1][4] = flightFormatedDuration;

            int counter = 0;
            listAddrFlightsFound.RemoveAt(0);
            foreach (string[] i in listAddrFlightsFound)
            {
                //first row is service and do not contain information
                this.dataGridView.Rows.Add(counter, i[0], i[2], i[3], i[4]);
                counter++;
            }

        }

        private static bool EqualZero(byte value)
        {
            return value == 0;
        }

        private static bool EqualOne(byte value)
        {
            return value == 255;
        }

#region searchFlightsbackgroundWorker methods

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> fileInfo = (List<string>)e.Argument;
            string fileName = fileInfo[0];
            long clustCount = (long)Convert.ToInt32(fileInfo[1]);
            string fileType = fileInfo[2];

            string[] splitedPath = fileName.Split('\\');
            string driveHandle = @"\\.\" + splitedPath[0];
            double percent = 100 / (double)clustCount;
            this.chosenDriveHandle = driveHandle;

            byte[] b = new byte[512];
            uint read = 0;

            int progressVal = 0,
                prevProgressVal = 0;

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
                        searchFlights(b, newFilePointer);
                        SetFilePointerEx(ptr, (long)clustNum - b.Length, out newFilePointer, (uint)SeekOrigin.Current);
                        

                        prevProgressVal = Convert.ToInt32(percent * i);
                        if (progressVal != prevProgressVal)
                        {
                            worker.ReportProgress(progressVal);
                        }
                        progressVal = prevProgressVal;
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
                        searchFlights(b, i);

                        prevProgressVal = Convert.ToInt32(percent * i);
                        if (progressVal != prevProgressVal)
                        {
                            worker.ReportProgress(progressVal);
                        }
                        progressVal = prevProgressVal;
                    }
                }
            }
        }

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fileButt.Enabled = true;
            exportPartition.Enabled = true;
            progressBar.Value = 100;
            int counter = 0;
            foreach (string[] i in listAddrFlightsFound)
            {
                counter++;
                this.dataGridView.Rows.Add(counter, i[0], i[2], i[3], i[4]);
            }

            if (qarTypesNames[qarType] != String.Empty)
            {
                this.Text = "QAR Viewer. Type - " + qarTypesNames[qarType] + ". " + globalFileName;
            }

            labelAction.Text = "reading QAR file complete";

            if (dataGridView.Rows.Count > 0)
            {
                dataGridView.Rows[dataGridView.CurrentCell.RowIndex].Selected = true;
            }
        }

#endregion

#region exportFlights backgroundWorker methods

        private void exportWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<object> exportOptions = (List<object>)e.Argument;
            string exportedFileName = (string)exportOptions[0];
            string qarTypesName = (string)exportOptions[1];
            int startCopyInd = (int)exportOptions[2];
            int endCopyInd = (int)exportOptions[3];
            int flightNum = (int)exportOptions[4];
            string flightDate = (string)exportOptions[5];
            string curFileName = (string)exportOptions[6];
           
            exportTesterFile(curFileName, 
                startCopyInd, 
                endCopyInd, 
                flightNum,
                flightDate,
                exportedFileName);      
        }

        private void exportWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void exportWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 100;
            labelAction.Text = "flight exported succesfully";
            export.Enabled = true;
            MessageBox.Show("Export complete", "Success!");
        }

#endregion

#region exportPartitionWorker backgroundWorker methods
        private void exportPartitionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = (string)e.Argument;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            long fileLength = 500400 * 512;

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    if (d.Name == fileName)
                    {
                        //fileLength = d.TotalSize;
                        break;
                    }
                }
            }

            long sectorsCount = fileLength / 512 ;

            string[] splitedPath = fileName.Split('\\');
            string driveHandle = @"\\.\" + splitedPath[0];
            double percent = 100 / (double)sectorsCount;

            byte[] b = new byte[512];
            uint read = 0;

            int progressVal = 0,
                prevProgressVal = 0;

            long newFilePointer = 0;

            IntPtr ptr = CreateFile(driveHandle,
                (int)FileAccess.Read,
                (int)FileShare.Read,
                0,
                3,
                (int)FileAttributes.Normal,
                IntPtr.Zero);

            string desctopPath = Environment.GetFolderPath(
                         System.Environment.SpecialFolder.DesktopDirectory);

            DateTime dt2 = DateTime.Now;
            string day = "";
            if (dt2.Day.ToString().Length > 1)
            {
                day = dt2.Day.ToString();
            }
            else
            {
                day = "0" + dt2.Day.ToString();            
            }

            string month = "";
            if (dt2.Month.ToString().Length > 1)
            {
                month = dt2.Month.ToString();
            }
            else
            {
                month = "0" + dt2.Month.ToString();
            }

            string exportFileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" +
                day + month + dt2.Year + ".par";

            using (BinaryWriter bw = new BinaryWriter(File.Open(exportFileName, FileMode.Create)))
            {
                if (ptr != (IntPtr)(-1)) //if can`t open file
                {
                    SetFilePointerEx(ptr, 0, out newFilePointer, (uint)SeekOrigin.Begin);
                    for (int i = 0; i < sectorsCount; i++)
                    {
                        ReadFile(ptr, b, (uint)b.Length, out read, 0);
                        //SetFilePointerEx(ptr, (long)clustNum - b.Length, out newFilePointer, (uint)SeekOrigin.Current);
                        //searchFlights(b, i);
                        bw.Write(b);

                        prevProgressVal = Convert.ToInt32(percent * i);
                        if (progressVal != prevProgressVal)
                        {
                            exportPartitionWorker.ReportProgress(progressVal);
                        }
                        progressVal = prevProgressVal;
                    }
                }
                else
                {
                    MessageBox.Show("Cant access partition", "Error");
                }
            }
            CloseHandle(ptr);

            /*SevenZip.LzmaAlone zip = new SevenZip.LzmaAlone();

            string[] args = new string[3];
            args[0] = "e";
            args[1] = exportFileName;
            exportFileName.Substring(0, -3);
            args[2] = exportFileName + "zip";

            try
            {
                zip.Main2(args);
            }
            catch (Exception e2)
            {
                MessageBox.Show("Error", "Caught exception " + e2);
            }*/
        }

        private void exportPartitionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void exportPartitionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fileButt.Enabled = true;
            exportPartition.Enabled = true;
            progressBar.Value = 100;

            labelAction.Text = "exporting partition complete";

            MessageBox.Show("Partition exported successfully", "Success");
        }
#endregion

        private void eraseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            if (erasePartitionBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (MessageBox.Show("Erase?", "Confirm", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                {               
                    fileButt.Enabled = false;
                    export.Enabled = false;
                    exportPartition.Enabled = false;

                    string folderName = erasePartitionBrowserDialog.SelectedPath;

                    if (erasePartitionWorker.IsBusy != true)
                    {
                        erasePartitionWorker.RunWorkerAsync(folderName);
                    }

                    labelAction.Text = "erasing partition";
                    return;
                }
            }
        }

        private void erasePartitionWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = (string)e.Argument;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            long fileLength = 500400 * 512;

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    if (d.Name == fileName)
                    {
                        //fileLength = d.TotalSize;
                        break;
                    }
                }
            }

            long sectorsCount = fileLength / 512 ;

            string[] splitedPath = fileName.Split('\\');
            string driveHandle = @"\\.\" + splitedPath[0];
            double percent = 100 / (double)sectorsCount;

            byte[] b = new byte[512];
            Array.Clear(b, 0, b.Length);
            uint read = 0;

            int progressVal = 0,
                prevProgressVal = 0;

            long newFilePointer = 0;

            IntPtr ptr = CreateFile(driveHandle,
                (int)FileAccess.ReadWrite,
                (int)FileShare.ReadWrite,
                0,
                3,
                (int)FileAttributes.Normal,
                IntPtr.Zero);

            if (ptr != (IntPtr)(-1)) //if can`t open file
            {
                SetFilePointerEx(ptr, 0, out newFilePointer, (uint)SeekOrigin.Begin);
                for (int i = 0; i < sectorsCount; i++)
                {
                    WriteFile(ptr, b, (uint)b.Length, out read, 0);

                    prevProgressVal = Convert.ToInt32(percent * i);
                    if (progressVal != prevProgressVal)
                    {
                        erasePartitionWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
            }
            else
            {
                MessageBox.Show("Cant access partition", "Error");
            }

            CloseHandle(ptr);
        }

        private void erasePartitionWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void erasePartitionWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            fileButt.Enabled = true;
            export.Enabled = true;
            exportPartition.Enabled = true;
            progressBar.Value = 100;

            labelAction.Text = "erasing partition complete";

            MessageBox.Show("Partition erased successfully", "Success");
        }
    }
}
