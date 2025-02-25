namespace BorisMobile.Utilities
{
    public class IO
    {
        public static byte[] ReadFully(Stream stream)
        {
            // Reliable way of reading full contents of file (doing a simple Read(wholeLength) won't work because it's only guaranteed to return however many bytes it said it read)
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public async static Task<byte[]> ReadFullFile(string fullFileName)
        {
            using (FileStream fs = new FileStream(fullFileName, FileMode.Open, FileAccess.Read))
            {
                return ReadFully(fs);
            }
        }

        public static void DumpWithTimestampToLogFile(string text)
        {
//#if !ClientBuild
//            string logFile = ConfigurationManager.AppSettings["logFile"];
//            if (!string.IsNullOrEmpty(logFile))
//            {
//                DumpToFile(logFile, DateTime.Now + "." + DateTime.Now.Millisecond + " " + text);
//            }
//#endif
        }

        public static void WriteMemoryStreamToFile(MemoryStream memoryStream, string fileName)
        {
            // 260118 Passive Fire Pro. 1.8GB PDF seems to get corrupted. Use the simple .NET method instead
            EnsureDirectoryOfFileExists(fileName);
            DumpWithTimestampToLogFile("WriteMemoryStreamToFile " + memoryStream.Length);
            System.IO.File.WriteAllBytes(fileName, memoryStream.GetBuffer());
            FileInfo info = new FileInfo(fileName);
            DumpWithTimestampToLogFile("Dumped to file with length " + info.Length);

            //WriteByteArrayToFile(fileName, memoryStream.GetBuffer(), (int)memoryStream.Length);
        }

        public static void WriteStreamToFile(System.IO.Stream stream, string fileName)
        {
            using (FileStream wrtr = new FileStream(fileName, FileMode.Create))
            {
                // Allocate byte buffer to hold stream contents
                byte[] inData = new byte[4096];
                int bytesRead = stream.Read(inData, 0, inData.Length);
                while (bytesRead > 0)
                {
                    wrtr.Write(inData, 0, bytesRead);
                    bytesRead = stream.Read(inData, 0, inData.Length);
                }
            }
        }
        public static void WriteByteArrayToFile(string fileName, byte[] bytes)
        {
            // 260118 Replaces the method below which seems to fail with very large files
            // 260118 Passive Fire Pro. 1.8GB PDF seems to get corrupted. Use the simple .NET method instead
            EnsureDirectoryOfFileExists(fileName);
            System.IO.File.WriteAllBytes(fileName, bytes);
        }

        public static void WriteByteArrayToFile(string fileName, byte[] bytes, int length)
        {
            // 260118 Passive Fire Pro. 1.8GB PDF seems to get corrupted. Aim not to use this method any more
            EnsureDirectoryOfFileExists(fileName);
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create))
            {
                fileStream.Write(bytes, 0, length);
            }
        }


        public static void WriteFileToStream(Stream stream, string outputFile)
        {
            // Allocate byte buffer to hold stream contents
            byte[] buffer = new byte[32768];
            using (FileStream fs = new FileStream(outputFile, FileMode.Open, FileAccess.Read))
            {
                while (true)
                {
                    int read = fs.Read(buffer, 0, buffer.Length);
                    if (read <= 0) // 1 byte is always returned unless we're at the end (.NET docs)
                        return;
                    stream.Write(buffer, 0, read);
                }
            }
        }

        public static void CreateOrOverwriteFile(string fullFileName, string content)
        {
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }
            EnsureDirectoryOfFileExists(fullFileName);
            using (StreamWriter sw = System.IO.File.CreateText(fullFileName))
            {
                sw.Write(content);
                sw.Flush();
            }
        }

        public static void EnsureDirectoryExists(string dirName)
        {
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }
        }

        public static void EnsureDirectoryOfFileExists(string fileName)
        {
            EnsureDirectoryExists(Path.GetDirectoryName(fileName));
        }

        public static void SafeDirectoryContentsDeleteLeaveStructure(string folder)
        {
            try
            {
                // If we delete the Temp folder the ASP.NET session gets restarted! We should be able to delete just the files though.
                if (Directory.Exists(folder))
                {
                    string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        if (!Directory.Exists(file)) // check that it's not a folder
                        {
                            IO.SafeFileDelete(file);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SafeDirectoryContentsDeleteIncludingStructureNotForWeb(string folder)
        {
            try
            {
                // If we delete the Temp folder the ASP.NET session gets restarted! We should be able to delete just the files though.
                Directory.Delete(folder, true);
            }
            catch (Exception)
            {
            }
        }

        public static void SafeFileDelete(string fileName)
        {
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
            }
            catch (Exception)
            {
            }
        }

        public static void MoveFileToSubFolder(string fullFileName, string newFolder)
        {
            string fileName = Path.GetFileName(fullFileName);
            string targetFile = Path.GetDirectoryName(fullFileName) + @"\" + newFolder + @"\" + fileName;
            EnsureDirectoryOfFileExists(targetFile);
            if (System.IO.File.Exists(targetFile))
            {
                System.IO.File.Delete(targetFile);
            }
            System.IO.File.Move(fullFileName, targetFile);
        }

        public static void DumpToFile(string fileName, string message)
        {
            try
            {
                using (StreamWriter sw = System.IO.File.AppendText(fileName))
                {
                    sw.WriteLine(message);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }
//#if MonoDroid
//        public static string CopyToExternalStorage(string sourceFullPathAndFileName)
//        {
//            try
//            {
//                string extStorage = Env.ExternalStorageDirectory.AbsolutePath; // NB. getExternalFilesDir is no use because it's tied to the app and gets deleted when the app is uninstalled
//                if (!string.IsNullOrEmpty(extStorage))
//                {
//                    byte[] source = IO.ReadFullFile(sourceFullPathAndFileName);
//                    string prefix = Path.GetFileNameWithoutExtension(sourceFullPathAndFileName);
//                    if (prefix.Length < 3) // get IllegalArgumentException in CreateTempFile because prefix needs to be 3 characters! didn't show up before because we didn't used to copy up.apk to the ext folder. This would break future upgrades
//                    {
//                        prefix = prefix + "pad";
//                    }
//                    Java.IO.File sdCardFile = Java.IO.File.CreateTempFile(prefix, Path.GetExtension(sourceFullPathAndFileName), Env.ExternalStorageDirectory);
//                    FileOutputStream fos = new FileOutputStream(sdCardFile);
//                    fos.Write(source);
//                    fos.Close();
//                    return sdCardFile.Path;
//                }
//            }
//            catch (Exception e)
//            {
//                Misc.DumpError("magic5", "CopyToExternalStorage exception " + e.ToString() + " on file " + sourceFullPathAndFileName);

//            }
//            return sourceFullPathAndFileName;
//        }
    }
}
