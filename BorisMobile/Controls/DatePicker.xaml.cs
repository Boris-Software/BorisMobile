namespace BorisMobile.Controls;

public partial class DatePicker : ContentView
{
	public DatePicker()
	{
		InitializeComponent();
        this.DateSelected += OnDateSelected;
    }
    public event EventHandler<DateChangedEventArgs> DateSelected;
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

    private void OnDateSelected(object sender, DateChangedEventArgs e)
    {
        DateSelected?.Invoke(this, new DateChangedEventArgs(e.OldDate, e.NewDate));
    }
}