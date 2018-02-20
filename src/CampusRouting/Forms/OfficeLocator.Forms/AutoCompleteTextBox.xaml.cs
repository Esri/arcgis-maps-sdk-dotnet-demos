using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
#if __ANDROID__
using SearchBox = Android.Widget.AutoCompleteTextView;
#elif __IOS__
using SearchBox = UIKit.UITextField;
#elif NETFX_CORE
using SearchBox = Windows.UI.Xaml.Controls.SearchBox;
#endif

namespace OfficeLocator.Forms
{
    public class AutoCompleteTextBox : ContentView
    {
        SearchBox textBox;
        public AutoCompleteTextBox()
        {
#if __ANDROID__
            textBox = new SearchBox(Application.Context);
#elif __IOS__
#elif NETFX_CORE
#endif
            Content = textBox;
        }

        

        private void UpdateSuggestions()
        {
        }

        public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create<AutoCompleteTextBox, string>(p => p.Placeholder, string.Empty, BindingMode.TwoWay, null, PlaceHolderChanged);

        public string Placeholder
        {
            get { return (string)GetValue(PlaceholderProperty); }
            set { SetValue(PlaceholderProperty, value); }
        }

        private static void PlaceHolderChanged(BindableObject obj, string oldPlaceHolderValue, string newPlaceHolderValue)
        {
            var autoCompleteView = obj as AutoCompleteTextBox;
            if (autoCompleteView != null)
            {
                //autoCompleteView._entText.Placeholder = newPlaceHolderValue;
            }
        }

        public static readonly BindableProperty TextProperty = BindableProperty.Create<AutoCompleteTextBox, string>(p => p.Text, string.Empty, BindingMode.TwoWay, null, TextValueChanged);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void TextValueChanged(BindableObject obj, string oldPlaceHolderValue, string newPlaceHolderValue)
        {
            var control = obj as AutoCompleteTextBox;

            if (control != null)
            {
                //control._btnSearch.IsEnabled = !string.IsNullOrEmpty(newPlaceHolderValue);
                control.UpdateSuggestions();
            }
        }
    }
}
