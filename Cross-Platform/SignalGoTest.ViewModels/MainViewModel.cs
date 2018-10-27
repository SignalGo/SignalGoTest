using MvvmGo.ViewModels;
using SignalGoTest.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SignalGoTest.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public static MainViewModel This { get; set; }
        public MainViewModel()
        {
            Load();
            This = this;
        }

        public static readonly string AddNewName = "Add New...";
        public AppDataInfo CurrentAppData { get; set; } = new AppDataInfo()
        {
            Items = new System.Collections.ObjectModel.ObservableCollection<ConnectionInfo>()
            {
                 new ConnectionInfo()
                 {
                      Name = AddNewName
                 }
             }
        };

        public void Save()
        {
            try
            {
                string serial = Newtonsoft.Json.JsonConvert.SerializeObject(CurrentAppData);
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                File.WriteAllText(path, serial, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"cannot save data : {ex.ToString()}");
            }
        }

        public void Load()
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appdata.db");
                if (!File.Exists(path))
                    return;
                CurrentAppData = Newtonsoft.Json.JsonConvert.DeserializeObject<AppDataInfo>(File.ReadAllText(path, Encoding.UTF8));
                foreach (ConnectionInfo item in CurrentAppData.Items)
                {
                    ConnectionInfo.DoOrder(item.Items);
                }
                if (!CurrentAppData.Items.Any(x => x.Name == AddNewName))
                    CurrentAppData.Items.Insert(0, new ConnectionInfo() { Name = AddNewName });
            }
            catch (Exception ex)
            {

            }
        }
    }
}
