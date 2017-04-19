using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGoTest
{
    public class AppDataInfo
    {
        public string ServerAddress { get; set; }
        public string ServiceName { get; set; }
        public List<MethodItemInfo> Items { get; set; }
        public List<HistoryCallInfo> Histories { get; set; } = new List<HistoryCallInfo>();
    }
}
