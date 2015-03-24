using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BitHoursApp.Updater.Core;

namespace BitHoursApp.Updater
{
    class Startup
    {
        [STAThread]
        public static void Main()
        {
            if (UACHelper.IsAdminRightsRequired() && !UACHelper.IsAdminRightsExists())
            {
                var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location)
                {
                    Verb = "runas"
                };

                var process = new Process
                {
                    StartInfo = info
                };

                try
                {
                    process.Start();
                }
                catch
                {
                }

                return;
            }

            //use same as MainApp guid to prevent running updater and app together
            if (SingleInstance<App>.InitializeAsFirstInstance(BitHoursAppUpdaterPaths.MainApplicationGuid, false))
            {
                if (CertificateHelper.Verify(BitHoursAppUpdaterPaths.MainApplicationProccess))
                {
                    var application = new App();
                    application.Run();
                }
                else
                {
                    MessageBox.Show("Sertificate is invalid", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                SingleInstance<App>.Cleanup();
            }
        }
    }
}