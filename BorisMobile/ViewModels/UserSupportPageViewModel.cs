using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class UserSupportPageViewModel : BaseViewModel
    {
        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
