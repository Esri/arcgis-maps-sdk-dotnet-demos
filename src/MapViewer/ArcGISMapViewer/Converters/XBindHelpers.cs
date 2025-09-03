using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISMapViewer.Converters
{
    public static class XBindHelpers
    {
        public static IEnumerable<Layer> ReverseLayerCollection(LayerCollection layerCollection)
        {
            return layerCollection.Reverse();
        }

        public static bool HasTimeSpan(TimeSpan time)
        {
            return time > TimeSpan.Zero;
        }
\    }
}
