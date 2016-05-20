using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;

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
                    Map = null;
				else
				{
					StatusMessage = "Loading Webmap...";
					IsLoadingWebMap = true;
                    Map = new Map(item);
                    await Map.LoadAsync();
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

		private Map m_Map;

		public Map Map
        {
			get { return m_Map; }
			set
			{
                m_Map = value;
				OnPropertyChanged("Map");
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