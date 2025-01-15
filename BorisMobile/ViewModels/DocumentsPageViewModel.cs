using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class DocumentsPageViewModel
    {
        public DocumentsPageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
