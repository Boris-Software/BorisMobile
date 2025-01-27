namespace BorisMobile.Controls;

public partial class ImageSelector : ContentView
{
	public ImageSelector()
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