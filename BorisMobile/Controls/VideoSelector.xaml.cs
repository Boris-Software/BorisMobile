namespace BorisMobile.Controls;

public partial class VideoSelector : ContentView
{
	public VideoSelector()
	{
		InitializeComponent();
	}
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

    public async void OnControlTapped(object sender, EventArgs args)
    {


    }
}