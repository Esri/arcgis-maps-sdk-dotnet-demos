using CommunityToolkit.Mvvm.ComponentModel;
using Esri.ArcGISRuntime;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ArcGISMapViewer.ViewModels
{
    public class AppSettings : ObservableObject
    {
        private ApplicationDataContainer localSettings;
        public AppSettings()
        {
            localSettings = ApplicationData.Current.LocalSettings.CreateContainer("Settings", ApplicationDataCreateDisposition.Always);
            var appResources = Application.Current.Resources;
            OAuthClientId = (string)appResources["OAuthClientID"];
            OAuthRedirectUrl = (string)appResources["OAuthRedirectUrl"];
            PortalUrl = new Uri((string)appResources["PortalUrl"] + "sharing/rest");
        }

        public string OAuthClientId { get; }
        public string OAuthRedirectUrl { get; }
        public Uri PortalUrl { get; }

        private T GetSetting<T>(T defaultValue, [CallerMemberName] string? key = null)
        {
            if(key is null) throw new ArgumentNullException(nameof(key));
            if (localSettings.Values.ContainsKey(key))
            {
                var v = localSettings.Values[key];
                if (typeof(T).IsEnum && v is int)
                    return (T)v;
                if (v is T value)
                    return value;
            }
            return defaultValue;
        }
        
        private void SetSetting<T>(T value, [CallerMemberName] string? key = null)
        {
            if (key is null) throw new ArgumentNullException(nameof(key));
            if(typeof(T).IsEnum)
                localSettings.Values[key] = Convert.ChangeType(value, typeof(int));
            else
                localSettings.Values[key] = value;
            OnPropertyChanged(key);
        }

        public Microsoft.UI.Xaml.ElementTheme Theme
        {
            get => GetSetting(ElementTheme.Default, nameof(Theme));
            set => SetSetting(value, nameof(Theme));
        }

        public string? PortalUser
        {
            get => GetSetting<string?>(null);
            set => SetSetting(value);
        }

        public LicenseInfo? License
        {
            get
            {
                var licenseString = GetSetting(string.Empty, nameof(License));
                if (string.IsNullOrEmpty(licenseString))
                    return null;
                return LicenseInfo.FromJson(licenseString);
            }
            set
            {
                SetSetting(value?.ToJson() ?? string.Empty, nameof(License));
            }
        }
    }
}
