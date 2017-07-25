using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace QAR_Reader_XP
{
    public partial class FormMain
    {
        private string createNewFile(string startCopyTime, string selectedFlight, string dim)
        {
            startCopyTime = startCopyTime.Split(' ')[0].Replace('.', '-');
            string newFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + qarTypesNames[qarType] + "-" + selectedFlight + "_Dt-" + startCopyTime + "_Src-CLU_0003." + dim;
            return newFile;
        }

        private void exportTesterFile(string selectedFileName,
            int startCopyInd,
            int endCopyInd,
            int flightNum,
            string flightDate,
            string exportedFileName)
        {
            string fileName = selectedFileName;

            const int bufferSize = 512;
            byte[] buffer = new byte[bufferSize];
            int rawHeaderLength = 128;

            double percent = 100 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newFile = exportedFileName;
            string tmpFileName = exportedFileName + ".tmp";
            long newFilePointer = 0;
            uint read = 0;
            byte by = 0;
            bool eof = false;
            int currentIndex = startCopyInd;
            long syncroPosition = 0;

            exportWorker.ReportProgress(0);
            IntPtr ptr = CreateFile(this.chosenDriveHandle,
                    (int)FileAccess.Read,
                    (int)FileShare.Read,
                    0,
                    3,
                    (int)FileAttributes.Normal | FILE_FLAG_NO_BUFFERING,
                    IntPtr.Zero);

            if (ptr != (IntPtr)(-1)) //if can`t open file
            {
                SetFilePointerEx(ptr, startCopyInd, out newFilePointer, (uint)SeekOrigin.Current);
                using (BinaryWriter bwr = new BinaryWriter(File.Open(tmpFileName, FileMode.Create)))
                {
                    while ((ReadFile(ptr, buffer, (uint)buffer.Length, out read, 0) != false)
                         && (currentIndex < endCopyInd)
                         && !eof
                    )
                    {
                        bwr.Write(buffer);

                        int emptyCount = 0;

                        for (int jj = 0; jj < buffer.Length; jj++)
                        {
                            if (buffer[jj] == 0)
                            {
                                emptyCount++;
                            }
                        }

                        if (emptyCount == buffer.Length)
                        {
                            eof = true;
                        }

                        prevProgressVal = Convert.ToInt32(percent * (currentIndex - startCopyInd));
                        if (progressVal != prevProgressVal)
                        {
                            if (progressVal > 100)
                            {
                                progressVal = 100;
                            }
                            exportWorker.ReportProgress(progressVal);
                        }
                        progressVal = prevProgressVal;
                        currentIndex += buffer.Length;
                    }
                }

                using (BinaryReader binaryReader = new BinaryReader(File.Open(tmpFileName, FileMode.Open)))
                {
                    using (BinaryWriter bwr = new BinaryWriter(File.Open(exportedFileName, FileMode.Create)))
                    {
                        for (int ii = 0; ii < rawHeaderLength; ii++) {
                            by = binaryReader.ReadByte();
                            bwr.Write(by);
                        }

                        while ((binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                            && (syncroPosition != -1)
                        ) {
                            syncroPosition = this.findSyncro(binaryReader);
                            if (syncroPosition != -1)  {
                                buffer = binaryReader.ReadBytes(buffer.Length);
                                bwr.Write(buffer);
                            }
                        }
                    }
                }
            }
            CloseHandle(ptr);
        }

        private long findSyncro(BinaryReader binaryReader) {
            const int bufferSize = 519;
            byte[] buffer = new byte[bufferSize];

            while (!((buffer[0] == 255)
                    && (((int)buffer[1] & 240) == 112)
                    && (buffer[512] == 255)
                    && (((int)buffer[513] & 240) == 112)
                    && (((buffer[518] == (buffer[6]) - 1))
                        || ((buffer[518] == (buffer[6]) + 59))
                    )
                )
                && (binaryReader.BaseStream.Position <= binaryReader.BaseStream.Length - bufferSize)
            ) {
                buffer = binaryReader.ReadBytes(buffer.Length);
                binaryReader.BaseStream.Seek((bufferSize * -1) + 1, SeekOrigin.Current);
            }

            binaryReader.BaseStream.Seek(-1, SeekOrigin.Current);
            if ((binaryReader.BaseStream.Position > binaryReader.BaseStream.Length - bufferSize)) {
                return -1;
            }

            return binaryReader.BaseStream.Position;
        }

        private string exportRawFile(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum, 
            string flightDate,
            string exportedFileName)
        {
            string fileName = selectedFileName;
            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;
            byte by = 0;
            int rawHeaderLength = 128;

            double percent = 100 / (double)(endCopyInd - startCopyInd);

            int emptyByteCounter = 0;
            string newFile = exportedFileName.Split('.')[0] + ".raw";

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                using (BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create)))
                {
                    reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                    //rewrite header
                    for (int i = startCopyInd; i < rawHeaderLength + startCopyInd; i++)
                    {
                        by = reader.ReadByte();
                        bwr.Write(by);
                    }

                    int progressVal = 0,
                        prevProgressVal = 0;

                    for (int i = startCopyInd + rawHeaderLength; i < endCopyInd; i++)
                    {
                        by = reader.ReadByte();
                        bwr.Write(by);

                        if ((by == 255) || (by == 0))
                        {
                            emptyByteCounter++;
                            if (emptyByteCounter > 32)
                            {
                                i = endCopyInd;
                            }
                        }
                        else
                        {
                            emptyByteCounter = 0;
                        }

                        prevProgressVal = Convert.ToInt32(percent * (i - startCopyInd));
                        if(progressVal != prevProgressVal)
                        {
                            exportWorker.ReportProgress(progressVal);                        
                        }
                        progressVal = prevProgressVal;
                    }
                }
            }

            return newFile;
        }

        private void exportRawFileWithBitAppending(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum,
            string flightDate,
            string exportedFileName)
        {
            string fileName = selectedFileName;
            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;
            byte by = 0;

            double percent = 100 / (double)(endCopyInd - startCopyInd);

            int emptyByteCounter = 0;
            string newFile = exportedFileName;

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                using (BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create)))
                {
                    reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                    //rewrite header
                    for (int i = startCopyInd; i < headerLength + startCopyInd; i++)
                    {
                        by = reader.ReadByte();
                        bwr.Write(by);
                    }

                    int progressVal = 0,
                        prevProgressVal = 0;

                    for (int i = startCopyInd + headerLength; i < endCopyInd; i++)
                    {
                        by = reader.ReadByte();
                        bwr.Write(by);

                        if ((by == 255) || (by == 0))
                        {
                            emptyByteCounter++;
                            if (emptyByteCounter > 32)
                            {
                                i = endCopyInd;
                            }
                        }
                        else
                        {
                            emptyByteCounter = 0;
                        }

                        prevProgressVal = Convert.ToInt32(percent * (i - startCopyInd));
                        if (progressVal != prevProgressVal)
                        {
                            exportWorker.ReportProgress(progressVal);
                        }
                        progressVal = prevProgressVal;
                    }
                }
            }
        }

        private void exportA320File(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum,
            string flightDate,
            string exportedFileName)
        {
            FileInfo f = new FileInfo(selectedFileName);
            int selected = dataGridView.CurrentCell.RowIndex;
            int frameLenth = 768;
            int subFrameLenth = 192;
            string syncroWord = "001001000111";
            string syncroWord2 = "010110111000";
            
            byte by = 0;
            byte[] atomWords = new byte[4],
                atomWordsPrepared = new byte[4],
                atomWordsNextFrameSyncro = new byte[4],
                atomWordsSubSyncro = new byte[4];

            double percent = 100 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newFile = exportedFileName;

            using (BinaryReader reader = new BinaryReader(File.Open(selectedFileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create));
                reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                //rewrite header
                int sycroWordPosition = startCopyInd;
                for (int i = startCopyInd; i < headerLength + startCopyInd; i++)
                {
                    by = reader.ReadByte();
                    bwr.Write(by);
                    sycroWordPosition++;
                }

                int rotationType = 0;
                FindSyncroWord(frameLenth,
                    subFrameLenth, 
                    syncroWord, 
                    syncroWord2,
                    ref rotationType,
                    reader,
                    endCopyInd);

                byte[] frame = new byte[frameLenth + 4];
                string[] syncWordVariants;

                while (reader.BaseStream.Position < endCopyInd - frameLenth - 4)
                {
                    if (rotationType % 2 > 0)
                    {
                        rotationType--;

                        atomWords = reader.ReadBytes(4);
                        atomWords = rotateAtomWords(atomWords, rotationType);
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);

                        bwr.Write(atomWords[2]);
                        bwr.Write(atomWords[3]);

                        for (int i = 0; i < frameLenth - 3; )
                        {
                            atomWords = reader.ReadBytes(4);
                            i += 4;
                            atomWords = rotateAtomWords(atomWords, rotationType);
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            i -= 1;

                            foreach (byte b in atomWords)
                            {
                                bwr.Write(b);
                            }
                        }

                        atomWords = reader.ReadBytes(4);
                        atomWords = rotateAtomWords(atomWords, rotationType);
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);

                        bwr.Write(atomWords[0]);
                        bwr.Write(atomWords[1]);

                        reader.BaseStream.Seek(-1, SeekOrigin.Current);       
                    }
                    else
                    {
                        frame = reader.ReadBytes(frameLenth + 4);
                        reader.BaseStream.Seek(-4, SeekOrigin.Current);

                        Array.Copy(frame, 0, atomWords, 0, 4);
                        atomWordsPrepared = atomWords;
                        syncWordVariants = rotateSyncroWord(atomWordsPrepared);

                        if (syncWordVariants[rotationType] == syncroWord)
                        {
                            for (int ind = 0; ind < frameLenth; )
                            {
                                Array.Copy(frame, ind, atomWords, 0, 4);
                                ind += 3;

                                atomWords = rotateAtomWords(atomWords, rotationType);

                                foreach (byte b in atomWords)
                                {
                                    bwr.Write(b);
                                }
                            }
                        }
                        else
                        {
                            rotationType = 0;
                            FindSyncroWord(frameLenth,
                                subFrameLenth,
                                syncroWord,
                                syncroWord2,
                                ref rotationType,
                                reader,
                                endCopyInd);
                        }                    
                    }
                   


                    /*if (((atomWords[0] == 0) && (atomWords[1] == 0) && (atomWords[2] == 0) && (atomWords[3] == 0)) ||
                        ((atomWords[0] == 255) && (atomWords[1] == 255) && (atomWords[2] == 255) && (atomWords[3] == 255)))
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
                    }*/

                    prevProgressVal = Convert.ToInt32(percent * (reader.BaseStream.Position - startCopyInd));
                    if (progressVal != prevProgressVal)
                    {
                        exportWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
                bwr.Close();
            }
        }

        private string removeHeadersA320CompFlashFile(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum, 
            string flightDate)
        {
            string fileName = selectedFileName;
            int a320CFblockLength = 8192;
            int a320CFblockWithoutHeaderLength = 8160;
            int a320CFheaderLength = 32;
            byte[] blockWithHeader = new byte[a320CFblockLength];
            byte[] cleanBlock = new byte[a320CFblockWithoutHeaderLength];
            byte[] hea = new byte[a320CFheaderLength];

            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;

            double percent = 50 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newTmpFile = createNewFile(flightDate, Convert.ToString(flightNum), "tmp");

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newTmpFile, FileMode.Create));
                reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                reader.Read(blockWithHeader, 0, blockWithHeader.Length);
                bwr.Write(blockWithHeader);

                bool endFlight = false;
                while(!endFlight && (reader.BaseStream.Position < endCopyInd))
                {
                    //reader.Read(hea, 0, hea.Length);
                    reader.BaseStream.Seek(a320CFheaderLength, SeekOrigin.Current);
                    reader.Read(cleanBlock, 0, cleanBlock.Length);
                    bwr.Write(cleanBlock);

                    prevProgressVal = Convert.ToInt32(percent * (reader.BaseStream.Position - startCopyInd));
                    if (progressVal != prevProgressVal)
                    {
                        exportWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
                bwr.Close();
            }

            return newTmpFile;
        }

        private void exportA320CompFlashFile(string tmpParamFileName,
            string exportedFileName)
        {
            FileInfo f = new FileInfo(tmpParamFileName);
            int selected = dataGridView.CurrentCell.RowIndex;
            int frameLenth = 768;
            int subFrameLenth = 192;
            int a320CFheaderLength = 32;
            string syncroWord = "001001000111";
            string syncroWord2 = "010110111000";

            byte by = 0;
            byte[] atomWords = new byte[4],
                atomWordsPrepared = new byte[4],
                atomWordsNextFrameSyncro = new byte[4],
                atomWordsSubSyncro = new byte[4];

            double percent = 50 / (double)f.Length;
            int endCopyInd = (int)f.Length;
            int progressVal = 0,
                prevProgressVal = 0;

            string newFile = exportedFileName;

            using (BinaryReader reader = new BinaryReader(File.Open(tmpParamFileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create));

                //rewrite header
                int sycroWordPosition = 0;
                for (int i = 0; i < a320CFheaderLength; i++)
                {
                    by = reader.ReadByte();
                    bwr.Write(by);
                    sycroWordPosition++;
                }

                int rotationType = 0;
                FindSyncroWord(frameLenth,
                    subFrameLenth,
                    syncroWord,
                    syncroWord2,
                    ref rotationType,
                    reader,
                    endCopyInd);

                byte[] frame = new byte[frameLenth + 4];
                string[] syncWordVariants;

                while (reader.BaseStream.Position < endCopyInd - frameLenth - 4)
                {
                    if (rotationType % 2 > 0)
                    {
                        rotationType--;

                        atomWords = reader.ReadBytes(4);
                        atomWords = rotateAtomWords(atomWords, rotationType);
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);

                        bwr.Write(atomWords[2]);
                        bwr.Write(atomWords[3]);

                        for (int i = 0; i < frameLenth - 3; )
                        {
                            atomWords = reader.ReadBytes(4);
                            i += 4;
                            atomWords = rotateAtomWords(atomWords, rotationType);
                            reader.BaseStream.Seek(-1, SeekOrigin.Current);
                            i -= 1;

                            foreach (byte b in atomWords)
                            {
                                bwr.Write(b);
                            }
                        }

                        atomWords = reader.ReadBytes(4);
                        atomWords = rotateAtomWords(atomWords, rotationType);
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);

                        bwr.Write(atomWords[0]);
                        bwr.Write(atomWords[1]);

                        reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    }
                    else
                    {
                        frame = reader.ReadBytes(frameLenth + 4);
                        reader.BaseStream.Seek(-4, SeekOrigin.Current);

                        Array.Copy(frame, 0, atomWords, 0, 4);
                        atomWordsPrepared = atomWords;
                        syncWordVariants = rotateSyncroWord(atomWordsPrepared);

                        if (syncWordVariants[rotationType] == syncroWord)
                        {
                            for (int ind = 0; ind < frameLenth; )
                            {
                                Array.Copy(frame, ind, atomWords, 0, 4);
                                ind += 3;

                                atomWords = rotateAtomWords(atomWords, rotationType);

                                foreach (byte b in atomWords)
                                {
                                    bwr.Write(b);
                                }
                            }
                        }
                        else
                        {
                            rotationType = 0;
                            FindSyncroWord(frameLenth,
                                subFrameLenth,
                                syncroWord,
                                syncroWord2,
                                ref rotationType,
                                reader,
                                endCopyInd);
                        }
                    }



                    /*if (((atomWords[0] == 0) && (atomWords[1] == 0) && (atomWords[2] == 0) && (atomWords[3] == 0)) ||
                        ((atomWords[0] == 255) && (atomWords[1] == 255) && (atomWords[2] == 255) && (atomWords[3] == 255)))
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
                    }*/

                    prevProgressVal = 50 + Convert.ToInt32(percent * (reader.BaseStream.Position));
                    if (progressVal != prevProgressVal)
                    {
                        exportWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
                bwr.Close();
            }
        }

        private void FindSyncroWord(int frameLenth,
            int subFrameLenth, 
            string syncroWord, 
            string syncroWord2,
            ref int rotationType,
            BinaryReader reader,
            int endCopyInd)
        {
            byte by = 0;
            byte[] atomWords = new byte[4],
                atomWordsPrepared = new byte[4],
                atomWordsNextFrameSyncro = new byte[4],
                atomWordsSubSyncro = new byte[4];

            string bitString = string.Empty, tempBitString = string.Empty;
            bool syncroWordFound = false;
            byte[] frame = new byte[frameLenth + 1];

            by = reader.ReadByte();
            atomWords[0] = by;

            by = reader.ReadByte();
            atomWords[1] = by;

            by = reader.ReadByte();
            atomWords[2] = by;

            string[] syncWordVariants, subSyncWordVariants;

            while (!syncroWordFound && (reader.BaseStream.Position < (endCopyInd - frameLenth)))
            {
                by = reader.ReadByte();
                atomWords[3] = by;

                syncWordVariants = rotateSyncroWord(atomWords);

                //array shift
                atomWords[0] = atomWords[1];
                atomWords[1] = atomWords[2];
                atomWords[2] = atomWords[3];

                rotationType = 0;

                for (int j = 0; j < syncWordVariants.Length; j++)
                {
                    if (syncWordVariants[j] == syncroWord)
                    {
                        frame = reader.ReadBytes(frameLenth);

                        atomWordsNextFrameSyncro[0] = frame[frame.Length - 4];
                        atomWordsNextFrameSyncro[1] = frame[frame.Length - 3];
                        atomWordsNextFrameSyncro[2] = frame[frame.Length - 2];
                        atomWordsNextFrameSyncro[3] = frame[frame.Length - 1];

                        atomWordsSubSyncro[0] = frame[subFrameLenth - 4];
                        atomWordsSubSyncro[1] = frame[subFrameLenth - 3];
                        atomWordsSubSyncro[2] = frame[subFrameLenth - 2];
                        atomWordsSubSyncro[3] = frame[subFrameLenth - 1];

                        syncWordVariants = rotateSyncroWord(atomWordsNextFrameSyncro);
                        subSyncWordVariants = rotateSyncroWord(atomWordsSubSyncro);

                        if ((syncWordVariants[j] == syncroWord) && (subSyncWordVariants[j] == syncroWord2))
                        {
                            syncroWordFound = true;
                            reader.BaseStream.Seek(-(frame.Length + 4), SeekOrigin.Current);
                            rotationType = j;
                            break;
                        }
                        else
                        {
                            reader.BaseStream.Seek(-(frame.Length), SeekOrigin.Current);
                        }
                    }
                }
            }
        }

        private void exportL39File(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum,
            string flightDate,
            string exportedFileName)
        {
            string fileName = selectedFileName;

            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;
            byte byParam = 0;
            byte [] byArr = new byte [5];
            int frameByteCounter = 0;
            int frameLength = 64;

            double percent = 100 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newFile = exportedFileName;
            string newWavFile = exportedFileName.Split('.')[0] + ".wav";

            int emptyByteCounter = 0;
            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                using (BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create)))
                {
                    //using (BinaryWriter bwrWav = new BinaryWriter(File.Open(newWavFile, FileMode.Create)))
                    //{
                        reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                        for (int i = startCopyInd; i < startCopyInd + headerLength; i++)
                        {
                            byParam = reader.ReadByte();
                            bwr.Write(byParam);
                        }

                        bool syncroWordFound = false;
                        while (!syncroWordFound && (reader.BaseStream.Position < endCopyInd))
                        {
                            byArr = reader.ReadBytes(5);
                            if ((byArr[0] == 255) && (byArr[2] == 127) && (byArr[4] == 255))
                            {
                                syncroWordFound = true;
                            }
                        }

                        bwr.Write(byArr[3]);
                        //bwrWav.Write(byArr[4]);

                        UInt32 chunkLength = (UInt32)((endCopyInd - startCopyInd - headerLength) / 2);
                        //writeWavHeader(bwrWav, chunkLength);

                        bool endCopy = false;
                        while (!endCopy && (reader.BaseStream.Position < endCopyInd))
                        {
                            if (frameByteCounter < frameLength)
                            {
                                byParam = reader.ReadByte();
                                reader.BaseStream.Seek(1, SeekOrigin.Current);
                                bwr.Write(byParam);
                                frameByteCounter++;
                            }
                            else
                            {
                                frameByteCounter = 0;
                                reader.BaseStream.Seek(frameLength * 2 * 16, SeekOrigin.Current);
                            }

                            //byWav = reader.ReadByte();
                            //bwrWav.Write(byWav);

                            if ((byParam == 255) || (byParam == 0))
                            {
                                emptyByteCounter++;
                                if (emptyByteCounter >= 32)
                                {
                                    if (reader.BaseStream.Position < endCopyInd - 5)
                                    {
                                        syncroWordFound = false;
                                        while (!syncroWordFound && (reader.BaseStream.Position < endCopyInd))
                                        {
                                            byArr = reader.ReadBytes(5);
                                            if ((byArr[0] == 255) && (byArr[2] == 127) && (byArr[4] == 255))
                                            {
                                                syncroWordFound = true;
                                            }
                                        }
                                    }

                                    if (syncroWordFound)
                                    {
                                        bwr.Seek(-2, SeekOrigin.Current);
                                        bwr.Write(byArr[3]);
                                        //bwrWav.Seek(-32, SeekOrigin.Current);
                                        //bwrWav.Write(byArr[4]);
                                    }
                                    else
                                    {
                                        endCopy = true;
                                    }
                                }
                            }
                            else
                            {
                                emptyByteCounter = 0;
                            }

                            prevProgressVal = Convert.ToInt32(percent * (reader.BaseStream.Position - startCopyInd));
                            if (progressVal != prevProgressVal)
                            {
                                exportWorker.ReportProgress(progressVal);
                            }
                            progressVal = prevProgressVal;
                        }
                    //}
                }
            }
        }

        private string exportParamSaab340File(string selectedFileName, 
            int startCopyInd, 
            int endCopyInd, 
            int flightNum, 
            string flightDate)
        {
            string fileName = selectedFileName;

            FileInfo f = new FileInfo(fileName);
            int fileLength = (int)f.Length;
            byte by = 0;

            double percent = 50 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newTmpFile = createNewFile(flightDate, Convert.ToString(flightNum), "tmp");

            using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newTmpFile, FileMode.Create));
                reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                //rewrite header
                for (int i = startCopyInd; i < headerLength + startCopyInd; i++)
                {
                    by = reader.ReadByte();
                    bwr.Write(by);
                }

                bool endFlight = false;
                while(!endFlight && (reader.BaseStream.Position < endCopyInd))
                {
                    by = reader.ReadByte();
                    if (by == 255)
                    {
                        by = reader.ReadByte();
                        bwr.Write(by);
                    }

                    /*if ((by == 0) || (by == 255))
                    {
                        emptyByteCounter++;
                        if (emptyByteCounter > 16)
                        {
                            endFlight = true;
                        }
                    }
                    else
                    {
                        emptyByteCounter = 0;
                    }*/

                    prevProgressVal = Convert.ToInt32(percent * (reader.BaseStream.Position - startCopyInd));
                    if (progressVal != prevProgressVal)
                    {
                        exportWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
                bwr.Close();
            }

            return newTmpFile;
        }

        private void checkSaab340File(string tmpParamFileName,
            string exportedFileName)
        {
            FileInfo f = new FileInfo(tmpParamFileName);
            int selected = dataGridView.CurrentCell.RowIndex;
            int frameLenth = 384;
            int subFrameLenth = 96;
            string syncroWord = "001001000111";
            string syncroWord2 = "010110111000";
            
            int startCopyInd = 0;
            int endCopyInd = (int)f.Length;
            byte by = 0;
            byte[] atomWords = new byte[4],
                atomWordsNextFrameSyncro = new byte[4],
                atomWordsSubSyncro = new byte[4];

            double percent = 50 / (double)(endCopyInd - startCopyInd);
            int progressVal = 0,
                prevProgressVal = 0;

            string newFile = exportedFileName;

            using (BinaryReader reader = new BinaryReader(File.Open(tmpParamFileName, FileMode.Open)))
            {
                BinaryWriter bwr = new BinaryWriter(File.Open(newFile, FileMode.Create));
                reader.BaseStream.Seek(startCopyInd, SeekOrigin.Begin);

                //rewrite header
                int sycroWordPosition = startCopyInd;
                for (int i = startCopyInd; i < headerLength + startCopyInd; i++)
                {
                    by = reader.ReadByte();
                    bwr.Write(by);
                    sycroWordPosition++;
                }

                string bitString = string.Empty, tempBitString = string.Empty;
                bool syncroWordFound = false;
                byte[] frame = new byte[frameLenth + 1];

                int rotationType = 0;

                by = reader.ReadByte();
                atomWords[0] = by;
                sycroWordPosition++;
                
                by = reader.ReadByte();
                atomWords[1] = by;
                sycroWordPosition++;

                by = reader.ReadByte();
                atomWords[2] = by;
                sycroWordPosition++;

                string [] syncWordVariants, subSyncWordVariants;

                while (!syncroWordFound && (reader.BaseStream.Position < (endCopyInd - frameLenth)))
                {
                    by = reader.ReadByte();
                    atomWords[3] = by;
                    sycroWordPosition++;

                    syncWordVariants = rotateSyncroWord(atomWords);

                    //array shift
                    atomWords[0] = atomWords[1];
                    atomWords[1] = atomWords[2];
                    atomWords[2] = atomWords[3];
                    
                    rotationType = 0;

                    for (int j = 0; j < syncWordVariants.Length; j++)
                    {
                        if (syncWordVariants[j] == syncroWord)
                        {
                            frame = reader.ReadBytes(frameLenth);

                            atomWordsNextFrameSyncro[0] = frame[frame.Length - 4];
                            atomWordsNextFrameSyncro[1] = frame[frame.Length - 3];
                            atomWordsNextFrameSyncro[2] = frame[frame.Length - 2];
                            atomWordsNextFrameSyncro[3] = frame[frame.Length - 1];

                            atomWordsSubSyncro[0] = frame[subFrameLenth - 4];
                            atomWordsSubSyncro[1] = frame[subFrameLenth - 3];
                            atomWordsSubSyncro[2] = frame[subFrameLenth - 2];
                            atomWordsSubSyncro[3] = frame[subFrameLenth - 1];

                            syncWordVariants = rotateSyncroWord(atomWordsNextFrameSyncro);
                            subSyncWordVariants = rotateSyncroWord(atomWordsSubSyncro);

                            if ((syncWordVariants[j] == syncroWord) && (subSyncWordVariants[j] == syncroWord2))
                            {
                                syncroWordFound = true;
                                reader.BaseStream.Seek(-(frame.Length + 4), SeekOrigin.Current);
                                rotationType = j;
                                break;
                            }
                            else
                            {
                                reader.BaseStream.Seek(-(frame.Length), SeekOrigin.Current);  
                            }
                        }
                    }
                }

                //if syncroword found in second subword use previous rotation type and loose 2 bytes
                if (rotationType % 2 > 0)
                {
                    rotationType--;

                    atomWords = reader.ReadBytes(4);
                    atomWords = rotateAtomWords(atomWords, rotationType);
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);

                    bwr.Write(atomWords[2]);
                    bwr.Write(atomWords[3]);
                    
                }

                for (int i = sycroWordPosition + 2; i < endCopyInd - 3;)
                {
                    atomWords = reader.ReadBytes(4);
                    i += 4;
                    atomWords = rotateAtomWords(atomWords, rotationType);
                    reader.BaseStream.Seek(-1, SeekOrigin.Current);
                    i -= 1;

                    foreach (byte b in atomWords)
                    {
                        bwr.Write(b); 
                    }

                    /*if (((atomWords[0] == 0) && (atomWords[1] == 0) && (atomWords[2] == 0) && (atomWords[3] == 0)) ||
                        ((atomWords[0] == 255) && (atomWords[1] == 255) && (atomWords[2] == 255) && (atomWords[3] == 255)))
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
                    }*/

                    prevProgressVal = Convert.ToInt32(50 + (percent * (reader.BaseStream.Position - startCopyInd)));
                    if (progressVal != prevProgressVal)
                    {
                        exportWorker.ReportProgress(progressVal);
                    }
                    progressVal = prevProgressVal;
                }
                bwr.Close();
            }

            //File.Delete(tmpParamFileName);
        }

        private byte[] rotateAtomWords(byte[] atomWords, int type)
        {
            byte[] rotatedAtomWords = new byte[atomWords.Length];
            string [] middleAtomWords;
            string tempBitString1, tempBitString2;

            middleAtomWords = rotateSyncroWord(atomWords);

            tempBitString1 = "0000" + middleAtomWords[type];
            tempBitString2 = "0000" + middleAtomWords[type + 1];

            rotatedAtomWords[0] = Convert.ToByte(tempBitString1.Substring(8, 8), 2);
            rotatedAtomWords[1] = Convert.ToByte(tempBitString1.Substring(0, 8), 2);
            rotatedAtomWords[2] = Convert.ToByte(tempBitString2.Substring(8, 8), 2);
            rotatedAtomWords[3] = Convert.ToByte(tempBitString2.Substring(0, 8), 2);

            return rotatedAtomWords;
        }

        private string[] rotateSyncroWord(byte[] atomWords)
        {
            string[] normWord = new String[8];
            string bitString = String.Empty;

            for (int i = 0; i < atomWords.Length; i++)
            {
                bitString += Convert.ToString(atomWords[i], 2).PadLeft(8, '0');
            }

            //3/5/8/7/1
            normWord[0] = bitString.Substring(23, 1) + bitString.Substring(8, 8) + bitString.Substring(0, 3);
            normWord[1] = bitString.Substring(27, 5) + bitString.Substring(16, 7);

            //5/3/1/7/8
            normWord[2] = bitString.Substring(9, 7) + bitString.Substring(0, 5);
            normWord[3] = bitString.Substring(29, 3) + bitString.Substring(16, 8) + bitString.Substring(8, 1);

            //8/4/4/8
            normWord[4] = bitString.Substring(12, 4) + bitString.Substring(0, 8);
            normWord[5] = bitString.Substring(16, 8) + bitString.Substring(8, 4);

            //6/2/2/6/8
            normWord[6] = bitString.Substring(10, 6) + bitString.Substring(0, 6);
            normWord[7] = bitString.Substring(30, 2) + bitString.Substring(16, 8) + bitString.Substring(8, 2);

            return normWord;  
        }

        private string[] rotateSyncroWord2(byte[] atomWords)
        {
            string[] normWord = new String[8];
            string bitString = String.Empty;

            for (int i = 0; i < atomWords.Length; i++)
            {
                bitString += Convert.ToString(atomWords[i], 2).PadLeft(8, '0');
            }

            //3/5/8/7/1
            normWord[0] = bitString.Substring(23, 1) + bitString.Substring(8, 8) + bitString.Substring(0, 3);
            normWord[1] = bitString.Substring(16, 7) + bitString.Substring(27, 5);

            //5/3/1/7/8
            normWord[2] = bitString.Substring(9, 7) + bitString.Substring(0, 5);
            normWord[3] = bitString.Substring(29, 3) + bitString.Substring(16, 8) + bitString.Substring(8, 1);

            //8/4/4/8
            normWord[4] = bitString.Substring(12, 4) + bitString.Substring(0, 8);
            normWord[5] = bitString.Substring(16, 8) + bitString.Substring(8, 4);

            //6/2/2/6/8
            normWord[6] = bitString.Substring(10, 6) + bitString.Substring(0, 6);
            normWord[7] = bitString.Substring(30, 2) + bitString.Substring(16, 8) + bitString.Substring(8, 2);

            return normWord;
        }

        private void writeWavHeader(BinaryWriter bw, UInt32 chunkLength)
        {
            // WAV-формат начинается с RIFF-заголовка:

            // Содержит символы "RIFF" в ASCII кодировке
            // (0x52494646 в big-endian представлении)
            bw.Write("RIFF");
            // 36 + subchunk2Size, или более точно:
            // 4 + (8 + subchunk1Size) + (8 + subchunk2Size)
            // Это оставшийся размер цепочки, начиная с этой позиции.
            // Иначе говоря, это размер файла - 8, то есть,
            // исключены поля chunkId и chunkSize.
            bw.Write((UInt32)(chunkLength + 36));
            // Содержит символы "WAVE"
            // (0x57415645 в big-endian представлении)
            bw.Write("WAVE");
            // Формат "WAVE" состоит из двух подцепочек: "fmt " и "data":
            // Подцепочка "fmt " описывает формат звуковых данных:

            // Содержит символы "fmt "
            // (0x666d7420 в big-endian представлении)
            bw.Write("fmt ");
            // 16 для формата PCM.
            // Это оставшийся размер подцепочки, начиная с этой позиции.
            bw.Write((UInt32)16);

            // Аудио формат, полный список можно получить здесь http://audiocoding.ru/wav_formats.txt
            // Для PCM = 1 (то есть, Линейное квантование).
            // Значения, отличающиеся от 1, обозначают некоторый формат сжатия.
            bw.Write((UInt16)1);

            // Количество каналов. Моно = 1, Стерео = 2 и т.д.
            bw.Write((UInt16)1);

            // Частота дискретизации. 8000 Гц, 44100 Гц и т.д.
            bw.Write((UInt32)8000);

            // sampleRate * numChannels * bitsPerSample/8
            bw.Write((UInt32)1);

            // numChannels * bitsPerSample/8
            // Количество байт для одного сэмпла, включая все каналы.
            bw.Write((UInt16)1);

            // Так называемая "глубиная" или точность звучания. 8 бит, 16 бит и т.д.
            bw.Write((UInt16)8);

            // Подцепочка "data" содержит аудио-данные и их размер.

            // Содержит символы "data"
            // (0x64617461 в big-endian представлении)
            bw.Write("data");

            // numSamples * numChannels * bitsPerSample/8
            // Количество байт в области данных.
            bw.Write(chunkLength);

            // Далее следуют непосредственно Wav данные.
        }

    }
}
