using BorisMobile.Controls;
using BorisMobile.DataHandler;
using BorisMobile.Helper;
using BorisMobile.Models;
using BorisMobile.Models.DynamicFormModels;
using BorisMobile.NativePlatformService;
using BorisMobile.Services.Interfaces;
using CommunityToolkit.Maui.Views;
using System.Net.Mail;
using System.Xml.Linq;

namespace BorisMobile.Services
{
    public class FormGenerationService : IFormGenerationService
    {
        WorkOrderList workOrder;
        AuditsInProgress inProgress;
        JobFormHandler jobFormHandler;
        private Dictionary<string, View> _controls;
        private Dictionary<string, string> _formAttachments;

        public FormGenerationService() {
            jobFormHandler = new JobFormHandler();
        }
        public async Task<Microsoft.Maui.Controls.Page> CreateDynamicForm(FormConfigModel formConfig, WorkOrderList workOrder, AuditsInProgress inProgress)
        {
            try
            {
                this.workOrder = workOrder;
                this.inProgress = inProgress;
                _controls = new();
                _formAttachments = new();

                return formConfig.SubDocumentModel.Pages.Count > 1
                ? await CreateTabbedPage(formConfig)
                : await CreateSinglePage(formConfig);
                 
    }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CreateDynamicForm: {ex}");
                return null;
            }
        }

        //private DynamicFormData _currentFormData;
        private FormConfigModel _formConfig;


