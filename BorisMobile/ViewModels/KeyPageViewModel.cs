using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BorisMobile.ViewModels
{
    public partial class KeyPageViewModel:BaseViewModel
    {
        [ObservableProperty]
        public WorkOrderList workOrder;

        [ObservableProperty]
        public IdAndDescriptionCollection keysList;

        KeyPageService workOrderService;
        public KeyPageViewModel(WorkOrderList workOrder)
        {
            WorkOrder = workOrder;
            workOrderService = new KeyPageService(WorkOrder);
            Init();

        }

        public async void Init()
        {
            KeysList = await workOrderService.GetKeysList();
            
        }
        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
