using BorisMobile.Models;
using BorisMobile.Models.DynamicFormModels;
using BorisMobile.Services.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Xml.Linq;

namespace BorisMobile.ViewModels
{
    public partial class JobFormPageViewModel : BaseViewModel
    {
        private readonly IXmlParserService _xmlParserService;
        private readonly IFormGenerationService _formGenerationService;

        public FormConfigModel CurrentFormConfig { get; private set; }

        [ObservableProperty]
        public Page dynamicForm;

        [ObservableProperty]
        public bool isLoading;

        [ObservableProperty]
        public string formTitle;

        WorkOrderList workOrder;
        AuditsInProgress inProgress;
        public Command SaveFormDataCommand { get; }

        public JobFormPageViewModel(IXmlParserService xmlParserService,
        IFormGenerationService formGenerationService,string xmlContent, WorkOrderList workOrder, AuditsInProgress inProgress)
        {
            this.workOrder = workOrder;
            this.inProgress = inProgress;
            _xmlParserService = xmlParserService;
            _formGenerationService = formGenerationService;
            IsLoading = true;

            XDocument doc = XDocument.Parse(xmlContent);
            var configElement = doc.Descendants("Config").FirstOrDefault();

            // Extract Document properties
            var documentElement = configElement.Element("Document");
            FormTitle = documentElement?.Attribute("desc")?.Value;
           // FormTitle = configElement.Element("Header")?.Element("ScreenTitle")?.Value ?? "Dynamic Form";

            Task.Delay(new TimeSpan(0, 0, 1)).ContinueWith(o => {
                LoadXmlConfiguration(xmlContent);
                //SaveFormDataCommand = new Command(SaveFormData);
            });
            
        }

        private async void LoadXmlConfiguration(string xmlContent)
        {
            try
            {
                CurrentFormConfig = await _xmlParserService.ParseXmlConfiguration(xmlContent);
                // Generate form UI
                MainThread.InvokeOnMainThreadAsync( async () =>
                {
                    DynamicForm = await _formGenerationService.CreateDynamicForm(CurrentFormConfig, workOrder,inProgress);
                });
                IsLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadXmlConfiguration  {ex.Message}");
            }
        }

        private void SaveFormData()
        {
            // Collect form data and update ElementModel values
            var xmlOutput = _xmlParserService.GenerateXmlFromFormData(CurrentFormConfig);
            // Send XML to server or save locally
        }

        //public void UpdateElementValue(string uniqueName, object value)
        //{
        //    var element = CurrentFormConfig.SubDocumentModel.Pages
        //        .SelectMany(p => p.Sections)
        //        .SelectMany(s => s.Elements)
        //        .FirstOrDefault(e => e.UniqueName == uniqueName);

        //    if (element != null)
        //    {
        //        element.Value = value;
        //    }
        //}

        [RelayCommand]
        public async void BackButtonClick()
        {
            var res = await App.Current.MainPage.DisplayAlert("Save Data", "Are you sure that you wish to cancel any changes that you have made?", "Yes", "No");
            if (res)
            {
                await App.Current.MainPage.Navigation.PopAsync();
            }
        }
    }
}
