using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class KeyPage : ContentPage
{
    public KeyPage(KeyPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}