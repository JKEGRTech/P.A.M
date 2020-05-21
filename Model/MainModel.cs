using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExpressGangLoader.Model
{
    public class MainModel : INotifyPropertyChanged
    {
        #region PropertyChanged Properties
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        }
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        public void RaisePropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region Define local Member
        private string _Hostname;
        private string _IpAddress;
        private string _DefaultIP;
        private string _MacAddress;
        private string _Model;
        private string _PartNumber;
        private string _SerialNumber;
        private string _Version;
        private bool _TlpTli;
        private bool _FailFLAG;
        private string _Password;
        private string _UserName;
        private string _FWUpdateStatus;
        private string _FW_File;
        private int _FWProgressStatus;
        private Visibility _progressVisibility;
        private Visibility _PassVisibility;
        private Visibility _FailVisibility;

        #endregion

        #region Members
        public string Hostname
        {
            get { return _Hostname; }
            set
            {
                _Hostname = value;

                NotifyPropertyChanged("Hostname");
            }
        }
        public string IpAddress
        {
            get { return _IpAddress; }
            set
            {
                _IpAddress = value;
                NotifyPropertyChanged("IpAddress");
            }
        }
        public string DefaultIP
        {
            get { return _DefaultIP; }
            set
            {
                _DefaultIP = value;
                NotifyPropertyChanged("DefaultIP");
            }
        }
        public string MacAddress
        {
            get { return _MacAddress; }
            set
            {
                _MacAddress = value;
                NotifyPropertyChanged("MacAddress");
            }
        }
        public string Model
        {
            get { return _Model; }
            set
            {
                _Model = value;
                NotifyPropertyChanged("Model");
            }
        }
        public string PartNumber
        {
            get { return _PartNumber; }
            set
            {
                _PartNumber = value;
                NotifyPropertyChanged("PartNumber");
            }
        }
        public string SerialNumber
        {
            get { return _SerialNumber; }
            set
            {
                _SerialNumber = value;
                NotifyPropertyChanged("SerialNumber");
            }
        }
        public string Version
        {
            get { return _Version; }
            set
            {
                _Version = value;
                NotifyPropertyChanged("Version");
            }
        }
        public bool TlpTli
        {
            get { return _TlpTli; }
            set
            {
                _TlpTli = value;
                NotifyPropertyChanged("TlpTli");
            }
        }
        public bool FailFLAG
        {
            get { return _FailFLAG; }
            set
            {
                _FailFLAG = value;
                NotifyPropertyChanged("FailFLAG");
            }
        }
        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                NotifyPropertyChanged("Password");
            }
        }
        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                NotifyPropertyChanged("UserName");
            }
        }
        public string FWUpdateStatus
        {
            get { return _FWUpdateStatus; }
            set
            {
                _FWUpdateStatus = value;
                NotifyPropertyChanged("FWUpdateStatus");
            }
        }
        public string FW_File
        {
            get { return _FW_File; }
            set
            {
                _FW_File = value;
                NotifyPropertyChanged("FW_File");
            }
        }
        public int FWProgressStatus
        {
            get { return _FWProgressStatus; }
            set
            {
                _FWProgressStatus = value;
                NotifyPropertyChanged("FWProgressStatus");
            }
        }
        public Visibility progressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                NotifyPropertyChanged("progressVisibility");
            }
        }
        public Visibility PassVisibility
        {
            get { return _PassVisibility; }
            set
            {
                _PassVisibility = value;
                NotifyPropertyChanged("PassVisibility");
            }
        }
        public Visibility FailVisibility
        {
            get { return _FailVisibility; }
            set
            {
                _FailVisibility = value;
                NotifyPropertyChanged("FailVisibility");
            }
        }

        #endregion

        #region MainModel Constructor
        public MainModel(string hostname,
                         string ipaddress,
                         string macaddress,
                         string model,
                         string partnumber,
                         string serialnumber,
                         string version,
                         string status,
                         string password,
                         string username,
                         bool tlptli,
                         string fwfile,
                         int fwprogres,
                         Visibility _progressvisibility,
                         Visibility _Passvisibility,
                         Visibility _Failvisibility,
                         bool failflag)
        {
            Hostname = hostname;
            IpAddress = ipaddress;
            DefaultIP = ipaddress;
            MacAddress = macaddress;
            Model = model;
            PartNumber = partnumber;
            SerialNumber = serialnumber;
            Version = version;
            Password = password;
            UserName = username;
            TlpTli = tlptli;
            FWUpdateStatus = status;
            FW_File = fwfile;
            FWProgressStatus = fwprogres;
            progressVisibility = _progressvisibility;
            PassVisibility = _Passvisibility;
            FailVisibility = _Failvisibility;
            FailFLAG = failflag;
        }
        #endregion

    }
}
