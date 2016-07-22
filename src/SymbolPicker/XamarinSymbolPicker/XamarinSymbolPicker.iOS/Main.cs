namespace XamarinSymbolPicker.iOS
{
    using UIKit;

#pragma warning disable SA1649 // File name must match first type name
    public class Application
#pragma warning restore SA1649 // File name must match first type name
    {
        // This is the main entry point of the application.
        private static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}
