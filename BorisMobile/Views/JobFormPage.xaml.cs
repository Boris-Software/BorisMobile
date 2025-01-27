using BorisMobile.Models;
using BorisMobile.Services.Interfaces;
using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class JobFormPage : ContentPage
{
	JobFormPageViewModel vm;

    public JobFormPage(WorkOrderList item)
	{
		vm = new JobFormPageViewModel(Application.Current.Handler.MauiContext.Services.GetService<IXmlParserService>(),
                Application.Current.Handler.MauiContext.Services.GetService<IFormGenerationService>(), item);
		InitializeComponent();
		BindingContext = vm;
	}
}