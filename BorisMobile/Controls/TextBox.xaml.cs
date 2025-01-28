namespace BorisMobile.Controls;

public partial class TextBox :  ContentView
{
	public TextBox()
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
    private int _minLength;
    public int MinLength
    {
        get => _minLength;

        set
        {
            _minLength = value;
            //entry.min = _Title;
        }
    }

    private int _maxLength;
    public int MaxLength
    {
        get => _maxLength;

        set
        {
            _maxLength = value;
            entry.MaxLength = _maxLength;
        }
    }

    private Keyboard _keyboardType;
    public Keyboard KeyboardType
    {
        get => _keyboardType;

        set
        {
            _keyboardType = value;
            entry.Keyboard = _keyboardType;
        }
    }
    
    private int? _lines;
    public int? Lines
    {
        get => _lines;

        set
        {
            _lines = value;

            if (_lines > 0)
            {
                entry.HeightRequest = 100; 
                border.HeightRequest = 100;
            }
        }
    }
    #endregion




}