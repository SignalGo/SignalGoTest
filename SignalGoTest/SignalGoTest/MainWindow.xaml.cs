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
using System.Xml.Linq;

namespace SignalGoTest
{
    /// <summary>
    /// my class comment
    /// </summary>
    public class MyClass
    {
        /// <summary>
        /// یک متد مثال است
        /// </summary>
        /// <param name="name">نام شخص مورد نظر</param>
        /// <param name="age">سن شخص مورد نظر</param>
        /// <returns>متن پیام می باشد</returns>
        /// <exception cref="MainWindow.btnconnect">وقتی کلاس پروفایل خالی باشد یا نام یا نام خانوادگی وارد نشده باشد</exception>
        /// <exception cref="MainWindow.txtAddress">خطای الکی</exception>
        public string ExampleProperty(string name, int age)
        {
            return "ascass";
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //try
            //{
            //    using (XamlCommentLoader loader = new XamlCommentLoader())
            //    {
            //        var method = typeof(MyClass).GetMethod("ExampleProperty");
            //        var documentation = loader.GetCommment(method);

            //    }


            //    //var doc = XElement.Load(@"D:\Github\Atitec.OffseeAPI\Atitec.OffseeAPI\Atitec.OffseeAPI.ConsoleServer\bin\Debug\Atitec.Models.xml");
            //    //var comments = doc.DescendantNodes().ToList();

            //    //foreach (var comment in comments)
            //    //{
            //    //    //comment.na
            //    //    var type = comment.GetType().ToString();
            //    //}
            //}
            //catch (Exception ex)
            //{


            //}
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
                provider.OnDisconnected = () =>
                {
                    btndisconnect_Click(null, null);
                };
                provider.Connect(txtAddress.Text);
                var result = provider.GetListOfServicesWithDetials(txtAddress.Text);
                result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.Name).ToList();
                provider.RegisterClientServiceInterface(result.Services.FirstOrDefault().ServiceName);

                btnconnect.IsEnabled = false;
                btndisconnect.IsEnabled = true;
                //foreach (var serviceDetails in result)
                //{
                //    foreach (var serviceInterface in serviceDetails.Services)
                //    {
                //        foreach (var method in serviceInterface.Methods)
                //        {
                //            foreach (var parameter in method.Parameters)
                //            {
                //                //parameter.Name
                //            }
                //        }
                //    }
                //}
                //List<MethodItemInfo> items = new List<SignalGoTest.MethodItemInfo>();
                //foreach (var server in result)
                //{
                //    foreach (var service in server.Services)
                //    {
                //        foreach (var method in service.Methods)
                //        {
                //            var m = new SignalGoTest.MethodItemInfo() { Name = method.MethodName };
                //            m.Items = new List<ParameterItemInfo>();
                //            if (method.Parameters != null)
                //                foreach (var p in method.Parameters)
                //                {
                //                    m.Items.Add(new ParameterItemInfo() { Name = p.Name, Type = p.Type });
                //                }
                //            items.Add(m);
                //        }
                //    }
                //}
                UpdateData((ProviderDetailsInfo)TreeViewServices.DataContext, result);
                TreeViewServices.DataContext = result;
                List<object> items = new List<object>();
                items.AddRange(result.Services);
                items.Add(result.WebApiDetailsInfo);
                items.Add(result.ProjectDomainDetailsInfo);
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
                //SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, ServiceName = txtServiceName.Text, Items = items });
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

