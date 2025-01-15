using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class SupportPage : ContentPage
{
    public SupportPage(SupportPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}