        private async Task<Microsoft.Maui.Controls.Page> CreateTabbedPage(FormConfigModel formConfig) {
            try
            {
                //_currentFormData = new DynamicFormData
                //{
                //    FormId = Guid.NewGuid().ToString(),
                //    CreatedDate = DateTime.Now
                //};
                _formConfig = formConfig;

                var DynamicPage = new ContentPage { };

                var mainStack = new Microsoft.Maui.Controls.StackLayout
                {
                    Padding = 10,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                var bodyStack = new Microsoft.Maui.Controls.StackLayout
                {
                    Margin = new Thickness(10, 15, 10, 0),
                    VerticalOptions = LayoutOptions.FillAndExpand
                };
                //Set Form Header
                var headerLabel = new Label { Text = formConfig.SubDocumentModel.Name, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 16 };
                bodyStack.Children.Add(headerLabel);


                var tabScrollView = new Microsoft.Maui.Controls.ScrollView
                {
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                    Orientation = ScrollOrientation.Horizontal,
                    Margin = new Thickness(0, 0, 0, 0)

                };

                // Horizontal layout for section labels
                var sectionLabelsLayout = new HorizontalStackLayout
                {
                    Spacing = 20, // Space between labels
                                  //HorizontalOptions = LayoutOptions.Center
                    Margin = new Thickness(0, 10, 0, 0)
                };


                // Dynamic data for sections

                // Dictionary to keep track of section labels and their corresponding content
                var sectionLabels = new Dictionary<Label, View>();

                // Content layout to display the currently selected section's content
                var contentLayout = new Microsoft.Maui.Controls.StackLayout { Margin = new Thickness(0, 0, 0, 0) };

                // Loop through sections to create UI
                foreach (var page in formConfig.SubDocumentModel.Pages)
                {
                    // Section Label
                    var sectionLabel = new Label
                    {
                        Text = page.Name,
                        FontSize = 16,
                        TextColor = Colors.Gray,
                        HorizontalTextAlignment = TextAlignment.Center
                    };

                    // Content for the section


                    var stack = new Microsoft.Maui.Controls.StackLayout { Margin = new Thickness(0, 10, 0, 0) };

                    foreach (var section in page.Sections)
                    {
                        if (!(bool)section.ReportOnly)
                        {
                            if (section.Condition0 == null || section.Condition0.Equals(""))
                            {
                                var sectionLayout = await CreateSectionLayout(section);
                                stack.Children.Add(sectionLayout);
                            }
                        }
                    }

                    var saveButton = new Button
                    {
                        CornerRadius = 10,
                        Margin = new Thickness(0, 15, 0, 10),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Text = "Save",
                        BackgroundColor = Color.FromHex("#00B1E3")
                    };
                    saveButton.Clicked += OnSaveButtonClicked;
                    stack.Children.Add(saveButton);

                    var mainLayout = new Microsoft.Maui.Controls.ScrollView
                    {
                        HeightRequest = DeviceDisplay.MainDisplayInfo.Height * 0.3,
                        Content = stack,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                        Margin = new Thickness(0, 0, 0, 0)

                    };

                    var sectionContent = mainLayout;

                    // Add underline using TextDecorations
                    sectionLabel.TextDecorations = TextDecorations.None;

                    // Initially hide the content except for the first section
                    sectionContent.IsVisible = false;
                    sectionLabels[sectionLabel] = sectionContent;

                    // Add label to the horizontal layout
                    sectionLabelsLayout.Children.Add(sectionLabel);

                    // Add content to the content layout
                    contentLayout.Children.Add(sectionContent);

                    // Tap gesture for label click
                    var tapGestures = new TapGestureRecognizer();
                    tapGestures.Tapped += (s, e) =>
                    {
                        foreach (var kvp in sectionLabels)
                        {
                            var label = kvp.Key;
                            var contentView = kvp.Value;

                            // Update label appearance and content visibility
                            if (label == sectionLabel)
                            {
                                label.TextColor = Color.FromHex("#00B1E3");
                                label.TextDecorations = TextDecorations.Underline;
                                contentView.IsVisible = true;
                            }
                            else
                            {
                                label.TextColor = Colors.Gray;
                                label.TextDecorations = TextDecorations.None;
                                contentView.IsVisible = false;
                            }
                        }
                    };
                    sectionLabel.GestureRecognizers.Add(tapGestures);
                }

                // Set the first section as active by default
                var firstLabel = sectionLabels.Keys.First();
                firstLabel.TextColor = Color.FromHex("#00B1E3");
                firstLabel.TextDecorations = TextDecorations.Underline;
                sectionLabels[firstLabel].IsVisible = true;

                // Add sections and content layout to the main layout
                tabScrollView.Content = sectionLabelsLayout;
                bodyStack.Children.Add(tabScrollView);
                bodyStack.Children.Add(contentLayout);

                mainStack.Children.Add(bodyStack);

                DynamicPage.Content = mainStack;

                return DynamicPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateTabbedPage: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }


        private async void OnSaveButtonClicked(object sender, EventArgs e)
        {
            // Validate form
            if (!FormValidationExtensions.ValidateForm(_formConfig))
            {
                await App.Current.MainPage.DisplayAlert("Validation Error", "Please fill in all mandatory fields.", "OK");
                //HighlightMandatoryFields();
                return;
            }
            else
            {
                //mandatory fields validation passed
                //proceed to save
                //// Serialize to XML
                string xmlData = FormValidationExtensions.SerializeToXml(_formConfig);

                // Save to database
                await SaveFormToDatabase(xmlData);

                ////add header data before sending to server
                ////< Header User = "6664" Customer = "7722" Audit = "15484" Location = "11712" WorkOrder = "4525" DateOfAudit = "2025-02-07T11:42:49" DateTimeStarted = "2025-02-07T11:42:49" DateTimeReleased = "2025-02-11T22:34:51" ReleaseStatus = "5" Platform = "Android" />
                //var finalXMLString = AddHeaderToXml(xmlData);


                ////start bg service to upload datat to server
                var uploader = new BackgroundUploader();
                _ = uploader.StartSync(); // Start upload in background (fire & forget)

                //SyncService syncDataHandler = new SyncService();
                //var result = await syncDataHandler.UploadDataToServer(finalXMLString, inProgress.IdGuid);
                //await App.Current.MainPage.Navigation.PopAsync();
                //if(result == 1)
                //{
                    //successfully saved
                    await App.Current.MainPage.Navigation.PopAsync();
               // }
            }
        }

        

        //private void CollectFormData()
        //{
        //    //_currentFormData.Pages.Clear();

        //    foreach (var page in _formConfig.SubDocumentModel.Pages)
        //    {
        //        var pageData = new PageData { Name = page.Name };

        //        foreach (var section in page.Sections)
        //        {
        //            var sectionData = new SectionData
        //            {
        //                Name = section.Description,
        //                IsRepeatable = section.IsRepeatable
        //            };

        //            // Collect elements from the section
        //            foreach (var element in section.Elements)
        //            {
        //                var elementData = new ElementData
        //                {
        //                    Name = element.Text,
        //                    Type = element.Type,
        //                    IsMandatory = element.IsMandatory,
        //                    Value = GetElementValue(element)
        //                };
        //                sectionData.Elements.Add(elementData);
        //            }

        //            pageData.Sections.Add(sectionData);
        //        }

        //        _currentFormData.Pages.Add(pageData);
        //    }
        //}

        //private string GetElementValue(ElementModel element)
        //{
        //    // Retrieve value based on control type
        //    switch (element.Type)
        //    {
        //        case "Combo":
        //            //var comboPicker = FindControl(element.UniqueName) as ComboBox;
        //            var comboPicker = FindControlInHierarchy(element,element.UniqueName) as ComboBox;
        //            return comboPicker?.SelectedItem?.ToString();

        //        //case "TextBox":
        //        //    var textBox = FindControl<TextBox>(element.UniqueName);
        //        //    return textBox?.Text;

        //        //case "Date":
        //        //    var datePicker = FindControl<Controls.DatePicker>(element.UniqueName);
        //        //    return datePicker?.Date.ToString();

        //        //case "Photo":
        //        //    var imageSelector = FindControl<ImageSelector>(element.UniqueName);
        //        //    return imageSelector?.ImagePath;

        //        // Add more cases as needed
        //        default:
        //            return string.Empty;
        //    }
        //}

        //private View FindControlInHierarchy(Element element, string automationId)
        //{
        //    if (element == null) return null;

        //    if (element is View view && view.AutomationId == automationId)
        //        return view;

        //    if (element is Layout<View> layout)
        //    {
        //        foreach (var child in layout.Children)
        //        {
        //            var result = FindControlInHierarchy(child, automationId);
        //            if (result != null)
        //                return result;
        //        }
        //    }

        //    return null;
        //}

        //private IView FindControl(string uniqueName) 
        //{
        //    // Implement a method to find controls by their unique name
        //    // This might involve searching through your layout hierarchies
        //    // You'll need to add a Tag or Name property to your controls

        //    // Find the element inside the StackLayout
        //    var element = _mainStack.Children.FirstOrDefault(c => c.AutomationId == uniqueName);
        //    View  c = element as ComboBox;
        //    return element; // Placeholder
        //}



        //private void HighlightMandatoryFields()
        //{
        //    foreach (var page in _currentFormData.Pages)
        //    {
        //        foreach (var section in page.Sections)
        //        {
        //            foreach (var element in section.Elements)
        //            {
        //                if (element.IsMandatory && string.IsNullOrWhiteSpace(element.Value))
        //                {
        //                    //HighlightMandatoryControl(element);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HighlightMandatoryControl(ElementData element)
        //{
        //    // Find the corresponding control and set its border or background to red
        //    //var control = FindControl(element.Name);
        //    //if (control != null)
        //    //{
        //    //    // Implement visual indication for mandatory fields
        //    //    // This might involve setting a red border or changing background color
        //    //}
        //}

        private async Task SaveFormToDatabase(string xmlData)
        {
            try
            {
                // Use your preferred database access method
                inProgress.XmlResults = xmlData;
                InProgressDataHandler handler = new InProgressDataHandler();
                await handler.UpdateAuditInProgress(inProgress);

                //save attachments if any exists
                await SaveAttachments();

            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Save Error", $"Failed to save form: {ex.Message}", "OK");
            }
        }

        private async Task SaveAttachments()
        {
            foreach (var item in _formAttachments)
            {
                string key = item.Key;
                string[] imageFiles = item.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);

                // Loop through each image filename for this key
                foreach (var imageFile in imageFiles)
                {
                    var trimmedFileName = imageFile.Trim(); // Remove any whitespace
                    if (!string.IsNullOrEmpty(trimmedFileName))
                    {
                        try
                        {
                            await InsertImageToDatabase(key, trimmedFileName, inProgress.IdGuid);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error inserting image {trimmedFileName} for key {key}: {ex.Message}");
                            // Handle error as needed
                        }
                    }
                }
            }
        }
        private async Task InsertImageToDatabase(string key, string imageFileName,Guid guid)
        {
            await jobFormHandler.InsertImageForForm(key,imageFileName,guid); 
        }
        //// Method to load existing form data
        //private async Task LoadExistingFormData(string formId)
        //{
        //    var existingEntry = await _databaseService.GetAuditInProgressByIdAsync(formId);
        //    if (existingEntry != null)
        //    {
        //        _currentFormData = FormValidationExtensions.DeserializeFromXml(existingEntry.XmlData);
        //        PopulateFormWithExistingData();
        //    }
        //}

        //private void PopulateFormWithExistingData()
        //{
        //    foreach (var page in _currentFormData.Pages)
        //    {
        //        foreach (var section in page.Sections)
        //        {
        //            foreach (var element in section.Elements)
        //            {
        //                SetControlValue(element);
        //            }
        //        }
        //    }
        //}

        //private void SetControlValue(ElementData elementData)
        //{
        //    // Similar to GetElementValue, but sets the value instead of retrieving
        //    switch (elementData.Type)
        //    {
        //        case "Combo":
        //            var comboPicker = FindControl(elementData.Name) as ComboBox;
        //            if (comboPicker != null)
        //                elementData.Value = comboPicker.SelectedItem.ToString();
        //            break;

        //            // Add similar cases for other control types
        //    }
        //}

        private async Task<View> CreateSectionLayout(SectionModel section)
        {
            var sectionMainStack = new Microsoft.Maui.Controls.StackLayout();

            //if ( !string.IsNullOrEmpty(section.Description) && !section.Description.Equals(" "))
            //{

            var headerStack = new HorizontalStackLayout
            {
                Spacing = 10
            };

            if (!string.IsNullOrEmpty(section.Description) && !section.Description.Equals(" "))
            {
                headerStack.Children.Add(new Label
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Text = section.Description,
                    FontSize = 15,
                    TextColor = Color.FromArgb("#00B1E3"),
                    FontAttributes = FontAttributes.Bold
                });
            }
            else
            {
                headerStack.Children.Add(new Label
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Text = section.Elements[0].Text,
                    TextColor = Colors.Black,
                    FontAttributes = FontAttributes.Bold
                });
            }

