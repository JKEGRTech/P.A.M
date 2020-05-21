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

namespace ExpressGangLoader.ViewModel
{
    public class MainVM : INotifyPropertyChanged
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
        //Create device discovery service for single network
        IDeviceDiscoveryService _iDiscoveryService = new UDPDeviceDiscoveryService();
        // create FirmwareBulkUpgradeSession instance
        IFirmwareBulkUpgradeSession _BulkUpgradeSession = new FirmwareBulkUpgradeSession();
        //Create LegacyUUT Device Discovery Instance
        UDPLegacyDeviceDiscoveryService LegacyDeviceDiscoveryService = new UDPLegacyDeviceDiscoveryService();
        //Create device discovery service timer
        DispatcherTimer DiscoveryTimer = new DispatcherTimer();
        //Create AvanteConnect service
        AvanteConnect avanteconnect = new AvanteConnect();
        //Create a Model to store data of UUTs
        public ObservableCollection<MainModel> UUTsList { get; set; }
        List<IFirmwareUpgradeDeviceFileInfo> devList { get; set; }
        public List<string> usedIP;
        public List<string> failedList;
        public string[] FixtureDeviceIP;
        public string[] TlpTli;

        #endregion

        #region Define local Member
        public bool _Linux_LegacyCheckCommand;
        public bool _Linux_LegacyCheckEnableCommand;
        public string _Linux_LegacyTypeCommand;
        public bool _IsIndeterminateReturn;
        public bool _Username_tb_Cadet;
        public string _UserName_tbTEXT;
        private int _TotalUUTReport;
        private int _SuccessReport;
        private int _FailedReport;
        private int _InProgressReport;
        public string _ExpressGangLoaderVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string FWFolderlocation = AppDomain.CurrentDomain.BaseDirectory + "FWFolder";
        public string IPset = "192.168.254.";
        public string SubNet = "255.255.255.0";
        public int IPChange = 100;
        public int icount = 0;
        public string _FWUpgrateStatus;
        public bool booltlptli;
        public string FWFileName;
        private string UserName = "admin";
        private string Password = "extron";
        private string FWFileType;
        private BackgroundWorker SetIP_bw;
        #endregion

        #region Members
        public bool Linux_LegacyCheckCommand
        {
            get => _Linux_LegacyCheckCommand;
            set
            {
                if (_Linux_LegacyCheckCommand != value)
                {
                    _Linux_LegacyCheckCommand = value;
                    LinuxOrLegacy(Linux_LegacyCheckCommand);
                    NotifyPropertyChanged("Linux_LegacyCheckCommand");
                }
            }
        }
        public string Linux_LegacyTypeCommand
        {
            get { return _Linux_LegacyTypeCommand; }
            set
            {
                if (_Linux_LegacyTypeCommand != value)
                {
                    _Linux_LegacyTypeCommand = value;
                    NotifyPropertyChanged("Linux_LegacyTypeCommand");
                }
            }
        }
        public bool Linux_LegacyCheckEnableCommand
        {
            get => _Linux_LegacyCheckEnableCommand;
            set
            {
                if (_Linux_LegacyCheckEnableCommand != value)
                {
                    _Linux_LegacyCheckEnableCommand = value;
                    NotifyPropertyChanged("Linux_LegacyCheckEnableCommand");
                }
            }
        }
        public string UserName_tbTEXT
        {
            get => _UserName_tbTEXT;
            set
            {
                if (!string.Equals(_UserName_tbTEXT, value))
                {
                    _UserName_tbTEXT = value;
                    NotifyPropertyChanged("UserName_tbTEXT");
                }
                if (value != "" && value != null)
                {
                    Linux_LegacyCheckEnableCommand = true;
                }
                else
                {
                    Linux_LegacyCheckEnableCommand = false;
                }
            }
        }
        public bool IsIndeterminateReturn
        {
            get => _IsIndeterminateReturn;
            set
            {
                if (_IsIndeterminateReturn != value)
                {
                    _IsIndeterminateReturn = value;
                    NotifyPropertyChanged("IsIndeterminateReturn");
                }
            }
        }
        public int SuccessReport
        {
            get { return _SuccessReport; }
            set
            {
                _SuccessReport = value;
                NotifyPropertyChanged("SuccessReport");
            }
        }
        public int FailedReport
        {
            get { return _FailedReport; }
            set
            {
                _FailedReport = value;
                NotifyPropertyChanged("FailedReport");
            }
        }
        public int TotalUUTReport
        {
            get { return _TotalUUTReport; }
            set
            {
                _TotalUUTReport = value;
                NotifyPropertyChanged("TotalUUTReport");
            }
        }
        public int InProgressReport
        {
            get { return _InProgressReport; }
            set
            {
                _InProgressReport = value;
                NotifyPropertyChanged("InProgressReport");
            }
        }
        public string ExpressGangLoaderVersion
        {
            get { return _ExpressGangLoaderVersion; }
            set
            {
                _ExpressGangLoaderVersion = value;
                NotifyPropertyChanged("ExpressGangLoaderVersion");
            }
        }
        public ICommand SearchExecute_Command { get; set; }
        public ICommand ClearArpExecute_Command { get; set; }
        public ICommand BrowseFileExecute_Command { get; set; }
        public ICommand ProgramExecutee_Command { get; set; }
        public ICommand FixtureSettingExecute_Command { get; set; }
        #endregion

