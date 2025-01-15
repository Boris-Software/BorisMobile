using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class KeyPageViewModel
    {
        public KeyPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
