using SignalGo.Shared.Models;
using System.Collections.ObjectModel;

namespace SignalGoTest
{
    public class AppDataInfo
    {
        public ObservableCollection<ConnectionData> Items { get; set; } = new ObservableCollection<ConnectionData>();
    }

    public class ConnectionData
    {
        public string Name { get; set; }
        public string ServerAddress { get; set; }
        public string ServiceName { get; set; }
        public ProviderDetailsInfo Items { get; set; }
        public ObservableCollection<HistoryCallInfo> Histories { get; set; } = new ObservableCollection<HistoryCallInfo>();
        public ObservableCollection<CallbackServiceLogInfo> CallbackCalls { get; set; } = new ObservableCollection<CallbackServiceLogInfo>();
    }
}
