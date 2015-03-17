using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using BitHoursApp.Common.Resources;
using BitHoursApp.MI.Models;
using BitHoursApp.MI.Snapshots;
using BitHoursApp.MI.TimeTracker;
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
            contracts = new ObservableCollection<ContractInfo>();
        }

        #region Services

        private ITimeTrackerManager timeTrackerManager;

        #endregion

        #region Properties

        private UserInfo userInfo;

        public UserInfo UserInfo
        {
            get
            {
                return userInfo;
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        private bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isActive, value);
            }
        }

        private readonly ObservableCollection<ContractInfo> contracts;
        public ObservableCollection<ContractInfo> Contracts
        {
            get
            {
                return contracts;
            }
        }

        private ContractInfo selectedContract;
        public ContractInfo SelectedContract
        {
            get
            {
                return selectedContract;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref selectedContract, value);
                this.RaisePropertyChanged(() => ElapsedTime);
            }
        }

        public string TrackingButtonContent
        {
            get
            {
                if (isRunning)
                    return CommonResourceManager.Instance.GetResourceString("Common_StopTrackingTime");
                else
                    return CommonResourceManager.Instance.GetResourceString("Common_StartTrackingTime");
            }
        }

        private bool isRunning;
        public bool IsRunning
        {
            get
            {
                return isRunning;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isRunning, value);
                this.RaisePropertyChanged(() => TrackingButtonContent);
            }
        }

        private bool isStarting;
        public bool IsStarting
        {
            get
            {
                return isStarting;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isStarting, value);
            }
        }

        private bool isStopping;
        public bool IsStopping
        {
            get
            {
                return isStopping;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isStopping, value);
            }
        }

        public TimeSpan ElapsedTime
        {
            get
            {
                return selectedContract != null ? selectedContract.ElapsedTime : new TimeSpan();
            }
        }        

        #endregion

        #region Commands

        public ReactiveCommand<object> TrackingCommand
        {
            get;
            private set;
        }

        #endregion

        #region Command implementation

        private void Tracking()
        {
            if (isRunning)
                StopTracking();
            else
                StartTracking();
        }

        private async void StartTracking()
        {
            if (selectedContract == null)
                return;

            if (timeTrackerManager != null)
                timeTrackerManager.Dispose();

            timeTrackerManager = new TimeTrackerManager(userInfo, selectedContract, () => this.RaisePropertyChanged(() => ElapsedTime));

            IsStarting = true;

            await timeTrackerManager.Start();

            IsStarting = false;

            IsRunning = true;
        }

        private async void StopTracking()
        {
            if (timeTrackerManager != null)
            {
                IsStopping = true;

                await timeTrackerManager.Stop();

                IsStopping = false;
            }

            IsRunning = false;
        }

        #endregion

        #region Infrastructure

        public virtual void Configure(BitHoursLoginObject loginObject)
        {
            userInfo = new UserInfo(loginObject);

            InitializeCommands();
            InitializeClockTimer();
            InitializeContracts();
        }

        private DispatcherTimer clockTimer;

        protected virtual void InitializeClockTimer()
        {
            clockTimer = new DispatcherTimer(new TimeSpan(0, 0, 1),
                                            DispatcherPriority.Normal,
                                            (s, e) => RaisePropertyChanged(() => CurrentTime),
                                            Dispatcher.CurrentDispatcher);
        }

        protected virtual async void InitializeContracts()
        {
            IsActive = true;

            var response = await BitHoursApi.Instance.GetContractsAsync(userInfo.UserId);

            if (response != null)
            {
                //temp
                contracts.Clear();
                contracts.Add(new ContractInfo { ContractTitle = "ContractTitle1", ContractOwner = "ContractOwner1" });
                contracts.Add(new ContractInfo { ContractTitle = "ContractTitle2", ContractOwner = "ContractOwner2" });

                if (response.HasError)
                {
                    //todo: error handling
                }
                else if (response.Result != null)
                {

                }
            }

            IsActive = false;
        }

        protected virtual void InitializeCommands()
        {
            var canTracking = this.WhenAny(x => x.SelectedContract, x => x.IsStarting, x => x.IsStopping,
                                            (x1, x2, x3) => x1.Value != null && !x2.Value && !x3.Value).StartWith(false);

            TrackingCommand = ReactiveCommand.Create(canTracking);
            TrackingCommand.Subscribe(x => Tracking());
        }

        #endregion

        #region IDisposable

        protected override void Disposing()
        {
            base.Disposing();

            if (clockTimer != null)
                clockTimer.Stop();

            if (timeTrackerManager != null)
                timeTrackerManager.Dispose();
        }

        #endregion
    }
}