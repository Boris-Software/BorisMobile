using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class DocumentStorePage : ContentPage
{
    public DocumentStorePage(DocumentStorePageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}