using BorisMobile.Controls;
using BorisMobile.DataHandler;
using BorisMobile.Models.DynamicFormModels;
using BorisMobile.Services.Interfaces;

namespace BorisMobile.Services
{
    public class FormGenerationService : IFormGenerationService
    {
        JobFormHandler jobFormHandler;
        public FormGenerationService() {
           jobFormHandler =new JobFormHandler();
        }
        public async Task<Page> CreateDynamicForm(FormConfigModel formConfig)
        {
            return formConfig.SubDocumentModel.Pages.Count > 1
            ? await CreateTabbedPage(formConfig)
            : CreateSinglePage(formConfig);
        }

        private async Task<Page> CreateTabbedPage(FormConfigModel formConfig) {

            var DynamicPage = new ContentPage { };

            var mainstack = new StackLayout
            {
                Padding = 10
            };

            //var grid = new Grid
            //{
            //    ColumnDefinitions = new ColumnDefinitionCollection
            //    {
            //        new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) },
            //        new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) },
            //        new ColumnDefinition { Width = new GridLength(0.7, GridUnitType.Star) }
            //    },
            //    RowDefinitions = new RowDefinitionCollection
            //    {
            //        new RowDefinition { Height = new GridLength(50, GridUnitType.Absolute) }
            //    }
            //};
            //var label1 = new Label { Padding = 10, FontFamily = "FaSolid", TextColor = Colors.Black, VerticalTextAlignment = TextAlignment.Center };
            //// Retrieve the backarrow resource from the resource dictionary
            //if (Microsoft.Maui.Controls.Application.Current.Resources.TryGetValue("backarrow", out var backArrowIcon))
            //{
            //    label1.Text = backArrowIcon.ToString();
            //}
            //else
            //{
            //    Console.WriteLine("Resource 'backarrow' not found!");
            //}

            //// Create the TapGestureRecognizer
            //var tapGesture = new TapGestureRecognizer();
            //tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "BackButtonClickCommand");
            //label1.GestureRecognizers.Add(tapGesture);

            //Grid.SetColumn(label1, 0);
            //Grid.SetRow(label1, 0);
            //grid.Children.Add(label1);

            //var label2 = new Label { VerticalTextAlignment = TextAlignment.Center, FontAttributes = FontAttributes.Bold, Text = formConfig.DocumentDescription, TextColor = Colors.Black, FontSize = 17 };
            //Grid.SetColumn(label2, 1);
            //Grid.SetRow(label2, 0);
            //grid.Children.Add(label2);

            //mainstack.Children.Add(grid);

            var bodyStack = new StackLayout
            {
                Margin = new Thickness(10, 15, 10, 0),
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            //Set Form Header
            var headerLabel = new Label { Text = formConfig.SubDocumentModel.Name, TextColor = Colors.Black,FontAttributes = FontAttributes.Bold, FontSize=16 };
            bodyStack.Children.Add(headerLabel);


            // Horizontal layout for section labels
            var sectionLabelsLayout = new HorizontalStackLayout
            {
                Spacing = 20, // Space between labels
                //HorizontalOptions = LayoutOptions.Center
                Margin=new Thickness(0,10,0,0)
            };

            
            // Dynamic data for sections
            
            // Dictionary to keep track of section labels and their corresponding content
            var sectionLabels = new Dictionary<Label, View>();

            // Content layout to display the currently selected section's content
            var contentLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.FillAndExpand };

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
                

                var stack = new StackLayout { Margin = new Thickness(0, 10, 0, 0) };

                foreach (var section in page.Sections)
                {
                    var sectionLayout = await CreateSectionLayout(section);
                    stack.Children.Add(sectionLayout);
                }
                var mainLayout = new ScrollView
                {
                    HeightRequest = DeviceDisplay.MainDisplayInfo.Height * 0.3,
                    Content = stack,
                    Margin = new Thickness(0,0,0,20)
                    
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
            bodyStack.Children.Add(sectionLabelsLayout);
            bodyStack.Children.Add(contentLayout);
            
            mainstack.Children.Add(bodyStack);

            DynamicPage.Content = mainstack;

            return DynamicPage;

        }


        private async Task<View> CreateSectionLayout(SectionModel section)
        {
            var sectionMainStack = new StackLayout();
            sectionMainStack.Children.Add(new Label
            {
                Margin=new Thickness(0,10,0,0),
                Text = section.Description,
                TextColor = Colors.Black,
                FontAttributes = FontAttributes.Bold
            });

            

            var sectionLayout = new VerticalStackLayout();

            // Section Title
            // Render section elements
            foreach (var element in section.Elements)
            {
                var control = await CreateControl(element);
                if (control != null)
                    sectionLayout.Children.Add(control);
            }
            sectionMainStack.Children.Add(sectionLayout);
            return sectionMainStack;
        }

        private async Task<View> CreateControl(ElementModel element)
        {
            return element.Type switch
            {
                "Combo" => await CreateComboBox(element),
                "TextBox" => CreateTextBox(element),
                "Integer" => CreateIntegerEntry(element),
                "Photo" => CreatePhotoUpload(element),
                "StaticText" => CreateStaticLabel(element),
                "Video" => CreateVideoUpload(element),
                "Date" => CreateDateField(element),
                _ => null
            };
        }

        private async Task<View> CreateComboBox(ElementModel element)
        {
            if (jobFormHandler == null)
                jobFormHandler = new JobFormHandler() ;
            var picker = new ComboBox
            {
                Title = element.Text,
                //IsRequired = element.IsMandatory
                ItemsSource = await jobFormHandler.GetComboBoxData(Convert.ToInt32(element.ListId))
            };

            // TODO: Populate picker from ListId
            return picker;
        }

        private View CreateTextBox(ElementModel element)
        {
            if (element.Text.Equals("Location"))
            {
                return new Controls.Location
                {
                    Title = element.Text
                };
            }
            else {
                return new TextBox
                {
                    Title = element.Text,
                    Lines = element.Lines,
                    KeyboardType = Keyboard.Text,
                    MaxLength = int.TryParse(element.MaxLength, out int max) ? max : 250
                };
            }
        }

        private View CreateIntegerEntry(ElementModel element)
        {
            return new TextBox
            {
                Title = element.Text,
                KeyboardType = Keyboard.Numeric,
                MaxLength = int.TryParse(element.MaxLength, out int max) ? max : 250
            };
        }

        
        private View CreateVideoUpload(ElementModel element)
        {
            return new VideoSelector
            {
                Title = element.Text,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private View CreateDateField(ElementModel element)
        {
            return new Controls.DatePicker
            {
                Title = element.Text,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private View CreatePhotoUpload(ElementModel element)
        {
            return new ImageSelector
            {
                Title = element.Text
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private View CreateStaticLabel(ElementModel element)
        {
            if (!element.Text.Equals("\n")) { 
                return new Label
                {
                    Text = element.Text
                };
            }
            return  null;
        }

        private void TakePhoto(string uniqueName)
        {
            // Implement photo capture logic
        }

        private Page CreateSinglePage(FormConfigModel formConfig) { return null;/* Implementation */ }

    }
}
