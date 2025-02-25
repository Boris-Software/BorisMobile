using BorisMobile.Models;
using BorisMobile.NativePlatformService.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BorisMobile.NativePlatformService
{
    public class LocationService : ILocationService
    {
        private readonly IGeolocation _geolocation;
        //private readonly IPermissions _permissions;

        public LocationService()//, IPermissions permissions)
        {
            _geolocation = Geolocation.Default;
            //_permissions = permissions;
        }

        public async Task<bool> CheckPermissionsAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }

        public async Task<bool> RequestPermissionsAsync()
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }

        public async Task<LocationData> GetCurrentLocationAsync()
        {
            try
            {
                var location = await _geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(5)
                });

                if (location != null)
                {
                    return new LocationData
                    {
                        Latitude = location.Latitude,
                        Longitude = location.Longitude,
                        Accuracy = location.Accuracy ?? 0,
                        Altitude = location.Altitude ?? 0,
                        VerticalAccuracy = location.VerticalAccuracy ?? 0,
                        Speed = location.Speed ?? 0,
                        Course = location.Course ?? 0,
                        Timestamp = location.Timestamp,
                        Provider = GetProvider()
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Location Error: {ex.Message}");
                throw;
            }
        }

        private string GetProvider()
        {
#if ANDROID
        var locationManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.LocationService) as Android.Locations.LocationManager;
        if (locationManager.IsProviderEnabled(Android.Locations.LocationManager.NetworkProvider))
            return "network";
        if (locationManager.IsProviderEnabled(Android.Locations.LocationManager.GpsProvider))
            return "gps";
#endif
            return "unknown";
        }
    }
}
