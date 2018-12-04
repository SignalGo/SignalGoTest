using MvvmGo.ViewModels;
using Newtonsoft.Json;
using SignalGo.Shared.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SignalGoTest.Models
{
    public class ConnectionInfo : PropertyChangedViewModel
    {
        public string Name { get; set; }
        public string ServerAddress { get; set; }
        public string ServiceName { get; set; }
        public ProviderDetailsInfo Items { get; set; }
        public List<object> ItemsSource
        {
            get
            {
                if (Items == null)
                    return null;
                List<object> items = new List<object>();
                items.AddRange(Items.Services);
                items.AddRange(Items.Callbacks);
                items.Add(Items.WebApiDetailsInfo);
                Items.ProjectDomainDetailsInfo.Models = Items.ProjectDomainDetailsInfo.Models.OrderBy(x => x.ObjectType == SignalGo.Shared.Helpers.SerializeObjectType.Enum).ThenBy(x => x.Name).ToList();
                items.Add(Items.ProjectDomainDetailsInfo);
                return items;
            }
        }
        public ObservableCollection<HistoryCallInfo> Histories { get; set; } = new ObservableCollection<HistoryCallInfo>();
        public ObservableCollection<CallbackServiceLogInfo> CallbackCalls { get; set; } = new ObservableCollection<CallbackServiceLogInfo>();

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

        [JsonIgnore]
        public ConnectionInfoViewHelper ConnectionInfoViewHelper { get; set; } = new ConnectionInfoViewHelper();
    }
}
