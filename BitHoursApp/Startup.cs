using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp
{
    class Startup
    {
        [STAThread]
        public static void Main()
        {
            bool isUpdatable = true;

            if (UACHelper.IsAdminRightsRequired() && !UACHelper.IsAdminRightsExists())
            {
                isUpdatable = false;

                var info = new ProcessStartInfo(Assembly.GetEntryAssembly().Location)
                {
                    Verb = "runas"
                };

                var process = new Process
                {                                 
                    StartInfo = info
                };

                bool isProcessRunning; 

                try
                {
                    process.Start();
                    isProcessRunning = true;
                }
                catch
                {
                    isProcessRunning = false;
                }

                if(isProcessRunning)
                    return;
            }

            if (SingleInstance<App>.InitializeAsFirstInstance(ProgramInfo.AssemblyGuid))
            {
                var application = new App(isUpdatable);
                application.Run();

                SingleInstance<App>.Cleanup();
            }
        }
    }
}
