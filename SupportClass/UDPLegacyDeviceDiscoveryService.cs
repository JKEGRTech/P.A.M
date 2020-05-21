using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ExpressGangLoader.SupportClass
{
    public class UDPLegacyDeviceDiscoveryService : INotifyPropertyChanged
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

        #region Define Value
        private string _IPaddress;
        private string _Hostname;
        private string _MacAddress;
        private string _Description;
        private string _PartNumber;
        private string _FirmwareVersion;
        private bool _Enable;
        private bool _ListReady;

        public string Hostname
        {
            get { return _Hostname; }
            set
            {
                _Hostname = value;
            }
        }
        public string IPaddress
        {
            get { return _IPaddress; }
            set
            {
                _IPaddress = value;
            }
        }
        public string MacAddress
        {
            get { return _MacAddress; }
            set
            {
                _MacAddress = value;
            }
        }
        public string PartNumber
        {
            get { return _PartNumber; }
            set
            {
                _PartNumber = value;
            }
        }
        public string Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
            }
        }
        public string FirmwareVersion
        {
            get { return _FirmwareVersion; }
            set
            {
                _FirmwareVersion = value;
            }
        }
        public bool Enable
        {
            get { return _Enable; }
            set
            {
                _Enable = value;
                LegacySearchEnable(value);
            }
        }
        public bool ListReady
        {
            get { return _ListReady; }
            set
            {
                _ListReady = value;
            }
        }
        private UdpClient listener;
        private IPEndPoint groupEP;
        // public event

        #endregion

        #region Define Local Member
        //Definded port for NonGM UUTs
        private const int listenPortNonGM = 1230;
        private IPAddress broadcast = IPAddress.Parse("192.168.254.255");
        //Sending Package of Data to request UUTs to identify itself
        private byte[] sendPackNonGM = { 0x55, 0x44, 0x50, 0x43, 0xff, 0xff, 0xff, 0xff, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x1b, 0x42, 0x43, 0x0d, 0x1b, 0x43, 0x41, 0x0d, 0x1b, 0x4d, 0x54, 0x0d, 0x31, 0x49 };
        private BackgroundWorker UDPLegacyService_BG = new BackgroundWorker();

        #endregion

        #region Constructor
        public UDPLegacyDeviceDiscoveryService()
        {
            //Run this Services As Async
            UDPLegacyService_BG.DoWork += UDPLegacyService_BG_DoWork;
            UDPLegacyService_BG.RunWorkerCompleted += UDPLegacyService_BG_DoWorkCompleted;
            UDPLegacyService_BG.WorkerSupportsCancellation = true;
            ListReady = false;
        }
        #endregion

        #region Method
        private void UDPLegacyService_BG_DoWork(object sender, DoWorkEventArgs e)
        {
            listener = new UdpClient(listenPortNonGM);
            groupEP = new IPEndPoint(broadcast, 1231);

            while (Enable)
            {
                listener.Send(sendPackNonGM, sendPackNonGM.Length, groupEP);
                byte[] bytes = listener.Receive(ref groupEP);
                string recieved = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                //Console.WriteLine(recieved);
                parseData(bytes, groupEP.Address.ToString());
                System.Threading.Thread.Sleep(1000);
            }
        }

        private void UDPLegacyService_BG_DoWorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            listener.Close();
        }
                
        private void parseData(byte[] inputByte, string IPdata)
        {
            string dataStr = Encoding.ASCII.GetString(inputByte, 0, inputByte.Length);
            Match pnMatch = Regex.Match(dataStr, @"60-[0-9]{3,4}-[0-9]{2}");

            if (pnMatch.Success)
            {
                string[] dataStrSplit = dataStr.Split(new string[] { "\0", "\r\n" }, StringSplitOptions.None);

                if (dataStrSplit.Length == 8)
                {
                    IPaddress = IPdata;
                    Hostname = dataStrSplit[2];
                    Description = dataStrSplit[6];
                    PartNumber = dataStrSplit[0];
                    FirmwareVersion = dataStrSplit[1];
                    MacAddress = "00-05-A6" + dataStrSplit[2].Substring(dataStrSplit[2].Length - 9);
                    raiseEvent(MacAddress);
                }
            }
        }

        public void LegacySearchEnable(bool enable)
        {
            if (enable)
            {
                //Console.WriteLine("DiscoveryService is Enable");
                UDPLegacyService_BG.RunWorkerAsync();
            }
            else
            {
                //Console.WriteLine("DiscoveryService is Disable");
                UDPLegacyService_BG.CancelAsync();

            }

        }

        public void ClearDiscoveredLegacyDeviceHistory()
        {            
            tempList.Clear();
        }

        public class DiscoveredDeviceArgs : EventArgs
        {
            public struct DiscoverStruct
            {
                public string IPaddress;
                public string Hostname;
                public string MacAddress;
                public string Description;
                public string PartNumber;
                public string FirmwareVersion;


                public DiscoverStruct(string hostname, string ipaddress, string macaddress, string description, string partnumber, string firmwareversion)
                {
                    Hostname = hostname;
                    IPaddress = ipaddress;
                    MacAddress = macaddress;
                    Description = description;
                    PartNumber = partnumber;
                    FirmwareVersion = firmwareversion;
                }
            }
            public DiscoverStruct messageData;

            public DiscoveredDeviceArgs(string hostname, string ipaddress, string macaddress, string description, string partnumber, string firmwareversion)
            {
                messageData = new DiscoverStruct( hostname,  ipaddress,  macaddress,  description,  partnumber,  firmwareversion);
            }

        }

        public delegate void argHandler(object garbageObject, DiscoveredDeviceArgs Args);
        public event argHandler eventHasBeenRaised;
        public DiscoveredDeviceArgs ArgsList = new DiscoveredDeviceArgs("", "", "", "", "", "");
        
        public void raiseEvent(string macaddress)
        {
            ArgsList.messageData.MacAddress.ToString();
            bool AddnewDevice = Discovered_Device(macaddress);
            if(AddnewDevice)
            {
                ArgsList = new DiscoveredDeviceArgs(Hostname, IPaddress, MacAddress, Description, PartNumber, FirmwareVersion);
                eventHasBeenRaised(this, ArgsList);
                
            }
            else
            {
                //Console.WriteLine(macaddress + " Is Duplicated");
            }
        }

        private List<string> tempList = new List<string>();
        private bool Discovered_Device(string macaddress)
        {
            if (tempList.Contains(macaddress) == false)
            {
                tempList.Add(macaddress);
                return true;
            }
            else
            {
                return false;
            }

        }
    }
    #endregion
}

