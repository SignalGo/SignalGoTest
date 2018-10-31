using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

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

            DialogBorder.Bind(IsVisibleProperty, new Binding("IsBusy", Avalonia.Data.BindingMode.OneWay) { Source = this });
            WhiteGrid.Bind(IsVisibleProperty, new Binding("IsBusy", Avalonia.Data.BindingMode.OneWay) { Source = this });
            TitleTextBlock.Bind(TextBlock.TextProperty, new Binding("Message", Avalonia.Data.BindingMode.OneWay) { Source = this });
        }

        private ProgressBar CurrentProgress { get; set; }
        private Grid ButtonsGrid { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

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
        public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<UserControl, string>(nameof(Message));

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
