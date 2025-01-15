using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class InformationPage : ContentPage
{
    public InformationPage(InformationPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}