using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class WorkFromAssetsPage : ContentPage
{
    public WorkFromAssetsPage(WorkFromAssetsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
