using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class WorkFromAssetsPage : ContentPage
{
    public WorkFromAssetsPage(WorkFromAssetsPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (drawingsPicker.IsFocused)
                drawingsPicker.Unfocus();

            drawingsPicker.Focus();
        });
    }
}
