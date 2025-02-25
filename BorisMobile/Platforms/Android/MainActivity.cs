using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using BorisMobile.Platforms.Android;

namespace BorisMobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public static Context context;
        public static Activity activity;
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this;
            activity = this;
            StartService(new Intent(this, typeof(BackgroundUploaderService)));
        }
    }
}
