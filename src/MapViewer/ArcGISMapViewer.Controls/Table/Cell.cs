using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace ArcGISMapViewer.Controls
{
    public abstract class Cell : ContentPresenter
    {

        public Cell()
        {
            this.Padding = new Thickness(2);
        }
    }
}
