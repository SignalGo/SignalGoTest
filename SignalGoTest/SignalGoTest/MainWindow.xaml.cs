using Newtonsoft.Json;
using SignalGo.Client;
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

namespace SignalGoTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Closing += MainWindow_Closing;
            InitializeComponent();
            LoadData();

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        ClientProvider provider = null;
        private void btnconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                provider = new ClientProvider();
                provider.Connect(txtAddress.Text);
                provider.RegisterClientServiceInterface(txtServiceName.Text);

                var result = provider.GetServiceDetial(txtServiceName.Text);
                btnconnect.IsEnabled = false;
                btndisconnect.IsEnabled = true;
                List<MethodItemInfo> items = new List<SignalGoTest.MethodItemInfo>();
                foreach (var item in result)
                {
                    var m = new SignalGoTest.MethodItemInfo() { Name = item.MethodName };
                    m.Items = new List<ParameterItemInfo>();
                    if (item.Parameters != null)
                        foreach (var p in item.Parameters)
                        {
                            m.Items.Add(new ParameterItemInfo() { Name = p.Name, Type = p.Type });
                        }
                    items.Add(m);
                }
                UpdateData((List<MethodItemInfo>)lstMethods.ItemsSource, items);
                lstMethods.ItemsSource = null;
                lstMethods.ItemsSource = items;
                SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, ServiceName = txtServiceName.Text, Items = items });
            }
            catch (Exception ex)
            {
                provider.Dispose();
                provider = null;
                btnconnect.IsEnabled = true;
                btndisconnect.IsEnabled = false;
                MessageBox.Show(ex.Message);
            }
        }

        public void UpdateData(List<MethodItemInfo> oldData, List<MethodItemInfo> newData)
        {
            if (oldData == null || newData == null)
                return;
            foreach (var item in oldData)
            {
                var find = (from x in newData where x.Name == item.Name && x.Items.Count == item.Items.Count select x).FirstOrDefault();
                if (find != null)
                {
                    foreach (var parameter in item.Items)
                    {
                        var p = (from x in find.Items where x.Name == parameter.Name select x).FirstOrDefault();
                        if (p != null)
                        {
                            p.IsJson = parameter.IsJson;
                            p.Value = parameter.Value;
                        }
                    }
                }
            }
        }

        public void SaveData(AppDataInfo data)
        {
            try
            {
                var serial = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                File.WriteAllText(path, serial, Encoding.UTF8);
            }
            catch (Exception ex)
            {

            }
        }

        public void LoadData()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                var appdata = Newtonsoft.Json.JsonConvert.DeserializeObject<AppDataInfo>(File.ReadAllText(path, Encoding.UTF8));
                txtAddress.Text = appdata.ServerAddress;
                txtServiceName.Text = appdata.ServiceName;
                lstMethods.ItemsSource = null;
                lstMethods.ItemsSource = appdata.Items;
                lstHistoryCalls.ItemsSource = appdata.Histories;
            }
            catch (Exception ex)
            {

            }
        }

        private void btndisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (provider != null)
            {
                provider.Dispose();
            }
            provider = null;
            btndisconnect.IsEnabled = false;
            btnconnect.IsEnabled = true;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, ServiceName = txtServiceName.Text, Items = (List<MethodItemInfo>)lstMethods.ItemsSource, Histories = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstMethods.SelectedItem == null)
                    return;
                btnSend.IsEnabled = false;
                var serviceName = txtServiceName.Text;
                var selectedMethod = (MethodItemInfo)lstMethods.SelectedItem;
                ServiceDetailMethod sendReq = new ServiceDetailMethod();
                sendReq.MethodName = selectedMethod.Name;
                sendReq.Parameters = new List<ServiceDetailParameterInfo>();
                if (selectedMethod.Items != null)
                {
                    foreach (var item in selectedMethod.Items)
                    {
                        sendReq.Parameters.Add(new ServiceDetailParameterInfo() { Name = item.Name, Type = item.Type, Value = item.IsJson ? item.Value : JsonConvert.SerializeObject(item.Value), IsJson = item.IsJson });
                    }
                }
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        string request = "";
                        var response = provider.SendRequest(serviceName, sendReq, out request);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            var history = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource;
                            if (history == null)
                                history = new List<SignalGoTest.HistoryCallInfo>();
                            response = FormatJson(response);
                            history.Insert(0, new HistoryCallInfo() { CallDateTime = DateTime.Now, MethodName = sendReq.MethodName, Request = FormatJson(request), Response = response });
                            lstHistoryCalls.ItemsSource = null;
                            lstHistoryCalls.ItemsSource = history;
                            txtReponse.Text = response;
                            btnSave_Click(null, null);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            txtReponse.Text = ex.Message + Environment.NewLine + ex.StackTrace;
                        }));
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        btnSend.IsEnabled = true;
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("error", ex.Message);
                btnSend.IsEnabled = true;
            }
        }

        private void loadTemplate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            try
            {
                if (lstMethods.SelectedItem == null)
                    return;
                btn.IsEnabled = false;
                var serviceName = txtServiceName.Text;
                var method = (MethodItemInfo)lstMethods.SelectedItem;
                var parameter = (ParameterItemInfo)btn.DataContext;
                var paramIndex = method.Items.IndexOf(parameter);
                MethodParameterDetails sendReq = new MethodParameterDetails();
                sendReq.MethodName = method.Name;
                sendReq.ParametersCount = method.Items.Count;
                sendReq.ServiceName = txtServiceName.Text;
                sendReq.ParameterIndex = paramIndex;

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var response = provider.GetMethodParameterDetial(sendReq);
                        Dispatcher.Invoke(new Action(() =>
                        {
                            txtReponse.Text = FormatJson(response);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            txtReponse.Text = ex.Message + Environment.NewLine + ex.StackTrace;
                        }));
                    }
                    Dispatcher.Invoke(new Action(() =>
                    {
                        btn.IsEnabled = true;
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("error", ex.Message);
                btn.IsEnabled = true;
            }
        }

        private static string FormatJson(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
