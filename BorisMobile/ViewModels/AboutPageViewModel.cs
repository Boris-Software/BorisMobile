using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class AboutPageViewModel:BaseViewModel
    {
        [ObservableProperty]
        private Models.DataOrganisations? companyDetails;

        [ObservableProperty]
        private string? userName;

        public AboutPageViewModel()
        {
            GetCompanyDetails();
            userName = Preferences.Default.Get("UserName", string.Empty);
        }

        public async void GetCompanyDetails()
        {
            AboutService aboutService = new AboutService();
            CompanyDetails = await aboutService.GetCompanyDetails();
        }

        [RelayCommand]
        public async void LogoutButtonClick()
        {
            //todo clear stack and redirect
            await App.Current.MainPage.Navigation.PushAsync(new SigninPage(new SigninPageViewModel()));

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();

        }
    }
}
