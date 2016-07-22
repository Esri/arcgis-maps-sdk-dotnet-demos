namespace XamarinSymbolPicker.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.LoadApplication(new XamarinSymbolPicker.App());
        }
    }
}
