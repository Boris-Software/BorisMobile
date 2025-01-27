using Microsoft.Maui.Graphics.Text;

namespace BorisMobile.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ComboBox : ContentView
{
	public ComboBox()
	{
		InitializeComponent();
	}

	#region binding
	public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(object), typeof(ComboBox));
	public object ItemsSource
	{
		get => GetValue(ItemsSourceProperty);
		set => SetValue(ItemsSourceProperty, value);
	}


    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(ComboBox));
    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);

        set => SetValue(SelectedItemProperty, value);
    }

    private string _ItemDisplayBinding;
    public string ItemDisplayBinding
    {
        get => _ItemDisplayBinding;

        set => _ItemDisplayBinding = value;
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

    
    #endregion

    public async void OnControlTapped(object sender, EventArgs args)
    {
        if (ItemsSource == null)
        {
            return;
        }

    }

    private void ThisPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (SelectedItem == null)
        {
            return;
        }

        string selectedValue = null;

        //For the int StaticData
        
    }
}