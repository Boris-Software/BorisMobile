using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class CreateNewFormPage : ContentPage
{
    public CreateNewFormPage(CreateNewFormPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}