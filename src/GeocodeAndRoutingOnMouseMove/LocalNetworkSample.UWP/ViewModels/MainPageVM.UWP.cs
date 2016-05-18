using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml;

namespace LocalNetworkSample
{
    //Contains part of the main view model specific to the Windows Store View
    public partial class MainPageVM : BaseViewModel
    {
        private async Task<bool> ShowErrorMessage(string title, string message, string okButton = null, string cancelButton = null)
        {
            Windows.UI.Popups.MessageDialog dialog = new Windows.UI.Popups.MessageDialog(message, title);
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            bool result = false;
            if (okButton != null)
            {
                dialog.Commands.Add(new Windows.UI.Popups.UICommand(okButton, _ => { result = true; }));
            }
            if (cancelButton != null)
            {
                dialog.Commands.Add(new Windows.UI.Popups.UICommand(cancelButton));
            }
            await dialog.ShowAsync();
            return result;
        }

        private bool m_isSidePanelOpen;

        public bool IsSidePanelOpen
        {
            get { return m_isSidePanelOpen || IsDesignMode; }
            set
            {
                m_isSidePanelOpen = value; OnPropertyChanged();
                //Make zoom/center operations offset if sidepanel is open
                //ViewController.DefaultMargin = value ?
                //	new Thickness(320 + 50, 50, 50, 50) :
                //	new Thickness(50);
            }
        }

    }
}
