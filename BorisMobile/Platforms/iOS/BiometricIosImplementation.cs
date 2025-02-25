using BorisMobile.NativePlatformService;

namespace BorisMobile.Platforms.iOS
{
    public class BiometricIosImplementation : IBiometricService
    {
        public string CheckAvailability()
        {
            return "Ios";
        }
    }
}
