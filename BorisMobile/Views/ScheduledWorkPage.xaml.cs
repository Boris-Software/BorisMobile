using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class ScheduledWorkPage : ContentPage
{
	public ScheduledWorkPage(ScheduledWorkViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (groupByPicker.IsFocused)
                groupByPicker.Unfocus();

            groupByPicker.Focus();
        });
    }
}