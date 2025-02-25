using Android.Content;
using Android.OS;
using BorisMobile.NativePlatformService;
using BorisMobile.NativePlatformService.Interfaces;
using Microsoft.Extensions.Logging;

namespace BorisMobile.Platforms.Android
{
    public class AndroidBackgroundService : IBackgroundService
    {
        private readonly IBackgroundUploader _syncService;
        private readonly ILogger<AndroidBackgroundService> _logger;
        private const int SERVICE_ID = 1000;

        public AndroidBackgroundService(IBackgroundUploader syncService, ILogger<AndroidBackgroundService> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task StartBackgroundService()
        {
            try
            {
                var intent = new Intent(Platform.CurrentActivity, typeof(BackgroundUploaderService));
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Platform.CurrentActivity.StartForegroundService(intent);
                }
                else
                {
                    Platform.CurrentActivity.StartService(intent);
                }

                await _syncService.StartSync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Android background service");
            }
        }

        public async Task StopBackgroundService()
        {
            try
            {
                var intent = new Intent(Platform.CurrentActivity, typeof(BackgroundUploaderService));
                Platform.CurrentActivity.StopService(intent);
                await _syncService.StopSync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping Android background service");
            }
        }
    }
}
