using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using System;

namespace RoutingSample.Models
{
    public class Maneuver : ModelBase
    {
        private string _text;
        private DirectionManeuverType _type;
#if XAMARIN
        private string _imageSource;
#else
        private Uri _imageUri;
#endif
        private TimeSpan _remainingTime;
        private TrackingDistance _remainingDistance;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        public DirectionManeuverType Type
        {
            get => _type;
            set
            {
                if (SetProperty(ref _type, value))
                {
                    // Link images in each project, adjust build action
                    //  Android - AndroidResource
                    //  iOS - Add images to Asset Catalogs
                    //  WPF - Resource
                    //  UWP - Content

#if XAMARIN
                    ImageSource = $"{_type}.png";
#elif NETFX_CORE
                    ImageUri = new Uri($"ms-appx:///Assets/Maneuvers/{_type}.png");
#else
                    ImageUri = new Uri($"pack://application:,,,/Assets/Maneuvers/{_type}.png");
#endif
                }
            }
        }


#if XAMARIN
        /// <summary>
        /// Gets the image source.
        /// </summary>
        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }
#else
        /// <summary>
        /// Gets the image URI.
        /// </summary>
        public Uri ImageUri

        {
            get => _imageUri;
            private set => SetProperty(ref _imageUri, value);
        }
#endif

        /// <summary>
        /// Gets or sets the remaining time.
        /// </summary>
        public TimeSpan RemainingTime
        {
            get => _remainingTime;
            set => SetProperty(ref _remainingTime, value);
        }

        /// <summary>
        /// Gets or sets the remaining distance.
        /// </summary>
        public TrackingDistance RemainingDistance
        {
            get => _remainingDistance;
            set => SetProperty(ref _remainingDistance, value);
        }
    }
}
