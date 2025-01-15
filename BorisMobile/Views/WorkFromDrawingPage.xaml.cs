using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class WorkFromDrawingPage : ContentPage
{
    public WorkFromDrawingPage(WorkFromDrawingPageViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
        }
    
}