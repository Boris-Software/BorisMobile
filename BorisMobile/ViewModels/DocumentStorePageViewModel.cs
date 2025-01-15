using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class DocumentStorePageViewModel:BaseViewModel
    {
        public DocumentStorePageViewModel()
        {

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync(false);
        }
    }
}
