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
        public IdAndDescriptionCollection workFrowDrawings;

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
                    LoadAssets();
                
            }
        }

        public WorkFromAssetsPageViewModel(WorkOrderList workOrder)
        {
            assetsService = new WorkFromAssetsService();
            drawingsService = new WorkFromDrawingsService();
            SelectedWorkOrder = workOrder;
            IdAndDescriptionListItem listData = new IdAndDescriptionListItem(0, "Please select the option");
            if(WorkFrowDrawings == null)
                WorkFrowDrawings = new IdAndDescriptionCollection();
            WorkFrowDrawings.Add(listData);

            Init();
        }

        public async void Init()
        {
            var list = await drawingsService.GetData(SelectedWorkOrder);

            foreach (var item in list)
            {
                WorkFrowDrawings.Add(item);
            }

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
        }

        

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }
    }
}
