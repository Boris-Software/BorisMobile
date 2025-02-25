﻿using BorisMobile.DataHandler.Data;
using BorisMobile.Models;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class WorkFromDrawingPageViewModel:BaseViewModel
    {
        WorkFromDrawingsService service;

        [ObservableProperty]
        WorkOrderList selectedWorkOrder;

        [ObservableProperty]
        public IdAndDescriptionCollection workFrowDrawings;

        public WorkFromDrawingPageViewModel(WorkOrderList workOrder)
        {
            service = new WorkFromDrawingsService();
            SelectedWorkOrder = workOrder;
            Init();
        }

        public async void Init()
        {
            WorkFrowDrawings = await service.GetData(SelectedWorkOrder);
        }

        [RelayCommand]
        public async void BackButtonClick()
        {
            await App.Current.MainPage.Navigation.PopAsync();
        }

        [RelayCommand]
        public async void MoreIconClicked()
        {
            await App.Current.MainPage.Navigation.PushAsync(new AboutPage(new AboutPageViewModel()));

        }
    }
}
