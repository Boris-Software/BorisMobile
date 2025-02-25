using BorisMobile.NativePlatformService;
using Foundation;
using UIKit;

namespace BorisMobile
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            var uploader = new BackgroundUploader();
            await uploader.StartUploadingAsync(); // Background upload
            completionHandler(UIBackgroundFetchResult.NewData);
        }
    }
}
