namespace BorisMobile.NativePlatformService
{
    public interface IBackgroundUploader
    {
        Task StartSync();
        Task StopSync();
    }
}
