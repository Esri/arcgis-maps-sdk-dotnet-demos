using Esri.ArcGISRuntime.Hydrography;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System;

namespace HydrographicsSample
{
    /// <summary>
    /// Window to browse an exchange set and select the datasets
    /// </summary>
    public partial class OpenExchangeSetWindow : Window
    {
        private List<DatasetVM> datasets;

        public OpenExchangeSetWindow(string filePath)
        {
            InitializeComponent();
            LoadDataset(filePath);
        }

        private async void LoadDataset(string filePath)
        {
            try
            {
                var set = new Esri.ArcGISRuntime.Hydrography.EncExchangeSet(new string[] { filePath });
                await set.LoadAsync();
                description.Text = set.Readme;
                datagrid.ItemsSource = datasets = new List<DatasetVM>(set.Datasets.Select(d => new DatasetVM(d)));
            }
            catch(System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading dataset");
                Cancel_Click(null, null);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            base.DialogResult = false;
            this.Close();
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            if (datasets.Where(t => t.Load).Any())
            {
                base.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("No datasets selected");
            }
        }

        public IEnumerable<EncDataset> SelectedDatasets
        {
            get
            {
                return datasets.Where(t => t.Load).Select(t => t.DataSet);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool state = (sender as CheckBox).IsChecked.Value;
            foreach (var item in datasets)
                item.Load = state;
        }
    }

    public class DatasetVM : INotifyPropertyChanged
    {
        public DatasetVM(EncDataset dataset)
        {
            DataSet = dataset;
        }

        internal EncDataset DataSet { get; }

        private bool _load;

        public bool Load
        {
            get { return _load; }
            set { _load = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Load))); }
        }

        public string Name => DataSet.Name;

        public string Volume => DataSet.VolumeName;

        public string Description => DataSet.Description;

        public string Extent => DataSet.Extent?.ToJson();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
