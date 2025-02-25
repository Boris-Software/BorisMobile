using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using BorisMobile.NativePlatformService;

namespace BorisMobile.Platforms.Android
{
    [Service]
    public class BackgroundUploaderService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            var channelId = "sync_service_channel";

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    channelId,
                    "Sync Service Channel",
                    NotificationImportance.Low);

                notificationManager.CreateNotificationChannel(channel);
            }

            var notification = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Sync Service")
                .SetContentText("Syncing attachments in background")
                .SetSmallIcon(Resource.Drawable.borislogo)
                .Build();

            StartForeground(1000, notification);

            var uploader = new BackgroundUploader();
            _ = uploader.StartSync();

            return StartCommandResult.Sticky;
        }
    }
}
