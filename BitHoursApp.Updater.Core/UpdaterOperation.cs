using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.Updater.Core
{
    public enum OperationStatus
    {
        None,
        Getting,
        Downloading,
        Computing,
        Deleting,
        Unzipping,
        Copying
    }

    public class Operation
    {
        public Operation(Action<OperationStatus> onOperationChanged)
        {
            OnOperationChanged = onOperationChanged;
        }

        public OperationStatus OperationStatus
        {
            get;
            set;
        }

        public Action<OperationStatus> OnOperationChanged { get; set; }
    }

    internal class OperationScope : IDisposable
    {
        private readonly Operation operation;

        public OperationScope(Operation operation, OperationStatus status)
        {
            this.operation = operation;
            this.operation.OperationStatus = status;

            if (this.operation.OnOperationChanged != null)
                this.operation.OnOperationChanged(status);
        }

        public void Dispose()
        {
            this.operation.OperationStatus = OperationStatus.None;
        }
    }
}