namespace BorisMobile.Controls;

public partial class Location :  ContentView
{
	public Location()
	{
		InitializeComponent();
        //this.TextChanged += OnTextChanged;

    }
    public event EventHandler<string> TextChanged;

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
    private string _enteredValue;
    public string EnteredValue
    {
        get => _enteredValue;

        set
        {
            _enteredValue = value;
            //titleLabel.Text = _enteredValue;
            TextChanged?.Invoke(this, _enteredValue);
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

    private void entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        EnteredValue = e.NewTextValue;
    }

    //private void OnTextChanged(object sender, TextChangedEventArgs e)
    //{
    //    TextChanged?.Invoke(this, new TextChangedEventArgs(e.OldTextValue, e.NewTextValue));
    //}
}