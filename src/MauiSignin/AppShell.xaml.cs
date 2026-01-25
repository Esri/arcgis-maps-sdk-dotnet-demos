namespace MauiSignin;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        BindingContext = AppSettings.Instance;
    }
    
    private async void SignOutButton_Clicked(object sender, EventArgs e)
    {
        await AppSettings.Instance.SignOut();
        App.Current!.Windows[0].Page = new StartupPage();
    }
}
