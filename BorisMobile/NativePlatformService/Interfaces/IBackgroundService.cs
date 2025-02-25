namespace BorisMobile.NativePlatformService.Interfaces
{
    public interface IBackgroundService
    {
        Task StartBackgroundService();
        Task StopBackgroundService();
    }
}
