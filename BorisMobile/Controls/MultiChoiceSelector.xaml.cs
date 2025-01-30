using BorisMobile.Models;
using System.Collections.ObjectModel;

namespace BorisMobile.Controls;

public partial class MultiChoiceSelector : ContentView
{
    public class ChoiceItem : BindableObject
    {
        public int Id { get; set; }
        public string Desc { get; set; }
        public int Seq { get; set; }
        public int List { get; set; }
        public int Score { get; set; }
        public string XmlDoc { get; set; }
        public double Width { get; set; }


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        public Color BackgroundColor => IsSelected ? Color.FromArgb("#00B96B") : Color.FromArgb("#788E98");
    }
    public MultiChoiceSelector()
	{
		InitializeComponent();
        BindingContext = this;
    }
    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(object), typeof(ComboBox));
    public object ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set  {
            SetValue(ItemsSourceProperty, value);

            List<ChoiceItem> items = new List<ChoiceItem>();
            foreach (var item in ItemsSource as List<GenericLists>)
            {
                ChoiceItem chItem = new ChoiceItem();
                chItem.Id = item.Id;
                chItem.Desc = item.Desc;
                chItem.Seq = item.Seq;
                chItem.List = item.List;
                chItem.Score = item.Score;
                chItem.XmlDoc = item.XmlDoc;
                if (chItem.Desc?.Length <= 3)
                {
                    chItem.Width = 80;
                }
                else
                {
                    chItem.Width = -1;
                }
                    items.Add(chItem);
            }
            Choices = new ObservableCollection<ChoiceItem>(items);
        }
    }

    public static readonly BindableProperty TitleProperty =
            BindableProperty.Create(nameof(Title), typeof(string), typeof(MultiChoiceSelector), string.Empty);

    public static readonly BindableProperty IsMandatoryProperty =
        BindableProperty.Create(nameof(IsMandatory), typeof(bool), typeof(MultiChoiceSelector), false);

    public static readonly BindableProperty ChoicesProperty =
        BindableProperty.Create(nameof(Choices), typeof(ObservableCollection<ChoiceItem>),
            typeof(MultiChoiceSelector), new ObservableCollection<ChoiceItem>(),
            propertyChanged: OnChoicesChanged);

    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(nameof(SelectedItems), typeof(ObservableCollection<string>),
            typeof(MultiChoiceSelector), new ObservableCollection<string>(),
            BindingMode.TwoWay);

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsMandatory
    {
        get => (bool)GetValue(IsMandatoryProperty);
        set => SetValue(IsMandatoryProperty, value);
    }

    public ObservableCollection<ChoiceItem> Choices
    {
        get => (ObservableCollection<ChoiceItem>)GetValue(ChoicesProperty);
        set => SetValue(ChoicesProperty, value);
    }

    public ObservableCollection<string> SelectedItems
    {
        get => (ObservableCollection<string>)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged;

    private static void OnChoicesChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MultiChoiceSelector control)
        {
            control.UpdateChoices();
        }
    }

    private void UpdateChoices()
    {
        // Update the CollectionView when choices change
        ChoicesCollection.ItemsSource = Choices;
    }
    private ChoiceItem _currentSelection;
    public string SelectedId
    {
        get => (string)GetValue(SelectedIdProperty);
        set => SetValue(SelectedIdProperty, value);
    }

    public static readonly BindableProperty SelectedIdProperty =
            BindableProperty.Create(nameof(SelectedId), typeof(string),
                typeof(MultiChoiceSelector), null,
                BindingMode.TwoWay);

    private void OnChoiceTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is ChoiceItem choice)
        {
            // If tapping already selected item, deselect it
            if (choice == _currentSelection)
            {
                choice.IsSelected = false;
                _currentSelection = null;
                SelectedId = null;
            }
            // Otherwise, select the new item and deselect the previous one
            else
            {
                if (_currentSelection != null)
                {
                    _currentSelection.IsSelected = false;
                }

                choice.IsSelected = true;
                _currentSelection = choice;
                SelectedId = choice.Id.ToString();
            }

            //SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(
            //    null,
            //    new[] { choice }));
        }

        //if (sender is Frame frame && frame.BindingContext is ChoiceItem choice)
        //{

        //    choice.IsSelected = !choice.IsSelected;

        //    if (choice.IsSelected)
        //    {
        //        if (!SelectedItems.Contains(choice.Id.ToString()))
        //            SelectedItems.Add(choice.Id.ToString());

        //        frame.BackgroundColor = choice.BackgroundColor;
        //    }
        //    else
        //    {
        //        if (SelectedItems.Contains(choice.Id.ToString()))
        //            SelectedItems.Remove(choice.Id.ToString());

        //        frame.BackgroundColor = Color.FromArgb("#788E98");
        //    }
        //}
    }

    
}