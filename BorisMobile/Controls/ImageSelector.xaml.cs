using BorisMobile.Helper;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BorisMobile.Controls;

public partial class ImageSelector : ContentView
{
	public ImageSelector()
	{
		InitializeComponent();
        // Initialize with a new collection if not already set
        if (SelectedImages == null)
        {
            SelectedImages = new ObservableCollection<string>();
        }
        pickerPopup.IsVisible = false;
        SelectedImages.CollectionChanged += OnImagesCollectionChanged;
        ShowPopupCommand = new Command(ShowPopup);

    }
    public event EventHandler<string> ImageSelected;
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
        nameof(SelectedImages), typeof(ObservableCollection<string>), typeof(ImageSelector),
        defaultValueCreator: bindable => new ObservableCollection<string>(),
        propertyChanged: OnSelectedImagesChanged);

    public ObservableCollection<string> SelectedImages
    {
        get => (ObservableCollection<string>)GetValue(SelectedImagesProperty);
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
                    //var stream = await result.OpenReadAsync();
                    //var imageSource = ImageSource.FromStream( () =>  result.OpenReadAsync().Result);
                    //var imageSource = ImageSource.FromStream(() => stream);
                    //SelectedImages.Add(imageSource);

                    var ImagePath = result.FullPath;
                    
                    // Copy file to storage
                    var destinationPath = Path.Combine(FilesHelper.GetUploadsDirectoryMAUI(), result.FileName);
                    await CopyFileToStorage(ImagePath, destinationPath);
                    //_image.Source = ImageSource.FromFile(ImagePath);
                    SelectedImages.Add(destinationPath);
                    ImageSelected?.Invoke(this, destinationPath);
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

    private async Task CopyFileToStorage(string sourcePath, string destinationPath)
    {
        try
        {
            //using var sourceStream = File.OpenRead(sourcePath);
            //using var destinationStream = File.Create(destinationPath);
            //await sourceStream.CopyToAsync(destinationStream);
            File.Move(sourcePath, destinationPath);
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
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

                    //var stream = await result.OpenReadAsync();
                    //var imageSource = ImageSource.FromStream(() => stream);
                    //SelectedImages.Add(imageSource);

                    var ImagePath = result.FullPath;
                    //_image.Source = ImageSource.FromFile(ImagePath);
                    var destinationPath = Path.Combine(FilesHelper.GetUploadsDirectoryMAUI(), result.FileName);
                    await CopyFileToStorage(ImagePath, destinationPath);
                    SelectedImages.Add(destinationPath);
                    ImageSelected?.Invoke(this, destinationPath);
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