using ExpressGangLoader.Commands;
using ExpressGangLoader.ErrorCatch;
using ExpressGangLoader.Model;
using ExpressGangLoader.View.Dialog;
using ExpressGangLoader.ViewModel.DialogVM;
using Extron.CDA.Tools;
using Extron.Communication;
using Extron.Communication.FileTransfer;
using Extron.Communication.Services.Base.Contracts;
using Extron.Communication.Services.Base.Services;
using Extron.Communication.Services.Contracts.Discovery;
using Extron.Communication.Services.Contracts.FirmwareUpgrade;
using Extron.Communication.Services.Data.FirmwareUpgrade;
using Extron.Communication.Services.Services.Discovery;
using Extron.Communication.Services.Services.FirmwareUpgrade;
using Extron.GlobalMessaging.Commands;
using Extron.GlobalMessaging.Commands.DeviceInformation;
using Extron.GlobalMessaging.Commands.NetworkInterfaces;
using Extron.Pro.Communication.Contracts.Enumerations;
using Extron.Pro.Communication.Contracts.GlobalMessaging;
using Extron.Pro.Communication.Contracts.IPLinkPro;
using Extron.Pro.Communication.IPLinkPro;
using Extron.Pro.Communication.Security.Contracts;
using Extron.Pro.Communication.Utility;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using TE.Module.Communication;
using TE.Module.Essentials;

namespace ExpressGangLoader.ViewModel.DialogVM
{
    public class FixtureSettingDialogViewModel : INotifyPropertyChanged
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

        #region Properties      
        public ICommand CheckFixtureOneIPExecute_Command { get; set; }
        public ICommand CheckFixtureTwoIPExecute_Command { get; set; }
        #endregion

        #region Define local Member   
        private string _Fixture1Connect_NotConnect;
        private string _Fixture2Connect_NotConnect;
        #endregion
        #region Members
        public string Fixture1Connect_NotConnect
        {
            get { return _Fixture1Connect_NotConnect; }
            set
            {
                if (_Fixture1Connect_NotConnect != value)
                {
                    _Fixture1Connect_NotConnect = value;
                    NotifyPropertyChanged("Fixture1Connect_NotConnect");
                }
            }
        }
        public string Fixture2Connect_NotConnect
        {
            get { return _Fixture2Connect_NotConnect; }
            set
            {
                if (_Fixture2Connect_NotConnect != value)
                {
                    _Fixture2Connect_NotConnect = value;
                    NotifyPropertyChanged("Fixture2Connect_NotConnect");
                }
            }
        }
        #endregion

        #region Execute Command
        private void CheckFixtureOneIPExecute(object parameter)
        {
            try
            {
                string FixtureOneIP = "192.168.254.10";
                DnsEndPoint dnsEndpoint = new DnsEndPoint(FixtureOneIP, 23);
                TelnetSettings ts = new TelnetSettings(dnsEndpoint, "extron");
                ExtronDevice ed = new ExtronDevice(ts);
                var connectState = ed.Connect(5000, ExtronDevice.ConnectionBehavior.Block);
                if (connectState == ExtronDevice.DeviceState.Connected)
                {
                    Fixture1Connect_NotConnect = "Connected";
                }
                else
                {
                    Fixture1Connect_NotConnect = "Error - Check Fixture 1";
                }
            }
            catch (Exception a)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(a.Message.ToString(), a.StackTrace.ToString());
                if (a.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(a.InnerException.Message.ToString(), a.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("CheckFixtureOneIPExecute", ErrorMessage, InnerErrorMessage, Environment.UserName);
            }
        }
        private void CheckFixtureTwoIPExecute(object parameter) 
        {           
            try 
            {
                string FixtureTwoIP = "192.168.254.11";
                DnsEndPoint dnsEndpoint = new DnsEndPoint(FixtureTwoIP, 23);
                TelnetSettings ts = new TelnetSettings(dnsEndpoint, "extron");
                ExtronDevice ed = new ExtronDevice(ts);
                var connectState = ed.Connect(5000, ExtronDevice.ConnectionBehavior.Block);
                if (connectState == ExtronDevice.DeviceState.Connected)
                {
                    Fixture2Connect_NotConnect = "Connected";
                }
                else
                {
                    Fixture2Connect_NotConnect = "Error - Check Fixture 2";
                }

            }
            catch (Exception a)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(a.Message.ToString(), a.StackTrace.ToString());
                if (a.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(a.InnerException.Message.ToString(), a.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("CheckFixtureTwoIPExecute", ErrorMessage, InnerErrorMessage, Environment.UserName);
            }
        }
        #endregion

        #region CanExecute Command
        private bool CanCheckFixtureIPExecute(object parameter) { return true; }
        #endregion

        #region Main Constructor
        public FixtureSettingDialogViewModel()
        {
            CheckFixtureOneIPExecute_Command = new RelayCommand(CheckFixtureOneIPExecute, CanCheckFixtureIPExecute);
            CheckFixtureTwoIPExecute_Command = new RelayCommand(CheckFixtureTwoIPExecute, CanCheckFixtureIPExecute);
            //AutoRun Fixture Check
            CheckFixtureOneIPExecute("");
            CheckFixtureTwoIPExecute("");
        }
        #endregion

        #region Methods          
        #endregion
    }
}

    