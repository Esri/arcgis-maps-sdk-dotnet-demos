using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
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

    public async Task<GeoModel?> LoadLastMapAsync()
    {
        var item = await AppSettings.GetLastPortalItemAsync();
        if(item != null)
        {
            PortalItem = item;
            return Map as GeoModel ?? Scene as GeoModel;
        }
        else
        {
            GeoModel = new Map(BasemapStyle.ArcGISStreets);
            return null;
        }
    }

    [ObservableProperty]
    private LocationDataSource _LocationDataSource = LocationDataSource.CreateDefault();

    public AppSettings AppSettings { get; } = new AppSettings();

    [ObservableProperty]
    private GeoModel? _geoModel;
    partial void OnGeoModelChanged(GeoModel? oldValue, GeoModel? newValue)
    {
        if (newValue is Map || oldValue is Map)
            OnPropertyChanged(nameof(Map));
        if (newValue is Scene || oldValue is Scene)
            OnPropertyChanged(nameof(Scene));
        if (oldValue is Map && newValue is Scene || oldValue is Scene && newValue is Map)
        {
            OnPropertyChanged(nameof(Is2D));
            OnPropertyChanged(nameof(Is3D));
        }
    }

    public Map? Map => GeoModel as Map;
    public Scene? Scene => GeoModel as Scene;

    public bool Is2D => GeoModel is Map;
    public bool Is3D => GeoModel is Scene;

    [ObservableProperty]
    private PortalItem? portalItem;

    partial void OnPortalItemChanged(PortalItem? value)
    {
        if (value is not null)
        {
            Scene? scene = null;
            var map = new Map(BasemapStyle.ArcGISStreets);
            if(value.Type == PortalItemType.WebScene)
                scene = new Scene(value);
            else if (value.Type == PortalItemType.WebMap)
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
            else if(value.Type == PortalItemType.SceneService)
            {
                scene = new Scene();
                scene.OperationalLayers.Add(new ArcGISSceneLayer(value));
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"{value.Type} not implemented");
            }
            if (value.Extent != null)
            {
                if(scene != null)
                    scene.InitialViewpoint = new Viewpoint(value.Extent);
                else
                    map.InitialViewpoint = new Viewpoint(value.Extent);
            }
            GeoModel = scene is null ? map : scene;
        }
        AppSettings.SetLastPortalItem(value);
    }

    [ObservableProperty]
    private string _windowSubTitle = string.Empty;

    private PortalUser? _PortalUser;

    public PortalUser? PortalUser
    {
        get { return _PortalUser; }
        private set
        {
            _PortalUser = value; OnPropertyChanged();
            OnPropertyChanged(nameof(Favorites));
            AppSettings.PortalUser = value?.FullName;
        }
    }

    [ObservableProperty]
    private bool _isMapVisible;
    [ObservableProperty]
    private bool _isAppMenuVisible = true;
    partial void OnIsAppMenuVisibleChanged(bool value)
    {
        IsMapVisible = !value;
    }
    partial void OnIsMapVisibleChanged(bool value)
    {
        IsAppMenuVisible = !value;
        if (value)
        {
            this.WindowSubTitle = GeoModel?.Item?.Title ?? string.Empty;
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

    private IList<PortalItem>? _Favorites;
    public IList<PortalItem> Favorites
    {
        get
        {
            if (_Favorites is null && this.PortalUser?.Portal != null)
            {
                _Favorites = new ObservableCollection<PortalItem>();
                var ids = AppSettings.GetSetting<string[]>(new string[] { }, $"Favorites_{this.PortalUser.UserId}");
                LoadFavorites(ids);
                ((INotifyCollectionChanged)_Favorites).CollectionChanged += (s, e) =>
                {
                    AppSettings.SetSetting(_Favorites.Select(p => p.ItemId).ToArray(), $"Favorites_{this.PortalUser.UserId}");
                };
            }
            return _Favorites ?? new List<PortalItem>();
        }
    }

    private async void LoadFavorites(string[] ids)
    {
        var results = await Task.WhenAll<PortalItem?>(ids.Select(async id =>
        {
            try
            {
                return await PortalItem.CreateAsync(PortalUser!.Portal, id).ConfigureAwait(false);
            }
            catch { return null; } // Skip and that fail to load
        }));
        foreach(var item in results.Where(i => i is not null))
        {
            Favorites.Add(item!);
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
        _Favorites = null;
        AppSettings.SetLastPortalItem(null);
        await Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveAndRevokeAllCredentialsAsync();
    }
}
