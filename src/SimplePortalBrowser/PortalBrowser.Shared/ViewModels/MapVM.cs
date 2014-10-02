using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.WebMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalBrowser.ViewModels
{
	public class MapVM : BaseViewModel
	{
		private ArcGISPortalItem m_portalItem;

		public ArcGISPortalItem PortalItem
		{
			get { return m_portalItem; }
			set { m_portalItem = value; LoadPortalItem(value); OnPropertyChanged("PortalItem"); }
		}

		private async void LoadPortalItem(ArcGISPortalItem item)
		{
			try
			{
				if (item == null)
					WebMapVM = null;
				else
				{
					StatusMessage = "Loading Webmap...";
					IsLoadingWebMap = true;
					var webmap = await WebMap.FromPortalItemAsync(item);
					WebMapVM = await WebMapViewModel.LoadAsync(webmap, item.ArcGISPortal);
					IsLoadingWebMap = false;
					StatusMessage = "";
				}
			}
			catch (System.Exception ex)
			{
				StatusMessage = "Webmap load failed: " + ex.Message;
				IsLoadingWebMap = false;
			}
		}

		private WebMapViewModel m_WebMapVM;

		public WebMapViewModel WebMapVM
		{
			get { return m_WebMapVM; }
			set
			{
				m_WebMapVM = value;
				OnPropertyChanged("WebMapVM");
			}
		}

		private string m_StatusMessage;

		public string StatusMessage
		{
			get { return m_StatusMessage; }
			set
			{
				m_StatusMessage = value;
				OnPropertyChanged("StatusMessage");
				System.Diagnostics.Debug.WriteLine(value);
			}
		}

		private bool m_IsLoadingWebMap = true;

		public bool IsLoadingWebMap
		{
			get { return m_IsLoadingWebMap; }
			set
			{
				m_IsLoadingWebMap = value;
				OnPropertyChanged("IsLoadingWebMap");
			}
		}
	}
}