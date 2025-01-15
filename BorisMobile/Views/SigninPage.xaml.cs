using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class SigninPage : ContentPage
{
	public SigninPage(SigninPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}