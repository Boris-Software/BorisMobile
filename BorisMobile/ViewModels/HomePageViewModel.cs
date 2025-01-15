using BorisMobile.Views;
using BorisMobile.XML;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using static BorisMobile.XML.ParseXML;

namespace BorisMobile.ViewModels
{
    public partial class HomePageViewModel : BaseViewModel
    {
        ParseXML parseData;

        [ObservableProperty]
        private ObservableCollection<Item> menuItems = new();

        List<Item> itemList = new();

        public HomePageViewModel() {
            parseData = new ParseXML();

            IsSyncVisible = false;
            Init();
        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }


        public void Init()
        {
            var m_configXml = new XmlConfigDoc(Helper.Constants.LAUNCHPAD);
            itemList = parseData.ParseItems(m_configXml.XmlDocument.InnerXml);
            List<Item>   itemL = itemList.Where(i => i.Page == 0).ToList();
            foreach (var item in itemL)
            {
                item.NewImage = item.Name.Replace(" ", string.Empty);
            }
            var ru = itemL.Where(x => x.Name == "Receive updates").FirstOrDefault();
            var sr = itemL.Where(x => x.Name == "Send reports").FirstOrDefault();
            Item itemSupport = new Item();
            itemSupport.Name = "Support";
            itemSupport.NewImage = "support";
            itemL.Add(itemSupport);

            itemL.Remove(ru);
            itemL.Remove(sr);
            
            menuItems = new ObservableCollection<Item>(itemL);
        }

        [RelayCommand]
        public async void Sync()
        {
            IsSyncVisible = !IsSyncVisible;
            //SyncService s = new SyncService();
            //var res = await s.TransferData(Preferences.Get("Token",string.Empty), Preferences.Get("UserName",string.Empty));

        }

        [RelayCommand]
        public async void MenuItemClicked(Item item)
        {
            if (item.Name.Equals("My Diary"))
            {
                var subPageItems = itemList.Where(c => c.Page == item.NewPage ).ToList();

                await App.Current.MainPage.Navigation.PushAsync(new MyDiaryPage(new MyDiaryPageViewModel(subPageItems)),true);
            }else if (item.Name.Equals("Assets"))
            {
                await App.Current.MainPage.Navigation.PushAsync(new AssetsPage(new AssetsPageViewModel()), true);
            }
            else if (item.Name.Equals("Support"))
            {
                await App.Current.MainPage.Navigation.PushAsync(new SupportPage(new SupportPageViewModel()), true);
            }
            else if (item.Name.Equals("Bulletins"))
            {
                await App.Current.MainPage.Navigation.PushAsync(new BulletinsPage(new BulletinsPageViewModel()), true);
            }
            else if (item.Name.Equals("Document Store"))
            {
                await App.Current.MainPage.Navigation.PushAsync(new DocumentStorePage(new DocumentStorePageViewModel()), true);
            }
        }

        
    }
}
