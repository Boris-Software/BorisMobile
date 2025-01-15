using CommunityToolkit.Mvvm.ComponentModel;

namespace BorisMobile.ViewModels
{
    public partial class BaseViewModel : ObservableValidator
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private double mainLayoutOpacity;

        [ObservableProperty]
        private bool isSyncVisible;
    }
}
