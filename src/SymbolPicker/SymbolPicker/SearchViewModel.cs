namespace SymbolPicker
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;
#if NETFX_CORE
    using System.Runtime.InteropServices.WindowsRuntime;
    using Windows.ApplicationModel;
    using Windows.Storage;
#endif
#if XAMARIN
    using Xamarin.Forms;
#elif NETFX_CORE
    using Windows.UI.Xaml.Media;
#else
    using System.Windows.Media;
#endif
    using Esri.ArcGISRuntime.Symbology;

    public sealed class SearchViewModel : INotifyPropertyChanged
    {
        private const string SPECIFICATION = "mil2525d";
        private IList<string> categoriesParameter = new List<string>();
        private IList<string> keysParameter = new List<string>();
        private IList<string> namesParameter = new List<string>();
        private IList<string> symbolClassesParameter = new List<string>();
        private IList<string> tagsParameter = new List<string>();
        private SymbolDictionary dictionary;

        public SearchViewModel()
        {
            var i = this.InitializeAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async Task InitializeAsync()
        {
            this.IsBusy = true;
            string path = string.Empty;
            string folder = string.Empty;
            var fileName = string.Format("{0}.stylx", SPECIFICATION);
            var assembly = typeof(SearchViewModel).GetTypeInfo().Assembly;
            try
            {
#if NETFX_CORE
                folder = ApplicationData.Current.LocalFolder.Path;
#else
                folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
                path = Path.Combine(folder, fileName);
                if (!File.Exists(path))
                {
                    var embeddedResourceName = string.Format("{0}.{0}.{1}", nameof(SymbolPicker), fileName);
                    using (var stream = assembly.GetManifestResourceStream(embeddedResourceName))
                    {
                        using (var mem = new MemoryStream())
                        {
                            stream.CopyTo(mem);
                            System.IO.File.WriteAllBytes(path, mem.ToArray());
                        }
                    }
                }

                this.dictionary = await SymbolDictionary.OpenAsync(SPECIFICATION, path);
                var result = await this.dictionary.SearchSymbolsAsync(new StyleSymbolSearchParameters());
                this.Categories = (from r in result where !string.IsNullOrEmpty(r.Category) select r.Category).Distinct();
                this.Keys = (from r in result where !string.IsNullOrEmpty(r.Key) select r.Key).Distinct().ToList();
                this.Names = (from r in result where !string.IsNullOrEmpty(r.Name) select r.Name).Distinct().ToList();
                this.SymbolClasses = (from r in result where !string.IsNullOrEmpty(r.SymbolClass) select r.SymbolClass).Distinct();
                this.Tags = (from r in result.SelectMany(t => t.Tags) select r).Distinct();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            this.SearchResults = Enumerable.Empty<SymbolSourceModel>();
            this.ResultCount = 0;
            this.IsBusy = false;
        }

        private bool isBusy;

        public bool IsBusy
        {
            get
            {
                return this.isBusy;
            }

            private set
            {
                this.isBusy = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<string> categories;

        public IEnumerable<string> Categories
        {
            get
            {
                return this.categories;
            }

            set
            {
                this.categories = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<string> keys;

        public IEnumerable<string> Keys
        {
            get
            {
                return this.keys;
            }

            set
            {
                this.keys = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<string> names;

        public IEnumerable<string> Names
        {
            get
            {
                return this.names;
            }

            set
            {
                this.names = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<string> symbolClasses;

        public IEnumerable<string> SymbolClasses
        {
            get
            {
                return this.symbolClasses;
            }

            set
            {
                this.symbolClasses = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<string> tags;

        public IEnumerable<string> Tags
        {
            get
            {
                return this.tags;
            }

            set
            {
                this.tags = value;
                this.OnPropertyChanged();
            }
        }

        private IEnumerable<SymbolSourceModel> searchResults;

        public IEnumerable<SymbolSourceModel> SearchResults
        {
            get
            {
                return this.searchResults;
            }

            set
            {
                this.searchResults = value;
                this.OnPropertyChanged();
            }
        }

        private int resultCount;

        public int ResultCount
        {
            get
            {
                return this.resultCount;
            }

            set
            {
                this.resultCount = value;
                this.OnPropertyChanged();
            }
        }

        private string selectedCategory;

        public string SelectedCategory
        {
            get
            {
                return this.selectedCategory;
            }

            set
            {
                this.selectedCategory = value;
                this.OnPropertyChanged();
            }
        }

        private string selectedKey;

        public string SelectedKey
        {
            get
            {
                return this.selectedKey;
            }

            set
            {
                this.selectedKey = value;
                this.OnPropertyChanged();
            }
        }

        private string selectedName;

        public string SelectedName
        {
            get
            {
                return this.selectedName;
            }

            set
            {
                this.selectedName = value;
                this.OnPropertyChanged();
            }
        }

        private string selectedSymbolClass;

        public string SelectedSymbolClass
        {
            get
            {
                return this.selectedSymbolClass;
            }

            set
            {
                this.selectedSymbolClass = value;
                this.OnPropertyChanged();
            }
        }

        private string selectedTag;

        public string SelectedTag
        {
            get
            {
                return this.selectedTag;
            }

            set
            {
                this.selectedTag = value;
                this.OnPropertyChanged();
            }
        }

        private string currentSelection;

        public string CurrentSelection
        {
            get
            {
                return this.currentSelection;
            }

            set
            {
                this.currentSelection = value;
                this.OnPropertyChanged();
            }
        }

        private bool categoriesStrictMatch;

        public bool CategoriesStrictMatch
        {
            get
            {
                return this.categoriesStrictMatch;
            }

            set
            {
                this.categoriesStrictMatch = value;
                this.OnPropertyChanged();
            }
        }

        private bool keysStrictMatch;

        public bool KeysStrictMatch
        {
            get
            {
                return this.keysStrictMatch;
            }

            set
            {
                this.keysStrictMatch = value;
                this.OnPropertyChanged();
            }
        }

        private bool namesStrictMatch;

        public bool NamesStrictMatch
        {
            get
            {
                return this.namesStrictMatch;
            }

            set
            {
                this.namesStrictMatch = value;
                this.OnPropertyChanged();
            }
        }

        private bool symbolClassesStrictMatch;

        public bool SymbolClassesStrictMatch
        {
            get
            {
                return this.symbolClassesStrictMatch;
            }

            set
            {
                this.symbolClassesStrictMatch = value;
                this.OnPropertyChanged();
            }
        }

        private bool tagsStrictMatch;

        public bool TagsStrictMatch
        {
            get
            {
                return this.tagsStrictMatch;
            }

            set
            {
                this.tagsStrictMatch = value;
                this.OnPropertyChanged();
            }
        }

        private SymbolSourceModel selectedResult;

        public SymbolSourceModel SelectedResult
        {
            get
            {
                return this.selectedResult;
            }

            set
            {
                this.selectedResult = value;
                this.OnPropertyChanged();
            }
        }

        private ICommand addSelection;

        public ICommand AddSelection
        {
            get
            {
                if (this.addSelection == null)
                {
                    this.addSelection = new DelegateCommand(this.OnAddSelection);
                }

                return this.addSelection;
            }
        }

        private void OnAddSelection(object parameter)
        {
            if (!(parameter is string) || string.IsNullOrEmpty((string)parameter))
            {
                return;
            }

            var parameterName = (string)parameter;
            switch (parameterName)
            {
                case "Category":
                    {
                        if (string.IsNullOrEmpty(this.SelectedCategory) || this.categoriesParameter.Contains(this.SelectedCategory))
                        {
                            return;
                        }

                        this.categoriesParameter.Add(this.SelectedCategory);
                        this.CurrentSelection = string.Join(";", this.categoriesParameter);
                        break;
                    }

                case "Key":
                    {
                        if (string.IsNullOrEmpty(this.SelectedKey) || this.keysParameter.Contains(this.SelectedKey))
                        {
                            return;
                        }

                        this.keysParameter.Add(this.SelectedKey);
                        this.CurrentSelection = string.Join(";", this.keysParameter);
                        break;
                    }

                case "Name":
                    {
                        if (string.IsNullOrEmpty(this.SelectedName) || this.namesParameter.Contains(this.SelectedName))
                        {
                            return;
                        }

                        this.namesParameter.Add(this.SelectedName);
                        this.CurrentSelection = string.Join(";", this.namesParameter);
                        break;
                    }

                case "SymbolClass":
                    {
                        if (string.IsNullOrEmpty(this.SelectedSymbolClass) || this.symbolClassesParameter.Contains(this.SelectedSymbolClass))
                        {
                            return;
                        }

                        this.symbolClassesParameter.Add(this.SelectedSymbolClass);
                        this.CurrentSelection = string.Join(";", this.symbolClassesParameter);
                        break;
                    }

                case "Tag":
                    {
                        if (string.IsNullOrEmpty(this.SelectedTag) || this.tagsParameter.Contains(this.SelectedTag))
                        {
                            return;
                        }

                        this.tagsParameter.Add(this.SelectedTag);
                        this.CurrentSelection = string.Join(";", this.tagsParameter);
                        break;
                    }
            }
        }

        private ICommand subtractSelection;

        public ICommand SubtractSelection
        {
            get
            {
                if (this.subtractSelection == null)
                {
                    this.subtractSelection = new DelegateCommand(this.OnSubtractSelection);
                }

                return this.subtractSelection;
            }
        }

        private void OnSubtractSelection(object parameter)
        {
            if (!(parameter is string) || string.IsNullOrEmpty((string)parameter))
            {
                return;
            }

            var parameterName = (string)parameter;
            switch (parameterName)
            {
                case "Category":
                    {
                        if (string.IsNullOrEmpty(this.SelectedCategory) || !this.categoriesParameter.Contains(this.SelectedCategory))
                        {
                            return;
                        }

                        this.categoriesParameter.Remove(this.SelectedCategory);
                        this.SelectedCategory = string.Empty;
                        this.CurrentSelection = string.Join(";", this.categoriesParameter);
                        break;
                    }

                case "Key":
                    {
                        if (string.IsNullOrEmpty(this.SelectedKey) || !this.keysParameter.Contains(this.SelectedKey))
                        {
                            return;
                        }

                        this.keysParameter.Remove(this.SelectedKey);
                        this.SelectedKey = string.Empty;
                        this.CurrentSelection = string.Join(";", this.keysParameter);
                        break;
                    }

                case "Name":
                    {
                        if (string.IsNullOrEmpty(this.SelectedName) || !this.namesParameter.Contains(this.SelectedName))
                        {
                            return;
                        }

                        this.namesParameter.Remove(this.SelectedName);
                        this.SelectedName = string.Empty;
                        this.CurrentSelection = string.Join(";", this.namesParameter);
                        break;
                    }

                case "SymbolClass":
                    {
                        if (string.IsNullOrEmpty(this.SelectedSymbolClass) || !this.symbolClassesParameter.Contains(this.SelectedSymbolClass))
                        {
                            return;
                        }

                        this.symbolClassesParameter.Remove(this.SelectedSymbolClass);
                        this.SelectedSymbolClass = string.Empty;
                        this.CurrentSelection = string.Join(";", this.symbolClassesParameter);
                        break;
                    }

                case "Tag":
                    {
                        if (string.IsNullOrEmpty(this.SelectedTag) || !this.tagsParameter.Contains(this.SelectedTag))
                        {
                            return;
                        }

                        this.tagsParameter.Remove(this.SelectedTag);
                        this.SelectedTag = string.Empty;
                        this.CurrentSelection = string.Join(";", this.tagsParameter);
                        break;
                    }
            }
        }

        private ICommand searchSymbols;

        public ICommand SearchSymbols
        {
            get
            {
                if (this.searchSymbols == null)
                {
                    this.searchSymbols = new DelegateCommand(this.OnSearchSymbols);
                }

                return this.searchSymbols;
            }
        }

        private async void OnSearchSymbols(object parameter)
        {
            this.IsBusy = true;
            var parameters = new StyleSymbolSearchParameters();
            if (this.categoriesParameter.Count > 0)
            {
                foreach (var category in this.categoriesParameter)
                {
                    parameters.Categories.Add(category);
                }
            }

            if (this.keysParameter.Count > 0)
            {
                foreach (var key in this.keysParameter)
                {
                    parameters.Keys.Add(key);
                }
            }

            if (this.namesParameter.Count > 0)
            {
                foreach (var name in this.namesParameter)
                {
                    parameters.Names.Add(name);
                }
            }

            if (this.symbolClassesParameter.Count > 0)
            {
                foreach (var symbolClass in this.symbolClassesParameter)
                {
                    parameters.SymbolClasses.Add(symbolClass);
                }
            }

            if (this.tagsParameter.Count > 0)
            {
                foreach (var tag in this.tagsParameter)
                {
                    parameters.Tags.Add(tag);
                }
            }

            parameters.CategoriesStrictMatch = this.CategoriesStrictMatch;
            parameters.KeysStrictMatch = this.KeysStrictMatch;
            parameters.NamesStrictMatch = this.NamesStrictMatch;
            parameters.SymbolClassesStrictMatch = this.SymbolClassesStrictMatch;
            parameters.TagsStrictMatch = this.TagsStrictMatch;
            var result = await this.dictionary.SearchSymbolsAsync(parameters);
            var symbolSource = new List<SymbolSourceModel>();
            if (result != null)
            {
                foreach (var r in result)
                {
                    ImageSource source = null;
                    try
                    {
                        source = await this.CreateSwatchAsync(r.Symbol);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                    }

                    if (source != null)
                    {
                        symbolSource.Add(new SymbolSourceModel(source, r));
                    }
                }
            }

            this.SearchResults = symbolSource;
            this.ResultCount = symbolSource.Count;
            this.IsBusy = false;
        }

        private async Task<ImageSource> CreateSwatchAsync(Symbol symbol)
        {
            var nativeImage = await symbol.CreateSwatchAsync();
#if __ANDROID__
            var memstream = new MemoryStream();
            await nativeImage.CompressAsync(Android.Graphics.Bitmap.CompressFormat.Png, 100, memstream);
            memstream.Seek(0L, SeekOrigin.Begin);
            return ImageSource.FromStream(() => memstream);
#elif __IOS__
            return ImageSource.FromStream(() => nativeImage.AsPNG().AsStream());
#elif NETFX_CORE && XAMARIN
            //var bmp = nativeImage as Windows.UI.Xaml.Media.Imaging.WriteableBitmap;
            //MemoryStream ms = new MemoryStream();
            //var stream = ms.AsRandomAccessStream();
            //var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.JpegEncoderId, stream);
            //Stream pixelStream = bmp.PixelBuffer.AsStream();
            //byte[] pixels = new byte[pixelStream.Length];
            //await pixelStream.ReadAsync(pixels, 0, pixels.Length);
            //var dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView()?.LogicalDpi ?? 96f;
            //encoder.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8, Windows.Graphics.Imaging.BitmapAlphaMode.Ignore, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, dpi, dpi, pixels);
            // await encoder.FlushAsync();
            //ms.Seek(0, SeekOrigin.Begin);
            //return ImageSource.FromStream(() => { return ms; });
#else
            return nativeImage;
#endif
        }

        private ICommand clear;

        public ICommand Clear
        {
            get
            {
                if (this.clear == null)
                {
                    this.clear = new DelegateCommand(this.OnClear);
                }

                return this.clear;
            }
        }

        private void OnClear(object parameter)
        {
            this.categoriesParameter.Clear();
            this.keysParameter.Clear();
            this.namesParameter.Clear();
            this.symbolClassesParameter.Clear();
            this.tagsParameter.Clear();
            this.IsBusy = false;
            this.SelectedCategory = string.Empty;
            this.SelectedKey = string.Empty;
            this.SelectedName = string.Empty;
            this.SelectedSymbolClass = string.Empty;
            this.SelectedTag = string.Empty;
            this.CurrentSelection = string.Empty;
            this.CategoriesStrictMatch = false;
            this.KeysStrictMatch = false;
            this.NamesStrictMatch = false;
            this.SymbolClassesStrictMatch = false;
            this.TagsStrictMatch = false;
            this.SearchResults = Enumerable.Empty<SymbolSourceModel>();
            this.ResultCount = 0;
        }
    }
}