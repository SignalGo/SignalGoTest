using MvvmGo.ViewModels;

namespace SignalGoTest.Models
{
    public enum LanguageType : byte
    {
        CSharp = 0,
        Angular = 1,
        Blazor = 2,
        JavaAndroid = 3,
        Swift = 4,
        Flutter = 5
    }

    public enum ServiceType : byte
    {
        SignalGo = 0,
        Soap = 1
    }

    public class CodeGeneratorInfo : PropertyChangedViewModel
    {
        private string _ServerNameSpace = "";
        private LanguageType _LanguageType = LanguageType.CSharp;
        private ServiceType _ServiceType = ServiceType.SignalGo;
        private bool _IsJustServices = false;
        private bool _IsAsyncMethods = true;
        private string _SaveFolderPath = "";

        public string ServerNameSpace
        {
            get
            {
                return _ServerNameSpace;
            }
            set
            {
                _ServerNameSpace = value;
                OnPropertyChanged(nameof(ServerNameSpace));
            }
        }

        public LanguageType LanguageType
        {
            get
            {
                return _LanguageType;
            }
            set
            {
                _LanguageType = value;
                OnPropertyChanged(nameof(LanguageType));
            }
        }

        public ServiceType ServiceType
        {
            get
            {
                return _ServiceType;
            }
            set
            {
                _ServiceType = value;
                OnPropertyChanged(nameof(ServiceType));
            }
        }

        public bool IsJustServices
        {
            get
            {
                return _IsJustServices;
            }
            set
            {
                _IsJustServices = value;
                OnPropertyChanged(nameof(IsJustServices));
            }
        }

        public bool IsAsyncMethods
        {
            get
            {
                return _IsAsyncMethods;
            }
            set
            {
                _IsAsyncMethods = value;
                OnPropertyChanged(nameof(IsAsyncMethods));
            }
        }

        public string SaveFolderPath
        {
            get
            {
                return _SaveFolderPath;
            }
            set
            {
                _SaveFolderPath = value;
                OnPropertyChanged(nameof(SaveFolderPath));
            }
        }
    }
}
