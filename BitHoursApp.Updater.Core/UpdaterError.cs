using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    //error codes
    public enum UpdaterError
    {        
        InvalidAppInfoPath,
        AppInfoLoadingFailed,
        InvalidXmlFormat,
        DeserializingFailed,
        DownloadingFailed,
        ComputingHashingFailed,
        InvalidHash,
        UnzippingFailed,
        VerifyingFailed,
        CopyingFailed,
        Unexpected,
    }
}
