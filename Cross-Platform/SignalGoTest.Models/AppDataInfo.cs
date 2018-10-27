using System;
using System.Collections.ObjectModel;

namespace SignalGoTest.Models
{
    public class AppDataInfo
    {
        public ObservableCollection<ConnectionInfo> Items { get; set; } = new ObservableCollection<ConnectionInfo>();
    }
}
