using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BitHoursApp.Updater.Core
{   
    [XmlRoot("applications")]
    public class ApplicationsInfo
    {
        [XmlElement("application")]
        public ApplicationInfo[] ApplicationInfo { get; set; }
    }

    public class ApplicationInfo
    {
        [XmlAttribute("name")]
        public string ApplicationName { get; set; }

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlElement("version", typeof(VersionInfo))]
        public VersionInfo Version { get; set; }
    }


    public class VersionInfo
    {
        [XmlAttribute("version")]
        public string Version { get; set; }
        [XmlAttribute("path")]
        public string Path { get; set; }
        [XmlAttribute("md5hash")]
        public string Md5hash { get; set; }
        [XmlAttribute("isCritical")]
        public bool IsCritical { get; set; }        
    }
}