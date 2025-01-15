using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class AboutPage : ContentPage
{
	public AboutPage(AboutPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}