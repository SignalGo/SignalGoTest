using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SignalGoTest
{
    public class HistoryCallInfo
    {
        public string MethodName { get; set; }
        public DateTime CallDateTime { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}
