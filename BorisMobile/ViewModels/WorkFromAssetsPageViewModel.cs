using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial  class WorkFromAssetsPageViewModel:BaseViewModel
    {
        public WorkFromAssetsPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
