using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SignalGo.Shared;
using SignalGoTest.Desktop.Views;
using SignalGoTest.Models;
using SignalGoTest.ViewModels;
using System.Linq;

namespace SignalGoTest.Desktop
{
    public class MainWindow : Window
    {
        public static MainWindow This { get; set; }
        public MainWindow()
        {
            This = this;
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
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainViewModel.This.Save();
        }

        private void lslConnections_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox ListConnections = this.FindControl<ListBox>("ListConnections");
            ConnectionInfoView ConnectionView = this.FindControl<ConnectionInfoView>("ConnectionView");
            AddNewView AddNewView = this.FindControl<AddNewView>("AddNewView");
            ConnectionInfo item = (ConnectionInfo)ListConnections.SelectedItem;
            if (item == null)
            {
                ListConnections.SelectedItem = MainViewModel.This.CurrentAppData.Items.First();
                return;
            }
            ((ConnectionInfoViewModel)((Control)ConnectionView.Content).DataContext).CurrentConnectionInfo = item;

            AddNewView.IsVisible = item.Name == "Add New...";
            ConnectionView.IsVisible = !AddNewView.IsVisible;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
