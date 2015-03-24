using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public static class UpdaterPaths
    {
        public const string updaterBasePath = "http://shop";
        public const string updaterDataPath = "updater.xml";

        public static string GetUpdaterDataPath()
        {
            var uri = new Uri(new Uri(updaterBasePath), updaterDataPath);
            return uri.AbsoluteUri;
        }

        public static string GetUpdatePath(string path)
        {
            var uri = new Uri(new Uri(updaterBasePath), path);
            return uri.AbsoluteUri;
        }        
    }
}
