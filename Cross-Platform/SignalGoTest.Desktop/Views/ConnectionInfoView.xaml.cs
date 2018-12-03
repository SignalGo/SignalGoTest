using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SignalGoTest.ViewModels;

namespace SignalGoTest.Desktop.Views
{
    public class ConnectionInfoView : UserControl
    {
        public ConnectionInfoView()
        {
            ConnectionInfoViewModel.ShowJsonTemplateWindowAction = async (value) =>
            {
                JsonTemplateWindow jsonTemplateWindow = new JsonTemplateWindow();
                jsonTemplateWindow.ZIndex = 100;
                jsonTemplateWindow.Text = value;
                await jsonTemplateWindow.ShowDialog();
            };

            InitializeComponent();
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
