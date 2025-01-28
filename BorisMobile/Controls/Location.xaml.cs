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

    #endregion
    public async void OnControlTapped(object sender, EventArgs args)
    {
        

    }
}