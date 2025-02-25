using BorisMobile.Models;
using BorisMobile.Services.Interfaces;
using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class JobFormPage : ContentPage
{
	JobFormPageViewModel vm;

    public JobFormPage(string xmlElement, WorkOrderList workOrder, AuditsInProgress inProgress)
	{
		vm = new JobFormPageViewModel(Application.Current.Handler.MauiContext.Services.GetService<IXmlParserService>(),
                Application.Current.Handler.MauiContext.Services.GetService<IFormGenerationService>(), xmlElement,workOrder,inProgress);
		InitializeComponent();
		BindingContext = vm;
	}
}