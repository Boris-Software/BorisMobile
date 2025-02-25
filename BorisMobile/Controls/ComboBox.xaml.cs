using Microsoft.Maui.Graphics.Text;

namespace BorisMobile.Controls;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ComboBox : ContentView
{
    public event EventHandler<SelectedItemChangedEventArgs> SelectedIndexChanged;

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
        if (ItemsSource == null)
        {
            return;
        }
        else
        {
            picker.Focus();
        }

    }

    public void ThisPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (picker.SelectedItem == null) return;

        SelectedItem = picker.SelectedItem;
        SelectedIndexChanged?.Invoke(this, new SelectedItemChangedEventArgs(SelectedItem));
    }

    public class SelectedItemChangedEventArgs : EventArgs
    {
        public object SelectedItem { get; }
        public SelectedItemChangedEventArgs(object selectedItem)
        {
            SelectedItem = selectedItem;
        }
    }
    //private void ThisPickerSelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if (SelectedItem == null)
    //    {
    //        return;
    //    }

    //    string selectedValue = null;

    //    //For the int StaticData

    //}
}