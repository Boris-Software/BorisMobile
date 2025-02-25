namespace BorisMobile.Helper
{
    public static class PermissionHelper
    {
        public static async Task<bool> CheckAndRequestPermission<T>() where T : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<T>();

            if (status == PermissionStatus.Granted)
            {
                return true; // Permission is already granted
            }

            if (Permissions.ShouldShowRationale<T>())
            {
                // Show a message to the user why this permission is needed
                await App.Current.MainPage.DisplayAlert("Permission Required",
                    "This feature requires access to your location.", "OK");
            }

            status = await Permissions.RequestAsync<T>();

            return status == PermissionStatus.Granted;
        }
    }
}
