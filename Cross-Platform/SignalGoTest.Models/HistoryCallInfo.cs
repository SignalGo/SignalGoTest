using System;
using System.Collections.Generic;
using System.Text;

namespace SignalGoTest.Models
{
    public class HistoryCallInfo
    {
        public string MethodName { get; set; }
        public DateTime CallDateTime { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
    }
}
