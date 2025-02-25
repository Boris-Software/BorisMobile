using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class ScanNFCPageViewModel : BaseViewModel
    {

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void SaveButtonClick()
        {
            
        }
    }
}
