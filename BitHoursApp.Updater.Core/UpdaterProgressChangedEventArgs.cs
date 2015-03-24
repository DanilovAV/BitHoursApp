using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public class UpdaterProgressChangedEventArgs : EventArgs
    {
        public UpdaterProgressChangedEventArgs(int progress, OperationStatus operation)
        {
            Progress = progress;
            Operation = operation;
        }

        public int Progress { get; private set; }

        public OperationStatus Operation { get; private set; }
    }
}