using BorisMobile.Models;
using BorisMobile.Repository;
using BorisMobile.ViewModels;
using BorisMobile.Views;
using Microsoft.Data.Sqlite;
using SQLite;


#if ANDROID
using Android.Content.Res;
#elif IOS
using Foundation;
#endif


namespace BorisMobile
{
    public partial class App : Application
    {
        static SqliteConnection _connection;

        public static SqliteConnection DatabaseConnection
        {
            get
            {
                _connection = new SqliteConnection($"Data Source={Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "BorisSqlite.db3")}");
                _connection.Open();
                return _connection;
            }
        }

        static SQLiteAsyncConnection database;

        public static SQLiteAsyncConnection Database
        {
            get
            {
                if (database == null)
                {
                    database = new SQLiteAsyncConnection(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "BorisSqlite.db3"));

                    using (SqliteConnection cn = DatabaseConnection)
                    {
                        cn.Open();
                        using (SqliteCommand cmd = cn.CreateCommand())
                        {
                            RunSQL(cmd, @"CREATE TABLE Attachments(Id INTEGER PRIMARY KEY NOT NULL,
                                                                                IdGuid GUID NOT NULL,
                                                                                UniqueName TEXT NOT NULL,
                                                                                AttachmentData BLOB NOT NULL,
                                                                                Status INT NOT NULL,
                                                                                Repeat INT,
                                                                                FileName TEXT,
                                                                                SubFormIdGuid GUID,
                                                                                IsCopiedFromWorkOrder BIT,
                                                                                PageRepeatListItemId INT,
                                                                                PKGuid GUID
                                                                                )");

                            RunSQL(cmd, @"CREATE TABLE tblAuditTrailDetails(Id INTEGER PRIMARY KEY NOT NULL,
                                                                                IdGuid GUID NOT NULL,
                                                                                XmlResults TEXT,
                                                                                Modified DATETIME NOT NULL,
                                                                                AuditTrailReason INT NOT NULL
                                                                                )");
                        }
                    }

                    database.CreateTableAsync<Audits>();
                    database.CreateTableAsync<AuditsForCustomer>();
                    database.CreateTableAsync<AuditsForLocation>();
                    database.CreateTableAsync<AuditsInProgress>();
                    database.CreateTableAsync<AuditTrail>();
                    database.CreateTableAsync<Models.Contacts>();
                    database.CreateTableAsync<Customers>();
                    database.CreateTableAsync<CustomersForGroup>();
                    database.CreateTableAsync<DataOrganisations>();
                    database.CreateTableAsync<DataTransferParameters>();
                    database.CreateTableAsync<ExtensionChildren>();
                    database.CreateTableAsync<Models.Extensions>();
                    database.CreateTableAsync<GenericAttachments>();
                    database.CreateTableAsync<GenericListDefinitions>();
                    database.CreateTableAsync<GenericLists>();
                    database.CreateTableAsync<GroupsForUser>();
                    database.CreateTableAsync<LocalContacts>();
                    database.CreateTableAsync<LocalCustomers>();
                    database.CreateTableAsync<LocalListEntries>();
                    database.CreateTableAsync<LocalLocations>();
                    database.CreateTableAsync<Locations>();
                    database.CreateTableAsync<LocationsForUser>();
                    database.CreateTableAsync<ReleasedAudits>();
                    database.CreateTableAsync<Settings>();
                    database.CreateTableAsync<SimpleMessages>();
                    database.CreateTableAsync<SupportingWork>();
                    database.CreateTableAsync<Translations>();
                    database.CreateTableAsync<Users>();
                    database.CreateTableAsync<WorkOrderAttachments>();
                    database.CreateTableAsync<WorkOrderDefinitions>();
                    database.CreateTableAsync<WorkOrders>();
                    database.CreateTableAsync<Attachments>();
                }

                return database;
            }
        }
        private static void RunSQL(SqliteCommand cmd, string strSQL)
        {
            try
            {
                // Use INT (not INTEGER) to stop the PK from being an alias for the ROWID (http://www.sqlite.org/lang_createtable.html)
                cmd.CommandText = strSQL;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("App RunSQL: " + ex.Message);
            }
        }

