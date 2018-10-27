using MvvmGo.Commands;
using MvvmGo.ViewModels;
using System;
using System.Linq;

namespace SignalGoTest.ViewModels
{
    public class AddNewViewModel : BaseViewModel
    {
        public AddNewViewModel()
        {
            AddCommand = new Command(() =>
            {
                MainViewModel.This.CurrentAppData.Items.Add(new Models.ConnectionInfo() { Name = Name });
                Name = "";
                MainViewModel.This.Save();
            }, () =>
            {
                bool result = !string.IsNullOrEmpty(Name.Trim()) && !MainViewModel.This.CurrentAppData.Items.Any(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase));
                return result;
            });
        }

        private string _Name = "";

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                OnPropertyChanged(nameof(Name));
                AddCommand.ValidateCanExecute();
            }
        }

        public Command AddCommand { get; set; }
    }
}
