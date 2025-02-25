using BorisMobile.Services;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace BorisMobile.NativePlatformService
{
    public class BackgroundUploader : IBackgroundUploader
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundUploader> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning;
        private const int MAX_RETRY_ATTEMPTS = 3;
        private const int RETRY_DELAY_MINUTES = 15;
        private const int SYNC_INTERVAL_MINUTES = 30;

        public BackgroundUploader()
        {
            //_serviceProvider = serviceProvider;
            //_logger = logger;
        }

        public async Task StartSync()
        {
            if (_isRunning) return;

            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                await RunSyncLoop(_cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in sync service");
            }
        }

        public Task StopSync()
        {
            _isRunning = false;
            _cancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        private async Task RunSyncLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                try
                {
                    //await using var scope = _serviceProvider.CreateAsyncScope();
                    //var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    //var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();
                    
                    SyncService syncService = new SyncService();

                    await syncService.SyncDataToServer();

                    // Get unsynced attachments with retry logic
                    //var unsynced = await dbContext.Attachments
                    //    .Where(a => !a.IsSynced
                    //        && (a.RetryCount < MAX_RETRY_ATTEMPTS)
                    //        && (!a.LastSyncAttempt.HasValue ||
                    //            a.LastSyncAttempt.Value.AddMinutes(RETRY_DELAY_MINUTES) < DateTime.UtcNow))
                    //    .ToListAsync();

                    //foreach (var attachment in unsynced)
                    //{
                    //    try
                    //    {
                    //        // Check if file exists
                    //        if (!File.Exists(attachment.FilePath))
                    //        {
                    //            _logger.LogWarning($"File not found: {attachment.FilePath}");
                    //            continue;
                    //        }

                    //        // Get related audit data
                    //        var audit = await dbContext.AuditsInProgress
                    //            .FirstOrDefaultAsync(a => a.Id == attachment.AuditInProgressId);

                    //        if (audit == null)
                    //        {
                    //            _logger.LogWarning($"Audit not found for attachment: {attachment.Id}");
                    //            continue;
                    //        }

                    //        // Upload file
                    //        var fileBytes = await File.ReadAllBytesAsync(attachment.FilePath);
                    //        var success = await apiClient.UploadAttachmentAsync(
                    //            attachment.Id,
                    //            audit.Id,
                    //            fileBytes,
                    //            Path.GetFileName(attachment.FilePath));

                    //        if (success)
                    //        {
                    //            attachment.IsSynced = true;
                    //            attachment.LastSyncAttempt = DateTime.UtcNow;
                    //            await dbContext.SaveChangesAsync();

                    //            _logger.LogInformation($"Successfully synced attachment: {attachment.Id}");
                    //        }
                    //        else
                    //        {
                    //            HandleSyncFailure(attachment, dbContext);
                    //        }
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        _logger.LogError(ex, $"Error syncing attachment: {attachment.Id}");
                    //        HandleSyncFailure(attachment, dbContext);
                    //    }
                    //}

                    // Wait for next sync interval
                    await Task.Delay(TimeSpan.FromMinutes(SYNC_INTERVAL_MINUTES), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in sync loop");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
            }
        }

        //private async void HandleSyncFailure(Attachment attachment, AppDbContext dbContext)
        //{
        //    attachment.RetryCount++;
        //    attachment.LastSyncAttempt = DateTime.UtcNow;

        //    if (attachment.RetryCount >= MAX_RETRY_ATTEMPTS)
        //    {
        //        _logger.LogWarning($"Max retry attempts reached for attachment: {attachment.Id}");
        //    }

        //    await dbContext.SaveChangesAsync();
        //}
    }
}
