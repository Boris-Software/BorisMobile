using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class SyncPage : ContentPage
{
	public SyncPage(SyncPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}