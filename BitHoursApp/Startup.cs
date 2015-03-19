using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp
{
    class Startup
    {
        [STAThread]
        public static void Main()
        {
            if (SingleInstance<App>.InitializeAsFirstInstance(ProgramInfo.AssemblyGuid))
            {
                var application = new App();
                application.Run();

                SingleInstance<App>.Cleanup();
            }
        }
    }
}
