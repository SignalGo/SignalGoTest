using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SignalGo.Shared.Models;
using SignalGoTest.ViewModels;
using System.Collections;

namespace SignalGoTest.Desktop.Views
{
    public class ConnectionInfoView : UserControl
    {
        public ConnectionInfoView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