        public App()
        {
            InitializeComponent();

            createAppFolders();
            //CopyRawFilesToAppFolderAsync();
            var db = Database;

            string provisionedMarkerFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "provisioned.dat");
            string authenticatedMarkerFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "authenticated.dat");

            //LaunchpadIssueCheck();
            //ChangeLaunchPadAsNeedtoDownload();
            //Preferences.Set("DataTransferCompleted", false);
            if (!File.Exists(provisionedMarkerFile) && !(File.Exists(authenticatedMarkerFile)))
            {
                MainPage = new NavigationPage(new CustomerCodePage(new CustomerCodePageViewModel()));
            }
            else
            {
                MainPage = new NavigationPage(new HomePage(new HomePageViewModel()));
            }
        }

        //private async Task LaunchpadIssueCheck()
        //{
        //    try
        //    {
        //        //launchpad.xml.tmp
        //        string m_attachmentsDir = Misc.GetAttachmentDirectoryMAUI(Helper.Constants.APP_NAME);
        //        var sourceFile = Path.Combine(m_attachmentsDir, "launchpad.xml.tmp");
        //        var destinationFilePath = Path.Combine(Misc.GetImagesDirectoryMAUI(), "launchpad.xml");

        //        if (!File.Exists(destinationFilePath))
        //        {
        //            if (Path.GetExtension(sourceFile) == ".xml.tmp")
        //            {
        //                string tempContent = File.ReadAllText(sourceFile);
        //                Debug.WriteLine("Temp XML Content: " + sourceFile + " " + tempContent);
        //                using var stream = File.OpenRead(sourceFile);

        //                using var destStream = File.Create(destinationFilePath);

        //                stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
        //                await stream.CopyToAsync(destStream);
        //                await destStream.FlushAsync(); // Ensure all data is written
        //            }
        //            else
        //            {
        //                using var stream = File.OpenRead(sourceFile);

        //                using var destStream = File.Create(destinationFilePath);

        //                stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
        //                await stream.CopyToAsync(destStream);
        //                await destStream.FlushAsync(); // Ensure all data is written
        //            }

        //            //using (Stream s = File.Create(destinationfile))
        //            //{
        //            //    stream.CopyTo(s);
        //            //}
        //        }
        //        else
        //        {
        //            IO.SafeFileDelete(destinationFilePath);
        //            File.Move(sourceFile, destinationFilePath);
        //            //string tempContent = File.ReadAllText(sourceFile);
        //            //Debug.WriteLine("Temp XML Content: " + sourceFile + " " + tempContent);
        //            //using var stream = File.OpenRead(sourceFile);

        //            //using var destStream = File.Create(destinationFilePath);

        //            //stream.Seek(0, SeekOrigin.Begin); // Ensure stream starts from beginning
        //            //await stream.CopyToAsync(destStream);
        //            //await destStream.FlushAsync(); // Ensure all data is written

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.ToString());
        //    }
        //}
        public async void ChangeLaunchPadAsNeedtoDownload()
        {
            IRepo<GenericAttachments> attachmentRepo = new Repo<GenericAttachments>(App.Database);
            var res = await attachmentRepo.Get();
            if (res != null)
            {
                //var launchList = res.Where(x => x.ShortFileName == "launchpad.xml").ToList();
                foreach (var item in res)
                {
                    item.NeedToDownload = 1;
                    await attachmentRepo.Update(item);
                }
            }
        }

        public async void createAppFolders()
        {
            try
            {
                //var m_currentDirectory = Misc.GetCurrentDirectory();
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "attachments")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "attachments"));
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "config")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "config"));
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "images")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "images"));

                await CopyRawFilesToAppFolderAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async Task CopyRawFilesToAppFolderAsync()
        {
            var m_attachmentsDir = Path.Combine(FileSystem.AppDataDirectory, "attachments");
            var m_configDir = Path.Combine(FileSystem.AppDataDirectory, "config");
            var m_imagesDir = Path.Combine(FileSystem.AppDataDirectory, "images");
            List<string> rawFiles = ListAssets("Config").ToList();


            //var streamTest = await FileSystem.OpenAppPackageFileAsync("Config/datatransfer.xml");
            //string mainDirectory = FileSystem.AppDataDirectory;
            //string subDirectory = "Config";
            //var filepath = Path.Combine(mainDirectory, subDirectory);
            //if (!System.IO.File.Exists(filepath))
            //{
            //    Console.WriteLine("Not Exists");
            //}
            //else
            //{
            //    Console.WriteLine("Exists");
            //}

            foreach (string assetFile in rawFiles)
            {
                if (assetFile != null && (assetFile.EndsWith(".xml") || assetFile.EndsWith(".html") || assetFile.EndsWith(".png") || assetFile.EndsWith(".jpg") || assetFile.EndsWith(".bmp") || assetFile.EndsWith(".db") || assetFile.EndsWith(".pdf") || assetFile.EndsWith(".gif")))
                {
                    string targetDir = "";
                    if (assetFile.EndsWith(".xml") || assetFile.EndsWith(".html") || assetFile.EndsWith(".gif"))
                    {
                        targetDir = m_configDir;

                    }
                    string targetFile = System.IO.Path.Combine(targetDir, assetFile);
                    if (!System.IO.File.Exists(targetFile))
                    {
                        using var stream = await FileSystem.OpenAppPackageFileAsync("Config/" + assetFile);

                        using (Stream s = File.Create(Path.Combine(m_configDir, assetFile)))
                        {
                            stream.CopyTo(s);
                        }
                    }
                    else
                    {
                        break; // we only want this to run on the initial install
                    }
                }

            }
        }


        public IEnumerable<string> ListAssets(string subfolder)
        {
#if ANDROID
            AssetManager assets = Platform.AppContext.Assets;
            string[] files = assets.List(subfolder);
            return files.ToList();

#elif IOS
            NSBundle mainBundle = NSBundle.MainBundle;
            string resourcesPath = mainBundle.ResourcePath;
            string subfolderPath = Path.Combine(resourcesPath, subfolder);

            if (Directory.Exists(subfolderPath))
            {
                string[] files = Directory.GetFiles(subfolderPath);
                return files.Select(Path.GetFileName).ToList();
            }
            else
            {
                return new List<string>();
            }

#else
            return new List<string>();
#endif
        }

        protected override void OnStart()
        {
            base.OnStart();
            bool isDBFirst = Preferences.Default.Get("DBFirstInsert", false);
            if (!isDBFirst)
            {
                Preferences.Default.Set("DBFirstInsert", true);
                IRepo<Settings> settingsRepo = new Repo<Settings>(App.Database);
                Settings s = new Settings();
                s.KeyName = Helper.Constants.DB_VERSION_SETTING_NAME;
                s.IntValue = Helper.Constants.NEW_DB_VERSION;
                settingsRepo.Insert(s);
            }
        }

    }
}
