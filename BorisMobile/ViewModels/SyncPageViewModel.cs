using BorisMobile.Helper;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace BorisMobile.ViewModels
{
    public partial class SyncPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private bool isFinished;

        [ObservableProperty]
        private bool isTransferred;

        [ObservableProperty]
        private string transferMessage;

        public DateTime startTime;
        public DateTime endTime;
        string token;
        string username;

        public SyncPageViewModel(string token,string username)
        {
            WeakReferenceMessenger.Default.Register<SyncUpdateMessage>(this, HandleSyncUpdateMessage);
            this.token = token;
            this.username = username;
            IsFinished = false;
            IsTransferred = false;
            startTime = DateTime.Now;
            TransferData();

        }
        private async void HandleSyncUpdateMessage(object recipient, SyncUpdateMessage message)
        {
            TransferMessage = message.Value;
        }

        public async void TransferData()
        {
            SyncService s = new SyncService();
            var res = await s.TransferData(token, username);
            if (res.Equals("success"))
            {
                Preferences.Set("DataTransferCompleted", true);
                await App.Current.MainPage.Navigation.PushAsync(new HomePage(new HomePageViewModel()));
            }
        }
    }
    }
