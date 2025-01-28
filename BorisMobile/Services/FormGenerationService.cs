using BorisMobile.Controls;
using BorisMobile.DataHandler;
using BorisMobile.Models.DynamicFormModels;
using BorisMobile.Services.Interfaces;
using CommunityToolkit.Maui.Core;
using System.Collections.ObjectModel;

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
            : await CreateSinglePage(formConfig);
        }

        private async Task<Page> CreateTabbedPage(FormConfigModel formConfig) {

            var DynamicPage = new ContentPage { };

            var mainstack = new StackLayout
            {
                Padding = 10
            };

            var bodyStack = new StackLayout
            {
                Margin = new Thickness(10, 15, 10, 0),
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            //Set Form Header
            var headerLabel = new Label { Text = formConfig.SubDocumentModel.Name, TextColor = Colors.Black,FontAttributes = FontAttributes.Bold, FontSize=16 };
            bodyStack.Children.Add(headerLabel);


            var tabScrollView = new ScrollView
            {
                Orientation = ScrollOrientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0)

            };

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

                var saveButton = new Button
                {
                    CornerRadius = 10,
                    Margin = new Thickness(0, 15, 0, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Text = "Save",
                    BackgroundColor = Color.FromHex("#00B1E3")
                };
                stack.Children.Add(saveButton);

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
            tabScrollView.Content = sectionLabelsLayout;
            bodyStack.Children.Add(tabScrollView);
            bodyStack.Children.Add(contentLayout);

            mainstack.Children.Add(bodyStack);

            DynamicPage.Content = mainstack;

            return DynamicPage;

        }


        private async Task<View> CreateSectionLayout(SectionModel section)
        {
            var sectionMainStack = new StackLayout();

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

            var repeatableSectionLayout = new VerticalStackLayout();

            // Add the initial section content
            var initialContent = await CreateSectionContent(section, repeatableSectionLayout,false);
            repeatableSectionLayout.Children.Add(initialContent);

            sectionMainStack.Children.Add(repeatableSectionLayout);
            return sectionMainStack;

            
            
        }

        private async Task<View> CreateSectionContent(SectionModel section, VerticalStackLayout container, bool isRepeating)
        {
            var contentStack = new VerticalStackLayout
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
                    deleteGesture.Tapped += (s, e) => DeleteRepeatableSection(contentStack, container);
                    headerStack.GestureRecognizers.Add(deleteGesture);
                }

                //deleteButton.Clicked += (s, e) => DeleteRepeatableSection(contentStack, container);
                //contentStack.Children.Add(deleteButton);
            }
            contentStack.Children.Add(headerStack);


            // Create a horizontal container for media elements
            var currentMediaGrid = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            },
                ColumnSpacing = 10
            };

            int mediaCount = 0;
            bool hasMediaElements = false;

            // Render section elements
            foreach (var element in section.Elements)
            {
                var control = await CreateControl(element);
                if (control != null)
                {
                    if (element.Type == "Photo" || element.Type == "Video")
                    {
                        hasMediaElements = true;
                        // Add to the grid
                        Grid.SetColumn(control, mediaCount % 2);
                        if (mediaCount % 2 == 0 && mediaCount > 0)
                        {
                            // Add the current grid and create a new one
                            contentStack.Children.Add(currentMediaGrid);
                            currentMediaGrid = new Grid
                            {
                                ColumnDefinitions =
                        {
                            new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                            new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                        },
                                ColumnSpacing = 10,
                                Margin = new Thickness(0, 10, 0, 0)
                            };
                        }
                        currentMediaGrid.Children.Add(control);
                        mediaCount++;
                    }
                    else
                    {
                        // If we have pending media elements, add the grid first
                        if (hasMediaElements && mediaCount > 0)
                        {
                            contentStack.Children.Add(currentMediaGrid);
                            currentMediaGrid = new Grid
                            {
                                ColumnDefinitions =
                        {
                            new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                            new ColumnDefinition(new GridLength(1, GridUnitType.Star))
                        },
                                ColumnSpacing = 10
                            };
                            mediaCount = 0;
                            hasMediaElements = false;
                        }
                        // Add non-media elements directly to the stack
                        contentStack.Children.Add(control);
                    }

                    //contentStack.Children.Add(control);
                }
            }
            // Add any remaining media elements in the grid
            if (hasMediaElements && mediaCount > 0)
            {
                contentStack.Children.Add(currentMediaGrid);
            }

            return contentStack;

        }
        private async Task AddRepeatableSection(SectionModel section, StackLayout parentStack)
        {
            var container = parentStack.Children.LastOrDefault() as VerticalStackLayout;
            if (container != null)
            {
                var newContent = await CreateSectionContent(section, container,true);
                container.Children.Add(newContent);
                UpdateDeleteButtonVisibility(container);
            }
        }

        private void DeleteRepeatableSection(View sectionContent, VerticalStackLayout container)
        {
            if (container.Children.Count > 1)  // Ensure at least one section remains
            {
                container.Children.Remove(sectionContent);
                UpdateDeleteButtonVisibility(container);
            }
        }

        private void UpdateDeleteButtonVisibility(VerticalStackLayout container)
        {
            // Hide delete buttons if only one section remains
            bool shouldShowDelete = container.Children.Count > 1;

            foreach (var child in container.Children)
            {
                if (child is VerticalStackLayout contentStack)
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
            return element.Type switch
            {
                "Combo" => await CreateComboBox(element),
                "TextBox" => await CreateTextBox(element),
                "Integer" => await CreateIntegerEntry(element),
                "Photo"  => await CreatePhotoUpload(element),
                "StaticText" => await CreateStaticLabel(element),
                "Video" => await CreateVideoUpload(element),
                "Date" => await CreateDateField(element),
                "Signature" => await CreateSignatureField(element),
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
            return new VideoSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
                //Command = new Command(() => TakePhoto(element.UniqueName))
            };
        }

        private async Task<View> CreateSignatureField(ElementModel element)
        {
            return new CommunityToolkit.Maui.Views.DrawingView
            {
                //Lines = new ObservableCollection<IDrawingLine>(),
                LineColor = Colors.Black,
                LineWidth = 5,
                HeightRequest = 100,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
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
            return new ImageSelector
            {
                Title = element.Text,
                IsMandatory = element.IsMandatory,
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
            return  null;
        }

        private void TakePhoto(string uniqueName)
        {
            // Implement photo capture logic
        }

        private async Task<Page> CreateSinglePage(FormConfigModel formConfig) {

            var DynamicPage = new ContentPage { };

            var mainstack = new StackLayout
            {
                Padding = 10
            };

            var bodyStack = new StackLayout
            {
                Margin = new Thickness(10, 15, 10, 0),
                VerticalOptions = LayoutOptions.FillAndExpand
            };
            //Set Form Header
            var headerLabel = new Label { Text = formConfig.SubDocumentModel.Name, TextColor = Colors.Black, FontAttributes = FontAttributes.Bold, FontSize = 16 };
            bodyStack.Children.Add(headerLabel);

            // Dynamic data for sections

            // Content layout to display the currently selected section's content
            var contentLayout = new VerticalStackLayout { VerticalOptions = LayoutOptions.FillAndExpand };

            // Loop through sections to create UI
            foreach (var page in formConfig.SubDocumentModel.Pages)
            {
                
                // Content for the section

                var stack = new StackLayout { Margin = new Thickness(0, 10, 0, 0) };

                foreach (var section in page.Sections)
                {
                    var sectionLayout = await CreateSectionLayout(section);
                    stack.Children.Add(sectionLayout);
                }

                var saveButton = new Button
                {
                    CornerRadius = 10,
                    Margin = new Thickness(0, 15, 0, 10),
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    Text = "Save",
                    BackgroundColor = Color.FromHex("#00B1E3")
                };
                stack.Children.Add(saveButton);

                var mainLayout = new ScrollView
                {
                    HeightRequest = DeviceDisplay.MainDisplayInfo.Height * 0.3,
                    Content = stack,
                    Margin = new Thickness(0, 0, 0, 20)

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

    }
}
