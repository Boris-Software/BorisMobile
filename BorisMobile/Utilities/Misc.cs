using System.Collections.ObjectModel;

namespace BorisMobile.Utilities
{
    public class Misc
    {
        public Misc()
        {
        }

        public static int ScaledInteger(int numerator, decimal multiplier)
        {
            decimal withDps = ((decimal)numerator) * (decimal)multiplier;
            return (int)(withDps + 0.5M); // division truncates the result, adding 0.5 means it's truncated correctly
        }

        public static int RoundUpOrDownDivision(int numerator, int denominator)
        {
            decimal withDps = ((decimal)numerator) / denominator;
            return (int)(withDps + 0.5M); // division truncates the result, adding 0.5 means it's truncated correctly
        }

        public static string ConvertDateTimeToISOString(DateTime dateTime)
        {
            return dateTime.ToString("s");
        }

        public static string ConvertDateTimeToDateOnlyString(DateTime dateTime)
        {
            return ConvertDateTimeToShortISO(dateTime).Substring(0, 10); // includes "-"s
        }

        public static DateTime ConvertISOStringToDateTime(string isoDateTime)
        {
            return DateTime.Parse(isoDateTime);
        }

        public static string ConvertISOStringToShortDateOnly(string isoDateTime)
        {
            DateTime dateTime = Misc.ConvertISOStringToDateTime(isoDateTime);
            return Misc.ConvertDateTimeToShortDateNoTime(dateTime);
        }

        public static string ConvertISOStringToShortDateTime(string isoDateTime)
        {
            DateTime dateTime = DateTime.Parse(isoDateTime);
            return ConvertDateTimeToShortDateAndTime(dateTime);
        }

        public static string ConvertDateTimeToShortDateAndTime(DateTime dateTime)
        {
            return ConvertDateTimeToShortDateNoTime(dateTime) + " " + dateTime.ToShortTimeString();
        }

        public static string ConvertDateTimeToShortISO(DateTime dateTime)
        {
            return ConvertDateTimeToISOString(dateTime);
            //return dateTime.ToString("yyyyMMddHHmm"); // !!!!! was yyMM... but caused Barry Bennett $timestamp date default to crash
        }

        public static string ConvertDateTimeToShortDateNoTime(DateTime dateTime)
        {
#if !MonoDroid
            return dateTime.ToShortDateString();
#else
            string shortDateString = dateTime.ToShortDateString();
            if (shortDateString.Length == 10) // it's including the year for some reason!!!!!
            {
                shortDateString = shortDateString.Substring(0, 6) + shortDateString.Substring(8, 2);
            }
            return shortDateString;
#endif
        }

        public static string ConvertDateTimeToLongDateNoTime(DateTime dateTime)
        {
            return dateTime.ToLongDateString();
        }

        public static string ConvertDateTimeToTimeOnlyWithSeconds(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        public static string ConvertDateTimeToTimeOnlyHHMM(DateTime dateTime)
        {
            return dateTime.ToString("HH:mm");
        }

        public static string QuoteString(string inString)
        {
            return ("\"" + inString + "\"");
        }

        public static string CreateTimeStampedFileName(string originalFileName)
        {
            string suffix = "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss", null);
            return ModifyFileNameBeforeExtension(originalFileName, suffix);
        }

        public static string CreateDateStampedFileName(string originalFileName)
        {
            string suffix = "_" + DateTime.Now.ToString("yyyyMMdd", null);
            return ModifyFileNameBeforeExtension(originalFileName, suffix);
        }

        public static string ModifyFileNameBeforeExtension(string originalFileName, string suffix)
        {
            int dot = originalFileName.LastIndexOf(".");
            if (dot != -1)
            {
                return originalFileName.Insert(dot, suffix);
            }
            return originalFileName += suffix;
        }

        public static void ResetLogFile(string fileName, bool archiveOld)
        {
            if (File.Exists(fileName))
            {
                if (archiveOld)
                {
                    string archiveFileName = Misc.CreateTimeStampedFileName(fileName);
                    File.Move(fileName, archiveFileName);
                }
                else
                {
                    IO.SafeFileDelete(fileName);
                }
            }
        }

