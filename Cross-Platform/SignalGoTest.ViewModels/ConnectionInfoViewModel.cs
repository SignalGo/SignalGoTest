using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Client;
using SignalGo.Shared;
using SignalGo.Shared.Models;
using SignalGoTest.Models;
using System;
using System.Threading.Tasks;

namespace SignalGoTest.ViewModels
{
    public class ConnectionInfoViewModel : BaseViewModel
    {
        //public static Action TreeViewItemsUpdatedAction { get; set; }
        private ConnectionInfo _CurrentConnectionInfo;
        public ConnectionInfoViewModel()
        {
            ConnectCommand = new Command(Connect, () => !IsConnected);
            DisconnectCommand = new Command(Disconnect, () => IsConnected);
            SaveCommand = new Command(async () =>
            {
                MainViewModel.This.Save();
                BusyContent = "Save success.";
                IsBusy = true;
                IsAlert = false;
                await Task.Delay(1000);
                IsBusy = false;
            });
        }

        private bool _IsAlert;
        private string _SearchText = "";
        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }
        public Command SaveCommand { get; set; }

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
            IsAlert = false;
            IsBusy = true;
            try
            {
                var currentView = CurrentConnectionInfo;
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
                var oldItems = currentView.Items;
                currentView.Items = result;
                currentView.OnPropertyChanged("ItemsSource");
                await currentView.ConnectionInfoViewHelper.CalculateDetails(result, oldItems, SearchText);
                //TreeViewItemsUpdatedAction?.Invoke();
            }
            catch (System.Exception ex)
            {
                BusyContent = ex.Message;
                IsAlert = true;
                await Task.Delay(3000);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
