using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitHoursApp.Mvvm;
using ReactiveUI;
using System.Windows;
using BitHoursApp.Wpf.ViewModels;
using BitHoursApp.Common.Reflection;
using BitHoursApp.MI.Models;
using System.Reactive.Linq;
using System.Diagnostics;
using System.Reflection;

namespace BitHoursApp.Wpf
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            Initialize();
        }

        #region Properties

        private ViewModelBase workArea;
        public ViewModelBase WorkArea
        {
            get
            {
                return workArea;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref workArea, value);
            }
        }

        private bool isLoggedIn;
        public bool IsLoggedIn
        {
            get
            {
                return isLoggedIn;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isLoggedIn, value);
            }
        }

        public bool AskExitConfirmation
        {
            get
            {
                var timeTrackerViewModel = WorkArea as TimeTrackerViewModel;

                if (timeTrackerViewModel != null)
                {
                    return timeTrackerViewModel.IsRunning || timeTrackerViewModel.IsStarting || timeTrackerViewModel.IsStopping;
                }

                return false;
            }
        }

        private string version;
        public string Version
        {
            get
            {
                if (version == null)
                    version = Assembly.GetEntryAssembly().GetName().Version.ToString();

                return version;
            }
        }

        #endregion

        #region Commands

        #region Menu Commands

        public ReactiveCommand<object> CloseCommand
        {
            get;
            private set;
        }

        public ReactiveCommand<object> LogoutCommand
        {
            get;
            private set;
        }

        public ReactiveCommand<object> RefreshRoomCommand
        {
            get;
            private set;
        }

        #endregion

        #endregion

        #region Infrastructure

        protected virtual void Initialize()
        {
            InitializeLogin();
            InitializeCommands();
        }

        protected virtual void InitializeLogin()
        {
            var loginViewModel = new LoginViewModel();
            WeakEventManager<LoginViewModel, SignedInEventArgs>.AddHandler(loginViewModel, "SignedIn", OnSignedIn);
            WorkArea = loginViewModel;
        }

        IObservable<bool> canLogout;

        protected virtual void InitializeCommands()
        {
            CloseCommand = ReactiveCommand.Create();
            CloseCommand.Subscribe(x => Application.Current.MainWindow.Close());

            canLogout = this.WhenAny(x => x.IsLoggedIn, x => x.Value);
            LogoutCommand = ReactiveCommand.Create(canLogout);
            LogoutCommand.Subscribe(x => Logout());

            RefreshRoomCommand = ReactiveCommand.Create(Observable.Return(false));
        }

        protected virtual void OnSignedIn(object sender, SignedInEventArgs e)
        {
            if (e.LoginObject == null)
                return;

            var userInfo = new UserInfo(e.LoginObject, e.SessionId);

            var trackingViewModel = new TimeTrackerViewModel();
            trackingViewModel.Configure(userInfo);
            WorkArea = trackingViewModel;

            var canRefresh = canLogout.CombineLatest(trackingViewModel
                                  .WhenAny(x => x.IsRunning,
                                     x => x.IsStarting,
                                     x => x.IsStopping,
                                     (x1, x2, x3) => !x1.Value && !x2.Value && !x3.Value),
                                     (x1, x2) => x1 && x2);

            CreateRefreshCommand(canRefresh);

            IsLoggedIn = true;
        }

        protected virtual void Logout()
        {
            if (WorkArea != null)
                WorkArea.Dispose();

            IsLoggedIn = false;

            CreateRefreshCommand(Observable.Return(false));

            InitializeLogin();
        }

        protected virtual void RefreshRoom()
        {
            var trackingViewModel = WorkArea as TimeTrackerViewModel;

            if (trackingViewModel != null)
                trackingViewModel.Refresh();
        }

        protected virtual void CreateRefreshCommand(IObservable<bool> canRefreshRoom)
        {
            if (RefreshRoomCommand != null)
                RefreshRoomCommand.Dispose();

            RefreshRoomCommand = ReactiveCommand.Create(canRefreshRoom);
            RefreshRoomCommand.Subscribe(x => RefreshRoom());

            this.RaisePropertyChanged(() => RefreshRoomCommand);
        }

        #endregion

    }
}