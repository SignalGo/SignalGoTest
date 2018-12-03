using Avalonia;
using Avalonia.Controls;
using MvvmGo.Commands;

namespace SignalGoTest.Desktop.Helpers
{
    public class CommandsViewModel
    {
        public CommandsViewModel()
        {
            CopyCommand = new Command<ContextMenu>(async (contextMenu) =>
            {
                TextBox textBox = contextMenu.Parent.Parent as TextBox;
                await Application.Current.Clipboard.SetTextAsync(textBox.Text);
            });
            CutCommand = new Command<ContextMenu>(async (contextMenu) =>
            {
                TextBox textBox = contextMenu.Parent.Parent as TextBox;
                await Application.Current.Clipboard.SetTextAsync(textBox.Text);
                textBox.Text = "";
            });
            SelctAllCommand = new Command<ContextMenu>((contextMenu) =>
            {
                TextBox textBox = contextMenu.Parent.Parent as TextBox;
                textBox.SelectionStart = 0;
                textBox.SelectionEnd = textBox.Text.Length;
            });
            PasteCommand = new Command<ContextMenu>(async (contextMenu) =>
            {
                TextBox textBox = contextMenu.Parent.Parent as TextBox;

                textBox.Text = await Application.Current.Clipboard.GetTextAsync();
            });

            ClearCommand = new Command<ContextMenu>((contextMenu) =>
            {
                TextBox textBox = contextMenu.Parent.Parent as TextBox;

                textBox.Text = "";
            });
        }
        public Command<ContextMenu> CutCommand { get; set; }
        public Command<ContextMenu> CopyCommand { get; set; }
        public Command<ContextMenu> PasteCommand { get; set; }
        public Command<ContextMenu> ClearCommand { get; set; }
        public Command<ContextMenu> SelctAllCommand { get; set; }
    }
}
