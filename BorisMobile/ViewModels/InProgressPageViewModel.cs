using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class InProgressPageViewModel:BaseViewModel
    {
        public InProgressPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
