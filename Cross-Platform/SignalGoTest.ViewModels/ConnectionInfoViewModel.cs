using MvvmGo.Commands;
using MvvmGo.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SignalGo.Client;
using SignalGo.Client.ClientManager;
using SignalGo.Shared;
using SignalGo.Shared.Helpers;
using SignalGo.Shared.Log;
using SignalGo.Shared.Models;
using SignalGoTest.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SignalGoTest.ViewModels
{
    public class ConnectionInfoViewModel : BaseViewModel
    {
        public static Action<string> ShowJsonTemplateWindowAction { get; set; }
        private ConnectionInfo _CurrentConnectionInfo;
        private ServiceDetailsRequestInfo _ServiceDetailsRequestInfo;
        public ConnectionInfoViewModel()
        {
            ConnectCommand = new Command(Connect, () => !IsConnected);
            DisconnectCommand = new Command(Disconnect, () => IsConnected);
            SaveCommand = new Command(async () =>
            {
                MainViewModel.This.Save();
                BusyContent = "Save success.";
                IsBusy = true;
                await Task.Delay(1000);
                IsBusy = false;
            });

            LoadSimpleTemplateCommand = new Command<ServiceDetailsParameterInfo>(async (parameter) =>
            {
                string result = await LoadTemplate(false, parameter);
                ShowJsonTemplateWindowAction(result);
            });

            LoadFullTemplateCommand = new Command<ServiceDetailsParameterInfo>(async (parameter) =>
            {
                string result = await LoadTemplate(true, parameter);
                ShowJsonTemplateWindowAction(result);
            });
            SendCommand = new Command(Send);
            HttpUpdateCommand = new Command(HttpUpdate);
        }


        private bool _IsAlert;
        private string _SearchText = "";
        private object _SelectedTreeItem;
        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }
        public Command SaveCommand { get; set; }
        public Command<ServiceDetailsParameterInfo> LoadSimpleTemplateCommand { get; set; }
        public Command<ServiceDetailsParameterInfo> LoadFullTemplateCommand { get; set; }
        public Command SendCommand { get; set; }
        public Command HttpUpdateCommand { get; set; }

        public override bool IsBusy
        {
            get
            {
                return base.IsBusy;
            }

            set
            {
                base.IsBusy = value;
                IsAlert = false;
            }
        }

        public ServiceDetailsRequestInfo ServiceDetailsRequestInfo
        {
            get
            {
                return _ServiceDetailsRequestInfo;
            }
            set
            {
                _ServiceDetailsRequestInfo = value;
                OnPropertyChanged(nameof(ServiceDetailsRequestInfo));
                OnPropertyChanged(nameof(IsRequestSelected));
            }
        }


        public bool IsRequestSelected
        {
            get
            {
                return ServiceDetailsRequestInfo != null;
            }
        }

        public ConnectionInfo CurrentConnectionInfo
        {
            get
            {
                return _CurrentConnectionInfo;
            }
            set
            {
                _CurrentConnectionInfo = value;
                OnPropertyChanged(nameof(CurrentConnectionInfo));
                OnConnectedChanged();
            }
        }

        public bool IsAlert
        {
            get
            {
                return _IsAlert;
            }
            set
            {
                _IsAlert = value;
                OnPropertyChanged(nameof(IsAlert));
            }
        }

        public bool IsConnected
        {
            get
            {
                return CurrentConnectionInfo.ConnectionInfoViewHelper.Provider.IsConnected;
            }
        }

        public string SearchText
        {
            get
            {
                return _SearchText;
            }
            set
            {
                _SearchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public object SelectedTreeItem
        {
            get
            {
                return _SelectedTreeItem;
            }
            set
            {
                _SelectedTreeItem = value;
                OnPropertyChanged(nameof(SelectedTreeItem));
            }
        }


        private void Disconnect()
        {
            CurrentConnectionInfo.ConnectionInfoViewHelper.Provider.Disconnect();
        }

        private void OnConnectedChanged()
        {
            OnPropertyChanged(nameof(IsConnected));
            ConnectCommand.ValidateCanExecute();
            DisconnectCommand.ValidateCanExecute();
        }


        public async void Connect()
        {
            BusyContent = "Connecting...";
            IsBusy = true;
            try
            {
                ConnectionInfo currentView = CurrentConnectionInfo;
                ClientProvider provider = currentView.ConnectionInfoViewHelper.Provider;
                provider.OnConnectionChanged = (connected) =>
                {
                    AsyncActions.RunOnUI(() =>
                    {
                        OnConnectedChanged();
                    });
                    //if (connected == ConnectionStatus.Disconnected)
                    //     btndisconnect_Click(null, null);
                };

                provider.OnCalledMethodAction = (callInfo) =>
                {
                    //AsyncActions.RunOnUI(() =>
                    //{
                    //    try
                    //    {
                    //        ConnectionData details = (ConnectionData)DataContext;
                    //        CallbackServiceLogInfo findService = details.CallbackCalls.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName);
                    //        if (findService == null)
                    //        {
                    //            details.CallbackCalls.Add(new CallbackServiceLogInfo { ServiceName = callInfo.ServiceName });
                    //            findService = details.CallbackCalls.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName);
                    //        }
                    //        CallbackMethodLogInfo logInfo = new CallbackMethodLogInfo() { DateTime = DateTime.Now, MethodName = callInfo.MethodName };
                    //        ServiceDetailsMethod findFromBase = details.Items.Callbacks.FirstOrDefault(x => x.ServiceName == callInfo.ServiceName).Methods.FirstOrDefault(x => x.MethodName == callInfo.MethodName && x.Requests.First().Parameters.Count == callInfo.Parameters.Length);

                    //        foreach (ParameterInfo item in callInfo.Parameters)
                    //        {
                    //            Newtonsoft.Json.Linq.JToken token = Newtonsoft.Json.Linq.JToken.Parse(item.Value);
                    //            string value = token.ToString(Newtonsoft.Json.Formatting.Indented);
                    //            ServiceDetailsParameterInfo findedParam = findFromBase.Requests.First().Parameters[Array.IndexOf(callInfo.Parameters, item)];
                    //            logInfo.Parameters.Add(new CallbackParameterLogInfo() { Value = value, Name = findedParam.Name, ParameterType = findedParam.Type });
                    //        }

                    //        findService.Calls.Insert(0, logInfo);
                    //        btnSave_Click(null, null);
                    //    }
                    //    catch (Exception ex)
                    //    {

                    //    }
                    //});
                };
                provider.ProviderSetting.IsEnabledToUseTimeout = true;
                provider.ProviderSetting.ReceiveDataTimeout = new TimeSpan(0, 0, 5);
                provider.ProviderSetting.SendDataTimeout = new TimeSpan(0, 0, 5);
                await provider.ConnectAsync(currentView.ServerAddress);

                BusyContent = "Receiving data...";
                ProviderDetailsInfo result = await provider.GetListOfServicesWithDetials(CurrentConnectionInfo.ServerAddress);
                ProviderDetailsInfo oldItems = currentView.Items;
                currentView.Items = result;
                currentView.OnPropertyChanged("ItemsSource");
                await currentView.ConnectionInfoViewHelper.CalculateDetails(result, oldItems, SearchText);
                //TreeViewItemsUpdatedAction?.Invoke();
            }
            catch (System.Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Connect");
                BusyContent = ex.Message;
                IsAlert = true;
                await Task.Delay(3000);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task<string> LoadTemplate(bool isFull, ServiceDetailsParameterInfo parameter)
        {
            try
            {
                BusyContent = "Getting Data...";
                IsBusy = true;
                ConnectionInfo current = CurrentConnectionInfo;
                List<object> items = current.ItemsSource;
                ServiceDetailsMethod method = (ServiceDetailsMethod)SelectedTreeItem;
                ServiceDetailsInfo service = (ServiceDetailsInfo)items.Where(x => (x.GetType() == typeof(ServiceDetailsInfo) && ((ServiceDetailsInfo)x).Services.Any(y => y.Methods.Any(j => j == method)))).FirstOrDefault();
                string serviceName = "";
                if (service != null)
                    serviceName = service.ServiceName;
                else
                {
                    WebApiDetailsInfo httpService = (WebApiDetailsInfo)items.Where(x => (x.GetType() == typeof(WebApiDetailsInfo) && ((WebApiDetailsInfo)x).HttpControllers.Any(y => y.Methods.Any(j => j == method)))).FirstOrDefault();
                    serviceName = httpService.HttpControllers.FirstOrDefault(x => x.Methods.Any(j => j == method)).Url.ToLower();
                }

                int paramIndex = method.Requests.First().Parameters.IndexOf(parameter);
                MethodParameterDetails sendReq = new MethodParameterDetails();
                sendReq.MethodName = method.MethodName;
                sendReq.ParametersCount = method.Requests.First().Parameters.Count;
                sendReq.ServiceName = serviceName;
                sendReq.ParameterIndex = paramIndex;
                sendReq.IsFull = isFull;
                string address = current.ServerAddress;

                string response = "";
                if (current.ConnectionInfoViewHelper.Provider.IsConnected)
                    response = await current.ConnectionInfoViewHelper.Provider.GetMethodParameterDetial(sendReq);
                else
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.Encoding = Encoding.UTF8;
                        webClient.Headers.Add("signalgo-servicedetail", "parameter");
                        response = await webClient.UploadStringTaskAsync(address, ClientSerializationHelper.SerializeObject(sendReq));
                    }
                }

                try
                {
                    return FormatJson(response);
                }
                catch (Exception ex)
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "LoadTemplate");
                return ex.ToString();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string FormatJson(string json)
        {
            try
            {
                object parsedJson = JsonConvert.DeserializeObject(json);
                if (parsedJson == null)
                    return json;
                else if (parsedJson is string)
                    parsedJson = JsonConvert.DeserializeObject((string)parsedJson);
                if (parsedJson == null)
                    return json;
                string result = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                if (result == null)
                    return json;
                return result;
            }
            catch (Exception ex)
            {
                return json;
            }
        }

        public async void Send()
        {
            try
            {
                BusyContent = "Sending...";
                IsBusy = true;
                ConnectionInfo current = CurrentConnectionInfo;
                List<object> items = current.ItemsSource;
                ServiceDetailsRequestInfo selectedRequest = ServiceDetailsRequestInfo;
                ServiceDetailsMethod selectedMethod = SelectedTreeItem as ServiceDetailsMethod;

                ServiceDetailsInfo service = (ServiceDetailsInfo)items.Where(x => (x.GetType() == typeof(ServiceDetailsInfo) && ((ServiceDetailsInfo)x).Services.Any(y => y.Methods.Any(j => j == selectedMethod)))).FirstOrDefault();
                string serviceName = "";
                bool isHttp = false;
                if (service != null)
                    serviceName = service.ServiceName;
                else
                {
                    isHttp = true;
                    WebApiDetailsInfo httpService = (WebApiDetailsInfo)items.Where(x => (x.GetType() == typeof(WebApiDetailsInfo) && ((WebApiDetailsInfo)x).HttpControllers.Any(y => y.Methods.Any(j => j == selectedMethod)))).FirstOrDefault();
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
                        //sendReq.Parameters.Add(new ServiceDetailsParameterInfo() { Name = item.Name, Type = item.Type, Value = item.IsJson ? item.Value : JsonConvert.SerializeObject(item.Value), IsJson = item.IsJson });
                        try
                        {
                            JToken.Parse(item.Value.ToString());
                        }
                        catch
                        {
                            item.Value = JsonConvert.SerializeObject(item.Value);
                        }
                        sendReq.Parameters.Add(new ServiceDetailsParameterInfo() { Name = item.Name, Type = item.Type, Value = item.Value, IsJson = true });
                    }
                }
                string response = "";
                if (!isHttp)
                {
                    response = await ConnectorExtensions.SendDataAsync(current.ConnectionInfoViewHelper.Provider, serviceName, sendMethod.MethodName, sendMethod.MethodToParameters(sendReq.Parameters, x => ClientSerializationHelper.SerializeObject(x)).ToArray());
                }
                else
                {
                    CookieContainer cookieContainer = new CookieContainer();
                    Uri.TryCreate(current.ServerAddress, UriKind.Absolute, out Uri uri);
                    using (HttpClientHandler handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                    {
                        using (System.Net.Http.HttpClient httpClient = new System.Net.Http.HttpClient(handler))
                        {
                            if (!string.IsNullOrEmpty(current.ConnectionInfoViewHelper.Session))
                                cookieContainer.SetCookies(uri, current.ConnectionInfoViewHelper.Session);
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
                            if (!string.IsNullOrEmpty(current.ConnectionInfoViewHelper.AttachmentFile) && File.Exists(current.ConnectionInfoViewHelper.AttachmentFile))
                            {
                                ByteArrayContent byteFile = new ByteArrayContent(File.ReadAllBytes(current.ConnectionInfoViewHelper.AttachmentFile));
                                byteFile.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                                {
                                    FileName = System.IO.Path.GetFileName(current.ConnectionInfoViewHelper.AttachmentFile)
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
                                    current.ConnectionInfoViewHelper.Session = values.First();
                                }
                                headers = httpresponse.Content.Headers;
                                if (headers.TryGetValues("content-disposition", out values))
                                {
                                    ContentDispositionHeaderValue dispo = ContentDispositionHeaderValue.Parse(values.First());
                                    string fileName = dispo.FileName;
                                    response = fileName;
                                    byte[] bytes = httpresponse.Content.ReadAsByteArrayAsync().Result;
                                    response += Environment.NewLine + "length:" + bytes.Length;
                                    //Dispatcher.Invoke(new Action(() =>
                                    //{
                                    //    SaveFileDialog dialog = new SaveFileDialog();
                                    //    dialog.FileName = fileName;
                                    //    if ((bool)dialog.ShowDialog())
                                    //    {
                                    //        File.WriteAllBytes(dialog.FileName, bytes);
                                    //    }
                                    //}));
                                }
                                else
                                {
                                    response = httpresponse.Content.ReadAsStringAsync().Result;
                                }
                            }
                        }
                    }
                }
                response = response == null ? "Sent success but result is null" : FormatJson(response);
                selectedRequest.Response = response;
            }
            catch (Exception ex)
            {
                AutoLogger.Default.LogError(ex, "Send");
                BusyContent = ex.Message;
                await Task.Delay(3000);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void HttpUpdate()
        {
            try
            {
                ConnectionInfo currentView = CurrentConnectionInfo;
                BusyContent = "Updating...";
                IsBusy = true;
                string address = CurrentConnectionInfo.ServerAddress;

                using (WebClient webClient = new WebClient())
                {
                    webClient.Encoding = Encoding.UTF8;
                    webClient.Headers.Add("signalgo-servicedetail", "full");
                    webClient.Headers.Add(HttpRequestHeader.UserAgent, "signalgo test");

                    string data = await webClient.UploadStringTaskAsync(address, "test");
                    ProviderDetailsInfo result = ClientSerializationHelper.DeserializeObject<ProviderDetailsInfo>(data);
                    ProviderDetailsInfo oldItems = currentView.Items;
                    currentView.Items = result;
                    currentView.OnPropertyChanged("ItemsSource");
                    await currentView.ConnectionInfoViewHelper.CalculateDetails(result, oldItems, SearchText);
                    BusyContent = "Success";
                    await Task.Delay(3000);
                }
            }
            catch (Exception ex)
            {
                BusyContent = ex.Message;
                AutoLogger.Default.LogError(ex, "HttpUpdate");
                await Task.Delay(3000);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}