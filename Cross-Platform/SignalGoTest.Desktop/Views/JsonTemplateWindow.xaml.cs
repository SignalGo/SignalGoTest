using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SignalGoTest.Desktop.Views
{
    public class JsonTemplateWindow : Window
    {
        public JsonTemplateWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        public string Text
        {
            set
            {
                TextBox TxtJson = this.FindControl<TextBox>("TxtJson");
                TxtJson.Text = value;
            }
        }
    }
}
