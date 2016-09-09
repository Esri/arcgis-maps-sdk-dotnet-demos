using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace XamarinPortalBrowser.ViewModels
{
    class MainViewModel
    {
        public ParameterCommand PushMapPageCommand { get; }
        private readonly INavigation _navigation;
        public MainViewModel(INavigation navigation)
        {
            _navigation = navigation;
            PushMapPageCommand = new ParameterCommand(PushMapPage, true);
        }

        private async void PushMapPage(MapPage mapPage)
        {
            await _navigation.PushAsync(mapPage);
        }

        //private void ListView_ItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    Navigation.PushAsync(new MapPage());
        //}
    }

    public class Command : ICommand
    {
        private Action _action;
        private bool _canExecute;

        public Command(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;

        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class ParameterCommand : ICommand
    {
        private Action<MapPage> _action;
        private bool _canExecute;
        private MapPage _param;

        public ParameterCommand(Action<MapPage> action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _param = parameter as MapPage;
            _action(_param);
        }
    }
}
