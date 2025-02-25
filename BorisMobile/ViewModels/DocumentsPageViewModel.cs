using BorisMobile.Helper;
using BorisMobile.Models;
using BorisMobile.Services;
using BorisMobile.Views;
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
            AttachmentList = new ObservableCollection<Attachments>(list);
        }


        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void DocumentClick(Attachments attachement)
        {
            await OpenFile(attachement);
        }

        private async Task OpenFile(Attachments attachments)
        {
            string filePath = Path.Combine(FilesHelper.GetAttachmentDirectoryMAUI(), attachments.FileName);

            if (File.Exists(filePath.ToLower()))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath.ToLower())
                });
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "File not found!", "OK");
            }

        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
