using BorisMobile.Views;
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
        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }

}
