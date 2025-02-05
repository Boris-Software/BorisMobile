using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BorisMobile.Controls;

public partial class ImageSelector : ContentView
{
	public ImageSelector()
	{
		InitializeComponent();
        pickerPopup.IsVisible = false;
        SelectedImages.CollectionChanged += OnImagesCollectionChanged;
        ShowPopupCommand = new Command(ShowPopup);

    }
    public Command ShowPopupCommand { get; private set; }

    private string _Title;
    public string Title
    {
        get => _Title;

        set
        {
            _Title = value;
            titleLabel.Text = _Title;
        }
    }
    private bool _isMandatory;
    public bool IsMandatory
    {
        get => _isMandatory;

        set
        {
            _isMandatory = value;
            mandatory.IsVisible = value;

        }
    }

    public static readonly BindableProperty SelectedImagesProperty = BindableProperty.Create(
        nameof(SelectedImages), typeof(ObservableCollection<ImageSource>), typeof(ImageSelector),
        defaultValue: new ObservableCollection<ImageSource>(),
        propertyChanged: OnSelectedImagesChanged);

    public ObservableCollection<ImageSource> SelectedImages
    {
        get => (ObservableCollection<ImageSource>)GetValue(SelectedImagesProperty);
        set => SetValue(SelectedImagesProperty, value);
    }

    public bool HasImages => SelectedImages.Any();

    private static void OnSelectedImagesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (ImageSelector)bindable;
        control.UpdateImageGallery();
    }

    private void OnImagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateImageGallery();
    }

    private void UpdateImageGallery()
    {
        imageGallery.Children.Clear();

        foreach (var imageSource in SelectedImages)
        {
            var frame = new Frame
            {
                Padding = 5,
                CornerRadius = 5,
                BorderColor = Colors.Gray,
                HeightRequest = 100,
                WidthRequest = 100
            };

            var grid = new Grid();

            var image = new Image
            {
                Source = imageSource,
                Aspect = Aspect.AspectFill
            };

            var deleteButton = new Button
            {
                Text = "×",
                TextColor = Colors.White,
                BackgroundColor = Colors.Red,
                CornerRadius = 15,
                HeightRequest = 30,
                WidthRequest = 30,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.End
            };

            deleteButton.Clicked += (s, e) =>
            {
                SelectedImages.Remove(imageSource);
            };

            grid.Children.Add(image);
            grid.Children.Add(deleteButton);
            frame.Content = grid;
            imageGallery.Children.Add(frame);
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Select a photo"
                });

                if (result != null)
                {
                    // save the file into local storage
                    //string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    //using Stream sourceStream = await photo.OpenReadAsync();
                    //using FileStream localFileStream = File.OpenWrite(localFilePath);

                    //await sourceStream.CopyToAsync(localFileStream);
                    var stream = await result.OpenReadAsync();
                    var imageSource = ImageSource.FromStream(() => stream);
                    SelectedImages.Add(imageSource);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            pickerPopup.IsVisible = false;
        }
    }

    private async void TapGestureRecognizer_Tapped_1(object sender, TappedEventArgs e)
    {
        try
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                var result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Take a photo"
                });

                if (result != null)
                {
                    // save the file into local storage
                    //string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    //using Stream sourceStream = await photo.OpenReadAsync();
                    //using FileStream localFileStream = File.OpenWrite(localFilePath);

                    //await sourceStream.CopyToAsync(localFileStream);

                    var stream = await result.OpenReadAsync();
                    var imageSource = ImageSource.FromStream(() => stream);
                    SelectedImages.Add(imageSource);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            pickerPopup.IsVisible = false;
        }
    }

    private void ShowPopup()
    {
        System.Diagnostics.Debug.WriteLine("Show popup called");
        pickerPopup.IsVisible = true;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        pickerPopup.IsVisible = true;
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        pickerPopup.IsVisible = false;

    }
}