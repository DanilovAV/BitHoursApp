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

    public class TimeTrackerViewModel : WindowViewModel, ITimeTrackerViewModel
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
                RaiseTimesPropertyChanged();
                this.RaisePropertyChanged(() => Memo);                
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

        public TimeSpan WeeklyElapsedTime
        {
            get
            {
                return selectedContract != null ? selectedContract.WeeklyElapsedTime : new TimeSpan();
            }
        }

        public bool IsMemoEnabled
        {
            get
            {
                return selectedContract != null && !IsRunning && !IsStarting && !IsStopping;
            }
        }

        public string Memo
        {
            get
            {
                return (selectedContract != null) ? selectedContract.Memo : String.Empty;
            }
            set
            {
                if (selectedContract == null || selectedContract.Memo == value)
                    return;

                selectedContract.Memo = value;

                this.RaisePropertyChanged(() => Memo);
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

        private void StartTracking()
        {
            if (selectedContract == null)
                return;

            if (timeTrackerManager != null)
                timeTrackerManager.Dispose();

            timeTrackerManager = new TimeTrackerManager(userInfo, selectedContract, RaiseTimesPropertyChanged);            

            timeTrackerManager.Start();            

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

        public virtual void Configure(UserInfo userInfo)
        {
            this.userInfo = userInfo;

            InitializeCommands();
            InitializeObservables();
            InitializeClockTimer();
            InitializeContracts();
        }
        
        public virtual void Refresh()
        {
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
            using (var act = new ActivityScope(this))
            {
                var contractsList = await GetContracts();

                contracts.Clear();

                foreach (var contract in contractsList)
                    contracts.Add(contract);
            }
        }

        protected virtual void InitializeCommands()
        {
            var canTracking = this.WhenAny(x => x.SelectedContract, x => x.IsStarting, x => x.IsStopping,
                                            (x1, x2, x3) => x1.Value != null && !x2.Value && !x3.Value).StartWith(false);

            TrackingCommand = ReactiveCommand.Create(canTracking);
            TrackingCommand.Subscribe(x => Tracking());
        }

        protected virtual void InitializeObservables()
        {
            DisposeList.Add(this.WhenAnyValue(x => x.IsRunning,
                                                x => x.IsStopping,
                                                x => x.IsStarting,
                                                x => x.SelectedContract)
                                 .Subscribe(x => this.RaisePropertyChanged(() => IsMemoEnabled)));
        }

        protected virtual async Task<List<ContractInfo>> GetContracts()
        {
            var contractList = new List<ContractInfo>();

            var response = await BitHoursApi.Instance.GetContractsAsync(userInfo);

            if (response != null)
            {                
                if (response.HasError)
                {
                    //todo: error handling
                }
                else if (response.Result != null)
                {
                    foreach (var contractObj in response.Result.data)
                    {
                        var contractInfo = contractObj.ToContractInfo();
                        contractList.Add(contractInfo);
                    }
                }
            }

            return contractList;
        }

        protected virtual void RaiseTimesPropertyChanged()
        {
            this.RaisePropertyChanged(() => ElapsedTime);
            this.RaisePropertyChanged(() => WeeklyElapsedTime);
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