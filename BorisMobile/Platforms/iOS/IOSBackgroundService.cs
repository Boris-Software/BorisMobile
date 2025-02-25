using BorisMobile.NativePlatformService;
using BorisMobile.NativePlatformService.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace BorisMobile.Platforms.iOS
{
    public class IOSBackgroundService : IBackgroundService
    {
        private readonly IBackgroundUploader _syncService;
        private readonly ILogger<IOSBackgroundService> _logger;
        private nint _taskId;

        public IOSBackgroundService(IBackgroundUploader syncService, ILogger<IOSBackgroundService> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task StartBackgroundService()
        {
            try
            {
                UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(
                    UIApplication.BackgroundFetchIntervalMinimum);

                _taskId = UIApplication.SharedApplication.BeginBackgroundTask("SyncService", async () =>
                {
                    await StopBackgroundService();
                });

                await _syncService.StartSync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting iOS background service");
            }
        }

        public async Task StopBackgroundService()
        {
            try
            {
                await _syncService.StopSync();

                if (_taskId != 0)
                {
                    UIApplication.SharedApplication.EndBackgroundTask(_taskId);
                    _taskId = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping iOS background service");
            }
        }
    }
}
