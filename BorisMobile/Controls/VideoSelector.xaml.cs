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
    public async void OnControlTapped(object sender, EventArgs args)
    {


    }
}