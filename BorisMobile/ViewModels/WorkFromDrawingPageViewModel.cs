using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class WorkFromDrawingPageViewModel:BaseViewModel
    {
        public WorkFromDrawingPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
