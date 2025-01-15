using BorisMobile.DataTransferController;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.ComponentModel.DataAnnotations;

namespace BorisMobile.ViewModels
{
    public partial class SigninPageViewModel : BaseViewModel
    {
        SigninService signinService = null;

        [ObservableProperty]
        [Required(ErrorMessage = "Required")]
        //[EmailAddress(ErrorMessage = "Not valid email address")]
        private string? email;

        [ObservableProperty]
        [StringLength(64, MinimumLength = 8,
            ErrorMessage = "The password should contain at least 8 characters.")]
        [Required(ErrorMessage = "Required")]
        private string? password;


        partial void OnEmailChanged(string? value)
        {
            ValidateAllProperties();
        }

        partial void OnPasswordChanged(string? value)
        {
            ValidateAllProperties();
        }

        public SigninPageViewModel()
        {
            ValidateAllProperties();
            IsLoading = false; MainLayoutOpacity = 1;
            signinService = new SigninService();

            Email = Preferences.Get("UserName", string.Empty);
        }
        

        [RelayCommand]
        async Task Submit()
        {
            ValidateAllProperties();

            if (HasErrors){
                return;
            }
            else{
                MainLayoutOpacity = 0.2;
                IsLoading = true;
                var response = await signinService.Signin(Email,Password);
                if (response == WebResponseState.Failed.ToString())
                {
                    IsLoading = false;
                    MainLayoutOpacity = 1;
                    await App.Current.MainPage.DisplayAlert("Log in failed", " Please enter correct email and password.", "OK");

                }
                else
                {
                    MainLayoutOpacity = 1;
                    IsLoading = false;
                    if (!Preferences.Get("DataTransferCompleted", false))
                    {
                        Preferences.Set("UserName", Email);
                        Preferences.Set("Token", response);
                        await App.Current.MainPage.DisplayAlert("Boris", "A data transfer will be run to download your data", "OK");


                        await App.Current.MainPage.Navigation.PushAsync(new SyncPage(new SyncPageViewModel(response, Email)));
                    }
                    else
                    {
                        Preferences.Set("UserName", Email);
                        Preferences.Set("Token", response);
                        await App.Current.MainPage.Navigation.PushAsync(new HomePage(new HomePageViewModel()));

                    }
                }
            }
        }

        [RelayCommand]
        async Task Clear()
        {
            Email = string.Empty;
            Password = string.Empty;
        }
    }
}
