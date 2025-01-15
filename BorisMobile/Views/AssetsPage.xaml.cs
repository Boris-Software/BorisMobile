using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class AssetsPage : ContentPage
{
    public AssetsPage(AssetsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}