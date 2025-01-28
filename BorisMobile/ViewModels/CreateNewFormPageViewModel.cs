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
        bool isLoading;

        [ObservableProperty]
        ObservableCollection<TemplateDocument> templateDocuments;

        public CreateNewFormPageViewModel(WorkOrderList workOrder) 
        {
            IsLoading = true;
            WorkOrder = workOrder;
            service = new CreateNewFormService();

            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                Init();
                //SaveFormDataCommand = new Command(SaveFormData);
            });
            
        }

        public async void Init()
        {
            List<TemplateDocument>  list = await service.GetCreateFormListData(WorkOrder);

            TemplateDocuments = new ObservableCollection<TemplateDocument>(list);

            IsLoading = false;
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void FormClick(TemplateDocument doc)
        {
            JobOptionsPageViewModel jobOptionsPageViewModel = new JobOptionsPageViewModel();
            jobOptionsPageViewModel.HandleJob(doc.InnerXml.ToString());
        }
    }
  
}
