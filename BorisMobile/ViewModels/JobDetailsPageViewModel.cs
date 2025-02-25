using BorisMobile.Models;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class JobDetailsPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        public WorkOrderList selectedItem;

        [ObservableProperty]
        public bool isLoading;
        public JobDetailsPageViewModel(WorkOrderList item ) {
            IsLoading = true;
            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                SelectedItem = item;
                IsLoading = false;
                //SaveFormDataCommand = new Command(SaveFormData);
            });
            
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void WFDrawingsClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new WorkFromDrawingPage(new WorkFromDrawingPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void InProgressClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new InProgressPage(new InProgressPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void CreateFormClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new CreateNewFormPage(new CreateNewFormPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void WorkFromAssetsClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new WorkFromAssetsPage(new WorkFromAssetsPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void DocumentsClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new DocumentsPage(new DocumentsPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void KeyClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new KeyPage(new KeyPageViewModel(SelectedItem)));
        }
        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
