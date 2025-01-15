using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class ScanNFCPage : ContentPage
{
	public ScanNFCPage(ScanNFCPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}