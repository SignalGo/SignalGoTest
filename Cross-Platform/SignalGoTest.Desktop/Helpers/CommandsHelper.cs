using Avalonia;
using Avalonia.Controls;

namespace SignalGoTest.Desktop.Helpers
{
    public class CommandsHelper : AvaloniaObject
    {
        public static readonly AvaloniaProperty<string> AttachCommandProperty = AvaloniaProperty.Register<CommandsHelper, string>(nameof(AttachCommand), notifying: (obj, value) =>
        {

        });

        public string AttachCommand
        {
            get { return GetValue(AttachCommandProperty); }
            set { SetValue(AttachCommandProperty, value); }
        }

        public static string GetAttachCommand(Control control)
        {
            return (string)control.GetValue(AttachCommandProperty);
        }

        public static void SetAttachCommand(Control control, bool value)
        {
            control.SetValue(AttachCommandProperty, value);
        }
    }
}
