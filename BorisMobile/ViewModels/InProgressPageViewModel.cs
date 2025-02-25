using BorisMobile.Models;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BorisMobile.ViewModels
{
    public partial class InProgressPageViewModel:BaseViewModel
    {
        InProgressService inProgressService;

        [ObservableProperty]
        public WorkOrderList workOrder;
        
        public List<InProgress> inProgressList;

        [ObservableProperty]
        public ObservableCollection<InProgress> filteredItems;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    FilterItems();
                }
            }
        }

        [ObservableProperty]
        bool isLoading;

        public InProgressPageViewModel(WorkOrderList workOrder)
        {
            IsLoading = true;
            WorkOrder = workOrder;
            inProgressService = new InProgressService();

            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                Init();
                //SaveFormDataCommand = new Command(SaveFormData);
            });
        }


        public async void Init()
        {
            List<InProgress> list = await inProgressService.GetInprogressListData(WorkOrder);

            inProgressList = new List<InProgress>(list);
            FilteredItems = new ObservableCollection<InProgress>(list);

            IsLoading = false;
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void DeleteProgress(InProgress inprogres)
        {
            var res = await App.Current.MainPage.DisplayAlert("Delete InProgress", "Are you sure want to delete the inprogress?", "Yes", "No");
            if (res)
            {
                await inProgressService.DeleteInProgress(inprogres);
                Init();
            }
        }

        private void FilterItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems = new ObservableCollection<InProgress>(inProgressList);
            }
            else
            {
                var filtered = inProgressList.Where(item => item.AuditDesc.ToLower().Contains(SearchText.ToLower())).ToList();
                FilteredItems.Clear();
                foreach (var item in filtered)
                    FilteredItems.Add(item);
            }
            OnPropertyChanged(nameof(FilteredItems));
        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
