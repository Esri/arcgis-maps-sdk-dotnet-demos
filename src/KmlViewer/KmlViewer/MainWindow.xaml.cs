using Microsoft.UI.Xaml;
using WinUIEx;

namespace KmlViewer
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow() 
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            WindowManager.Get(this).PersistenceId = "MainWindow";
        }
    }
}
