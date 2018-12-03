using Avalonia;
using Avalonia.Markup.Xaml;

namespace SignalGoTest.Desktop
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public void ContextMenu_Initialized(object sender, System.EventArgs e)
        {

        }
    }
}
