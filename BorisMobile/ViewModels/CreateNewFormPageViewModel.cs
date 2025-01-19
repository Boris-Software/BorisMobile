using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using BorisMobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BorisMobile.ViewModels
{
    public partial class CreateNewFormPageViewModel : BaseViewModel
    {
        CreateNewFormService service;

        [ObservableProperty]
        WorkOrderList workOrder;

        [ObservableProperty]
        ObservableCollection<TemplateDocument> templateDocuments;

        public CreateNewFormPageViewModel(WorkOrderList workOrder) 
        { 
            WorkOrder = workOrder;
            service = new CreateNewFormService();
            Init();
        }

        public async void Init()
        {
            List<TemplateDocument>  list = await service.GetCreateFormListData(WorkOrder);

            templateDocuments = new ObservableCollection<TemplateDocument>(list);

        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
  
}
