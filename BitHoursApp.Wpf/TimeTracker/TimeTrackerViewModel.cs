using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.MI.WebApi;
using BitHoursApp.Mvvm;
using ReactiveUI;

namespace BitHoursApp.Wpf.ViewModels
{
    public interface ITimeTrackerViewModel
    {
    }

    public class TimeTrackerViewModel : ViewModelBase, ITimeTrackerViewModel
    {
        public TimeTrackerViewModel()
        {
        }

        #region Properties

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref name, value);
            }
        }

        #endregion

        //todo: to wrap config object
        public void Configure(BitHoursLoginObject loginObject)
        {
            //just for test now
            Name = String.Format("Hello, {0}", loginObject.name);
        }
    }
}
