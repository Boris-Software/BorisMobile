using BorisMobile.Views;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class AssetsPageViewModel:BaseViewModel
    {
        public AssetsPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void UserSupportClick()
        {
            await App.Current.MainPage.Navigation.PushAsync(new UserSupportPage(new UserSupportPageViewModel()));
        }
        [RelayCommand]
        public async void ScanAssetClick()
        {
            await App.Current.MainPage.Navigation.PushAsync(new ScanNFCPage(new ScanNFCPageViewModel()));
        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
