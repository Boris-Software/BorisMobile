namespace BorisMobile.Controls;

public partial class TimeSelector : ContentView
{
	public TimeSelector()
	{
		InitializeComponent();
        //this.DateSelected += OnDateSelected;

    }
    public event EventHandler<string> Time;

    private string _Title;
    public string Title
    {
        get => _Title;

        set
        {
            _Title = value;
            titleLabel.Text = _Title;
        }
    }private string _timeEntered;
    public string TimeEntered
    {
        get => _timeEntered;

        set
        {
            _timeEntered = value;
            //titleLabel.Text = _timeEntered;
            Time?.Invoke(this, TimeEntered);
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
    //private void OnDateSelected(object sender, Timec e)
    //{
    //    DateSelected?.Invoke(this, new DateSelectedEventArgs(e.OldDate, e.NewDate));
    //}
}