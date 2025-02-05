using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using BorisMobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace BorisMobile.ViewModels
{
    public partial  class WorkFromAssetsPageViewModel:BaseViewModel
    {
        WorkFromAssetsService assetsService;
        WorkFromDrawingsService drawingsService;

        [ObservableProperty]
        WorkOrderList selectedWorkOrder;

        [ObservableProperty]
        private bool isLoading;

        //[ObservableProperty]
        //public IdAndDescriptionCollection workFrowDrawings;
        private IdAndDescriptionCollection _workFrowDrawings;
        public IdAndDescriptionCollection WorkFrowDrawings
        {
            get => _workFrowDrawings;
            set
            {
                _workFrowDrawings = value;
                OnPropertyChanged(nameof(_workFrowDrawings));
            }
        }

        [ObservableProperty]
        public ObservableCollection<WorkFromAssets> assetsList;

        private object _selectedDrawing;
        public object SelectedDrawing
        {
            get => _selectedDrawing;
            set
            {
                
                    _selectedDrawing = value;
                    OnPropertyChanged();
                    IsLoading = true;
                    
                    Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                        LoadAssets();
                    });

            }
        }

        public WorkFromAssetsPageViewModel(WorkOrderList workOrder)
        {
            IsLoading = true;
            assetsService = new WorkFromAssetsService();
            drawingsService = new WorkFromDrawingsService();
            SelectedWorkOrder = workOrder;
            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                Init();
                //SaveFormDataCommand = new Command(SaveFormData);
            });
            
        }

        public async void Init()
        {
            var list = await drawingsService.GetData(SelectedWorkOrder);

            IdAndDescriptionListItem listData = new IdAndDescriptionListItem(0, "Please select the option");
            if (WorkFrowDrawings == null)
                WorkFrowDrawings = new IdAndDescriptionCollection();

            //list.Add(listData);
            WorkFrowDrawings.Add(listData);

            foreach (var item in list)
            {
                WorkFrowDrawings.Add(item);
            }
            OnPropertyChanged(nameof(WorkFrowDrawings));
            IsLoading = false;
            //_selectedDrawing = WorkFrowDrawings[0];
        }


        public async void LoadAssets()
        {
            
            int drawingListId = (SelectedDrawing as IdAndDescriptionListItem).Id;
            if (drawingListId != -1 && drawingListId !=0)
            {
                List<WorkFromAssets> resultList = await assetsService.GetData(SelectedWorkOrder,drawingListId);
                //Collection<object> list = resultList;
                
                AssetsList = new ObservableCollection<WorkFromAssets>(resultList);
            }
            IsLoading = false;
        }

        

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
