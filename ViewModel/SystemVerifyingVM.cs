using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExpressGangLoader.ViewModel
{
    public class SystemVerifyingVM : INotifyPropertyChanged
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
        #endregion

        #region Constructor
        public SystemVerifyingVM()
        {
            ///Running system verification
            ///Step 1 -> check internet connection to intranet by running HttpWebRequestsystem
            ///If return true => Http Respond 
            ///if return false => Cannnot connect to intranet
            if( HttpWebRequestsystem() )
            {
               //MessageSystem("Connecting to Extron Intranet Success");
            }
            else
            {
               //MessageSystem("Trouble connecting to Extron Intranet. Please try later.");
            }

            ///Step 2 -> Check connection to PCS4

        }
        #endregion

        #region Checking HttpWebRequestsystem
        public bool HttpWebRequestsystem()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://intranet.extron.com//app/TestVersionCheck/default.aspx");
            request.Timeout = 5000;
            request.Credentials = CredentialCache.DefaultNetworkCredentials;  
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            if(response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Verification connection to PCS4
        #endregion
    }
}















///IP 192.168.200 - 216
///Port : 23
///Turn On : GM CMD w1*1pc
///Turn Off : GM CMD w1*0pc
///Turn On : GM CMD w2*1pc
///Turn Off : GM CMD w2*0pc
///Turn On : GM CMD w3*1pc
///Turn Off : GM CMD w3*0pc
///Turn On : GM CMD w4*1pc
///Turn Off : GM CMD w4*0pc