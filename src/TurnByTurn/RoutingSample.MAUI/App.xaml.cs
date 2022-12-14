namespace RoutingSample.MAUI
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var rootNav = new NavigationPage(new LoginPage());

            MainPage = rootNav;
        }
    }
}