        public void UpdateData(ProviderDetailsInfo oldData, ProviderDetailsInfo newData)
        {
            if (oldData == null || newData == null)
                return;
            foreach (var server in oldData.Services)
            {
                var findServer = newData.Services.FirstOrDefault(x => x.FullNameSpace == server.FullNameSpace);
                if (findServer == null)
                    continue;
                foreach (var service in server.Services)
                {
                    var findService = findServer.Services.FirstOrDefault(x => x.FullNameSpace == service.FullNameSpace);
                    foreach (var method in service.Methods)
                    {
                        if (findService == null)
                            continue;
                        var find = (from x in findService.Methods where x.MethodName == method.MethodName && x.Parameters.Count == method.Parameters.Count select x).FirstOrDefault();
                        if (find != null)
                        {
                            foreach (var parameter in method.Parameters)
                            {
                                var p = (from x in find.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                if (p != null)
                                {
                                    p.IsJson = parameter.IsJson;
                                    p.Value = parameter.Value?.ToString();
                                    p.TemplateValue = parameter.TemplateValue?.ToString();
                                }
                            }
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
                //txtServiceName.Text = appdata.ServiceName;
                List<object> items = new List<object>();
                items.AddRange(appdata.Items.Services);
                items.Add(appdata.Items.WebApiDetailsInfo);
                items.Add(appdata.Items.ProjectDomainDetailsInfo);
                TreeViewServices.DataContext = appdata.Items;
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
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
                provider.OnDisconnected = null;
                provider.Dispose();
            }
            provider = null;
            Dispatcher.Invoke(new Action(() =>
            {
                btndisconnect.IsEnabled = false;
                btnconnect.IsEnabled = true;
            }));

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, Items = (ProviderDetailsInfo)TreeViewServices.DataContext, Histories = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TreeViewServices.SelectedItem == null || !(TreeViewServices.SelectedItem is ServiceDetailsMethod))
                    return;
                btnSend.IsEnabled = false;
                var selectedMethod = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                var serviceName = ((ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).FirstOrDefault()).ServiceName;
                ServiceDetailsMethod sendReq = new ServiceDetailsMethod();
                sendReq.MethodName = selectedMethod.MethodName;
                sendReq.Parameters = new List<ServiceDetailsParameterInfo>();
                if (selectedMethod.Parameters != null)
                {
                    foreach (var item in selectedMethod.Parameters)
                    {
                        sendReq.Parameters.Add(new ServiceDetailsParameterInfo() { Name = item.Name, Type = item.Type, Value = item.IsJson ? item.Value : JsonConvert.SerializeObject(item.Value), IsJson = item.IsJson });
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
                            response = response == null ? "void ok" : FormatJson(response);
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
                if (TreeViewServices.SelectedItem == null || !(TreeViewServices.SelectedItem is ServiceDetailsMethod))
                    return;
                var column = DGRequestValues.Columns[2] as DataGridTextColumn;
                DataGridRow row = DGRequestValues.ItemContainerGenerator.ContainerFromIndex((DGRequestValues.SelectedIndex)) as DataGridRow;
                var cellTextBox = ((TextBlock)(column.GetCellContent(row)));

                btn.IsEnabled = false;
                var serviceName = ((ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).FirstOrDefault()).ServiceName;
                var method = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                var parameter = (ServiceDetailsParameterInfo)btn.DataContext;
                var paramIndex = method.Parameters.IndexOf(parameter);
                MethodParameterDetails sendReq = new MethodParameterDetails();
                sendReq.MethodName = method.MethodName;
                sendReq.ParametersCount = method.Parameters.Count;
                sendReq.ServiceName = serviceName;
                sendReq.ParameterIndex = paramIndex;

                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var response = provider.GetMethodParameterDetial(sendReq);
                        Dispatcher.Invoke(new Action(() =>
                        {
                            parameter.TemplateValue = FormatJson(response);
                            BindingExpression bindingExpression = DGRequestValues.GetBindingExpression(DataGrid.ItemsSourceProperty);
                            Binding parentBinding = bindingExpression.ParentBinding;

                            var source = DGRequestValues.ItemsSource;
                            DGRequestValues.ItemsSource = null;
                            DGRequestValues.ItemsSource = source;
                            DGRequestValues.SetBinding(DataGrid.ItemsSourceProperty, parentBinding);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            parameter.TemplateValue = ex.Message + Environment.NewLine + ex.StackTrace;
                            BindingExpression bindingExpression = DGRequestValues.GetBindingExpression(DataGrid.ItemsSourceProperty);
                            Binding parentBinding = bindingExpression.ParentBinding;

                            var source = DGRequestValues.ItemsSource;
                            DGRequestValues.ItemsSource = null;
                            DGRequestValues.ItemsSource = source;
                            DGRequestValues.SetBinding(DataGrid.ItemsSourceProperty, parentBinding);
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

        private void btnToString_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtReponse.Text = JsonConvert.DeserializeObject<string>(txtReponse.Text);
            }
            catch (Exception ex)
            {
                txtReponse.Text = ex.ToString();
            }
        }
    }
}
