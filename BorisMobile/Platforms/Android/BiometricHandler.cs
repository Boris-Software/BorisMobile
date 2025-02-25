using Android.Content;
using Android.Widget;
using AndroidX.Core.Hardware.Fingerprint;
using AndroidX.Core.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.Platforms.Android
{
    public class BiometricHandler : FingerprintManagerCompat.AuthenticationCallback
    {
        //private Context mainActivity;

        //public bool AuthResult;
        //public BiometricHandler(Context mainActivity)
        //{
        //    this.mainActivity = mainActivity;
        //}
        //internal void StartAuthentication(FingerprintManagerCompat fingerprintManager, FingerprintManagerCompat.CryptoObject cryptoObject)
        //{
        //    CancellationSignal cancellationSignal = new CancellationSignal();
        //    fingerprintManager.Authenticate(cryptoObject, 0, cancellationSignal, this, null);
        //}
        //public override void OnAuthenticationFailed()
        //{
        //    BiometricAndroidImplementation.IsAutSucess = false;
        //    Toast.MakeText(mainActivity, "Fingerprint Authentication failed!", ToastLength.Long).Show();
        //    MessagingCenter.Send<string>("Auth", "Fail");
        //}
        //public override void OnAuthenticationSucceeded(FingerprintManagerCompat.AuthenticationResult result)
        //{
        //    BiometricAndroidImplementation.IsAutSucess = true;
        //    Toast.MakeText(mainActivity, "Fingerprint Authentication Success", ToastLength.Long).Show();
        //    MessagingCenter.Send<string>("Auth", "Success");
        //}
    }
}