        #region Execute Command
        /// <summary>
        /// Step 1 : Enable progress bar
        /// Step 2 : Clear UUTsList 
        /// Step 3 : Clear Legacy Unit Statistics report
        /// Step 4 : Check if User select Legacy or Linux Type of loading firmware
        /// Step 5 : Start discovery timer count down
        /// </summary>
        /// <param name="parameter"></param>
        private void SearchExecute(object parameter)
        {
            try
            {
                IsIndeterminateReturn = true; // progressbar

                //Clear UUTsList and discorver
                UUTsList.Clear();

                //Clear LegacyUploaderService_StatisticsChanged
                LegacyUploaderService_StatisticsChanged(5);

                //Check if Programming Legacy or Linux
                //False => Legacy
                //True => Linux
                if (Linux_LegacyCheckCommand)
                {
                    Console.WriteLine("Running Linux");
                    //ClearDiscoveredDeviceHistory before running UDP
                    _iDiscoveryService.ClearDiscoveredDeviceHistory();
                    //Start discovery
                    _iDiscoveryService.Enabled = true; // begin discovery

                }
                else
                {
                    Console.WriteLine("Running Legacy");
                    //ClearDiscoveredLegacyDeviceHistory before running UDP
                    LegacyDeviceDiscoveryService.ClearDiscoveredLegacyDeviceHistory();
                    //Start discovery
                    LegacyDeviceDiscoveryService.Enable = true;
                    //Add to List
                    LegacyDeviceList();
                }

                DiscoveryTimer.Start();// Count Down 10 second Disable Discovery Broadcasting Mode and clear ARP

            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("SearchExecute", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }

        }

        /// <summary>
        /// Step 1 : Delete current file in FW folder
        /// Step 2 : Open Dialog window 
        /// Step 3 : Switch file type between .eff and .s19 base on unit loading type
        /// Step 4 : Copy Seleted FW file to local machine
        /// Step 5 : Update UI status to let user know software ready for uploading fw
        /// </summary>
        /// <param name="parameter"></param>
        private void BrowseFileExecute(object parameter)
        {
            try
            {
                //Check for firmware folder is available
                //Create if it is Not
                if (!Directory.Exists(FWFolderlocation))
                {
                    Directory.CreateDirectory(FWFolderlocation);
                }
                //If there FW -> delete them
                if (Directory.EnumerateFiles(FWFolderlocation).Any())
                {
                    DirectoryInfo di = new DirectoryInfo(FWFolderlocation);
                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                }

                if (Linux_LegacyCheckCommand)
                {
                    FWFileType = "eff(*.eff)|*.eff|All files (*.*)|*.*";
                }
                else
                {
                    FWFileType = "s19(*.s19)|*.s19|All files (*.*)|*.*";
                }

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = FWFileType;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string _FWFileName = Path.GetFileName(ofd.FileName).ToString();
                        string localpath = Path.Combine(Path.GetDirectoryName(ofd.FileName), ofd.FileName).ToString();
                        string tempdest = Path.Combine(FWFolderlocation, _FWFileName);
                        File.Copy(localpath, tempdest, true);
                        File.SetAttributes(tempdest, FileAttributes.Normal);
                        FWFileName = tempdest;
                    }
                }
                //Assign FW to UUTList
                Parallel.ForEach(UUTsList, item =>
                {
                    item.FW_File = FWFileName.Replace(FWFolderlocation + "\\", "").ToString();
                    item.FWUpdateStatus = "Ready";
                });
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("BrowseFileExecute", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }

        }

        /// <summary>
        /// Step 1 : Run ClearARP method
        /// </summary>
        /// <param name="parameter"></param>
        private void ClearArpExecute(object parameter)
        {
            Console.WriteLine("ClearArpExecute");
            ClearARP();
        }

