using BorisMobile.Models;

namespace BorisMobile.NativePlatformService.Interfaces
{
    public interface ILocationService
    {
        Task<LocationData> GetCurrentLocationAsync();
        Task<bool> CheckPermissionsAsync();
        Task<bool> RequestPermissionsAsync();
    }
}
