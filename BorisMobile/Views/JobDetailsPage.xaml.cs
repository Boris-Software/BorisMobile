using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class JobDetailsPage : ContentPage
{
	public JobDetailsPage(JobDetailsPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}