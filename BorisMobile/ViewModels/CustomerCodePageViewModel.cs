using BorisMobile.DataTransferController;
using BorisMobile.Services;
using BorisMobile.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BorisMobile.ViewModels
{
    public partial class CustomerCodePageViewModel : BaseViewModel
    {
        string provPassword = "doesntmattersameforeveryone";
        CustomerCodeService customerCodeService = null;
        public Action<bool> OnLoginFailed { get; set; }

        string customerCode = string.Empty;

        [ObservableProperty]
        private string? cuscodeone;

        partial void OnCuscodeoneChanged(string? value)
        {
            customerCode += value;
        }

        [ObservableProperty]
        private string? cuscodetwo;

        partial void OnCuscodetwoChanged(string? value)
        {
            customerCode += value;
        }

        [ObservableProperty]
        private string? cuscodethree;
        partial void OnCuscodethreeChanged(string? value)
        {
            customerCode += value;
        }
        [ObservableProperty]
        private string? cuscodefour;
        partial void OnCuscodefourChanged(string? value)
        {
            customerCode += value;
        }
        [ObservableProperty]
        private string? cuscodefive;
        partial void OnCuscodefiveChanged(string? value)
        {
            customerCode += value;
        }
        [ObservableProperty]
        private string? cuscodesix;
        partial void OnCuscodesixChanged(string? value)
        {
            customerCode += value;
        }


        public CustomerCodePageViewModel()
        {
            MainLayoutOpacity = 1;
            customerCodeService = new CustomerCodeService();
        }

        [RelayCommand]
        async void Clear()
        {
            Cuscodeone = string.Empty;
            Cuscodetwo = string.Empty;
            Cuscodethree = string.Empty;
            Cuscodefour = string.Empty;
            Cuscodefive = string.Empty;
            Cuscodesix = string.Empty;
            customerCode=string.Empty;
            OnLoginFailed?.Invoke(true);
        }

        [RelayCommand]
        async Task Continue()
        {
            MainLayoutOpacity = 0.2;
            IsLoading = true;
            
            var res = await customerCodeService.ProvisionCustomerCode(customerCode, provPassword);
            
            IsLoading = false;
            MainLayoutOpacity = 1;
            if (res == WebResponseState.Success)
            {
                await App.Current.MainPage.Navigation.PushAsync(new SigninPage(new SigninPageViewModel()));
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Boris", "Not a valid customer code. Please try again.", "OK");
            }
        }


    }
}
