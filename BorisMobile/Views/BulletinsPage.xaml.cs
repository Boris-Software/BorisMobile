using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class BulletinsPage : ContentPage
{
    public BulletinsPage(BulletinsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}