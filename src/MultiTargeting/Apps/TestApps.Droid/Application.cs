using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Runtime;
using Esri.ArcGISRuntime;

namespace TestApps.Droid
{
    [Android.App.Application]
    class Application : Android.App.Application
    {
        public Application(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {

        }

        public override void OnCreate()
        {
            base.OnCreate();

            // Deployed applications must be licensed at the Lite level or greater. 
            // See https://developers.arcgis.com/licensing for further details.

            // Initialize the ArcGIS Runtime before any components are created.
            ArcGISRuntimeEnvironment.Initialize();
        }
    }
}