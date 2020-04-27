using PortalBrowser.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinPortalBrowser
{
	public partial class MapPage : ContentPage
    {
		public MapPage (MapVM mapVM)
		{
            this.BindingContext = mapVM;
			InitializeComponent ();
		}

        public MapPage()
        {
            InitializeComponent();
        }
    }
}
