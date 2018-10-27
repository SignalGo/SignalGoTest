using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SignalGo.Shared;
using SignalGoTest.Desktop.Views;
using SignalGoTest.Models;
using SignalGoTest.ViewModels;

namespace SignalGoTest.Desktop
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            AsyncActions.InitializeUIThread();
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Width = 1000;
            Height = 500;
            WindowState = WindowState.Maximized;
            //Button a;
            //a.Template = new templ
            //Avalonia.Markup.Xaml.Templates.DataTemplate a;
            //a.DataType
            ListBox ListConnections = this.FindControl<ListBox>("ListConnections");
            ListConnections.SelectedIndex = 1;
        }

        private void lslConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox ListConnections = this.FindControl<ListBox>("ListConnections");
            ConnectionInfoView ConnectionView = this.FindControl<ConnectionInfoView>("ConnectionView");
            AddNewView AddNewView = this.FindControl<AddNewView>("AddNewView");
            ConnectionInfo item = (ConnectionInfo)ListConnections.SelectedItem;
            ((ConnectionInfoViewModel)ConnectionView.DataContext).CurrentConnectionInfo = item;

            AddNewView.IsVisible = item.Name == "Add New...";
            ConnectionView.IsVisible = !AddNewView.IsVisible;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
