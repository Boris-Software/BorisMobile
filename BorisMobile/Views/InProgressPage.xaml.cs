using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class InProgressPage : ContentPage
{
    public InProgressPage(InProgressPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}