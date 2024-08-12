using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Portal;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;

namespace ArcGISMapViewer.ViewModels;

public partial class ApplicationViewModel : ObservableObject
{
    public static ApplicationViewModel Instance { get; } = new ApplicationViewModel();

    private ApplicationViewModel()
    {
        _ = LocationDataSource.StartAsync();
    }

    public async Task<Map?> LoadLastMapAsync()
    {
        var item = await AppSettings.GetLastPortalItemAsync();
        if(item != null)
        {
            PortalItem = item;
            return Map;
        }
        else
        {
            Map = new Map(BasemapStyle.ArcGISStreets);
            return null;
        }
    }

    [ObservableProperty]
    private LocationDataSource _LocationDataSource = LocationDataSource.CreateDefault();

    public AppSettings AppSettings { get; } = new AppSettings();

    [ObservableProperty]
    private Map? _map;

    [ObservableProperty]
    private PortalItem? portalItem;

    partial void OnPortalItemChanged(PortalItem? value)
    {
        if (value is not null)
        {

            var map = new Map(BasemapStyle.ArcGISStreets);
            if (value.Type == PortalItemType.WebMap)
                map = new Map(value);
            else if(value.Type == PortalItemType.FeatureService || value.Type == PortalItemType.WFS)
                map.OperationalLayers.Add(new FeatureLayer(value));
            else if (value.Type == PortalItemType.WMS)
                map.OperationalLayers.Add(new WmsLayer(value));
            else if (value.Type == PortalItemType.KML)
                map.OperationalLayers.Add(new KmlLayer(value));
            else if (value.Type == PortalItemType.VectorTileService)
                map.OperationalLayers.Add(new ArcGISVectorTiledLayer(value));
            else if (value.Type == PortalItemType.MapService)
                map.OperationalLayers.Add(new ArcGISMapImageLayer(value));
            else if (value.Type == PortalItemType.FeatureCollection)
                map.OperationalLayers.Add(new FeatureCollectionLayer(new Esri.ArcGISRuntime.Data.FeatureCollection(value)));
            else
            {
                System.Diagnostics.Debug.WriteLine($"{value.Type} not implemented");
            }
            if (value.Extent != null)
                map.InitialViewpoint = new Viewpoint(value.Extent);
            Map = map;
        }
        AppSettings.SetLastPortalItem(value);
    }

    [ObservableProperty]
    private string _windowSubTitle;

    private PortalUser? _PortalUser;

    public PortalUser? PortalUser
    {
        get { return _PortalUser; }
        private set
        {
            _PortalUser = value; OnPropertyChanged();
            AppSettings.PortalUser = value?.FullName;
        }
    }

    [ObservableProperty]
    private bool _isMapVisible;
    [ObservableProperty]
    private bool _isAppMenuVisible;
    partial void OnIsAppMenuVisibleChanged(bool value)
    {
        IsMapVisible = !value;
    }
    partial void OnIsMapVisibleChanged(bool value)
    {
        IsAppMenuVisible = !value;
        if (value)
        {
            this.WindowSubTitle = Map?.Item?.Title ?? string.Empty;
        }
    }

    public async Task SetUserAsync(PortalUser? value)
    {
        PortalUserThumbnail = null;
        OnPropertyChanged(nameof(PortalUserThumbnail));
        PortalUser = value;
        if (value != null)
        {
            try
            {
                await RefreshUserThumbnail(value);
            }
            catch { }
        }
    }

    private async Task RefreshUserThumbnail(PortalUser value)
    {
        try
        {
            using var stream = await value.GetThumbnailDataAsync();
            if (stream is not null)
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                PortalUserThumbnail = bitmap;
            }
            else
            {
                PortalUserThumbnail = null;
            }
            OnPropertyChanged(nameof(PortalUserThumbnail));
        }
        catch
        {
        }
    }

    public ImageSource? PortalUserThumbnail { get; private set; }

    public async Task SignOut()
    {
        PortalUser = null;
        AppSettings.SetLastPortalItem(null);
        await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
    }
}
