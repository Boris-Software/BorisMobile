using BorisMobile.Models;
using BorisMobile.Repository;
using BorisMobile.ViewModels;
using BorisMobile.Views;
using BorisMobile.Helper;

using BorisMobile.NativePlatformService.Interfaces;


#if ANDROID
using Android.Content.Res;
using BorisMobile.Platforms.Android;
#elif IOS
using BorisMobile.Platforms.iOS;
using Foundation;
#endif

namespace BorisMobile
{
    public partial class App : Application
    {
        private readonly IBackgroundService _backgroundService;
        public App(IBackgroundService backgroundService)
        {
            InitializeComponent();
            _backgroundService = backgroundService;


#if ANDROID
            DependencyService.Register<BiometricAndroidImplementation>();
#elif IOS
            DependencyService.Register<BiometricIosImplementation>();
#endif
            createAppFolders();
            
            var db = DBHelper.Database;

            string provisionedMarkerFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "provisioned.dat");
            string authenticatedMarkerFile = System.IO.Path.Combine(FileSystem.AppDataDirectory, "authenticated.dat");

            if (!File.Exists(provisionedMarkerFile) && !(File.Exists(authenticatedMarkerFile)))
            {
                MainPage = new NavigationPage(new CustomerCodePage(new CustomerCodePageViewModel()));
            }
            else
            {
                _backgroundService.StartBackgroundService();
                MainPage = new NavigationPage(new SigninPage(new SigninPageViewModel()));
            }
        }

        public async void ChangeLaunchPadAsNeedtoDownload()
        {
            IRepo<GenericAttachments> attachmentRepo = new Repo<GenericAttachments>(DBHelper.Database);
            var res = await attachmentRepo.Get();
            if (res != null)
            {
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
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "attachments")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "attachments"));
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "config")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "config"));
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "images")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "images"));
                if (!Directory.Exists(Path.Combine(FileSystem.AppDataDirectory, "uploads")))
                    Directory.CreateDirectory(Path.Combine(FileSystem.AppDataDirectory, "uploads"));

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

        protected override async void OnStart()
        {
            base.OnStart();
            
            bool isDBFirst = Preferences.Default.Get("DBFirstInsert", false);
            if (!isDBFirst)
            {
                Preferences.Default.Set("DBFirstInsert", true);
                IRepo<Settings> settingsRepo = new Repo<Settings>(DBHelper.Database);
                Settings s = new Settings();
                s.KeyName = Helper.Constants.DB_VERSION_SETTING_NAME;
                s.IntValue = Helper.Constants.NEW_DB_VERSION;
                settingsRepo.Insert(s);
            }
        }

        protected override async void OnSleep()
        {
            base.OnSleep();
            await _backgroundService.StopBackgroundService();
        }

    }
}
