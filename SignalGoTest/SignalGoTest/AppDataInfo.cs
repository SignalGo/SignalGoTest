using SignalGo.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace SignalGoTest
{
    public class AppDataInfo
    {
        public List<ConnectionData> Items = new List<ConnectionData>();
    }

    public class ConnectionData
    {
        public string Name { get; set; }
        public string ServerAddress { get; set; }
        public string ServiceName { get; set; }
        public ProviderDetailsInfo Items { get; set; }
        public List<HistoryCallInfo> Histories { get; set; } = new List<HistoryCallInfo>();
        public ObservableCollection<CallbackServiceLogInfo> CallbackCalls { get; set; } = new ObservableCollection<CallbackServiceLogInfo>();
    }
}
