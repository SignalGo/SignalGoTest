using MvvmGo.Commands;
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
            RenameCommand = new Command<ConnectionInfo>(Rename);
            SaveRenameCommand = new Command<ConnectionInfo>(SaveRename);
            RemoveCommand = new Command<ConnectionInfo>(Remove);
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

        public Command<ConnectionInfo> RenameCommand { get; set; }
        public Command<ConnectionInfo> SaveRenameCommand { get; set; }
        public Command<ConnectionInfo> RemoveCommand { get; set; }
        public Command OKCommand { get; set; }
        public Command CancelCommand { get; set; }

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


        private void SaveRename(ConnectionInfo connectionInfo)
        {
            connectionInfo.IsRenameMode = false;
            connectionInfo.OnPropertyChanged(nameof(connectionInfo.Name));
            Save();
        }

        private void Rename(ConnectionInfo connectionInfo)
        {
            connectionInfo.IsRenameMode = true;
        }


        private void Remove(ConnectionInfo connectionInfo)
        {
            BusyContent = $"Do you want to remove \"{connectionInfo.Name}\"?";
            IsBusy = true;
            OKCommand = new Command(() =>
            {
                CurrentAppData.Items.Remove(connectionInfo);
                Save();
                IsBusy = false;
            });
            CancelCommand = new Command(() =>
            {
                IsBusy = false;
            });
            OnPropertyChanged(nameof(OKCommand));
            OnPropertyChanged(nameof(CancelCommand));
        }
    }
}
