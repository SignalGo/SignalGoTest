using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SignalGoTest.Desktop.Views
{
    public class AddNewView : UserControl
    {
        public AddNewView()
        {
            this.InitializeComponent();
        }

        public void ContextMenu_ContextMenuOpening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
