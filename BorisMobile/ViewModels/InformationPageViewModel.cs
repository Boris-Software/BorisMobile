using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class InformationPageViewModel:BaseViewModel
    {
        public InformationPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync(false);
        }
    }
}
