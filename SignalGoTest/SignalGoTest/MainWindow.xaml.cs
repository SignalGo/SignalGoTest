using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Client;
using SignalGo.Shared;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SignalGoTest
{
    /// <summary>
    /// my class comment
    /// </summary>
    public class MyClass
    {
        public string Ali { get; set; }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AsyncActions.InitializeUIThread();
           
            FindCommand = new RelayCommand((obj) =>
            {
                return true;
            }, (obj) =>
            {
                //txtSearch.Focus();
                //txtSearch.SelectAll();
            });
            this.InputBindings.Add(new KeyBinding(FindCommand, Key.F, ModifierKeys.Control));

            Closing += MainWindow_Closing;
            InitializeComponent();
            LoadData();

        }

        public ICommand FindCommand { get; private set; }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public static AppDataInfo CurrentAppData { get; set; } = new AppDataInfo();

        public static void SaveData()
        {
            try
            {
                var serial = Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAppData);
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                File.WriteAllText(path, serial, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"cannot save data : {ex.ToString()}");
            }
        }

        public void LoadData()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                CurrentAppData = Newtonsoft.Json.JsonConvert.DeserializeObject<AppDataInfo>(File.ReadAllText(path, Encoding.UTF8));
                foreach (var item in CurrentAppData.Items)
                {
                    ConnectionInfo.DoOrder(item.Items);
                    tabControl.Items.Insert(0, new TabItem() { DataContext = item, Style = (Style)tabControl.TryFindResource("removableTab"), Content = new ConnectionInfo() { DataContext = item } });
                }

                //txtAddress.Text = appdata.ServerAddress;
                //List<object> items = new List<object>();
                //items.AddRange(appdata.Items.Services);
                //items.AddRange(appdata.Items.Callbacks);
                //items.Add(appdata.Items.WebApiDetailsInfo);
                //items.Add(appdata.Items.ProjectDomainDetailsInfo);
                //TreeViewServices.DataContext = appdata.Items;
                //TreeViewServices.ItemsSource = null;
                //TreeViewServices.ItemsSource = items;
                //fullItems = items;
                //lstHistoryCalls.ItemsSource = appdata.Histories;
            }
            catch (Exception ex)
            {

            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTabName.Text))
            {
                MessageBox.Show("please fill name");
                return;
            }
            var connectionData = new ConnectionData() { Name = txtTabName.Text };
            CurrentAppData.Items.Add(connectionData);
            tabControl.Items.Insert(0, new TabItem() { DataContext = connectionData, Style = (Style)tabControl.TryFindResource("removableTab"), Content = new ConnectionInfo() { DataContext = connectionData } });

            txtTabName.Text = "";
            SaveData();
        }

        private void btnDeleteTab_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Delete Tab", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                var btn = (Button)sender;
                var connectionData = (ConnectionData)btn.DataContext;
                TabItem findItem = null;
                foreach (TabItem item in tabControl.Items)
                {
                    if (item.DataContext == connectionData)
                    {
                        findItem = item;
                        break;
                    }
                }

                tabControl.Items.Remove(findItem);
                CurrentAppData.Items.Remove(connectionData);
                SaveData();
            }
        }
    }
}
