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
        //await GoToAsync("StartupPage");
        Application.Current!.MainPage = new StartupPage();
    }
}
