using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using MvvmGo.Commands;

namespace SignalGoTest.Desktop.Views
{
    public class BusyDialogView : UserControl
    {
        public BusyDialogView()
        {
            InitializeComponent();
            Border DialogBorder = this.FindControl<Border>("DialogBorder");
            Grid WhiteGrid = this.FindControl<Grid>("WhiteGrid");
            TextBlock TitleTextBlock = this.FindControl<TextBlock>("TitleTextBlock");

            CurrentProgress = this.FindControl<ProgressBar>("CurrentProgress");
            ButtonsGrid = this.FindControl<Grid>("ButtonsGrid");
            CancelButton = this.FindControl<Button>("CancelButton");
            OkButton = this.FindControl<Button>("OkButton");
            DialogBorder.Bind(IsVisibleProperty, new Binding("IsBusy", Avalonia.Data.BindingMode.OneWay) { Source = this });
            WhiteGrid.Bind(IsVisibleProperty, new Binding("IsBusy", Avalonia.Data.BindingMode.OneWay) { Source = this });
            TitleTextBlock.Bind(TextBlock.TextProperty, new Binding("Message", Avalonia.Data.BindingMode.OneWay) { Source = this });
        }

        private ProgressBar CurrentProgress { get; set; }
        private Grid ButtonsGrid { get; set; }
        private Button CancelButton { get; set; }
        private Button OkButton { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        public static readonly StyledProperty<Command> OkCommandProperty = AvaloniaProperty.Register<UserControl, Command>(nameof(OkCommand), notifying: (obj, value) =>
        {
            if (!value)
            {
                BusyDialogView current = (BusyDialogView)obj;
                current.OkButton.Command = current.OkCommand;
            }
        });
        public static readonly StyledProperty<Command> CancelCommandProperty = AvaloniaProperty.Register<UserControl, Command>(nameof(CancelCommand), notifying: (obj, value) =>
        {
            if (!value)
            {
                BusyDialogView current = (BusyDialogView)obj;
                current.CancelButton.Command = current.CancelCommand;
            }
        });
        public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<UserControl, bool>(nameof(IsBusy));
        public static readonly StyledProperty<bool> IsAlertProperty = AvaloniaProperty.Register<UserControl, bool>(nameof(IsAlert), notifying: (obj, value) =>
        {
            if (!value)
            {
                BusyDialogView current = (BusyDialogView)obj;
                current.ButtonsGrid.IsVisible = current.IsAlert;
                current.CurrentProgress.IsVisible = !current.IsAlert;
            }
        });
        public static readonly StyledProperty<bool> ShowCancelProperty = AvaloniaProperty.Register<UserControl, bool>(nameof(ShowCancel), notifying: (obj, value) =>
        {
            if (!value)
            {
                BusyDialogView current = (BusyDialogView)obj;
                current.CancelButton.IsVisible = current.ShowCancel;
            }
        });
        public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<UserControl, string>(nameof(Message));
        public Command OkCommand
        {
            get { return GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }
        public Command CancelCommand
        {
            get { return GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
        public bool IsBusy
        {
            get { return GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public bool IsAlert
        {
            get { return GetValue(IsAlertProperty); }
            set { SetValue(IsAlertProperty, value); }
        }
        public bool ShowCancel
        {
            get { return GetValue(ShowCancelProperty); }
            set { SetValue(ShowCancelProperty, value); }
        }

        public string Message
        {
            get { return GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        private Control _Child;

        public Control Child
        {
            get
            {
                return _Child;
            }
            set
            {
                _Child = value;
                Grid ChildGrid = this.FindControl<Grid>("ChildGrid");
                value.Bind(DataContextProperty, new Binding("DataContext", Avalonia.Data.BindingMode.OneWay) { Source = ChildGrid });
                ChildGrid.Children.Clear();
                ChildGrid.Children.Add(value);
            }
        }
    }
}
