using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui;
using System.Collections.ObjectModel;
using static BorisMobile.XML.ParseXML;

namespace BorisMobile.ViewModels
{
    public partial class MyDiaryPageViewModel : BaseViewModel
    {
        [ObservableProperty]
        private ObservableCollection<Item> menuItems = new();

        public MyDiaryPageViewModel(List<Item> items)
        {
            var ru = items.Where(x => x.Name == "Back to main menu").FirstOrDefault();
            items.Remove(ru);
            menuItems = new ObservableCollection<Item>(items);
        }
        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void MenuItemClicked(Item item)
        {
            if(item.Name.Equals("Scheduled Work"))
            {
                await App.Current.MainPage.Navigation.PushAsync(new ScheduledWorkPage(new ScheduledWorkViewModel(item)));

            }
            else if(item.Name.Equals("Clock Card"))
            {

            }
        }

        //[RelayCommand]
        //public async void ScheduledWorkClicked()
        //{
        //}

        
    }
}
