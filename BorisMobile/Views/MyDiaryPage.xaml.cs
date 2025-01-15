using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class MyDiaryPage : ContentPage
{
	public MyDiaryPage(MyDiaryPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}