        public static void RemoveOldLogFiles(string baseFileName, int maxAgeSeconds)
        {
            string dirName = Path.GetDirectoryName(baseFileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(baseFileName);
            Collection<string> filesToDelete = new Collection<string>();

            if (Directory.Exists(dirName))
            {
                foreach (string oneFile in Directory.GetFiles(dirName))
                {
                    if (oneFile.IndexOf(fileNameWithoutExtension) != -1)
                    {
                        int index = oneFile.IndexOf(fileNameWithoutExtension);
                        string bitAfter = oneFile.Substring(index + fileNameWithoutExtension.Length + 1);
                        int dot = bitAfter.LastIndexOf(".");
                        string dateBit = bitAfter.Substring(0, dot);
                        string isoDate = dateBit.Substring(0, 4) + "-" + dateBit.Substring(4, 2) + "-" + dateBit.Substring(6, 2) + "T" + dateBit.Substring(9, 2) + ":" + dateBit.Substring(11, 2);
                        DateTime dateTime;
#if !PocketPC
                        if (DateTime.TryParse(isoDate, out dateTime))
                        {
#else
                    dateTime = DateTime.Parse(isoDate);
#endif
                            TimeSpan ts = DateTime.Now - dateTime;
                            if (ts.TotalSeconds > maxAgeSeconds)
                            {
                                filesToDelete.Add(oneFile);
                            }
#if !PocketPC
                        }
#endif
                    }
                }
            }
            foreach (string oneDeletion in filesToDelete)
            {
                IO.SafeFileDelete(oneDeletion);
            }
        }

        public static string ExceptionToString(Exception ex)
        {
            string message = "";
            if (ex != null)
            {
                message += ex.Message;
                if (ex.StackTrace != null)
                {
                    message += "\n" + ex.StackTrace;
                }
                if (ex.InnerException != null)
                {
                    message += "\n" + ExceptionToString(ex.InnerException);
                }
            }
            return message;
        }

        public static void DumpError(string appName, string message)
        {
            DumpToFile(appName, CreateTimeStampedFileName("error.txt"), message);
        }

        public static void DumpError_Day(string appName, string message)
        {
            DumpToFile(appName, CreateDateStampedFileName("error.txt"), message);
        }

        public static void DumpTrace(string appName, string message)
        {
            DumpToFile(appName, "trace.txt", DateTime.Now + ": " + message);
        }

        private static void DumpToFile(string appName, string baseFile, string message)
        {
            try
            {
                string errorFile = GetOutputDirectory(appName, true) + Path.DirectorySeparatorChar + baseFile;
                using (StreamWriter sw = File.AppendText(errorFile))
                {
                    sw.WriteLine(message);
                    sw.Close();
                }
            }
            catch (Exception) { }
        }

        public static void DumpError(string appName, Exception ex)
        {
            DumpError(appName, ExceptionToString(ex));
        }

        public static string GetOutputDirectory(string appName)
        {
            return GetOutputDirectory(appName, true);
        }

        public static string GetOutputDirectory(string appName, bool useStandardDir)
        {
            return GetStandardDirectory(appName, useStandardDir, "Output");
        }

        public static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        public static string ConvertBytesToMegabytesDisplay(long bytes)
        {
            return ConvertBytesToMegabytes(bytes).ToString("0.00") + "MB";
        }

        public static string GetTempDirectory(string appName)
        {
            string tempDir = GetCurrentDirectory();
#if !MonoDroid && !MonoTouch
            return @"\Temp";
#elif MonoTouch
            return tempDir; 
#else

            // return GetAttachmentDirectory(appName); Nokia TECL hack

            // This is correct, line above a hack for one Nokia using TECL operative. Maybe Android 10 related as well.

            // https://stackoverflow.com/questions/46911486/xamarin-system-unauthorizedaccessexception-access-to-the-path-is-denied/46933816#46933816
            //return Env.ExternalStorageDirectory.AbsolutePath;
            throw new Exception("GetTempDirectory deprecated in Misc, use UIUtils.");
#endif
        }

        public static string GetTempPhotoFileName()
        {
            return "magic5.jpg";
        }
        public static string GetTempVideoFileName()
        {
            return "magic5.mp4";
        }

        public static byte[] GetUploadAttachmentData(byte[] dbBytes, string attachmentId, string rootFolder)
        {
            if (dbBytes.Length == 1 && dbBytes[0] == 0 && !string.IsNullOrEmpty(rootFolder))
            {
                string attachmentPath = FullPathForUploadAttachment(rootFolder, attachmentId);
                //if (!string.IsNullOrEmpty(attachmentPath)) // backed out just to be on the safe side && File.Exists(attachmentPath)) // GSI 11/11/15 - Could not find file  blah\14.jpg. Send back null to prevent a backlog. Could be dangerous if it keeps happening or starts happening because of another problem. // 151216, same thing happened again at GSI with 14.jpg!
                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath)) // backed out just to be on the safe side && File.Exists(attachmentPath)) // GSI 11/11/15 - Could not find file  blah\14.jpg. Send back null to prevent a backlog. Could be dangerous if it keeps happening or starts happening because of another problem. // 151216, same thing happened again at GSI with 14.jpg!                {
                {
                    return IO.ReadFullFile(attachmentPath);
                }
            }
            return dbBytes;
        }

        public static string FullPathForUploadAttachment(string rootFolder, string id)
        {
            if (!string.IsNullOrEmpty(rootFolder))
            {
                return string.Format("{0}{2}{1}.jpg", rootFolder, id, Path.DirectorySeparatorChar);
            }
            return null;
        }


        public static System.Drawing.Color GetColourFromColourName(string colourName)
        {
            return System.Drawing.Color.FromName(colourName);
        }

        // Switch to downloaded attachments from here

        public static string GetFullPathForAttachment(string appName, string fileName)
        {
            string fullFileName = GetAttachmentDirectory(appName) + Path.DirectorySeparatorChar + fileName;
#if MonoTouch
            return fullFileName;
#else
            return fullFileName.ToLower();
#endif
        }
        public static string GetSentryChaceDirectory(string appName)
        {
            return GetStandardDirectory(appName, true, "Sentry");
        }

