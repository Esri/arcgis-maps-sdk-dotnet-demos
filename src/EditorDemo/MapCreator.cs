using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Labeling;
using Esri.ArcGISRuntime.Symbology;
using System.IO;

namespace EditorDemo
{
    /// <summary>
    /// Helper class for generating a simple local map to work with
    /// </summary>
    internal static class MapCreator
    {
        /// <summary>
        /// Generates the OGC Conformance Map from the OGC Simple Features Specification v1.1.0
        /// </summary>
        /// <param name="recreate">Deletes any previous instance and creates a new database</param>
        /// <returns>Map backed by a local mobile geodatabase</returns>
        public static async Task<Map> CreateOgcConformanceMap(bool recreate = true, string filename = "MapData.geodatabase")
        {
            Map map = new Map();
            var sr = SpatialReferences.WebMercator;
            Geodatabase? gdb = null;
            if(File.Exists(filename))
            {
                if(recreate)
                    File.Delete(filename);
                else
                    gdb = await Esri.ArcGISRuntime.Data.Geodatabase.OpenAsync(filename);
            }
            if (gdb is null)
            {
                gdb = await Esri.ArcGISRuntime.Data.Geodatabase.CreateAsync(filename);
                var lakes = await gdb.CreateTableAsync(CreateTable("lakes", sr, GeometryType.Polygon, ("name", FieldType.Text)));
                var road_segments = await gdb.CreateTableAsync(CreateTable("road_segments", sr, GeometryType.Polyline, ("name", FieldType.Text), ("aliases", FieldType.Text), ("num_lanes", FieldType.Int32)));
                var divided_routes = await gdb.CreateTableAsync(CreateTable("divided_routes", sr, GeometryType.Polyline, ("name", FieldType.Text), ("num_lanes", FieldType.Int32)));
                var forests = await gdb.CreateTableAsync(CreateTable("forests", sr, GeometryType.Polygon, ("name", FieldType.Text)));
                var bridges = await gdb.CreateTableAsync(CreateTable("bridges", sr, GeometryType.Point, ("name", FieldType.Text)));
                var streams = await gdb.CreateTableAsync(CreateTable("streams", sr, GeometryType.Polyline, ("name", FieldType.Text)));
                var buildings = await gdb.CreateTableAsync(CreateTable("buildings", sr, GeometryType.Polygon, ("address", FieldType.Text)));
                var ponds = await gdb.CreateTableAsync(CreateTable("ponds", sr, GeometryType.Polygon, ("name", FieldType.Text)));
                var named_places = await gdb.CreateTableAsync(CreateTable("named_places", sr, GeometryType.Polygon, ("name", FieldType.Text)));
                var map_neatlines = await gdb.CreateTableAsync(CreateTable("map_neatlines", sr, GeometryType.Polygon));
                // lakes
                var f = lakes.CreateFeature();
                f.Attributes["name"] = "Blue Lake";
                f.Geometry = new Polygon(Convert(new double[][] { new[] { 52d, 18, 66, 23, 73, 9, 48, 6, 52, 18 }, new[] { 59d, 18, 67, 18, 67, 13, 59, 13, 59, 18 } }, sr), sr);
                await lakes.AddFeatureAsync(f);
                // roads
                f = road_segments.CreateFeature();
                f.Attributes["name"] = "Route 5";
                f.Geometry = new Polyline(Convert(new double[] { 0, 18, 10, 21, 16, 23, 28, 26, 44, 31 }, sr), sr);
                await road_segments.AddFeatureAsync(f);
                f = road_segments.CreateFeature();
                f.Attributes["name"] = "Route 5";
                f.Attributes["aliases"] = "Main Street";
                f.Geometry = new Polyline(Convert(new double[] { 44, 31, 56, 34, 70, 38 }, sr), sr);
                await road_segments.AddFeatureAsync(f);
                f = road_segments.CreateFeature();
                f.Attributes["name"] = "Route 5";
                f.Geometry = new Polyline(Convert(new double[] { 70, 38, 72, 48 }, sr), sr);
                await road_segments.AddFeatureAsync(f);
                f = road_segments.CreateFeature();
                f.Attributes["name"] = "Main Street";
                f.Geometry = new Polyline(Convert(new double[] { 70, 38, 84, 42 }, sr), sr);
                await road_segments.AddFeatureAsync(f);
                f = road_segments.CreateFeature();
                f.Attributes["name"] = "Dirt Road by Green Forest";
                f.Geometry = new Polyline(Convert(new double[] { 28, 26, 28, 0 }, sr), sr);
                await road_segments.AddFeatureAsync(f);
                // Divided Routes
                f = divided_routes.CreateFeature();
                f.Attributes["name"] = "Route 75";
                f.Geometry = new Polyline(Convert(new double[][] { new[] { 10d, 48, 10, 21, 10, 0 }, new[] { 16d, 0, 16, 23, 16, 48 } }, sr), sr);
                await divided_routes.AddFeatureAsync(f);
                // forests
                f = forests.CreateFeature();
                f.Attributes["name"] = "Green Forest";
                f.Geometry = new Polygon(Convert(new double[][] { new[] { 28d, 26, 28, 0, 84, 0, 84, 42, 28, 26 }, new[] { 52d, 18, 66, 23, 73, 9, 48, 6, 52, 18 }, new[] { 59d, 18, 67, 18, 67, 13, 59, 13, 59, 18 } }, sr), sr);
                await forests.AddFeatureAsync(f);
                // bridges
                f = bridges.CreateFeature();
                f.Attributes["name"] = "Cam Bridge";
                f.Geometry = new MapPoint(44, 31, sr);
                await bridges.AddFeatureAsync(f);
                // streams
                f = streams.CreateFeature();
                f.Attributes["name"] = "Cam Stream";
                f.Geometry = new Polyline(Convert(new double[] { 38, 48, 44, 41, 41, 36, 44, 31, 52, 18 }, sr), sr);
                await streams.AddFeatureAsync(f);
                f = streams.CreateFeature();
                f.Geometry = new Polyline(Convert(new double[] { 76, 0, 78, 4, 73, 9 }, sr), sr);
                await streams.AddFeatureAsync(f);
                // Buildings
                f = buildings.CreateFeature();
                f.Attributes["address"] = "123 Main Street";
                f.Geometry = new Polygon(Convert(new double[] { 50d, 31, 54, 31, 54, 29, 50, 29, 50, 31 }, sr), sr);
                await buildings.AddFeatureAsync(f);
                f = buildings.CreateFeature();
                f.Attributes["address"] = "215 Main Street";
                f.Geometry = new Polygon(Convert(new double[] { 66d, 34, 62, 34, 62, 32, 66, 32, 66, 34 }, sr), sr);
                await buildings.AddFeatureAsync(f);
                // ponds
                f = ponds.CreateFeature();
                f.Attributes["name"] = "Stock Pond";
                f.Geometry = new Polygon(Convert(new double[][] { new[] { 24d, 44, 22, 42, 24, 40, 24, 44 }, new[] { 26d, 44, 26, 40, 28, 42, 26, 44 } }, sr), sr);
                await ponds.AddFeatureAsync(f);
                // Named places
                f = named_places.CreateFeature();
                f.Attributes["name"] = "Ashton";
                f.Geometry = new Polygon(Convert(new[] { 62d, 48, 84, 48, 84, 30, 56, 30, 56, 34, 62, 48 }, sr), sr);
                await named_places.AddFeatureAsync(f);
                f = named_places.CreateFeature();
                f.Attributes["name"] = "Goose Island";
                f.Geometry = new Polygon(Convert(new[] { 67d, 13, 67, 18, 59, 18, 59, 13, 67, 13 }, sr), sr);
                await named_places.AddFeatureAsync(f);
                // Neatlines
                f = map_neatlines.CreateFeature();
                f.Geometry = new Polygon(Convert(new[] { 0d, 0, 0, 48, 84, 48, 84, 0, 0, 0 }, sr), sr);
                await map_neatlines.AddFeatureAsync(f);
            }
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("map_neatlines"), System.Drawing.Color.LightGray));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("lakes"), System.Drawing.Color.CornflowerBlue, outlineColor: System.Drawing.Color.Blue));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("forests"), System.Drawing.Color.Green));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("named_places"), System.Drawing.Color.Gray, hatch: true));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("buildings"), System.Drawing.Color.Gray, System.Drawing.Color.Black, 1));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("ponds"), System.Drawing.Color.LightBlue));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("road_segments"), System.Drawing.Color.DarkGray, width : 3));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("divided_routes"), System.Drawing.Color.DarkGray, width: 8));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("streams"), System.Drawing.Color.CornflowerBlue, width : 5));
            map.OperationalLayers.Add(CreateLayer(gdb.GetGeodatabaseFeatureTable("bridges"), System.Drawing.Color.Black));
            return map;
        }
        private static FeatureLayer CreateLayer(FeatureTable? table, System.Drawing.Color color, System.Drawing.Color? outlineColor = null, double width = 2, bool hatch = false)
        {
            if(table is null) throw new ArgumentNullException(nameof(table));
            var layer = new FeatureLayer(table);
            var nameField = table.Fields.FirstOrDefault(f => f.FieldType == FieldType.Text);
            if (nameField != null)
            {
                layer.LabelsEnabled = true;
                layer.LabelDefinitions.Add(new LabelDefinition(new SimpleLabelExpression($"[{nameField.Name}]"), new TextSymbol() {  Color = System.Drawing.Color.Black, Size = 10 }));
            }
            Symbol? symbol = null;
            if (table.GeometryType == GeometryType.Point)
                symbol = new SimpleMarkerSymbol() { Color = color, Size = 8 };
            else if (table.GeometryType == GeometryType.Polyline)
                symbol = new SimpleLineSymbol() { Color = color, Width = width };
            else if (table.GeometryType == GeometryType.Polygon)
            {
                symbol = new SimpleFillSymbol() { Color = color, Style = hatch ? SimpleFillSymbolStyle.ForwardDiagonal : SimpleFillSymbolStyle.Solid, Outline = outlineColor.HasValue ? new SimpleLineSymbol() { Width = width, Color = outlineColor.Value } : null };
            }
            layer.Renderer = new SimpleRenderer(symbol);
            return layer;
        }

        private static IEnumerable<IEnumerable<MapPoint>> Convert(double[][] vertices, SpatialReference sr)
        {
            foreach (var part in vertices)
                yield return Convert(part, sr);
        }
        private static IEnumerable<MapPoint> Convert(double[] vertices, SpatialReference sr)
        {
            for (int i = 0; i < vertices.Length; i+=2)
            {
                yield return new MapPoint(vertices[i], vertices[i + 1]);
            }
        }

        private static TableDescription CreateTable(string name, SpatialReference sr, GeometryType geometryType, params (string, FieldType)[] fields)
        {
            var desc = new TableDescription(name, sr, geometryType);
            foreach (var f in fields)
            {
                desc.FieldDescriptions.Add(new FieldDescription(f.Item1, f.Item2));
            }
            return desc;
        }
    }
}
