using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class BulletinsPageViewModel:BaseViewModel
    {
        public BulletinsPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync(false);
        }
    }

}
