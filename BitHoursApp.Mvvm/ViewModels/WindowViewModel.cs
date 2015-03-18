using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace BitHoursApp.Mvvm
{
    public class WindowViewModel : ViewModelBase
    {
        private bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            protected set
            {
                this.RaiseAndSetIfChanged(ref isActive, value);
            }
        }

        protected class ActivityScope : IDisposable
        {
            private WindowViewModel viewModel;

            public ActivityScope(WindowViewModel viewModel)
            {
                this.viewModel = viewModel;
                viewModel.IsActive = true;
            }

            public void Dispose()
            {
                viewModel.IsActive = false;
            }
        }
    }
}