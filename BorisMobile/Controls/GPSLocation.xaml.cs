using BorisMobile.NativePlatformService.Interfaces;
using System.Diagnostics;

namespace BorisMobile.Controls;

public partial class GPSLocation : ContentView
{
    private readonly ILocationService _locationService;
    public GPSLocation()
	{
        _locationService = Application.Current.Handler.MauiContext.Services.GetService<ILocationService>();
        //_locationService = locationService;
        InitializeComponent();
        locationresults.IsVisible = false;
        GetLocationAsync();

    }

    public async Task GetLocationAsync()
    {
        try
        {
            if (!await _locationService.CheckPermissionsAsync())
            {
                if (!await _locationService.RequestPermissionsAsync())
                {
                    await Shell.Current.DisplayAlert("Error", "Location permission is required", "OK");
                    return;
                }
            }

            var location = await _locationService.GetCurrentLocationAsync();
            if (location != null)
            {
                string locationInfo = $"Location[{location.Provider} " +
                    $"{location.Latitude},{location.Longitude} " +
                    $"hAcc={location.Accuracy} " +
                    $"et={DateTime.Now - location.Timestamp:d\\dhh\\hmm\\mss\\s} " +
                    $"alt={location.Altitude} " +
                    $"vAcc={location.VerticalAccuracy} " +
                    $"speed={location.Speed} " +
                    $"course={location.Course}]";

                Debug.WriteLine(locationInfo);
                // Use the location data as needed
                locationresults.IsVisible = true;
                EnteredValue = $"$GPS{locationInfo}";
                //TextChanged?.Invoke(this, locationInfo);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public event EventHandler<string> LocationChanged;

    #region binding

    private string _Title;
    public string Title
    {
        get => _Title;

        set
        {
            _Title = value;
            titleLabel.Text = _Title;
        }
    }
    private string _enteredValue;
    public string EnteredValue
    {
        get => _enteredValue;

        set
        {
            _enteredValue = value;
            //titleLabel.Text = _enteredValue;
            LocationChanged?.Invoke(this, _enteredValue);
        }
    }
    private bool _isMandatory;
    public bool IsMandatory
    {
        get => _isMandatory;

        set
        {
            _isMandatory = value;
            mandatory.IsVisible = value;

        }
    }

    #endregion

    public async void OnControlTapped(object sender, EventArgs args)
    {


    }

    private void entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        EnteredValue = e.NewTextValue;
    }

    //private void OnTextChanged(object sender, TextChangedEventArgs e)
    //{
    //    TextChanged?.Invoke(this, new TextChangedEventArgs(e.OldTextValue, e.NewTextValue));
    //}
}