        /// <summary>
        /// Step 1 : Enable progress bar
        /// Step 2 : run backgroundworker methor to release application from main thread 
        /// Step 3 : LongRunningBackgroundWork will determin which type of loading method to run and execute the appropriate method
        /// </summary>
        /// <param name="parameter"></param>
        private void ProgramExecute(object parameter)
        {
            try
            {
                IsIndeterminateReturn = true; // progressbar   
                ///<summary>
                ///Run SetIp in the background
                ///Step 1 - Create BackgroundWork
                ///       - Check every UUT in the list and stating setting IP
                ///Step 2 - Check for Empty IP starting at 192.168.254.100
                ///         Check for null respond and set that IP as static IP for UUT
                ///Step 3 - Run SetIPAddrTest to set IP on specific uutMAC timeout 5 seconds
                ///Step 4 - Check IP add using MACaddress of UUTs
                ///       - Check for respond of UUTs with the new IP address
                ///Step 5 - Update UI with new IP address
                ///</summary>
                SetIP_bw = new BackgroundWorker();
                SetIP_bw.DoWork += LongRunningBackgroundWork;
                SetIP_bw.RunWorkerCompleted += LongRunningBackgroundWorkCompleted;
                SetIP_bw.RunWorkerAsync();
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("ProgramExecute", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        /// <summary>
        /// Step 1 : Check for connection to fixture
        /// </summary>
        /// <param name="parameter"></param>
        private async void ExecuteFixtureSettingDialog(object parameter)
        {
            //Set up a MVVM
            try
            {   
                var view = new FixtureSettingDialog
                {
                    DataContext = new FixtureSettingDialogViewModel()
                };
                //show the dialog
                var result = await DialogHost.Show(view, "FixtureSettingDialog");
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("ExecuteFixtureSettingDialog", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }
        #endregion

        #region CanExecute Command
        private bool CanSearchExecute(object parameter)
        {
            if (UserName_tbTEXT != "" && UserName_tbTEXT != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CanBrowseFileExecute(object parameter)
        {
            if (UserName_tbTEXT != "" && UserName_tbTEXT != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CanProgramExecute(object parameter)
        {
            if (UserName_tbTEXT != "" && UserName_tbTEXT != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool CanClearArpExecute(object parameter) { return true; }
        private bool CanFixtureSettingExecute(object parameter) { return true; }
        #endregion

        #region Main Constructor
        public MainVM()
        {
            //Default setting to Test Linux Unit
            Linux_LegacyCheckCommand = true;
            //Device file information into a List
            devList = new List<IFirmwareUpgradeDeviceFileInfo>();
            //Creating Collection
            UUTsList = new ObservableCollection<MainModel>();
            //Creating list of usedIP
            usedIP = new List<string>();
            //Creating list of Failed UUT
            failedList = new List<string>();
            //Creating list of FixtureDevice MAC address // Change this into a method to load from list
            FixtureDeviceIP = new string[2] { "192.168.254.10", "192.168.254.11" };
            // check for TLP and TLI
            TlpTli = new string[4] { "TLP", "TLI", "tlp", "tli" };
            //Call StopDiscover Method
            StopDiscoveryUUT();

            //RelayCommand
            SearchExecute_Command = new RelayCommand(SearchExecute, CanSearchExecute);
            ClearArpExecute_Command = new RelayCommand(ClearArpExecute, CanClearArpExecute);
            BrowseFileExecute_Command = new RelayCommand(BrowseFileExecute, CanBrowseFileExecute);
            ProgramExecutee_Command = new RelayCommand(ProgramExecute, CanProgramExecute);
            FixtureSettingExecute_Command = new RelayCommand(ExecuteFixtureSettingDialog, CanFixtureSettingExecute);

            #region iDiscoveryService
            //Step 1 - Subscribe devices discovered 
            _iDiscoveryService.DeviceDiscovered += _iDiscoveryService_SingleDeviceDiscovered;
            #endregion

            #region BulkUpgradeSession
            // subscribe async events
            _BulkUpgradeSession.UpgradeCompleted += _UploaderService_UpgradeCompleted;
            _BulkUpgradeSession.IndividualStatusChanged += _UploaderService_IndividualStatusChanged;
            _BulkUpgradeSession.IndividualProgressChanged += _UploaderService_IndividualProgressChanged;
            _BulkUpgradeSession.StatisticsChanged += _UploaderService_StatisticsChanged;
            #endregion

        }
        #endregion

        #region Method  
        /// <summary>
        /// First step in loading FW
        /// Background Worker to change IP and run Loading type method
        /// Step 1 : Mode 5 Reset uut with Serial Number
        /// Step 2 : Setting Static IP address to all UUT using ArpHelper function
        /// Step 3 : Run ClearARP Method => provide clean arp table to work with
        /// Step 4 : Verify UUT is set with static IP by look it up using MAC Address
        /// Step 5 : Run ClearARP Method => provide clean arp table to work with
        /// Step 6 : Base  on UUT type, run Linux prorgaming typr or Legacy Programming Type
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LongRunningBackgroundWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                #region Step 1 : Mode 5 Reset uut with Serial Number
                if (Linux_LegacyCheckCommand)
                {
                    //Run Mode 5 reset to all Linux UUT
                    LinuxUUT_Mode5_SerialNumber();
                }

                else
                {
                    //Run Mode 5 reset to all Legacy UUT
                    LegacyUUT_Mode5_SerialNumber();
                }
                #endregion

                #region Step 2 : Setting Static IP address to all UUT using ArpHelper function
                //Setting IP for UUTs in the network
                foreach (MainModel item in UUTsList)
                {
                    item.FWUpdateStatus = "Setting IP";
                    string uutMAC = item.MacAddress; // Parse MAC from list to local value
                    string SetIP = emptyIPCheck();
                    ArpHelper.SetIPAddrTest(SetIP, uutMAC, 10000, SubNet);
                }
                #endregion                                

                #region Step 3 : Verify UUT is set with static IP by look it up using MAC Address
                // Verifying Connection with UUT in the network
                // look for current IP using uutMAC                                                                   
                // run PingTest with new IP address - 4 times                                                                    
                // If each failed - Wait 8 second and ping again                                                                   
                // If pass, break - and add to tempList                                                                   
                // If failed all  ping -> failed uut with failed ping status
                foreach (MainModel item in UUTsList)
                {
                    item.FWUpdateStatus = "Verifying New IP";
                    string uutMAC = item.MacAddress; // Parse MAC from list to local value
                    string uutIP = ArpHelper.ResolveIpAddress(uutMAC); // look up IP using MAC   
                    Console.WriteLine(uutMAC + "---" + uutIP);
                    int pass = 0;

                    for (int i = 0; i < 5; i++)
                    {
                        if (PingTest(uutIP))
                        {
                            pass++;
                            break;
                        }
                        else
                        {
                            Thread.Sleep(8000);
                        }
                    }
                    if (pass > 0)
                    {
                        item.FWUpdateStatus = "IP Changed";
                        item.IpAddress = uutIP;
                    }
                    else
                    {
                        item.FWUpdateStatus = "Failed to change IP";
                    }
                }
                #endregion

                #region Step 4 : Run ClearARP Method => provide clean arp table to work with
                //Run ClearArp to clean cache
                ClearARP();
                #endregion

                #region  Step 5 : Base  on UUT type, run Linux prorgaming typr or Legacy Programming Type


                //Depend on the UUT type, 2 seperate process will happen to program the UUT
                //Running Linux Type Program
                if (Linux_LegacyCheckCommand)
                {

                    Console.WriteLine("Program Linux UUT");
                    //Adding devInforNo for programming from UUTsList
                    foreach (MainModel devitems in UUTsList)
                    {
                        IFirmwareUpgradeDeviceFileInfo devInforNo = new FirmwareUpgradeDeviceFileInfo()
                        {
                            //IPAddress, UserName, Password, FileName, IsTlpTli are required,   
                            IPAddress = devitems.IpAddress,
                            FileName = FWFileName,
                            IsTlpTli = booltlptli,
                            Password = "extron",
                            UserName = "admin",
                            WaitForRebootAfterSuccessfulUpgrade = true
                        };
                        // add every device file information into a List
                        devList.Add(devInforNo);
                    }

                    // upgrade N devices fw simultaneously in a row
                    _BulkUpgradeSession.SimultaneousConnections = devList.Count();
                    // start bulk firmware upgrade
                    _BulkUpgradeSession.StartUpgrade(devList);
                }

                //Running Legacy Type Program
                else
                {
                    /// 1. Run LegacyUUT_ExtronFWLoader to load FW
                    /// 2. Run LegacyUUT_UUTQuerryResetDefaultIP to query FW version and Mode 5 reset
                    /// 3. Run usedIP.Clear to clear all used IP address for next programming batch
                    /// 4. Run SetIP_bw.Dispose to clear out used memory                    
                    Console.WriteLine("Program Legacy UUT");
                    //run ExtronFWLoader to program Legacy UUT
                    LegacyUUT_FirmwareUploadSession();
                    
                    if(LegacyUUT_UUTRetryUploadFLAG)
                    {
                        LegacyUUT_UUTRetryUpload();
                    }
                    LegacyUUT_UUTQuerryResetDefaultIP();
                    usedIP.Clear(); // Clear usedIP for next programming
                    SetIP_bw.Dispose();//dispose to clear memory
                    ClearARP();//Clear ARP table
                    FWUploadRecord();
                    IsIndeterminateReturn = false; // progressbar
                }

                #endregion

            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LongRunningBackgroundWork", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        //RetryUpload FLAG if upload FW failed
        private bool LegacyUUT_UUTRetryUploadFLAG = false;

        //RetryUpload FLAG if upload FW failed
        private bool LegacyUUT_UUTRetriedFailFLAG = false;

        //Verifying and Completed the Background Worker
        void LongRunningBackgroundWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Error.Message.ToString(), e.Error.StackTrace.ToString());
                if (e.Error.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.Error.InnerException.Message.ToString(), e.Error.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LongRunningBackgroundWorkCompleted", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        #region General Method
        //ping IP address Test        
        public bool PingTest(string pingvalue)
        {
            bool status = true;
            try
            {
                Ping ping = new Ping();
                PingReply pingresult = ping.Send(pingvalue);
                //If Ping and return respond -> Is assigned to a device
                if (pingresult.Status.ToString() == "Success")
                {
                    return status = true;
                }
                //else IP is not assigned to a device
                else
                {
                    return status = false;
                }
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("PingTest", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            return status;
        }

        //check for empty IP address
        public string emptyIPCheck()
        {
            string readyIP = "";
            bool FoundIP = true;
            try
            {
                while (FoundIP)
                {
                    string TempIP = IPset + IPChange.ToString(); // Set IP 192.168.254.XXX to tempIP to check for availability                
                                                                 //Ping the new IP to check for it availability               
                                                                 //If Ping and return respond -> IP address was taken -> need new one
                    if (PingTest(TempIP))
                    {
                        if (!usedIP.Contains(TempIP))
                        {
                            usedIP.Add(TempIP);
                        }
                        IPChange++;
                    }
                    //else Return no respond-> set IP as open IP for UUT
                    //Check if current IP was used before from usedIP list
                    //if not set this as open IP
                    //else check the next IP value
                    else
                    {
                        if (!usedIP.Contains(TempIP))
                        {
                            usedIP.Add(TempIP);
                            readyIP = TempIP;
                            FoundIP = false;
                        }
                        else
                        {
                            FoundIP = true;
                            IPChange++;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("emptyIPCheck", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            return readyIP;
        }

        //Discovery Stop
        void DiscoveryTimer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine("TimeOut");
            _iDiscoveryService.Enabled = false;
            LegacyDeviceDiscoveryService.Enable = false;
            IsIndeterminateReturn = false;
            DiscoveryTimer.Stop();
        }

        //StopDiscovery Method
        void StopDiscoveryUUT()
        {
            //Stop Discovery after 7 second
            DiscoveryTimer.Interval = new TimeSpan(0, 0, 7);
            DiscoveryTimer.Tick += DiscoveryTimer_Tick;
        }

        //Linux or Legacy Selected Method
        void LinuxOrLegacy(bool toggleData)
        {
            if (toggleData)
            {
                Linux_LegacyTypeCommand = "LINUX";
            }
            else
            {
                Linux_LegacyTypeCommand = "LEGACY";
            }
        }

        //Getting SerialNumber of UUT from Avante using MACaddress
        private string MACaddressToSerialNumberFromAvante(string macaddress)
        {
            string[] avanteresponded = avanteconnect.GetAvanteInfo(macaddress, "", true, false);
            string SNfromMAC = avanteresponded[2].ToString().Substring(0, 7);
            return SNfromMAC;
        }

        //ClearArp
        void ClearARP()
        {
            try
            {
                Process cleararp = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Verb = "runas";
                cleararp.StartInfo = startInfo;
                cleararp.StartInfo.RedirectStandardOutput = true;
                cleararp.StartInfo.RedirectStandardInput = true;
                cleararp.StartInfo.UseShellExecute = false;
                try
                {
                    cleararp.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return;
                }
                StreamWriter sw = cleararp.StandardInput;
                sw.WriteLine("netsh interface ipv4 delete arpcache"); // Arp  Clearn Cache
                sw.WriteLine("netsh interface ipv4 delete destinationcache"); // Arp Clearn Dest Cache
                sw.Close();
                cleararp.WaitForExit();
                //Console.WriteLine(cleararp.StandardOutput.ReadToEnd());
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("emptyIPCheck", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }
        #endregion
       
        #region Legacy Unit
        //Calling argHandler to flag searching for Legacy Devices
        void LegacyDeviceList()
        {
            //Trigger the event in DiscoveryServiceCLass
            LegacyDeviceDiscoveryService.eventHasBeenRaised += new UDPLegacyDeviceDiscoveryService.argHandler(myFunction);
        }

        //Access Argument parameter and add them to UUTsList
        void myFunction(object n, UDPLegacyDeviceDiscoveryService.DiscoveredDeviceArgs e)
        {
            // Check if Current Discovered List contain any duplicate
            App.Current.Dispatcher.Invoke(delegate // <--- HERE
            {
                if (UUTsList.Any(InListData => InListData.MacAddress == e.messageData.MacAddress) == false)
                {
                    if (!FixtureDeviceIP.Contains(e.messageData.IPaddress))
                    {
                        UUTsList.Add(new MainModel(e.messageData.Hostname,
                                     e.messageData.IPaddress,
                                     e.messageData.MacAddress,
                                     "",
                                     e.messageData.PartNumber,
                                     e.messageData.SerialNumber,
                                     e.messageData.FirmwareVersion,
                                     "Connected",
                                     Password,
                                     UserName,
                                     booltlptli,
                                     "",
                                     0,
                                     Visibility.Visible,
                                     Visibility.Collapsed,
                                     Visibility.Collapsed,
                                     false));
                        LegacyUploaderService_StatisticsChanged(1); // report total number of UUT detected from UDP broadcast
                        //Stop old timer and start new timer when new Device detected
                        DiscoveryTimer.Stop();
                        DiscoveryTimer.Start();
                    }
                }
            });
        }

        //Setup CDAFirmwareUploadSession
        void LegacyUUT_FirmwareUploadSession()
        {
            try
            {
                bool executeupload = true;
                SupportedDevices supportedDevices = new SupportedDevices();
                CDAFirmwareUploadSession cDAFirmwareUploadSession = new CDAFirmwareUploadSession(supportedDevices.SelectedList, "");               
                Parallel.ForEach(UUTsList, item =>
                {
                    DnsEndPoint dnsEndpoint = new DnsEndPoint(item.IpAddress, 23);
                    TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.Password);
                    ExtronDevice ed = new ExtronDevice(ts);
                    var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);
                    item.FWUpdateStatus = "Verifying Connection to UUT";
                    if (connectState == ExtronDevice.DeviceState.Connected)
                    {                        
                        // use cda command to load firmware
                        // note: blocking for testing purposes   
                        cDAFirmwareUploadSession.AddDevice(ed);
                        cDAFirmwareUploadSession.TelnetMaxResetTime = 10;
                        var bytes = File.ReadAllBytes(FWFileName);
                        string Firmwaretypecase = FWloaderDevice_MethodLoadType(ed.PartNumber);
                        switch (Firmwaretypecase)
                        {
                            case "IPL_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IPL_TYPE); //IPL_Type
                                break;
                            case "LINUX_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.LINUX_TYPE); //LINUX_TYPE
                                break;
                            case "UNIVERSAL_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.UNIVERSAL_TYPE); //UNIVERSAL_TYPE
                                break;
                            case "STELLARIS_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.STELLARIS_TYPE); //STELLARIS_TYPE
                                break;
                            case "VSC_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.VSC_TYPE); //VSC_TYPE
                                break;
                            case "MPS_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.MPS_TYPE); //MPS_TYPE
                                break;
                            case "NEW_MPS_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.NEW_MPS_TYPE); //NEW_MPS_TYPE
                                break;
                            case "CARD_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.CARD_TYPE); //CARD_TYPE
                                break;
                            case "IDM_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IDM_TYPE); //IDM_TYPE
                                break;
                            case "ARCHIVE_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.ARCHIVE_TYPE); //ARCHIVE_TYPE
                                break;
                            case "IN_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IN_TYPE); //IN_TYPE
                                break;
                            case "IPL_LRG_BIN_TYPE":
                                cDAFirmwareUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IPL_LRG_BIN_TYPE); //IPL_LRG_BIN_TYPE
                                break;
                            default:
                                item.FWUpdateStatus = Firmwaretypecase;
                                executeupload = false;
                                break;
                        }
                        if (executeupload)
                        {
                            item.FWUpdateStatus = "Uploading";                            
                            cDAFirmwareUploadSession.DeviceTransferProgress += CDAFirmwareUploadSession_DeviceTransferProgress;
                            cDAFirmwareUploadSession.DeviceUploadCompleted += CDAFirmwareUploadSession_DeviceUploadCompleted;
                            cDAFirmwareUploadSession.DeviceFailedToConnect += CDAFirmwareUploadSession_DeviceFailedToUpload;
                            cDAFirmwareUploadSession.DeviceFailedToUpload += CDAFirmwareUploadSession_DeviceFailedToUpload;                            
                            cDAFirmwareUploadSession.Upload();                            
                        }
                        
                    }
                    else
                    {
                        item.FWUpdateStatus = connectState.ToString();
                    }
                });
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }
                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LegacyUUT_FirmwareUploadSession", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        //checking FirmwareLoaderDeviceList for the loadMethod
        private string FWloaderDevice_MethodLoadType(string uutPN)
        {
            string FWLoaderDeviceDirectory = System.AppDomain.CurrentDomain.BaseDirectory + "ExpressGangLoaderDeviceList\\ExpressGangLoaderDevices.xml";
            //const string FWLoaderDeviceDirectory = @"C:\Users\Public\Documents\Extron\DeviceList\Devices.xml";
            string methoType = null;
            string uutlvl = uutPN.Substring(0, 2);
            string uutlvlidentifier = uutPN.Substring(3, 4);
            if (uutlvlidentifier.Contains("-"))
            {
                uutlvlidentifier = uutlvlidentifier.Replace("-", "");
            }
            uutPN = uutlvl + "-" + uutlvlidentifier;
            try
            {
                XDocument deviceListXML;
                XmlTextReader xmlReader;
                using (xmlReader = new XmlTextReader(FWLoaderDeviceDirectory))
                {
                    deviceListXML = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace);
                    foreach (var device in deviceListXML.Descendants("Device"))
                    {
                        if (device.FirstAttribute.Value.Contains(uutPN))
                        {
                            methoType = device.FirstAttribute.NextAttribute.NextAttribute.Value; ;
                            break;
                        }
                        else
                        {
                            methoType = "No PN found in DeviceList";
                        }
                    }
                }
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("FWloaderDevice_MethodLoadType", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            return methoType;
        }

        //Reset Legacy Devices using ARP_Helper
        void LegacyUUT_UUTQuerryResetDefaultIP()
        {
            try
            {
                int index = 0;
                //Reset IP for LegacyUUTs in the network
                //step 1 : create Telnet Communication with UUT
                //step 2 : Sent SIS *Q<ENTER> to query UUT FW
                //step 3 : Sent SIS <ESC>ZQQQ<ENTER> to performce Mode5 reset
                //Step 4 : Catch exception due to disconnect from local network to default network
                //Setp 5 : Ping default IP to verify mode 5 reset successfull


                //Query the UUT FW with static IP
                Parallel.ForEach(UUTsList, item =>
                {
                    bool uutconnectingawait = true;
                    DnsEndPoint dnsEndpoint = new DnsEndPoint(item.IpAddress, 23);
                    TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.Password);
                    ExtronDevice ed = new ExtronDevice(ts);
                    SISCommand sISCommand = new SISCommand("*Q" + "\x0D");
                    while (uutconnectingawait)
                    {
                        int value = ++index;
                        var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);
                        item.FWUpdateStatus = "Await Connection";
                        if (connectState == ExtronDevice.DeviceState.Connected)
                        {
                            uutconnectingawait = false;
                        }
                        if (value > 5)
                        {
                            break;
                        }
                    }
                    if (!uutconnectingawait)
                    {
                        var uutQueryFW = ed.SendSISCommand(sISCommand, ExtronDevice.ConnectionBehavior.Block);
                        if (item.Version != uutQueryFW.StrResponse)
                        {
                            item.Version = uutQueryFW.StrResponse;
                        }
                        item.FWUpdateStatus = "Successful";
                        ed.Disconnect();
                    }

                });

                //Reset UUT 
                Parallel.ForEach(UUTsList, item =>
                {
                    DnsEndPoint dnsEndpoint = new DnsEndPoint(item.IpAddress, 23);
                    TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.Password);
                    ExtronDevice ed = new ExtronDevice(ts);
                    var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);
                    SISCommand sISCommand = new SISCommand("\x1B" + "ZQQQ" + "\x0D");
                    if (connectState == ExtronDevice.DeviceState.Connected)
                    {
                        item.IpAddress = item.DefaultIP;
                        ed.SendSISCommandAsync(sISCommand);
                    }
                    else
                    {
                        item.FWUpdateStatus = "Failled to Reset UUT";
                    }

                });
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LegacyUUT_UUTQuerryResetDefaultIP", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        // Occurs when individual fw upgrade progress changed for Legacy Product
        void CDAFirmwareUploadSession_DeviceTransferProgress(object sender, Extron.Communication.FirmwareTransfer.DeviceFirmwareProgress e)
        {
           
            try
            {
                Parallel.ForEach(UUTsList, item =>
                {                    
                    if (item.IpAddress == e.Device.ToString().Replace("ExtronDevice: ", "").Replace(":23", ""))
                    {
                        Console.WriteLine(e.TotalSizeTransferred.ToString());
                        item.FWProgressStatus = e.PercentageCompleted;
                    }
                });
            }
            catch (Exception a)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(a.Message.ToString(), a.StackTrace.ToString());
                if (a.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(a.InnerException.Message.ToString(), a.InnerException.StackTrace.ToString());
                }
                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("CDAFirmwareUploadSession_DeviceTransferProgress", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            Console.WriteLine(e.PercentageCompleted.ToString());
        }

        // Occurs when individual legacy device upgrade completed
        void CDAFirmwareUploadSession_DeviceUploadCompleted(object sender, ExtronDevice device)
        {
            try
            {
                Parallel.ForEach(UUTsList, item =>
                {                    
                    if (item.IpAddress == device.TelnetSetting.HostDnsEndPoint.Host.ToString())
                    {
                        item.Version = device.FirmwareVersion;                        
                    }                    
                });
            }
            catch (Exception a)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(a.Message.ToString(), a.StackTrace.ToString());
                if (a.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(a.InnerException.Message.ToString(), a.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("CDAFirmwareUploadSession_DeviceUploadCompleted", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        //Retry to upload application
        void LegacyUUT_UUTRetryUpload()
        {
            try
            {
                bool executeupload = true;
                SupportedDevices supportedDevices = new SupportedDevices();
                CDAFirmwareUploadSession cDAFirmwareReUploadSession = new CDAFirmwareUploadSession(supportedDevices.SelectedList, "");
                Parallel.ForEach(UUTsList, item =>
                {
                    if (item.FailFLAG)
                    {
                        //Check for connectState
                        item.FWUpdateStatus = "Verifying Connection to UUT";
                        DnsEndPoint dnsEndpoint = new DnsEndPoint(item.IpAddress, 23);
                        TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.Password);
                        ExtronDevice ed = new ExtronDevice(ts);
                        var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);

                        if (connectState == ExtronDevice.DeviceState.Connected)
                        {
                            // use cda command to load firmware
                            // note: blocking for testing purposes
                            cDAFirmwareReUploadSession.AddDevice(ed);
                            cDAFirmwareReUploadSession.TelnetMaxResetTime = 10;
                            var bytes = File.ReadAllBytes(FWFileName);
                            string Firmwaretypecase = FWloaderDevice_MethodLoadType(ed.PartNumber);
                            switch (Firmwaretypecase)
                            {
                                case "IPL_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IPL_TYPE); //IPL_Type
                                    break;
                                case "LINUX_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.LINUX_TYPE); //LINUX_TYPE
                                    break;
                                case "UNIVERSAL_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.UNIVERSAL_TYPE); //UNIVERSAL_TYPE
                                    break;
                                case "STELLARIS_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.STELLARIS_TYPE); //STELLARIS_TYPE
                                    break;
                                case "VSC_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.VSC_TYPE); //VSC_TYPE
                                    break;
                                case "MPS_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.MPS_TYPE); //MPS_TYPE
                                    break;
                                case "NEW_MPS_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.NEW_MPS_TYPE); //NEW_MPS_TYPE
                                    break;
                                case "CARD_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.CARD_TYPE); //CARD_TYPE
                                    break;
                                case "IDM_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IDM_TYPE); //IDM_TYPE
                                    break;
                                case "ARCHIVE_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.ARCHIVE_TYPE); //ARCHIVE_TYPE
                                    break;
                                case "IN_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IN_TYPE); //IN_TYPE
                                    break;
                                case "IPL_LRG_BIN_TYPE":
                                    cDAFirmwareReUploadSession.QueueBuffer(bytes, ed, SupportedDevice.FirmwareType.IPL_LRG_BIN_TYPE); //IPL_LRG_BIN_TYPE
                                    break;
                                default:
                                    item.FWUpdateStatus = Firmwaretypecase;
                                    executeupload = false;
                                    break;
                            }
                            if (executeupload)
                            {
                                item.FWUpdateStatus = "Uploading";
                                cDAFirmwareReUploadSession.DeviceTransferProgress += CDAFirmwareUploadSession_DeviceTransferProgress;
                                cDAFirmwareReUploadSession.DeviceUploadCompleted += CDAFirmwareUploadSession_DeviceUploadCompleted;
                                cDAFirmwareReUploadSession.DeviceFailedToConnect += CDAFirmwareUploadSession_DeviceFailedToUpload;
                                cDAFirmwareReUploadSession.DeviceFailedToUpload += CDAFirmwareUploadSession_DeviceFailedToUpload;
                                cDAFirmwareReUploadSession.Upload();
                            }
                        }
                        else
                        {   
                            item.FWUpdateStatus = connectState.ToString();
                        }
                    }                    
                });    
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }
                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LegacyUUT_UUTRetryUpload", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        // Occurs when individual legacy device Failed to Upload
        void CDAFirmwareUploadSession_DeviceFailedToUpload(object sender, FailedDevice device)
        {
            try
            {                
                foreach (MainModel item in UUTsList)
                {
                   
                    if (item.IpAddress == device.Device.TelnetSetting.HostDnsEndPoint.Host.ToString())
                    {
                        //Log failed device for record
                        new Task(() =>
                        {
                            FailedDeviceList(item.PartNumber, item.SerialNumber, item.MacAddress, device.Status.ToString(), device.Status.GetTypeCode().ToString());
                        }).Start();                       



                        //if Retried failed again - Fail the uut                       
                        if (LegacyUUT_UUTRetriedFailFLAG)
                        {
                            LegacyUUT_UUTRetryUploadFLAG = false;
                            item.FailFLAG = false;
                            item.FWUpdateStatus = "FAIL - Await for Record";
                            item.progressVisibility = Visibility.Collapsed;
                            item.PassVisibility = Visibility.Collapsed;
                            item.FailVisibility = Visibility.Visible;
                            Console.WriteLine("CDAFirmwareUploadSession_DeviceFailedToUpload" + "-- If - Status: " + device.Status.ToString());
                        }
                        //else run retryupload
                        else
                        {
                            item.FWUpdateStatus = "FAIL - Retrying";
                            item.FailFLAG = true;
                            LegacyUUT_UUTRetryUploadFLAG = true;
                            LegacyUUT_UUTRetriedFailFLAG = true;
                            Console.WriteLine("CDAFirmwareUploadSession_DeviceFailedToUpload" +"-- else - Status: "+device.Status.ToString());
                        }                     
                    }
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

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("CDAFirmwareUploadSession_DeviceFailedToUpload", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        // Occurs when there changes in process
        void LegacyUploaderService_StatisticsChanged(int staticttypechanged)
        {
            try
            {
                if (staticttypechanged == 1)
                {
                    TotalUUTReport++;
                }
                if (staticttypechanged == 2)
                {
                    SuccessReport++;
                    InProgressReport -= 1;
                }
                if (staticttypechanged == 3)
                {
                    FailedReport++;
                    InProgressReport -= 1;
                }
                if (staticttypechanged == 4)
                {
                    InProgressReport++;
                }
                if (staticttypechanged == 5)
                {
                    TotalUUTReport = 0;
                    SuccessReport = 0;
                    FailedReport = 0;
                    InProgressReport = 0;
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

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LegacyUploaderService_StatisticsChanged", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }


        }

       //PerformceMode5reset of UUT using SerialNumber
       void LegacyUUT_Mode5_SerialNumber()
        {
            try
            {
                Parallel.ForEach(UUTsList, item =>
                {
                    // Add to Inprogress
                    LegacyUploaderService_StatisticsChanged(4);
                    DnsEndPoint dnsEndpoint = new DnsEndPoint(item.IpAddress, 23);
                    TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.SerialNumber);
                    ExtronDevice ed = new ExtronDevice(ts);
                    var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);
                    SISCommand sISCommand = new SISCommand("\x1B" + "ZQQQ" + "\x0D");
                    if (connectState == ExtronDevice.DeviceState.Connected)
                    {
                        item.FWUpdateStatus = "Reseting UUT";
                        ed.SendSISCommandAsync(sISCommand);
                    }
                    else
                    {
                        Console.WriteLine(connectState);
                    }
                });
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }
                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LegacyUUT_Mode5_SerialNumber", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }


        }

       //PerformceMode5reset of UUT using DefaultIP
       void LegacyUUT_Mode5_DefaultIP()
        {
            Parallel.ForEach(UUTsList, item =>
            {
                DnsEndPoint dnsEndpoint = new DnsEndPoint(item.DefaultIP, 23);
                TelnetSettings ts = new TelnetSettings(dnsEndpoint, item.Password);
                ExtronDevice ed = new ExtronDevice(ts);
                var connectState = ed.Connect(10000, ExtronDevice.ConnectionBehavior.Block);
                SISCommand sISCommand = new SISCommand("\x1B" + "ZQQQ" + "\x0D");
                if (connectState == ExtronDevice.DeviceState.Connected)
                {
                    item.FWUpdateStatus = "Reseting UUT";
                    ed.SendSISCommandAsync(sISCommand);
                }
                else
                {
                    Console.WriteLine(connectState);
                }
            });
        }
       #endregion

        #region Linux Unit
        // Occurs when a device is discovered
        void _iDiscoveryService_SingleDeviceDiscovered(IDeviceDiscoveryService sender, DiscoveryEventArgs evtArg)
        {
            try
            {
                App.Current.Dispatcher.Invoke(delegate // <--- HERE
                {
                    // Check if Current Discovered List contain any duplicate
                    if (UUTsList.Any(InListData => InListData.MacAddress == evtArg.Message.MacAddress) == false)
                    {
                        if (!FixtureDeviceIP.Contains(evtArg.Message.IpAddress))
                        {
                            //check if Connected UUTs a TLP/TlI or not                      
                            foreach (string x in TlpTli)
                            {
                                if (evtArg.Message.Hostname.Contains(x))
                                {
                                    booltlptli = true;
                                }
                            }
                            UUTsList.Add(new MainModel(evtArg.Message.Hostname,
                                          evtArg.Message.IpAddress,
                                          evtArg.Message.MacAddress,
                                          evtArg.Message.Model,
                                          evtArg.Message.PartNumber,
                                          evtArg.Message.SerialNumber,
                                          evtArg.Message.Version,
                                          "Connected",
                                          Password,
                                          UserName,
                                          booltlptli,
                                          "",
                                          0,
                                          Visibility.Visible,
                                          Visibility.Collapsed,
                                          Visibility.Collapsed,
                                          false));
                            //Stop old timer and start new timer when new Device detected
                            DiscoveryTimer.Stop();
                            DiscoveryTimer.Start();
                        }
                    }
                });
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("_iDiscoveryService_SingleDeviceDiscovered", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        // Occurs when individual fw upgrade progress changed
        void _UploaderService_IndividualProgressChanged(object sender, FirmwareBulkUpgradeIndividualProgressEventArgs e)
        {
            try
            {
                foreach (MainModel items in UUTsList)
                {
                    if (items.IpAddress == e.IPAddress.ToString())
                    {
                        items.FWProgressStatus = e.Progress;

                    }
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

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("_UploaderService_IndividualProgressChanged", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            // e.Progress  --- progress value
            // e.IPAddress --- IP address
        }

        // Occurs when individual fw upgrade status changed
        void _UploaderService_IndividualStatusChanged(object sender, FirmwareBulkUpgradeIndividualStatusEventArgs e)
        {
            try
            {
                foreach (MainModel items in UUTsList)
                {
                    if (items.IpAddress == e.IPAddress.ToString())
                    {
                        if (e.FailedArguments != null)
                        {
                            items.FWUpdateStatus = "FAIL - Await for Record";
                            items.progressVisibility = Visibility.Collapsed;
                            items.PassVisibility = Visibility.Collapsed;
                            items.FailVisibility = Visibility.Visible;
                            items.FailFLAG = true;
                            new Task(() =>
                            {
                                FailedDeviceList(items.PartNumber, items.SerialNumber, items.MacAddress, e.FailedArguments.ErrorMessage.ToString(), e.FailedArguments.ErrorCode.ToString());

                            }).Start();
                        }
                        else
                        {
                            items.FWUpdateStatus = e.Status.ToString();
                        }
                    }
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

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("_UploaderService_IndividualStatusChanged", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }

        // Occurs when bulk fw upgrade statistics changed
        void _UploaderService_StatisticsChanged(object sender, FirmwareBulkUpgradeStatisticsEventArgs e)
        {

            TotalUUTReport = e.Total; // --- total devices
            SuccessReport = e.Successful; // --- Successful devices
            FailedReport = e.Failed; // --- Failed devices
            InProgressReport = e.InProgress; // --- In Progress devices
        }

        // Occurs when bulk fw upgrade completed
        void _UploaderService_UpgradeCompleted(object sender, FirmwareBulkUpgradeFailEventArgs e)
        {
            UUTQuerryandResetFW();
            devList.Clear(); //Clear devList for next programming  
            usedIP.Clear(); // Clear usedIP for next programming
            SetIP_bw.Dispose();//dispose to clear memory
            ClearARP();//Clear ARP table
            FWUploadRecord();
            IsIndeterminateReturn = false; // progressbar
        }

        //Check UUT FW version after completed reboot
        void UUTQuerryandResetFW()
        {
            try
            {
                foreach (MainModel items in UUTsList)
                {
                    // Creat IPLinkProSessions with Static IP address
                    IIPLinkProSession _IPLinkProSSH = new IPLinkProSession(IPLinkProAccountTypeEnum.Generic, items.IpAddress, items.UserName, items.Password)
                    {
                        DesiredConnectionType = SessionChannelFlag.SSH,
                        KeepAliveOption = KeepAliveOptionsEnum.Interval,
                        KeepAliveOptionValue = 5
                    };

                    // New FirmwareVersionGMCommand
                    IGMCommand FirmwareVersionGMCommand = new FirmwareVersionGMCommand();

                    // Send GM command using GetGMCommand helper function in Extron.Communication.Services.Base dll
                    // This is use to verify the version loaded into the UUT
                    IGMCommService gmService = new GMCommServiceBase();
                    IGMCommand response = gmService.GetGMCommand(_IPLinkProSSH, FirmwareVersionGMCommand, 10000);

                    if (!String.IsNullOrWhiteSpace(response.Value.ToString()))
                    {
                        int charLocation = response.Value.ToString().IndexOf("*");
                        if (charLocation > 0)
                        {
                            App.Current.Dispatcher.Invoke(delegate // <--- HERE
                            {
                                items.Version = response.Value.ToString().Substring(0, charLocation);
                            });
                        }
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(delegate // <--- HERE
                        {
                            items.Version = "Failed to Verify";
                        });
                    }


                    // After Query the FW version of UUT
                    // Reset UUT IP address to default IP address
                    IGMCommand AbsoluteResetGMCommand = new AbsoluteResetGMCommand() { ResetMode = AbsoluteResetModeEnum.AbsoluteSystemReset };
                    response = gmService.GetGMCommand(_IPLinkProSSH, AbsoluteResetGMCommand, 10000);


                    // After reset uut to restore default ip address
                    // Query the respond and update UI
                    IGMCommand IPAddressGMCommand = new IPAddressGMCommand();
                    response = gmService.GetGMCommand(_IPLinkProSSH, IPAddressGMCommand, 10000);

                    //Update UI with new IP Address
                    App.Current.Dispatcher.Invoke(delegate // <--- HERE
                    {
                        items.IpAddress = response.Value.ToString();
                    });
                }
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("UUTQuerryandResetFW", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }

        }

        //PerformceMode5reset of UUT using SerialNumber
        void LinuxUUT_Mode5_SerialNumber()
        {
            try
            {
                foreach (MainModel item in UUTsList)
                {

                    IIPLinkProSession _IPLinkProSSH = new IPLinkProSession(IPLinkProAccountTypeEnum.Generic, item.IpAddress, item.UserName, MACaddressToSerialNumberFromAvante(item.MacAddress))
                    {
                        DesiredConnectionType = SessionChannelFlag.SSH,
                        KeepAliveOption = KeepAliveOptionsEnum.Interval,
                        KeepAliveOptionValue = 5
                    };

                    // Create a gmService instance for each device
                    IGMCommService gmService = new GMCommServiceBase();

                    // Reset UUT to default in order to return password to default
                    IGMCommand AbsoluteResetGMCommand = new AbsoluteResetGMCommand() { ResetMode = AbsoluteResetModeEnum.AbsoluteSystemReset };
                    IGMCommand response = gmService.GetGMCommand(_IPLinkProSSH, AbsoluteResetGMCommand, 10000);

                    IGMCommand IPAddressGMCommand = new IPAddressGMCommand();
                    response = gmService.GetGMCommand(_IPLinkProSSH, IPAddressGMCommand, 10000);
                    
                    if(response != null)
                    {
                        if (response.Value.ToString() == item.IpAddress)
                        {
                            //Update UI with new IP Address
                            App.Current.Dispatcher.Invoke(delegate // <--- HERE
                            {
                                item.IpAddress = response.Value.ToString();
                            });
                        }
                    }                  
                   
                }
            }
            catch (Exception er)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(er.Message.ToString(), er.StackTrace.ToString());
                if (er.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(er.InnerException.Message.ToString(), er.InnerException.StackTrace.ToString());
                }
                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("LinuxUUT_Mode5_SerialNumber", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }
        #endregion

        #region Last Step after loading FW completed

        ///<summary>
        /// Upload FWRec to the server
        /// 1) Check the uploadStatus for digitalTag
        /// 2) Check FailFLAG of UUT
        ///     a) True -> UUT failed upload process
        ///     b) False -> UUT upload successful
        /// 3) Update status Accordingly
        ///</summary>
        void FWUploadRecord()
        {
            try
            {
                foreach (MainModel items in UUTsList)
                {
                    items.FWProgressStatus = 0; // reset FWprogressbar                
                    bool uploadStatus = UUTsDigitalTag(items.SerialNumber, items.PartNumber, items.FW_File, items.Version, items.FWUpdateStatus); 
                    if (uploadStatus)
                    {
                        if (items.FailFLAG) 
                        {
                            items.FWUpdateStatus = "FAIL - Record Uploaded";
                            items.progressVisibility = Visibility.Collapsed;
                            items.PassVisibility = Visibility.Collapsed;
                            items.FailVisibility = Visibility.Visible;
                            // Add to Fail Count
                            LegacyUploaderService_StatisticsChanged(3);
                        }
                        else
                        {
                            items.FWUpdateStatus = "PASS - Record Uploaded";
                            items.progressVisibility = Visibility.Collapsed;
                            items.PassVisibility = Visibility.Visible;
                            items.FailVisibility = Visibility.Collapsed;
                            // Add to Successed Count
                            LegacyUploaderService_StatisticsChanged(2);
                        }

                    }
                    else
                    {
                        if (items.FailFLAG)
                        {
                            items.FWUpdateStatus = "FAIL - Record Failed to Upload";
                            items.progressVisibility = Visibility.Collapsed;
                            items.PassVisibility = Visibility.Collapsed;
                            items.FailVisibility = Visibility.Visible;
                            // Add to Fail Count
                            LegacyUploaderService_StatisticsChanged(3);
                        }
                        else
                        {
                            items.FWUpdateStatus = "PASS - Record Failed to Upload";
                            items.progressVisibility = Visibility.Collapsed;
                            items.PassVisibility = Visibility.Visible;
                            items.FailVisibility = Visibility.Collapsed;
                            // Add to Successed Count
                            LegacyUploaderService_StatisticsChanged(2);
                        }
                    }


                }
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("FWUploadRecord", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }


        }

        //UploadRecord for UUTs
        private bool UUTsDigitalTag(string sn, string pn, string fw, string uutFW, string result)
        {
            bool returnUUTDigitalTag = false;
            try
            {
                string user = UserName_tbTEXT; // get user 
                string serialNum = sn;
                string partnumber = pn;
                string ICnum = "NA";
                string FWpartNum = fw.Substring(0, 9).ToString();// get 49 level
                string fw_version = "NA"; // get UUT FWversion
                string fw_build = "NA"; // get UUT FW build version
                if (uutFW.Contains("\n"))
                {
                    uutFW = uutFW.Replace("\n", "");
                }
                if (uutFW.Contains("-b"))
                {
                    string[] fw_split = Regex.Split(uutFW, @"-b");
                    fw_version = "V" + fw_split[0];
                    fw_build = fw_split[1];
                }
                else
                {
                    fw_version = "V" + uutFW;
                }
                string status_overall = "";
                if (result == "Successful")
                {
                    status_overall = "Pass";
                }
                else
                {
                    status_overall = "Fail";
                }
                //Console.WriteLine(serialNum + "\\" + partnumber + "\\" + ICnum + "\\" + FWpartNum + "\\" + fw_version + "\\" + fw_build + "\\" + user + "\\" + status_overall);
                /// <summary>
                /// FW Record Tag function to create a FW record tag
                /// </summary>
                fwTagHandler FWtagRecord = new fwTagHandler(serialNum, partnumber, ICnum, FWpartNum, fw_version, fw_build, user, status_overall);
                if (!FWtagRecord.uploadRec())
                {
                    return returnUUTDigitalTag = false; //"Failed to Upload"
                }
                else
                {
                    return returnUUTDigitalTag = true; //"Upload Successful"
                }
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("UUTsDigitalTag", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
            return returnUUTDigitalTag;
        }

        //FailedDeviceList
        void FailedDeviceList(string pn, string sn, string mac, string emsg, string ecode)
        {
            string FailedrecordsDir = AppDomain.CurrentDomain.BaseDirectory + "FailedRecord" + "\\" + pn + "\\"; ;
            string FailedrecordPath = FailedrecordsDir + sn + "_ErrorCode_" + ecode + ".txt"; ;
            try
            {
                List<string> FailedDeviceContent = new List<string>
            {
                               "----------------------------------------------------------------------------",
                string.Format( "PROGRAMMING FAILED RECORD REPORT                                            "),
                               "----------------------------------------------------------------------------",
                string.Format( "Part Number: ----- "  + pn),
                string.Format( "Serial Number: --- " + sn),
                string.Format( "MAC Address: ----- " + mac),
                               "----------------------------------------------------------------------------",
                string.Format( "Failed Message:--- " + emsg),
                string.Format( "Failed Code: ----- " + ecode),
                               "---------------------------------------------------------------------------- ",
                "FAILED DETAILS::"
            };
                // if Record directory does not exist
                if (!Directory.Exists(FailedrecordsDir))
                {
                    Directory.CreateDirectory(FailedrecordsDir);
                }

                File.WriteAllLines(FailedrecordPath, FailedDeviceContent);
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("FailedDeviceList", ErrorMessage, InnerErrorMessage, UserName_tbTEXT);
            }
        }
        #endregion

        #endregion
    }
}
