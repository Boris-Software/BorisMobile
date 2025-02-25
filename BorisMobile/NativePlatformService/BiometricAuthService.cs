// BiometricAuthService.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

#if ANDROID
using Android.Content.PM;
using Android.Hardware.Biometrics;
using Android.OS;
using Java.Util.Concurrent;
using static Android.Hardware.Biometrics.BiometricPrompt;
#endif

#if IOS
using Foundation;
using LocalAuthentication;
using UIKit;
#endif

namespace BorisMobile.NativePlatformService
{
//    public class BiometricAuthService : IBiometricAuthService
//    {
//#if IOS
//        private async Task<bool> IsBiometricsAvailableIOSAsync()
//        {
//            var context = new LAContext();
//            NSError error;

//            if (context.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out error))
//            {
//                return true;
//            }

//            return false;
//        }

//        private async Task<bool> AuthenticateIOSAsync(string reason)
//        {
//            try
//            {
//                var context = new LAContext();
//                var result = await context.EvaluatePolicy(
//                    LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
//                    reason);

//                return result;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }
//#endif

//#if ANDROID
//    private async Task<bool> IsBiometricsAvailableAndroidAsync()
//    {
//        var biometricManager = BiometricManager.From(Platform.CurrentActivity);
//        var result = biometricManager.CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);
        
//        return result == BiometricManager.BiometricSuccess;
//    }

//    private async Task<bool> AuthenticateAndroidAsync(string reason)
//    {
//        try
//        {
//            var activity = Platform.CurrentActivity;
//            var executor = Executors.NewSingleThreadExecutor();
//            var biometricPrompt = new BiometricPrompt(activity, executor, new BiometricCallbacks());
            
//            var promptInfo = new BiometricPrompt.PromptInfo.Builder()
//                .SetTitle("Biometric Authentication")
//                .SetSubtitle(reason)
//                .SetNegativeButtonText("Cancel")
//                .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong)
//                .Build();

//            var tcs = new TaskCompletionSource<bool>();
            
//            activity.RunOnUiThread(() =>
//            {
//                biometricPrompt.Authenticate(promptInfo);
//            });

//            return await tcs.Task;
//        }
//        catch (Exception ex)
//        {
//            return false;
//        }
//    }

//    private class BiometricCallbacks : BiometricPrompt.AuthenticationCallback
//    {
//        private TaskCompletionSource<bool> _tcs;

//        public BiometricCallbacks(TaskCompletionSource<bool> tcs)
//        {
//            _tcs = tcs;
//        }

//        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
//        {
//            _tcs.SetResult(true);
//        }

//        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence errString)
//        {
//            _tcs.SetResult(false);
//        }

//        public override void OnAuthenticationFailed()
//        {
//            _tcs.SetResult(false);
//        }
//    }
//#endif

//        public async Task<bool> IsBiometricsAvailableAsync()
//        {
//#if IOS
//            return await IsBiometricsAvailableIOSAsync();
//#elif ANDROID
//        return await IsBiometricsAvailableAndroidAsync();
//#else
//        return false;
//#endif
//        }

//        public async Task<bool> AuthenticateAsync(string reason)
//        {
//#if IOS
//            return await AuthenticateIOSAsync(reason);
//#elif ANDROID
//        return await AuthenticateAndroidAsync(reason);
//#else
//        return false;
//#endif
//        }
//    }
}
