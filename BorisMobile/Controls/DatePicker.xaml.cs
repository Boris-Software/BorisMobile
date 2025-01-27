namespace BorisMobile.Controls;

public partial class DatePicker : ContentView
{
	public DatePicker()
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
}