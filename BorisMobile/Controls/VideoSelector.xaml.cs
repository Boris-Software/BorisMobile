using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace BorisMobile.Controls;

public partial class VideoSelector : ContentView
{
	public VideoSelector()
	{
		InitializeComponent();
        BindingContext = this;
        SelectedVideos.CollectionChanged += OnVideosCollectionChanged;
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
    public static readonly BindableProperty SelectedVideosProperty = BindableProperty.Create(
         nameof(SelectedVideos), typeof(ObservableCollection<VideoInfo>), typeof(VideoSelector),
         defaultValue: new ObservableCollection<VideoInfo>(),
         propertyChanged: OnSelectedVideosChanged);

    public ObservableCollection<VideoInfo> SelectedVideos
    {
        get => (ObservableCollection<VideoInfo>)GetValue(SelectedVideosProperty);
        set => SetValue(SelectedVideosProperty, value);
    }

    public bool HasVideos => SelectedVideos.Any();

    private static void OnSelectedVideosChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (VideoSelector)bindable;
        control.UpdateVideoGallery();
    }

    private void OnVideosCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateVideoGallery();
    }

    private void OnVideoButtonClicked(object sender, EventArgs e)
    {
        videopickerPopup.IsVisible = true;
    }

    private async void OnPickVideoTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickVideoAsync();
            if (result != null)
            {
                var video = new VideoInfo
                {
                    FilePath = result.FullPath,
                    Duration = await GetVideoDuration(result.FullPath),
                    Thumbnail = await GenerateVideoThumbnail(result.FullPath)
                };

                SelectedVideos.Add(video);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to pick video: " + ex.Message, "OK");
        }
        finally
        {
            videopickerPopup.IsVisible = false;
        }
    }

    private async void OnRecordVideoTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await MediaPicker.CaptureVideoAsync();
            if (result != null)
            {
                var video = new VideoInfo
                {
                    FilePath = result.FullPath,
                    Duration = await GetVideoDuration(result.FullPath),
                    Thumbnail = await GenerateVideoThumbnail(result.FullPath)
                };

                SelectedVideos.Add(video);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to record video: " + ex.Message, "OK");
        }
        finally
        {
            videopickerPopup.IsVisible = false;
        }
    }

    private void UpdateVideoGallery()
    {
        videoGallery.Children.Clear();

        foreach (var video in SelectedVideos)
        {
            var frame = new Frame
            {
                Padding = 5,
                CornerRadius = 5,
                BorderColor = Colors.Gray,
                HeightRequest = 120,
                WidthRequest = 160
            };

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Star },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Thumbnail image with play button overlay
            var thumbnailGrid = new Grid();

            var thumbnail = new Image
            {
                Source = video.Thumbnail ?? "video_placeholder.png",
                Aspect = Aspect.AspectFill,
                HeightRequest = 90
            };

            var playButton = new Image
            {
                Source = "play.png",
                HeightRequest = 30,
                WidthRequest = 30,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            thumbnailGrid.Children.Add(thumbnail);
            thumbnailGrid.Children.Add(playButton);

            // Video duration and delete button
            var controlsGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var durationLabel = new Label
            {
                Text = $"{video.Duration:mm\\:ss}",
                VerticalOptions = LayoutOptions.Center
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
                Padding = new Thickness(0)
            };

            deleteButton.Clicked += (s, e) =>
            {
                SelectedVideos.Remove(video);
            };

            controlsGrid.Children.Add(durationLabel);
            controlsGrid.Children.Add(deleteButton);
            Grid.SetColumn(deleteButton, 1);

            // Add all elements to the main grid
            grid.Children.Add(thumbnailGrid);
            grid.Children.Add(controlsGrid);
            Grid.SetRow(controlsGrid, 1);

            // Add tap gesture to play video
            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) => await PlayVideo(video.FilePath);
            thumbnailGrid.GestureRecognizers.Add(tapGesture);

            frame.Content = grid;
            videoGallery.Children.Add(frame);
        }
    }

    private async Task PlayVideo(string filePath)
    {
        try
        {
            //await MediaElement.PlatformPlay(filePath);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Failed to play video: " + ex.Message, "OK");
        }
    }

    private async Task<TimeSpan> GetVideoDuration(string filePath)
    {
        // This is a placeholder - you'll need to implement platform-specific code
        // to get the actual video duration
        return TimeSpan.FromSeconds(0);
    }

    private async Task<ImageSource> GenerateVideoThumbnail(string filePath)
    {
        // This is a placeholder - you'll need to implement platform-specific code
        // to generate the actual video thumbnail
        return "video_placeholder.png";
    }

    private void ImageButton_Clicked_1(object sender, EventArgs e)
    {
        videopickerPopup.IsVisible = true;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        videopickerPopup.IsVisible = false;
    }
}

public class VideoInfo
{
    public string FilePath { get; set; }
    public TimeSpan Duration { get; set; }
    public ImageSource Thumbnail { get; set; }
}