            if (section.IsRepeatable)
            {
                var addButton = new Image
                {
                    Source = "add",
                    Margin = new Thickness(5, 8, 0, 0),
                    BackgroundColor = Colors.Transparent
                };

                headerStack.Children.Add(addButton);
                var addGesture = new TapGestureRecognizer();
                addGesture.Tapped += async (s, e) => await AddRepeatableSection(section, sectionMainStack);
                headerStack.GestureRecognizers.Add(addGesture);

                //addButton.Clicked += async (s, e) => await AddRepeatableSection(section, sectionMainStack);
                //headerStack.Children.Add(addButton);
            }
            sectionMainStack.Children.Add(headerStack);
            //}

            var repeatableSectionLayout = new Microsoft.Maui.Controls.StackLayout();

            // Add the initial section content
            var initialContent = await CreateSectionContent(section, repeatableSectionLayout, false,null);
            repeatableSectionLayout.Children.Add(initialContent);

            sectionMainStack.Children.Add(repeatableSectionLayout);
            return sectionMainStack;
        }

        private async Task<View> CreateSectionContent(SectionModel section, Microsoft.Maui.Controls.StackLayout container, bool isRepeating,RepeatableInstance newInstance)
        {
            try
            {
                var contentStack = new Microsoft.Maui.Controls.StackLayout
                {
                    Spacing = 5
                };

                var headerStack = new HorizontalStackLayout
                {
                    Spacing = 10
                };

                if (section.IsRepeatable)
                {
                    if (isRepeating)
                    {
                        if (!string.IsNullOrEmpty(section.Description) && !section.Description.Equals(" "))
                        {
                            headerStack.Children.Add(new Label
                            {
                                Margin = new Thickness(0, 10, 0, 0),
                                Text = section.Description,
                                TextColor = Colors.Black,
                                FontAttributes = FontAttributes.Bold
                            });
                        }
                        else
                        {
                            headerStack.Children.Add(new Label
                            {
                                Margin = new Thickness(0, 10, 0, 0),
                                Text = section.Elements[0].Text,
                                TextColor = Colors.Black,
                                FontAttributes = FontAttributes.Bold
                            });
                        }

                        var deleteButton = new Image
                        {
                            Source = "delete",
                            Margin = new Thickness(5, 4, 0, 0),
                            BackgroundColor = Colors.Transparent,
                        };
                        headerStack.Children.Add(deleteButton);
                        var deleteGesture = new TapGestureRecognizer();
                        deleteGesture.Tapped += (s, e) => DeleteRepeatableSection(contentStack, container, newInstance, section);
                        headerStack.GestureRecognizers.Add(deleteGesture);
                    }

                    //deleteButton.Clicked += (s, e) => DeleteRepeatableSection(contentStack, container);
                    //contentStack.Children.Add(deleteButton);
                }
                else
                {
                    // this is not repeatable section. want to render it as normal element only once
                }
                contentStack.Children.Add(headerStack);


                
                if (newInstance != null)
                {
                    // Render section elements
                    foreach (var element in newInstance.Elements)
                    {
                        if (!(bool)section.ReportOnly)
                        {
                            var control = await CreateControl(element);
                            contentStack.Children.Add(control);

                        }
                    }
                }
                else
                {
                    // Render section elements
                    foreach (var element in section.Elements)
                    {
                        if (!(bool)section.ReportOnly)
                        {
                            var control = await CreateControl(element);
                            contentStack.Children.Add(control);

                        }
                    }
                }
                // Add any remaining media elements in the grid
                //if (hasMediaElements && mediaCount > 0)
                //{
                //    contentStack.Children.Add(currentMediaGrid);
                //}
                return contentStack;
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return null;
            }
        }
        private async Task AddRepeatableSection(SectionModel section, Microsoft.Maui.Controls.StackLayout parentStack)
        {
            var newInstance = new RepeatableInstance
            {
                Score = section.RepeatableInstances.Count,
                Elements = section.Elements.Select(e => new ElementModel
                {
                    UniqueName = e.UniqueName,
                    Type = e.Type,
                    Text = e.Text,
                    Value = null,  // New instance starts with empty values
                    AllowCreateNew = e.AllowCreateNew,
                    DefaultValue = e.DefaultValue,
                    ListId = e.ListId,
                    MaxLength = e.MaxLength,
                    MinValue = e.MinValue,
                    MaxValue = e.MaxValue,
                    IsMandatory = e.IsMandatory,
                    Lines = e.Lines,
                    PenWidth = e.PenWidth,
                    ArrangeHorizontally = e.ArrangeHorizontally,
                    UseListItemImages = e.UseListItemImages,
                    ReportOnly = e.ReportOnly,
                    Condition0 = e.Condition0,
                    Condition1 = e.Condition1,
                    TextCol0 = e.TextCol0,
                    EntityType0 = e.EntityType0,
                    EntityType1 = e.EntityType1,
                    OutputField = e.OutputField,
                    Calculation = e.Calculation,
                    ResultType = e.ResultType,
                    CurrencySymbol = e.CurrencySymbol,
                    ExternalSystemField = e.ExternalSystemField,
                    GPSUse = e.GPSUse,
                    NetworkUse = e.NetworkUse,
                    MinuteIncrement = e.MinuteIncrement,
                    CollectResultsAnyway = e.CollectResultsAnyway
                }).ToList()
            };
            // Add to our model
            section.RepeatableInstances.Add(newInstance);

            var container = parentStack.Children.LastOrDefault() as Microsoft.Maui.Controls.StackLayout;
            if (container != null)
            {
                var newContent = await CreateSectionContent(section, container, true, newInstance);
                container.Children.Add(newContent);
                UpdateDeleteButtonVisibility(container);
            }
        }

        private void DeleteRepeatableSection(View sectionContent, Microsoft.Maui.Controls.StackLayout container,RepeatableInstance instance,SectionModel section)
        {
            // Remove from the model
            section.RepeatableInstances.Remove(instance);

            if (container.Children.Count > 1)  // Ensure at least one section remains
            {
                container.Children.Remove(sectionContent);
                UpdateDeleteButtonVisibility(container);
            }
        }

        private void UpdateDeleteButtonVisibility(Microsoft.Maui.Controls.StackLayout container)
        {
            // Hide delete buttons if only one section remains
            bool shouldShowDelete = container.Children.Count > 1;

            foreach (var child in container.Children)
            {
                if (child is Microsoft.Maui.Controls.StackLayout contentStack)
                {
                    var deleteButton = contentStack.Children.FirstOrDefault() as ImageButton;
                    if (deleteButton != null)
                    {
                        deleteButton.IsVisible = shouldShowDelete;
                    }
                }
            }
        }
        private async Task<View> CreateControl(ElementModel element)
        {
            //if (element.Condition0==null || !element.Condition0.Equals("resultValueX;dummy;equals;dummy"))
            //{
            if (element.Condition0 == null || element.Condition0.Equals(""))
            {
                var control = await CreateControlForBoth(element);
                //control.IsVisible = true;
                
                return control;
            }
            else
            {
                //resultValue;dummy;equals;dummy
                if (!element.Condition0.Equals("resultValueX;dummy;equals;dummy") || !element.Condition0.Equals("resultValue;dummy;equals;dummy"))
                {
                    var control = await CreateControlForBoth(element);
                    //element.IsVisible = false;
                    control.IsVisible = false;
                    return control;
                }
                return null;
            }
        }
        //private Dictionary<string, ElementModel> _elements = new();
        
        //private ConditionEvaluator _conditionEvaluator;
        private async Task<View> CreateControlForBoth(ElementModel element)
        {
            var control = await MainThread.InvokeOnMainThreadAsync(async () => element.Type switch
            {
                "Combo" => await CreateComboBox(element),
                "TextBox" => await CreateTextBox(element),
                "Integer" => await CreateIntegerEntry(element),
                "Photo" => await CreatePhotoUpload(element),
                "StaticText" => await CreateStaticLabel(element),
                "Video" => await CreateVideoUpload(element),
                "Date" => await CreateDateField(element),
                "Signature" => await CreateSignatureField(element),
                "MultiChoice" => await CreateMultiChoice(element),
                "OutputField" => await CreateOutputField(element),
                "ActionButtons" => await CreateActionButtons(element),
                "Score" => await CreateScore(element),
                "GPSEarliest" => await CreateGPSEarliest(element),
                "GPSLatest" => await CreateGPSLatest(element),
                "Time" => await CreateTime(element),
                "GenericAttachments" => await CreateGenericAttachments(element),
                //"ExternalSystemField" => await CreateExternalSystemField(element

                _ => null
            });

            if (control != null)
            {
                control.AutomationId = element.UniqueName;
                //control.SetBinding(View.IsVisibleProperty, new Binding(nameof(ElementModel.IsVisible), source: element));
                BindControlToElement(control, element);

                _controls[element.UniqueName] = control;
            }
            return control;
        }

        private void BindControlToElement(View control, ElementModel element)
        {
            
            switch (control)
            {
                case ComboBox combo:
                    combo.SelectedIndexChanged += (s, e) =>
                    {
                        if (e.SelectedItem is GenericLists)
                        {
                            var item = e.SelectedItem as GenericLists;
                            element.UpdateValue(item.Id.ToString());
                            UpdateElementsVisibility(combo.AutomationId, item.Id.ToString());
                        }
                    };
                    break;

                case TextBox textBox:
                    textBox.TextChanged += (s, e) =>
                    {
                        try
                        {
                            element.UpdateValue(textBox.EnteredValue);
                            UpdateElementsVisibility(textBox.AutomationId, textBox.EnteredValue);
                        }
                        catch(Exception ex) {
                            Console.WriteLine("Exception :" + ex); 
                        }
                    };
                    break;

                case Controls.Location location:
                    location.TextChanged += (s, e) =>
                    {
                        element.UpdateValue(location.EnteredValue);
                        UpdateElementsVisibility(location.AutomationId, location.EnteredValue);

                    };
                    break;

                case Controls.DatePicker datePicker:
                    datePicker.DateSelected += (s, e) =>
                    {
                        element.UpdateValue(e.NewDate.ToString("yyyy-MM-dd"));
                        UpdateElementsVisibility(datePicker.AutomationId, e.NewDate.ToString("yyyy-MM-dd"));
                    };
                    break;

                case Controls.TimeSelector timePicker:
                    timePicker.Time += (s, time) =>
                    {
                        element.UpdateValue(time);
                        UpdateElementsVisibility(timePicker.AutomationId, time);

                    };
                    break;

                case ImageSelector imageSelector:
                    imageSelector.ImageSelected += (s, path) =>
                    {
                        if (_formAttachments.TryGetValue(imageSelector.AutomationId, out var existingImagePaths))
                        {
                            if (!string.IsNullOrEmpty(existingImagePaths))
                            {
                                _formAttachments[imageSelector.AutomationId] = $"{existingImagePaths},{path}";
                            }
                            //_formAttachments[imageSelector.AutomationId] = path;
                        }
                        else
                        {
                            _formAttachments[imageSelector.AutomationId] = path;
                        }

                        element.UpdateValue("1");
                        UpdateElementsVisibility(imageSelector.AutomationId, path);
                    };
                    break;
                case GPSLocation gpsLocation:
                    gpsLocation.LocationChanged += (s, location) =>
                    {
                        element.UpdateValue(location);
                        UpdateElementsVisibility(gpsLocation.AutomationId, location);
                    };
                    break;
            }
        }

        void UpdateElementsVisibility(string automationId,string value)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var dependentControls = _formConfig.SubDocumentModel.Pages
                        .SelectMany(p => p.Sections)
                        .SelectMany(s => s.Elements)
                        .Where(e => !string.IsNullOrEmpty(e.Condition0) &&
                                   e.Condition0.Contains($";{automationId}"));

                    foreach (var dependent in dependentControls)
                    {
                        if (_controls.TryGetValue(dependent.UniqueName, out var dependentControl))
                        {
                            dependentControl.IsVisible = !string.IsNullOrEmpty(value);
                        }
                    }
                }
            });
            
        }
        private async Task<View> CreateComboBox(ElementModel element)
        {
            var picker = new ComboBox
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //IsRequired = element.IsMandatory
                ItemsSource = await jobFormHandler.GetComboBoxData(Convert.ToInt32(element.ListId))
            };

            // TODO: Populate picker from ListId
            return picker;
        }

        private async Task<View> CreateTextBox(ElementModel element)
        {
            if (element.Text.Equals("Location"))
            {
                return new Controls.Location
                {
                    Title = element.Text,
                    IsMandatory = element.IsMandatory,
                };
            }
            else {
                return new TextBox
                {
                    Title = element.Text,
                    IsMandatory = element.IsMandatory,
                    Lines = element.Lines,
                    KeyboardType = Keyboard.Text,
                    MaxLength = int.TryParse(element.MaxLength, out int max) ? max : 250
                };
            }
        }

        private async Task<View> CreateIntegerEntry(ElementModel element)
        {
            return new TextBox
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                KeyboardType = Keyboard.Numeric,
                MaxLength = int.TryParse(element.MaxLength, out int max) ? max : 250
            };
        }

        private async Task<View> CreatePhotoVideoUpload(ElementModel element)
        {
            return new VideoSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private async Task<View> CreateVideoUpload(ElementModel element)
        {
            await HandleImagePermission();
            return new VideoSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private async Task<View> CreateSignatureField(ElementModel element)
        {
            var stack = new Microsoft.Maui.Controls.StackLayout();

            var title = new Label
            {
                Text = element.Text,
                TextColor = Colors.Black
            };
            stack.Children.Add(title);
            var sign = new DrawingView
            {
                //Lines = new ObservableCollection<IDrawingLine>(),
                LineColor = Colors.Black,
                LineWidth = 5,
                HeightRequest = 100,
                BackgroundColor = Colors.AliceBlue,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            stack.Children.Add(sign);
            return stack;
        }

        private async Task<View> CreateMultiChoice(ElementModel element)
        {
            return new MultiChoiceSelector
            {
                //Lines = new ObservableCollection<IDrawingLine>(),
                //LineColor = Colors.Black,
                //LineWidth = 5,
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                ItemsSource = await jobFormHandler.GetComboBoxData(Convert.ToInt32(element.ListId))

            };
        }
        private async Task<View> CreateExternalSystemField(ElementModel element)
        {
            return new MultiChoiceSelector
            {
                //Lines = new ObservableCollection<IDrawingLine>(),
                //LineColor = Colors.Black,
                //LineWidth = 5,
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //ItemsSource = await jobFormHandler.GetComboBoxData(Convert.ToInt32(element.EternalSystemField))

            };
        }



        private async Task<View> CreateOutputField(ElementModel element)
        {
            if (element.Text.Equals("Date"))
            {
                var stack = new Microsoft.Maui.Controls.StackLayout();

                var titleLabel = new Label
                {
                    Text = element.Text,
                    TextColor = Colors.Black,
                };
                stack.Children.Add(titleLabel);

                var dateLabel = new Label
                {
                    Text = DateTime.Now.ToString("dd/MM/yyyy"),
                    TextColor = Colors.Black,

                };
                stack.Children.Add(dateLabel);
                return stack;
            }

            else
            {
                var stack = new Microsoft.Maui.Controls.StackLayout();

                var titleLabel = new Label
                {
                    Text = element.Text,
                    TextColor = Colors.Black,
                };
                stack.Children.Add(titleLabel);
                if (element.OutputField != null && !element.OutputField.Equals("-1"))
                {

                    var result = await jobFormHandler.GetOutputFielddata(element.OutputField, workOrder.workOrder.LocationId, workOrder.workOrder.CustomerId, workOrder.workOrder.UserId);

                    var dateLabel = new Label
                    {

                        Text = result, // TODO bind with list ID in the element get the name 
                        TextColor = Colors.Black,

                    };
                    stack.Children.Add(dateLabel);
                }

                return stack;
            }

        }

        private async Task<View> CreateActionButtons(ElementModel element)
        {
            return new Button
            {
                Text = element.TextCol0,
                HorizontalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromArgb("#00B1E3"),
                Padding = new Thickness(50, 2, 50, 2),
                IsVisible = string.IsNullOrEmpty(element.Condition0) ? true : false,
            };

        }
        private async Task<View> CreateScore(ElementModel element)
        {
            return new Label
            {
                Text = element.Text,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
            };

        }

        private async Task<View> CreateGPSLatest(ElementModel element)
        {
            await HandleGPSPermission();
            var control = new BorisMobile.Controls.GPSLocation()
            {
                Title = element.Text.ToUpper(),
                IsMandatory = element.IsMandatory,
            };
            //var stack = new Microsoft.Maui.Controls.StackLayout();
            //stack.Orientation = StackOrientation.Horizontal;
            //var titleLabel = new Label
            //{
            //    Text = "GPS",
            //    TextColor = Colors.Black,
            //    HorizontalOptions = LayoutOptions.Start,
            //};
            //stack.Children.Add(titleLabel);

            //var responseLabel = new Label
            //{
            //    Text = "Location Recorded",
            //    FontAttributes = FontAttributes.Bold,
            //    TextColor = Colors.Black,
            //    HorizontalOptions = LayoutOptions.Start,
            //};
            //stack.Children.Add(responseLabel);
            //return stack;
            return control;
        }

        private async Task<View> CreateGPSEarliest(ElementModel element)
        {
            await HandleGPSPermission();
            var stack = new Microsoft.Maui.Controls.StackLayout();

            var titleLabel = new Label
            {
                Text = element.Text,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
            };
            stack.Children.Add(titleLabel);

            var responseLabel = new Label
            {
                Text = "Location Recorded",
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.Black,
                HorizontalOptions = LayoutOptions.Start,
            };
            stack.Children.Add(responseLabel);
            return stack;
        }

        private async Task<View> CreateTime(ElementModel element)
        {
            //var stack = new StackLayout();

            //var titleLabel = new Label
            //{
            //    Text = element.Text,
            //    TextColor = Colors.Black,
            //    HorizontalOptions = LayoutOptions.Start,
            //};
            //stack.Children.Add(titleLabel);

            //var timePicker = new TimePicker
            //{
            //    TextColor = Colors.Black,
            //    HorizontalOptions = LayoutOptions.FillAndExpand,
            //};
            //stack.Children.Add(timePicker);
            //return stack;

            return new TimeSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
            };
        }
        private async Task<View> CreateGenericAttachments(ElementModel element)
        {
            DocumentsService documentsService = new DocumentsService(workOrder);
            List<Attachments> list = await documentsService.GetDocumentsList();

            var stack = new Microsoft.Maui.Controls.StackLayout();
            foreach (Attachments attachment in list)
            {
                var button = new Button
                {
                    Text = attachment.DisplayName,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Background = Color.FromArgb("#00B1E3")
                };
                button.Clicked += async (sender, e) => await OpenFile(attachment);
                stack.Children.Add(button);
            }
            return stack;
        }

        private async Task OpenFile(Attachments attachments)
        {
            string filePath = Path.Combine(FilesHelper.GetAttachmentDirectoryMAUI(), attachments.FileName.ToLower());

            if (File.Exists(filePath))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                });
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "File not found!", "OK");
            }

        }

        private async Task<View> CreateDateField(ElementModel element)
        {
            return new Controls.DatePicker
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private async Task<View> CreatePhotoUpload(ElementModel element)
        {
            await HandleImagePermission();
            return new ImageSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                IsVisible = true,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private async Task<View> CreateStaticLabel(ElementModel element)
        {
            if (!element.Text.Equals("\n")) {
                return new Label
                {
                    Text = element.Text,
                    TextColor = Colors.Black,
                    //IsMandatory = element.IsMandatory,
                };
            }
            return null;
        }

        private void TakePhoto(string uniqueName)
        {
            // Implement photo capture logic
        }

        private async Task<Microsoft.Maui.Controls.Page> CreateSinglePage(FormConfigModel formConfig) {

            try
            {
                var DynamicPage = new ContentPage { };

                var mainstack = new Microsoft.Maui.Controls.StackLayout
                {
                    Padding = 10
                };

                var bodyStack = new Microsoft.Maui.Controls.StackLayout
                {
                    Margin = new Thickness(10, 15, 10, 0),
                    VerticalOptions = LayoutOptions.FillAndExpand
                };
                //Set Form Header
                var headerLabel = new Label { Text = formConfig.SubDocumentModel.Name, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 16 };
                bodyStack.Children.Add(headerLabel);

                // Dynamic data for sections

                // Content layout to display the currently selected section's content
                var contentLayout = new Microsoft.Maui.Controls.StackLayout { Margin = new Thickness(0, 0, 0, 100) };

                // Loop through sections to create UI
                foreach (var page in formConfig.SubDocumentModel.Pages)
                {

                    // Content for the section

                    var stack = new Microsoft.Maui.Controls.StackLayout { Margin = new Thickness(0, 10, 0, 0) };

                    foreach (var section in page.Sections)
                    {
                        if (!(bool)section.ReportOnly)
                        {
                            if (section.Condition0 == null || section.Condition0.Equals(""))
                            {
                                var sectionLayout = await CreateSectionLayout(section);
                                stack.Children.Add(sectionLayout);
                            }
                        }
                    }

                    var saveButton = new Button
                    {
                        CornerRadius = 10,
                        Margin = new Thickness(0, 15, 0, 100),
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        Text = "Save",
                        BackgroundColor = Color.FromHex("#00B1E3")
                    };
                    saveButton.Clicked += OnSaveButtonClicked;
                    stack.Children.Add(saveButton);

                    var mainLayout = new Microsoft.Maui.Controls.ScrollView
                    {
                        HeightRequest = DeviceDisplay.MainDisplayInfo.Height * 0.3,
                        Content = stack,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Never,
                        Margin = new Thickness(0, 0, 0, 300)

                    };

                    var sectionContent = mainLayout;

                    // Add content to the content layout
                    contentLayout.Children.Add(sectionContent);
                }
                // Add sections and content layout to the main layout
                bodyStack.Children.Add(contentLayout);

                mainstack.Children.Add(bodyStack);

                DynamicPage.Content = mainstack;

                return DynamicPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error CreateSinglePage: {ex}");
                return null;
            }
        }

    #region helper methods
    async Task HandleGPSPermission()
        {
            bool isLocationGranted = await PermissionHelper.CheckAndRequestPermission<Permissions.LocationWhenInUse>();

            //await PermissionHelper.CheckAndRequestPermission<Permissions.Camera>();
            //await PermissionHelper.CheckAndRequestPermission<Permissions.StorageRead>();

            if (!isLocationGranted)
            {
                //await App.Current.MainPage.DisplayAlert("Permission Denied",
                    //"GPS functionality will not work without location access.", "OK");
            }
        }

        async Task HandleImagePermission()
        {
            //bool isLocationGranted = await PermissionHelper.CheckAndRequestPermission<Permissions.LocationWhenInUse>();

            await PermissionHelper.CheckAndRequestPermission<Permissions.Camera>();
            await PermissionHelper.CheckAndRequestPermission<Permissions.StorageRead>();

            //if (!isLocationGranted)
            //{
            //    //await App.Current.MainPage.DisplayAlert("Permission Denied",
            //    //"GPS functionality will not work without location access.", "OK");
            //}
        }
        #endregion
    }
}
