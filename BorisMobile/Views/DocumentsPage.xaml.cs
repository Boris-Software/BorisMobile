using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class DocumentsPage : ContentPage
{
    public DocumentsPage(DocumentsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}