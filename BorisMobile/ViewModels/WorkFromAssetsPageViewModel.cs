using BorisMobile.Models;
using BorisMobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial  class WorkFromAssetsPageViewModel:BaseViewModel
    {
        WorkFromAssetsService service;

        [ObservableProperty]
        WorkOrderList selectedWorkOrder;

        public WorkFromAssetsPageViewModel(WorkOrderList workOrder)
        {
            service = new WorkFromAssetsService();
            SelectedWorkOrder = workOrder;
            Init();
        }

        public async void Init()
        {
            //await service.GetData(SelectedWorkOrder);
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
