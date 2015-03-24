using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public class UpdaterOperationChangedEventArgs : EventArgs
    {
        public UpdaterOperationChangedEventArgs(OperationStatus operationStatus)
        {
            OperationStatus = operationStatus;            
        }

        public OperationStatus OperationStatus { get; private set; }        
    }
}