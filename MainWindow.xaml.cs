using ExpressGangLoader.ErrorCatch;
using ExpressGangLoader.View.SystemVerify;
using ExpressGangLoader.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TE.Module.Essentials;

namespace ExpressGangLoader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            #region TVCM Update
#if !DEBUG
            ///Run TVC to Update Express FW Loader software
            try
            {
            CheckFiles myCf = new CheckFiles();
            myCf.installFromTVCHelper("FirmwareLoad", "ExpressGangLoader", @"C:\TestFiles\FirmwareLoad\ExpressGangLoader\ExpressGangLoader.exe", 2, 1);
            }
            catch (Exception e)
            {
                string InnerErrorMessage = "";
                string ErrorMessage = string.Concat(e.Message.ToString(), e.StackTrace.ToString());
                if (e.InnerException != null)
                {
                    InnerErrorMessage = string.Concat(e.InnerException.Message.ToString(), e.InnerException.StackTrace.ToString());
                }

                ExpressFWLoaderError.ExpressFWLoaderErrorMessenger("TVCM Update", ErrorMessage, InnerErrorMessage, "");
            }
#endif
            #endregion

            #region Admin Right
#if !DEBUG
            // Verify if ExpressGangLoader is running as admin
            //if YES -> Do nothing
            //else -> Restart
            Tools mt = new Tools();
            if (!mt.IsRunAsAdmin())
            {
                ProcessStartInfo proc = new ProcessStartInfo();
                proc.UseShellExecute = true;
                proc.WorkingDirectory = Environment.CurrentDirectory;
                proc.FileName = Assembly.GetEntryAssembly().CodeBase;

                proc.Verb = "runas";

                try
                {
                    Process.Start(proc);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("This program must be run as an administrator! \n\n" + ex.ToString());
                }
            }            
#endif
            #endregion

            InitializeComponent();

            ///Everything running Express FW loader
            ///Begin with checking all PCS, RS232, LAN commuinication
            ///Display # of available port and self-test software and hardware
            //SystemVerifyView verify = new SystemVerifyView();

            ///MainViewModel with command
            MainVM mvm = new MainVM();
            DataContext = mvm;
            
        }
    }
}
