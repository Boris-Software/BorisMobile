using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class SupportPageViewModel
    {
        public SupportPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync(false);
        }
    }
}
