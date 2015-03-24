using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp
{
    public static class UACHelper
    {
        //simple check for read/write files 
        public static bool IsAdminRightsRequired()
        {
            var checkFile = Path.GetRandomFileName();

            try
            {
                var fs = File.Create(checkFile, 1, FileOptions.DeleteOnClose);
                fs.Dispose();
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }

            return false;
        }

        public static bool IsAdminRightsExists()
        {
            bool isElevated;

            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                isElevated = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                isElevated = false;
            }

            return isElevated;
        }
    }
}