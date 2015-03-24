using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public class UpdaterException : Exception
    {
        private UpdaterError errorCode; 

        public UpdaterException(UpdaterError errorCode)
        {            
            this.errorCode = errorCode;
        }

        public UpdaterError ErrorCode
        {
            get
            {
                return errorCode;
            }
        }
    }
}
