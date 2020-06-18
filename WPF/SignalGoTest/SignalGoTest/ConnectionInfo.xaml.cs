using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Client;
using SignalGo.Client.ClientManager;
using SignalGo.Shared;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace SignalGoTest
{
    /// <summary>
    /// Interaction logic for ConnectionInfo.xaml
    /// </summary>
    public partial class ConnectionInfo : UserControl
    {
        public ConnectionInfo()
        {
            DataContextChanged += ConnectionInfo_DataContextChanged;
            InitializeComponent();
        }

        private void ConnectionInfo_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadData();
        }

        private List<object> fullItems { get; set; }

        //object _oldSelectedItem = null;
        //object _newSelectedItem = null;

        private ClientProvider provider = new ClientProvider();
        private void btnconnect_Click(object sender, RoutedEventArgs e)
        {
            btnSend.IsEnabled = true;
            busyIndicator.BusyContent = "Connecting...";
            busyIndicator.IsBusy = true;
            string address = txtAddress.Text;
            string search = txtSearch.Text;
            // _oldSelectedItem = TreeViewServices.SelectedItem;
            SaveTreeViewSelectedItem();
            AsyncActions.Run(async () =>
            {
                try
                {
                    provider.OnConnectionChanged = (connected) =>
                    {
                        if (connected == ConnectionStatus.Disconnected)
                            btndisconnect_Click(null, null);
                    };

                    provider.OnCalledMethodAction = (callInfo) =>
                    {
                        AsyncActions.RunOnUI(() =>
                        {
                            try
                            {
                                ConnectionData details = (ConnectionData)DataContext;
                                CallbackServiceLogInfo findService = details.CallbackCalls.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName);
                                if (findService == null)
                                {
                                    details.CallbackCalls.Add(new CallbackServiceLogInfo { ServiceName = callInfo.ServiceName });
                                    findService = details.CallbackCalls.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName);
                                }
                                CallbackMethodLogInfo logInfo = new CallbackMethodLogInfo() { DateTime = DateTime.Now, MethodName = callInfo.MethodName };
                                ServiceDetailsMethod findFromBase = details.Items.Callbacks.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName).Methods.FirstOrDefault(x => x.MethodName == callInfo.MethodName && x.Requests.First().Parameters.Count == callInfo.Parameters.Length);

                                foreach (ParameterInfo item in callInfo.Parameters)
                                {
                                    Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JToken.Parse(item.Value);
                                    string value = token.ToString(Newtonsoft.Json.Formatting.Indented);
                                    ServiceDetailsParameterInfo findedParam = findFromBase.Requests.First().Parameters[Array.IndexOf(callInfo.Parameters, item)];
                                    logInfo.Parameters.Add(new CallbackParameterLogInfo() { Value = value, Name = findedParam.Name, ParameterType = findedParam.Type });
                                }

                                findService.Calls.Insert(0, logInfo);
                                btnSave_Click(null, null);
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    };
                    provider.ProviderSetting.IsEnabledToUseTimeout = true;
                    provider.ProviderSetting.ReceiveDataTimeout = new TimeSpan(0, 0, 5);
                    provider.ProviderSetting.SendDataTimeout = new TimeSpan(0, 0, 5);
                    provider.ConnectAsync(address).GetAwaiter().GetResult();
                    AsyncActions.RunOnUI(() =>
                    {
                        busyIndicator.BusyContent = "Receiving data...";
                        btnconnect.IsEnabled = false;
                        btndisconnect.IsEnabled = true;
                    });
                    ProviderDetailsInfo result = await provider.GetListOfServicesWithDetials(address);
                    await CalculateDetails(result, search);
                    //SelectTreeViewOldItem();
                    //SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, ServiceName = txtServiceName.Text, Items = items });
                }
                catch (Exception ex)
                {
                    AsyncActions.RunOnUI(() =>
                    {
                        provider.Disconnect();
                        btnconnect.IsEnabled = true;
                        btndisconnect.IsEnabled = false;
                        MessageBox.Show(ex.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                        busyIndicator.IsBusy = false;
                    });
                }
            });

        }

        public async Task CalculateDetails(ProviderDetailsInfo result, string search)
        {
            DoOrder(result);
            result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.Name).ToList();
            //if (result.Services.Count > 0)
            //{
            //    foreach (var item in result.Services)
            //    {
            //        AsyncActions.RunOnUI(() =>
            //        {
            //            busyIndicator.BusyContent = $"RegisterService {item.ServiceName}...";
            //        });
            //        provider.RegisterServerService(item.ServiceName);
            //    }
            //}
            AsyncActions.RunOnUI(() =>
            {
                ConnectionData connectionData = (ConnectionData)DataContext;
                connectionData.Items = result;
                UpdateData((ProviderDetailsInfo)TreeViewServices.DataContext, result);
                result.ProjectDomainDetailsInfo.Models = result.ProjectDomainDetailsInfo.Models.OrderBy(x => x.ObjectType).ToList();
                TreeViewServices.DataContext = result;
                List<object> items = new List<object>();
                items.AddRange(result.Services);
                items.AddRange(result.Callbacks);
                items.Add(result.WebApiDetailsInfo);
                items.Add(result.ProjectDomainDetailsInfo);
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
                fullItems = items;
                MainWindow.SaveData();
                busyIndicator.IsBusy = false;
            });
            if (!string.IsNullOrEmpty(search))
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    TextBox_TextChanged(txtSearch, null);
                    SelectTreeViewOldItem();
                }));
        }


        public static void DoOrder(ProviderDetailsInfo result)
        {
            if (result == null || result.Services == null)
                return;
            foreach (ServiceDetailsInfo serviceClass in result.Services)
            {
                foreach (ServiceDetailsInterface interfaceInfo in serviceClass.Services)
                {
                    interfaceInfo.Methods = interfaceInfo.Methods?.OrderBy(x => x.MethodName).ToList();
                }
            }
        }

        public void UpdateData(ProviderDetailsInfo oldData, ProviderDetailsInfo newData)
        {
            if (oldData == null || newData == null)
                return;
            newData.IsExpanded = oldData.IsExpanded;
            newData.IsSelected = oldData.IsSelected;

            foreach (ServiceDetailsInfo server in oldData.Services)
            {
                ServiceDetailsInfo findServer = newData.Services.FirstOrDefault(x => x.FullNameSpace == server.FullNameSpace);
                if (findServer == null)
                    continue;
                //if (_oldSelectedItem == server)
                //    _newSelectedItem = findServer;
                findServer.IsExpanded = server.IsExpanded;
                findServer.IsSelected = server.IsSelected;
                foreach (ServiceDetailsInterface service in server.Services)
                {
                    ServiceDetailsInterface findService = findServer.Services.FirstOrDefault(x => x.FullNameSpace == service.FullNameSpace);
                    if (findService != null)
                    {
                        findService.IsExpanded = service.IsExpanded;
                        findService.IsSelected = service.IsSelected;
                        //if (_oldSelectedItem == service)
                        //    _newSelectedItem = findService;
                    }

                    foreach (ServiceDetailsMethod method in service.Methods)
                    {
                        if (findService == null)
                            continue;
                        ServiceDetailsMethod find = (from x in findService.Methods where x.MethodName == method.MethodName && x.Requests.First().Parameters.Count == method.Requests.First().Parameters.Count select x).FirstOrDefault();
                        if (find != null)
                        {
                            //if (_oldSelectedItem == method)
                            //    _newSelectedItem = find;
                            find.IsExpanded = method.IsExpanded;
                            find.IsSelected = method.IsSelected;
                            foreach (ServiceDetailsRequestInfo request in method.Requests)
                            {
                                ServiceDetailsRequestInfo findRequest = find.Requests.FirstOrDefault(x => x.Name == request.Name);
                                if (findRequest == null)
                                {
                                    ServiceDetailsRequestInfo clonedReq = request.Clone();
                                    ServiceDetailsRequestInfo defRequest = find.Requests.FirstOrDefault();
                                    foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                                    {
                                        ServiceDetailsParameterInfo p = (from x in defRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                        if (p != null)
                                        {
                                            //if (_oldSelectedItem == parameter)
                                            //    _newSelectedItem = p;
                                            //p.IsExpanded = parameter.IsExpanded;
                                            //p.IsJson = parameter.IsJson;
                                            //p.IsSelected = parameter.IsSelected;
                                            //p.TemplateValue = parameter.TemplateValue;
                                            //p.Value = parameter.Value;
                                            parameter.Type = p.Type;
                                            parameter.FullTypeName = p.FullTypeName;
                                            parameter.Comment = p.Comment;
                                            clonedReq.Parameters.Add(parameter.Clone());
                                        }
                                    }
                                    find.Requests.Add(clonedReq);
                                }
                                else
                                {
                                    foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                                    {
                                        ServiceDetailsParameterInfo p = (from x in findRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                        if (p != null)
                                        {
                                            //if (_oldSelectedItem == parameter)
                                            //    _newSelectedItem = p;
                                            parameter.IsExpanded = p.IsExpanded;
                                            parameter.IsSelected = p.IsSelected;
                                            p.IsJson = parameter.IsJson;
                                            p.Value = parameter.Value?.ToString();
                                            p.TemplateValue = parameter.TemplateValue?.ToString();
                                            p.Type = parameter.Type;
                                            p.FullTypeName = parameter.FullTypeName;
                                            p.Comment = parameter.Comment;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            newData.WebApiDetailsInfo.IsExpanded = oldData.WebApiDetailsInfo.IsExpanded;
            newData.WebApiDetailsInfo.IsSelected = oldData.WebApiDetailsInfo.IsSelected;

            foreach (HttpControllerDetailsInfo server in oldData.WebApiDetailsInfo.HttpControllers)
            {
                HttpControllerDetailsInfo findServer = newData.WebApiDetailsInfo.HttpControllers.FirstOrDefault(x => x.Url == server.Url);
                if (findServer == null)
                    continue;
                findServer.IsExpanded = server.IsExpanded;
                findServer.IsSelected = server.IsSelected;
                foreach (ServiceDetailsMethod method in server.Methods)
                {
                    ServiceDetailsMethod find = (from x in findServer.Methods where x.MethodName == method.MethodName && x.Requests.First().Parameters.Count == method.Requests.First().Parameters.Count select x).FirstOrDefault();
                    if (find != null)
                    {
                        find.IsExpanded = method.IsExpanded;
                        find.IsSelected = method.IsSelected;
                        foreach (ServiceDetailsRequestInfo request in method.Requests)
                        {
                            ServiceDetailsRequestInfo findRequest = find.Requests.FirstOrDefault(x => x.Name == request.Name);
                            if (findRequest == null)
                            {
                                findRequest = request.Clone();
                                foreach (ServiceDetailsParameterInfo item in request.Parameters)
                                {
                                    findRequest.Parameters.Add(item.Clone());
                                }
                                find.Requests.Add(findRequest);
                            }
                            foreach (ServiceDetailsParameterInfo parameter in request.Parameters)
                            {
                                ServiceDetailsParameterInfo p = (from x in findRequest.Parameters where x.Name == parameter.Name select x).FirstOrDefault();
                                if (p != null)
                                {
                                    parameter.IsExpanded = p.IsExpanded;
                                    parameter.IsSelected = p.IsSelected;
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

        //public void SaveData(AppDataInfo data)
        //{
        //    try
        //    {
        //        var serial = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        //        string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
        //        File.WriteAllText(path, serial, Encoding.UTF8);
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}

        public void LoadData()
        {
            try
            {
                ConnectionData connectionData = (ConnectionData)DataContext;
                if (connectionData == null || connectionData.Items == null)
                    return;
                //string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                //var appdata = Newtonsoft.Json.JsonConvert.DeserializeObject<AppDataInfo>(File.ReadAllText(path, Encoding.UTF8));
                //txtAddress.Text = appdata.ServerAddress;
                //txtServiceName.Text = appdata.ServiceName;
                List<object> items = new List<object>();
                items.AddRange(connectionData.Items.Services);
                items.AddRange(connectionData.Items.Callbacks);
                items.Add(connectionData.Items.WebApiDetailsInfo);
                items.Add(connectionData.Items.ProjectDomainDetailsInfo);
                TreeViewServices.DataContext = connectionData.Items;
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = items;
                fullItems = items;
                lstHistoryCalls.ItemsSource = connectionData.Histories;
            }
            catch (Exception ex)
            {

            }
        }

        private void btndisconnect_Click(object sender, RoutedEventArgs e)
        {
            bool canDisconnect = false;
            Dispatcher.Invoke(new Action(() =>
            {
                canDisconnect = btndisconnect.IsEnabled;
                btndisconnect.IsEnabled = false;
                btnconnect.IsEnabled = true;
            }));

            if (provider != null && provider.IsConnected)
            {
                provider.Disconnect();
            }


        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveData();
            //SaveData(new SignalGoTest.AppDataInfo() { ServerAddress = txtAddress.Text, Items = (ProviderDetailsInfo)TreeViewServices.DataContext, Histories = (List<HistoryCallInfo>)lstHistoryCalls.ItemsSource });
        }

        //public static string CreateExceptionFromResponseErrors(HttpResponseMessage response)
        //{
        //    var httpErrorObject = response.Content.ReadAsStringAsync().Result;

        //    // Create an anonymous object to use as the template for deserialization:
        //    var anonymousErrorObject =
        //        new { message = "", ModelState = new Dictionary<string, string[]>() };

        //    // Deserialize:
        //    var deserializedErrorObject =
        //        JsonConvert.DeserializeAnonymousType(httpErrorObject, anonymousErrorObject);

        //    // Now wrap into an exception which best fullfills the needs of your application:
        //    StringBuilder result = new StringBuilder();
        //    // Sometimes, there may be Model Errors:
        //    if (deserializedErrorObject.ModelState != null)
        //    {
        //        var errors =
        //            deserializedErrorObject.ModelState
        //                                    .Select(kvp => string.Join(". ", kvp.Value));
        //        for (int i = 0; i < errors.Count(); i++)
        //        {
        //            // Wrap the errors up into the base Exception.Data Dictionary:
        //            result.AppendLine(i.ToString() + ": " + errors.ElementAt(i));
        //        }
        //    }
        //    // Othertimes, there may not be Model Errors:
        //    else
        //    {
        //        var error =
        //            JsonConvert.DeserializeObject<Dictionary<string, string>>(httpErrorObject);
        //        foreach (var kvp in error)
        //        {
        //            // Wrap the errors up into the base Exception.Data Dictionary:
        //            result.AppendLine(kvp.Key + ": " + kvp.Value);
        //        }
        //    }
        //    return result.ToString();
        //}

        private string session = "";
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lstRequests.SelectedItem == null || !(lstRequests.SelectedItem is ServiceDetailsRequestInfo))
                    return;
                btnSend.IsEnabled = false;
                ServiceDetailsRequestInfo selectedRequest = (ServiceDetailsRequestInfo)lstRequests.SelectedItem;
                ServiceDetailsMethod selectedMethod = TreeViewServices.SelectedItem as ServiceDetailsMethod;

                ServiceDetailsInfo service = (ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).Where(x => (x.GetType() == typeof(ServiceDetailsInfo) && ((ServiceDetailsInfo)x).Services.Any(y => y.Methods.Any(j => j == selectedMethod)))).FirstOrDefault();
                string serviceName = "";
                bool isHttp = false;
                if (service != null)
                    serviceName = service.ServiceName;
                else
                {
                    isHttp = true;
                    WebApiDetailsInfo httpService = (WebApiDetailsInfo)((List<object>)TreeViewServices.ItemsSource).Where(x => (x.GetType() == typeof(WebApiDetailsInfo) && ((WebApiDetailsInfo)x).HttpControllers.Any(y => y.Methods.Any(j => j == selectedMethod)))).FirstOrDefault();
                    serviceName = httpService.HttpControllers.FirstOrDefault(x => x.Methods.Any(j => j == selectedMethod)).Url.ToLower();
                }
                ServiceDetailsMethod sendMethod = new ServiceDetailsMethod();
                ServiceDetailsRequestInfo sendReq = new ServiceDetailsRequestInfo();
                sendMethod.MethodName = selectedMethod.MethodName;
                sendReq.Parameters = new List<ServiceDetailsParameterInfo>();
                if (selectedRequest.Parameters != null)
                {
                    foreach (ServiceDetailsParameterInfo item in selectedRequest.Parameters)
                    {
                        sendReq.Parameters.Add(new ServiceDetailsParameterInfo() { Name = item.Name, Type = item.Type, Value = item.IsJson ? item.Value : JsonConvert.SerializeObject(item.Value), IsJson = item.IsJson });
                    }
                }
                Uri.TryCreate(txtAddress.Text, UriKind.Absolute, out Uri uri);
                System.Threading.Tasks.Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        string request = "";
                        string response = "";
                        if (!isHttp)
                        {
                            response = await ConnectorExtensions.SendDataAsync(provider, serviceName, sendMethod.MethodName, sendMethod.MethodToParameters(sendReq.Parameters, x => ClientSerializationHelper.SerializeObject(x)).ToArray());
                        }
                        else
                        {
                            CookieContainer cookieContainer = new CookieContainer();
                            using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                            {
                                using (var httpClient = new System.Net.Http.HttpClient(handler))
                                {
                                    if (!string.IsNullOrEmpty(session))
                                        cookieContainer.SetCookies(uri, session);
                                    MultipartFormDataContent form = new MultipartFormDataContent();
                                    foreach (ServiceDetailsParameterInfo item in sendReq.Parameters)
                                    {
                                        //form.Add(new StringContent(item.Name), JsonConvert.SerializeObject(JsonConvert.DeserializeObject(item.Value.ToString()), Formatting.None));
                                        StringContent jsonPart = new StringContent(item.Value.ToString(), Encoding.UTF8, "application/json");
                                        jsonPart.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data");
                                        jsonPart.Headers.ContentDisposition.Name = item.Name;
                                        jsonPart.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                                        form.Add(jsonPart);
                                    }
                                    if (!string.IsNullOrEmpty(attachmentFile) && File.Exists(attachmentFile))
                                    {
                                        ByteArrayContent byteFile = new ByteArrayContent(File.ReadAllBytes(attachmentFile));
                                        byteFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                        {
                                            FileName = System.IO.Path.GetFileName(attachmentFile)
                                        };
                                        form.Add(byteFile);
                                    }
                                    //if (!string.IsNullOrEmpty(session))
                                    //    httpClient.DefaultRequestHeaders.coo.Add("Cookie", session);
                                    HttpResponseMessage httpresponse = await httpClient.PostAsync("http://" + uri.Host + ":" + uri.Port + "/" + serviceName + "/" + sendMethod.MethodName, form);
                                    if (!httpresponse.IsSuccessStatusCode)
                                    {
                                        // Unwrap the response and throw as an Api Exception:
                                        response = httpresponse.Content.ReadAsStringAsync().Result;
                                    }
                                    else
                                    {
                                        httpresponse.EnsureSuccessStatusCode();
                                        HttpHeaders headers = httpresponse.Headers;
                                        IEnumerable<string> values;
                                        if (headers.TryGetValues("Set-Cookie", out values))
                                        {
                                            session = values.First();
                                        }
                                        headers = httpresponse.Content.Headers;
                                        if (headers.TryGetValues("content-disposition", out values))
                                        {
                                            ContentDispositionHeaderValue dispo = ContentDispositionHeaderValue.Parse(values.First());
                                            string fileName = dispo.FileName;
                                            response = fileName;
                                            byte[] bytes = httpresponse.Content.ReadAsByteArrayAsync().Result;
                                            response += Environment.NewLine + "length:" + bytes.Length;
                                            Dispatcher.Invoke(new Action(() =>
                                            {
                                                SaveFileDialog dialog = new SaveFileDialog();
                                                dialog.FileName = fileName;
                                                if ((bool)dialog.ShowDialog())
                                                {
                                                    File.WriteAllBytes(dialog.FileName, bytes);
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            response = httpresponse.Content.ReadAsStringAsync().Result;
                                        }
                                    }
                                }
                            }
                        }
                        Dispatcher.Invoke(new Action(() =>
                        {
                            ObservableCollection<HistoryCallInfo> history = (ObservableCollection<HistoryCallInfo>)lstHistoryCalls.ItemsSource;
                            if (history == null)
                                history = new ObservableCollection<SignalGoTest.HistoryCallInfo>();
                            response = response == null ? "Sent success but result is null" : FormatJson(response);
                            history.Insert(0, new HistoryCallInfo() { CallDateTime = DateTime.Now, MethodName = sendMethod.MethodName, Request = FormatJson(request), Response = response });
                            lstHistoryCalls.ItemsSource = null;
                            lstHistoryCalls.ItemsSource = history;
                            SetText(txtReponse, response);
                            ShowInTreeView(response, false);
                            btnSave_Click(null, null);
                        }));
                    }
                    catch (System.Net.Http.HttpRequestException ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            SetText(txtReponse, ex.Message);
                            ShowInTreeView(ex.Message, true);
                        }));
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(new Action(() =>
                        {
                            SetText(txtReponse, ex.Message + Environment.NewLine + ex.StackTrace);
                            ShowInTreeView(ex.Message, true);
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

        void SetText(RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }
        string GetText(RichTextBox richTextBox)
        {
            return ((Run)((Paragraph)richTextBox.Document.Blocks.First()).Inlines.First()).Text;
        }

        void ShowInTreeView(string json, bool isException)
        {
            if (isException)
            {
                rawDataTab.IsSelected = true;
            }
            else
            {
                var token = JToken.Parse(json);

                var children = new List<JToken>();
                if (token != null)
                {
                    children.Add(token);
                }

                jsonDataTreeView.ItemsSource = null;
                jsonDataTreeView.ItemsSource = children;
            }
        }

        private void loadTemplate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            try
            {
                bool isFull = btn.Content.ToString().Contains("Full");
                if (lstRequests.SelectedItem == null || !(lstRequests.SelectedItem is ServiceDetailsRequestInfo))
                    return;
                DataGridTextColumn column = DGRequestValues.Columns[2] as DataGridTextColumn;
                DataGridRow row = DGRequestValues.ItemContainerGenerator.ContainerFromIndex((DGRequestValues.SelectedIndex)) as DataGridRow;
                TextBlock cellTextBox = ((TextBlock)(column.GetCellContent(row)));

                btn.IsEnabled = false;
                ServiceDetailsMethod method = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                ServiceDetailsInfo service = (ServiceDetailsInfo)((List<object>)TreeViewServices.ItemsSource).Where(x => (x.GetType() == typeof(ServiceDetailsInfo) && ((ServiceDetailsInfo)x).Services.Any(y => y.Methods.Any(j => j == method)))).FirstOrDefault();
                string serviceName = "";
                if (service != null)
                    serviceName = service.ServiceName;
                else
                {
                    WebApiDetailsInfo httpService = (WebApiDetailsInfo)((List<object>)TreeViewServices.ItemsSource).Where(x => (x.GetType() == typeof(WebApiDetailsInfo) && ((WebApiDetailsInfo)x).HttpControllers.Any(y => y.Methods.Any(j => j == method)))).FirstOrDefault();
                    serviceName = httpService.HttpControllers.FirstOrDefault(x => x.Methods.Any(j => j == method)).Url.ToLower();
                }

                ServiceDetailsParameterInfo parameter = (ServiceDetailsParameterInfo)btn.DataContext;
                int paramIndex = method.Requests.First().Parameters.IndexOf(parameter);
                MethodParameterDetails sendReq = new MethodParameterDetails();
                sendReq.MethodName = method.MethodName;
                sendReq.ParametersCount = method.Requests.First().Parameters.Count;
                sendReq.ServiceName = serviceName;
                sendReq.ParameterIndex = paramIndex;
                sendReq.IsFull = isFull;
                string address = txtAddress.Text;
                System.Threading.Tasks.Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        string response = "";
                        if (provider.IsConnected)
                            response = await provider.GetMethodParameterDetial(sendReq);
                        else
                        {
                            WebClient webClient = new WebClient();
                            webClient.Headers.Add("signalgo-servicedetail", "parameter");
                            response = webClient.UploadString(address, ClientSerializationHelper.SerializeObject(sendReq));
                        }

                        Dispatcher.Invoke(new Action(() =>
                        {
                            try
                            {
                                parameter.TemplateValue = FormatJson(response);
                            }
                            catch (Exception ex)
                            {
                                parameter.TemplateValue = response;
                            }
                            BindingExpression bindingExpression = DGRequestValues.GetBindingExpression(DataGrid.ItemsSourceProperty);
                            Binding parentBinding = bindingExpression.ParentBinding;

                            System.Collections.IEnumerable source = DGRequestValues.ItemsSource;
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

                            System.Collections.IEnumerable source = DGRequestValues.ItemsSource;
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
            try
            {
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                if (parsedJson == null)
                    return json;
                else if (parsedJson is string)
                    parsedJson = JsonConvert.DeserializeObject(parsedJson);
                if (parsedJson == null)
                    return json;
                dynamic result = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                if (result == null)
                    return json;
                return result;
            }
            catch (Exception ex)
            {
                return json;
            }
        }

        private void btnToString_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetText(txtReponse, JsonConvert.DeserializeObject<string>(GetText(txtReponse)));
            }
            catch (Exception ex)
            {
                SetText(txtReponse, ex.ToString());
            }
        }

        private Type SelectedItemType { get; set; }
        private object SelectedItem { get; set; }
        private int RequestListSelectedIndex { get; set; } = 0;
        private string SelectedUniqName { get; set; }
        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                VirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as VirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(new Rect(i, i, i, i));

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }
        private TreeViewItem GetTreeViewItemByName(ItemsControl container, string uniqName)
        {
            if (container != null)
            {
                if (GetObjectUniqName(container.DataContext) == uniqName)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the 
                // virtualizing case even if the item is marked 
                // expanded we still need to do this step in order to 
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter, 
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);


                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                VirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as VirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so 
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(new Rect(i, i, i, i));

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the 
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItemByName(subContainer, uniqName);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }

        public void SaveTreeViewSelectedItem()
        {
            if (TreeViewServices.SelectedItem == null)
                return;
            RequestListSelectedIndex = lstRequests.SelectedIndex;
            SelectedItem = TreeViewServices.SelectedItem;
            if (SelectedItem == null)
                SelectedItemType = null;
            else
            {
                SelectedItemType = SelectedItem.GetType();
                SelectedUniqName = GetObjectUniqName(SelectedItem);

            }

        }

        public void SelectTreeViewOldItem()
        {
            if (SelectedItem == null)
                return;
            TreeViewItem find = GetTreeViewItem(TreeViewServices, SelectedItem);
            if (find != null)
                find.IsSelected = true;
            else
            {
                find = GetTreeViewItemByName(TreeViewServices, SelectedUniqName);
                if (find != null)
                    find.IsSelected = true;
            }
        }

        public string GetObjectUniqName(object obj)
        {
            if (obj.GetType() == typeof(HttpControllerDetailsInfo))
            {
                HttpControllerDetailsInfo result = (HttpControllerDetailsInfo)obj;
                return result.GetType().FullName + result.Url;
            }
            else if (obj.GetType() == typeof(ModelDetailsInfo))
            {
                ModelDetailsInfo result = (ModelDetailsInfo)obj;
                return result.GetType().FullName + result.FullNameSpace + result.Name;
            }
            else if (obj.GetType() == typeof(ProjectDomainDetailsInfo))
            {
                ProjectDomainDetailsInfo result = (ProjectDomainDetailsInfo)obj;
                return result.GetType().FullName;
            }
            else if (obj.GetType() == typeof(ProviderDetailsInfo))
            {
                ProviderDetailsInfo result = (ProviderDetailsInfo)obj;
                return result.GetType().FullName;
            }
            else if (obj.GetType() == typeof(ServiceDetailsInfo))
            {
                ServiceDetailsInfo result = (ServiceDetailsInfo)obj;
                return result.GetType().FullName + result.FullNameSpace + result.ServiceName + result.NameSpace;
            }
            else if (obj.GetType() == typeof(ServiceDetailsInterface))
            {
                ServiceDetailsInterface result = (ServiceDetailsInterface)obj;
                return result.GetType().FullName + result.FullNameSpace + result.NameSpace;
            }
            else if (obj.GetType() == typeof(ServiceDetailsMethod))
            {
                ServiceDetailsMethod result = (ServiceDetailsMethod)obj;
                return result.GetType().FullName + result.MethodName + result.Requests.First().Parameters.Count + result.ReturnType;
            }
            else if (obj.GetType() == typeof(ServiceDetailsParameterInfo))
            {
                ServiceDetailsParameterInfo result = (ServiceDetailsParameterInfo)obj;
                return result.GetType().FullName + result.Name + result.FullTypeName + result.Type;
            }
            else if (obj.GetType() == typeof(WebApiDetailsInfo))
            {
                WebApiDetailsInfo result = (WebApiDetailsInfo)obj;
                return result.GetType().FullName;
            }
            return Guid.NewGuid().ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = ((TextBox)sender).Text.ToLower();
            if (string.IsNullOrEmpty(value))
            {
                TreeViewServices.ItemsSource = null;
                TreeViewServices.ItemsSource = fullItems;
            }
            else
            {
                TreeViewServices.ItemsSource = null;
                List<object> newItems = new List<object>();
                foreach (object item in fullItems)
                {
                    ServiceDetailsInfo cloneServiceDetailsInfo = null;
                    WebApiDetailsInfo cloneWebApiDetailsInfo = null;
                    ProjectDomainDetailsInfo cloneProjectDomainDetailsInfo = null;
                    bool canAdd = false;
                    if (item.GetType() == typeof(ServiceDetailsInfo))
                    {
                        ServiceDetailsInfo result = (ServiceDetailsInfo)item;
                        cloneServiceDetailsInfo = result.Clone();

                        foreach (ServiceDetailsInterface service in result.Services)
                        {
                            ServiceDetailsInterface cloneService = service.Clone();
                            foreach (ServiceDetailsMethod method in service.Methods)
                            {
                                if (method.MethodName.ToLower().Contains(value))
                                {
                                    if (!cloneServiceDetailsInfo.Services.Contains(cloneService))
                                        cloneServiceDetailsInfo.Services.Add(cloneService);
                                    cloneService.Methods.Add(method);
                                    canAdd = true;
                                    continue;
                                }

                                foreach (ServiceDetailsRequestInfo request in method.Requests)
                                {
                                    foreach (ServiceDetailsParameterInfo p in request.Parameters)
                                    {
                                        if (p.Name.ToLower().Contains(value))
                                        {
                                            if (!cloneServiceDetailsInfo.Services.Contains(cloneService))
                                                cloneServiceDetailsInfo.Services.Add(cloneService);
                                            cloneService.Methods.Add(method);
                                            canAdd = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (canAdd)
                            newItems.Add(cloneServiceDetailsInfo);
                    }
                    else if (item.GetType() == typeof(ProjectDomainDetailsInfo))
                    {
                        ProjectDomainDetailsInfo result = (ProjectDomainDetailsInfo)item;
                        cloneProjectDomainDetailsInfo = result.Clone();

                        foreach (ModelDetailsInfo service in result.Models)
                        {
                            ModelDetailsInfo cloneService = service.Clone();
                            if (service.FullNameSpace.ToLower().Contains(value) || service.Name.ToLower().Contains(value))
                            {
                                if (!cloneProjectDomainDetailsInfo.Models.Contains(cloneService))
                                    cloneProjectDomainDetailsInfo.Models.Add(cloneService);
                                //cloneService.Models.Add(method);
                                canAdd = true;
                                continue;
                            }
                        }
                        if (canAdd)
                            newItems.Add(cloneProjectDomainDetailsInfo);
                    }
                    else if (item.GetType() == typeof(CallbackServiceDetailsInfo))
                    {
                        CallbackServiceDetailsInfo result = (CallbackServiceDetailsInfo)item;
                        CallbackServiceDetailsInfo cloneCallbackServiceDetailsInfo = result.Clone();

                        foreach (ServiceDetailsMethod method in result.Methods)
                        {
                            if (method.MethodName.ToLower().Contains(value))
                            {
                                cloneCallbackServiceDetailsInfo.Methods.Add(method);
                                canAdd = true;
                                continue;
                            }
                            foreach (ServiceDetailsRequestInfo request in method.Requests)
                            {
                                foreach (ServiceDetailsParameterInfo p in request.Parameters)
                                {
                                    if (p.Name.ToLower().Contains(value))
                                    {
                                        cloneCallbackServiceDetailsInfo.Methods.Add(method);
                                        canAdd = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (canAdd)
                            newItems.Add(cloneCallbackServiceDetailsInfo);
                    }
                    else if (item.GetType() == typeof(WebApiDetailsInfo))
                    {
                        WebApiDetailsInfo result = (WebApiDetailsInfo)item;
                        cloneWebApiDetailsInfo = result.Clone();

                        foreach (HttpControllerDetailsInfo service in result.HttpControllers)
                        {
                            HttpControllerDetailsInfo cloneService = service.Clone();
                            foreach (ServiceDetailsMethod method in service.Methods)
                            {
                                if (method.MethodName.ToLower().Contains(value))
                                {
                                    if (!cloneWebApiDetailsInfo.HttpControllers.Contains(cloneService))
                                        cloneWebApiDetailsInfo.HttpControllers.Add(cloneService);
                                    cloneService.Methods.Add(method);
                                    canAdd = true;
                                    continue;
                                }

                                foreach (ServiceDetailsRequestInfo request in method.Requests)
                                {
                                    foreach (ServiceDetailsParameterInfo p in request.Parameters)
                                    {
                                        if (p.Name.ToLower().Contains(value))
                                        {
                                            if (!cloneWebApiDetailsInfo.HttpControllers.Contains(cloneService))
                                                cloneWebApiDetailsInfo.HttpControllers.Add(cloneService);
                                            cloneService.Methods.Add(method);
                                            canAdd = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        if (canAdd)
                            newItems.Add(cloneWebApiDetailsInfo);
                    }
                }
                TreeViewServices.ItemsSource = newItems;
            }
        }

        private void TreeViewServices_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            btnRemoveAttachment_Click(null, null);
        }

        private void btnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ObservableCollection<ServiceDetailsRequestInfo> requests = lstRequests.ItemsSource as ObservableCollection<ServiceDetailsRequestInfo>;
                ServiceDetailsMethod selectedMethod = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                if (requests == null || selectedMethod == null)
                    return;
                string newName = newRequestName.Text.Trim();
                if (requests.Any(x => x.Name == newName) || string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("name is exist or empty!");
                    return;
                }
                ServiceDetailsRequestInfo request = new ServiceDetailsRequestInfo() { Name = newName, Parameters = new List<ServiceDetailsParameterInfo>() };
                foreach (ServiceDetailsParameterInfo item in selectedMethod.Requests.FirstOrDefault().Parameters)
                {
                    ServiceDetailsParameterInfo clone = item.Clone();
                    clone.Value = null;
                    clone.TemplateValue = "";
                    request.Parameters.Add(clone);
                }
                requests.Add(request);
                newRequestName.Text = "";
                MainWindow.SaveData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteRequest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ServiceDetailsMethod parent = (ServiceDetailsMethod)TreeViewServices.SelectedItem;
                ServiceDetailsRequestInfo item = (ServiceDetailsRequestInfo)((MenuItem)sender).DataContext;
                if (item.Name == "Default")
                    return;
                parent.Requests.Remove(item);
                btnSave_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private string attachmentFile = null;
        private void btnAttachment_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if ((bool)dialog.ShowDialog())
            {
                attachmentFile = dialog.FileName;
                btnAttachment.Content = "Attach File (1)";
            }
        }

        private void btnRemoveAttachment_Click(object sender, RoutedEventArgs e)
        {
            attachmentFile = null;
            btnAttachment.Content = "Attach File (0)";
        }

        private void lstRequests_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RequestListSelectedIndex != -1 && lstRequests.SelectedIndex == -1)
            {
                e.Handled = true;
                lstRequests.SelectedIndex = RequestListSelectedIndex;
                //if (lstRequests.SelectedIndex == RequestListSelectedIndex)
                //{
                //    //RequestListSelectedIndex = -1;
                //}
                return;
            }
            btnRemoveAttachment_Click(null, null);
        }

        private void btnHtttpUpdate_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.BusyContent = "Updating...";
            busyIndicator.IsBusy = true;
            string search = txtSearch.Text;
            string address = txtAddress.Text;
            AsyncActions.Run(async () =>
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("signalgo-servicedetail", "full");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "signalgo test");

                    string data = webClient.UploadString(address, "test");
                    ProviderDetailsInfo result = ClientSerializationHelper.DeserializeObject<ProviderDetailsInfo>(data);
                    await CalculateDetails(result, search);
                    AsyncActions.RunOnUI(() =>
                    {
                        MessageBox.Show("success", "ok", MessageBoxButton.OK, MessageBoxImage.Information);
                        busyIndicator.IsBusy = false;
                    });
                }
                catch (Exception ex)
                {
                    AsyncActions.RunOnUI(() =>
                    {
                        MessageBox.Show(ex.Message, "error", MessageBoxButton.OK, MessageBoxImage.Error);
                        busyIndicator.IsBusy = false;
                    });
                }
            });
        }
    }
}