        public static string GetAttachmentDirectory(string appName)
        {
            //return Path.Combine(GetCurrentDirectory(), "attachments");
            return GetAttachmentDirectory(appName, true);
        }

        public static string GetAttachmentDirectoryMAUI(string appName)
        {
            return Path.Combine(GetCurrentDirectory(), "attachments");
            //return GetAttachmentDirectory(appName, true);
        }

        public static string GetAttachmentDirectory(string appName, bool useStandardDir)
        {
            return GetStandardDirectory(appName, useStandardDir, "Attachments");
        }

        public static string GetUploadAttachmentDirectory(string appName)
        {
            return GetStandardDirectory(appName, true, "UploadAttachments");
        }

        private static string GetStandardDirectory(string appName, bool useStandardDir, string subDir)
        {
            //#if !MonoTouch
            //            string baseDir = useStandardDir ? GetRootDataFolder(appName) : Utilities.Environment.GetMyDocumentsFolder();
            //            baseDir += Path.DirectorySeparatorChar + subDir;
            //            baseDir = baseDir.ToLower();
            //#else
            //            string baseDir = GetCurrentDirectory();
            //            baseDir += "/Attachments";
            //#endif
            //            IO.EnsureDirectoryExists(baseDir);

            string baseDir = "";
            return baseDir;
        }

        //        private static string GetStandardDirectoryMAUI(string appName, bool useStandardDir, string subDir)
        //        {
        //#if !MonoTouch
        //            string baseDir = useStandardDir ? Environment.GetRootDataFolder(appName) : Utilities.Environment.GetMyDocumentsFolder();
        //            baseDir += Path.DirectorySeparatorChar + subDir;
        //            baseDir = baseDir.ToLower();
        //#else
        //            string baseDir = GetCurrentDirectory();
        //            baseDir +=  "/" + subDir;
        //#endif
        //            magic5.General.Utilities.IO.EnsureDirectoryExists(baseDir);
        //            return baseDir;
        //        }
        public static string GetConfigDirectory()
        {
            string configDir = GetCurrentDirectory();
#if !MonoDroid
            configDir += @"\Config";
#else
            configDir += "/config";
#endif
            return configDir;
        }

        public static string GetConfigDirectoryMAUI()
        {
            return Path.Combine(GetCurrentDirectory(), "config");
            //            string configDir = GetCurrentDirectory();
            //#if !MonoDroid
            //            configDir += @"\config";
            //#else
            //            configDir += "/config";
            //#endif
            //            return configDir;
        }

        public static string GetImagesDirectory()
        {
            string imageDir = GetCurrentDirectory();
#if !MonoDroid
            imageDir += @"\Images";
#else
            imageDir += "/images";
#endif
            return imageDir;
        }

        public static string GetImagesDirectoryMAUI()
        {
            return Path.Combine(GetCurrentDirectory(), "images");
            //            string imageDir = GetCurrentDirectory();
            //#if !MonoDroid
            //            imageDir += @"\Images";
            //#else
            //            imageDir += "/images";
            //#endif
            //            return imageDir;

        }
        public static string GetCurrentDirectory()
        {
#if PocketPC
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
#elif MonoDroid
            //return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location); ;
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); 
#elif MonoTouch
            return Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));
#else
            return Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
#endif
        }

        public static string[] SplitString(string s, string delimeter)
        {
            // http://social.msdn.microsoft.com/Forums/en-US/netfxcompact/thread/914a350f-e0e9-45e0-91a4-6b4b2168e780/
            if (s == null)
                throw new ArgumentNullException("StringToBeSplit is null.");
            if (delimeter == null)
                throw new ArgumentNullException("Delimeter is null.");

            int dsum = 0;
            int ssum = 0;
            int dl = delimeter.Length;
            int sl = s.Length;

            if (dl == 0 || sl == 0 || sl < dl)
                return new string[] { s };

            char[] cd = delimeter.ToCharArray();
            char[] cs = s.ToCharArray();
            List<string> retlist = new List<string>();
            for (int i = 0; i < dl; i++)
            {
                dsum += cd[i];
                ssum += cs[i];
            }

            int start = 0;
            for (int i = start; i < sl - dl; i++)
            {
                if (i >= start && dsum == ssum && s.Substring(i, dl) == delimeter)
                {
                    retlist.Add(s.Substring(start, i - start));
                    start = i + dl;
                }
                ssum += cs[i + dl] - cs[i];
            }
            if (dsum == ssum && s.Substring(sl - dl, dl) == delimeter)
            {
                retlist.Add(s.Substring(start, sl - dl - start));
                //retlist.Add("");
            }
            else
            {
                retlist.Add(s.Substring(start, sl - start));
            }
            return retlist.ToArray();
        }

        public static string[] StringArrayFromString(string inputText)
        {
            string[] results = null;
            if (inputText.IndexOf('\r') != -1)
            {
                results = inputText.Replace("\n", "").Split('\r');
            }
            else
            {
                results = inputText.Split('\n');
            }
            return results;
        }
    }
}
