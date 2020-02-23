using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Esri.ArcGISRuntime.Symbology;

namespace SymbolEditorApp.Controls.RendererEditors
{
    /// <summary>
    /// Interaction logic for HeatMapRendererEditor.xaml
    /// </summary>
    public partial class HeatMapRendererEditor : UserControl
    {
        public HeatMapRendererEditor()
        {
            InitializeComponent();
            DataContext = Renderer;
        }

        public Renderer Renderer
        {
            get { return (Renderer)GetValue(RendererProperty); }
            set { SetValue(RendererProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Renderer"/> Dependency Property.
        /// </summary>
        public static readonly DependencyProperty RendererProperty =
            DependencyProperty.Register("Renderer", typeof(Renderer), typeof(HeatMapRendererEditor), new PropertyMetadata(null, (d, e) => ((HeatMapRendererEditor)d).OnRendererPropertyChanged(e)));

        private void OnRendererPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!updatingRenderer)
                Model = HeatMapRendererModel.FromRenderer(e.NewValue as Renderer);
        }

        public HeatMapRendererModel Model
        {
            get { return (HeatMapRendererModel)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Model"/> Dependency Property
        /// </summary>       
        public static readonly DependencyProperty ModelProperty =
            DependencyProperty.Register("Model", typeof(HeatMapRendererModel), typeof(HeatMapRendererEditor), new PropertyMetadata(null));

        bool updatingRenderer;
        private void ValueChanged(object sender, EventArgs e)
        {
            updatingRenderer = true;
            Renderer = Model.AsRenderer();
            updatingRenderer = false;
        }

        [DataContract]
        public class HeatMapRendererModel
        {
            private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(HeatMapRendererModel));

            public HeatMapRendererModel()
            {
                blurRadius = 10;
                ColorStops = new ObservableCollection<ColorStop>();
                ColorStops.Add(new ColorStop() { colorInternal = new byte[] { 133, 193, 200, 0 } });
                ColorStops.Add(new ColorStop() { colorInternal = new byte[] { 133, 193, 200, 0 }, ratio = 0.01 });
                maxPixelIntensity = 1249.2897582229123;
                minPixelIntensity = 0;
            }

            public static implicit operator Renderer(HeatMapRendererModel d) => d.AsRenderer();
            public static implicit operator HeatMapRendererModel(Renderer d) => FromRenderer(d);

            public static HeatMapRendererModel FromRenderer(Renderer renderer)
            {
                if (renderer == null) return null;
                var json = renderer.ToJson();
                return serializer.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json))) as HeatMapRendererModel;
            }
            public Renderer AsRenderer()
            {
                using (var ms = new MemoryStream())
                {
                    serializer.WriteObject(ms, this);
                    return Esri.ArcGISRuntime.Symbology.Renderer.FromJson(Encoding.UTF8.GetString(ms.ToArray()));
                }
            }
            [DataMember]
            public string type { get; set; } = "heatmap";
            [DataMember]
            public double blurRadius { get; set; }
            [DataMember(Name ="colorStops")]
            internal ColorStop[] colorStopsInternal { get; set; }
            public ObservableCollection<ColorStop> ColorStops { get; set; }
            [DataMember]
            public double minPixelIntensity { get; set; }
            [DataMember]
            public double maxPixelIntensity { get; set; }

            [OnSerializing]
            public void Serializing(StreamingContext context)
            {
                colorStopsInternal = ColorStops.ToArray();
            }
            [OnDeserialized]
            public void Serialized(StreamingContext context)
            {
                ColorStops = new ObservableCollection<ColorStop>(colorStopsInternal ?? new ColorStop[] { });
            }
        }

        [DataContract]
        public class ColorStop
        {
            [DataMember]
            public double ratio { get; set; }
            [DataMember(Name ="color")]
            public byte[] colorInternal { get; set; }
            public System.Drawing.Color Color
            {
                get {
                    return System.Drawing.Color.FromArgb(colorInternal[3], colorInternal[0], colorInternal[1], colorInternal[2]);
                }
                set
                {
                    colorInternal = new byte[] { value.R, value.G, value.B, value.A };
                }
            }
        }

        private void NumberBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((sender as UniversalWPF.NumberBox).NumberFormatter as UniversalWPF.DecimalFormatter).FractionDigits = 2;
        }
    }
}
