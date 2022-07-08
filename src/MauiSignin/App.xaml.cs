namespace MauiSignin;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new StartupPage();
        //MainPage = new AppShell();
    }
}
