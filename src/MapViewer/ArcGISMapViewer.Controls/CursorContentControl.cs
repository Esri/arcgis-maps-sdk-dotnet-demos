using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISMapViewer.Controls
{
    public partial class CursorContentControl : ContentControl
    {
        public CursorContentControl()
        {
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
        public Windows.UI.Core.CoreCursorType Cursor
        {
            get { return (Windows.UI.Core.CoreCursorType)GetValue(CursorProperty); }
            set { SetValue(CursorProperty, value); }
        }

        public static readonly DependencyProperty CursorProperty =
            DependencyProperty.Register(nameof(Cursor), typeof(Windows.UI.Core.CoreCursorType), typeof(CursorContentControl), new PropertyMetadata(Windows.UI.Core.CoreCursorType.Arrow, (s,e)=>((CursorContentControl)s).OnCursorPropertyChanged()));

        private void OnCursorPropertyChanged()
        {
            ProtectedCursor = Microsoft.UI.Input.InputCursor.CreateFromCoreCursor(new Windows.UI.Core.CoreCursor(Cursor, 0));
        }
    }
}
