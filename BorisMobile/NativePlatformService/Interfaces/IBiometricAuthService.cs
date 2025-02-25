namespace BorisMobile.NativePlatformService.Interfaces
{
    public interface IBiometricAuthService
    {
        Task<bool> IsBiometricsAvailableAsync();
        Task<bool> AuthenticateAsync(string reason);
    }
}
