using BorisMobile.ViewModels;

namespace BorisMobile.Views;

public partial class CustomerCodePage : ContentPage
{
	public CustomerCodePage(CustomerCodePageViewModel vm)
	{
        
		InitializeComponent();
        BindingContext = vm;


        vm.OnLoginFailed = ((obj) =>
        {
            cusone.Focus();
        });
    }

    private void cusone_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(e.NewTextValue.Length ==1)
            custwo.Focus();
        else if (e.NewTextValue.Length == 0)
        {
            DisableContinueButton();
        }
    }

    private void custwo_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(e.NewTextValue.Length==1)
            custhree.Focus();
        else if (e.NewTextValue.Length == 0)
        {
            cusone.Focus();
            DisableContinueButton();
        }
    }

    private void custhree_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length == 1)
            cusfour.Focus();
        else if (e.NewTextValue.Length == 0)
        {
            custwo.Focus();
            DisableContinueButton();
        }
    }

    private void cusfour_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length == 1)
            cusfive.Focus();
        else if (e.NewTextValue.Length == 0)
        {
            custhree.Focus();
            DisableContinueButton();
        }
    }

    private void cusfive_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length == 1)
            cussix.Focus();
        else if (e.NewTextValue.Length == 0)
        {
            cusfour.Focus();
            DisableContinueButton();
        }
    }

    private void cussix_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (e.NewTextValue.Length == 1)
        {
            btnContinue.IsEnabled = true;
            btnContinue.BackgroundColor = Color.FromHex("#00B1E3");
        }
        else if (e.NewTextValue.Length == 0)
        {
            cusfive.Focus();
            DisableContinueButton();
        }
    }

    private void DisableContinueButton()
    {
        btnContinue.IsEnabled = false;
        btnContinue.BackgroundColor = Color.FromHex("#ADBCCC");
    }
}