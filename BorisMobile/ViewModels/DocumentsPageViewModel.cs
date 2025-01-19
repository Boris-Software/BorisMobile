using BorisMobile.Models;
using BorisMobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BorisMobile.ViewModels
{
    public partial class DocumentsPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        ObservableCollection<Attachments> attachmentList;

        DocumentsService documentsService;

        [ObservableProperty]
        WorkOrderList workOrderList;
        public DocumentsPageViewModel(WorkOrderList workOrder)
        {
            workOrderList = workOrder;
            documentsService = new DocumentsService(WorkOrderList);
            Init();

        }
        public async void Init()
        {
            var list   = await documentsService.GetDocumentsList();
            attachmentList = new ObservableCollection<Attachments>(list);
        }


        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        
    }
}
