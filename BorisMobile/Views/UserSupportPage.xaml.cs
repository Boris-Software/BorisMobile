using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class UserSupportPage : ContentPage
{
	public UserSupportPage(UserSupportPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}