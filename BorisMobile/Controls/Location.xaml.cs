namespace BorisMobile.Controls;

public partial class Location :  ContentView
{
	public Location()
	{
		InitializeComponent();
	}

    #region binding

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


    #endregion
    public async void OnControlTapped(object sender, EventArgs args)
    {
        

    }
}