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

        #endregion

        #region Commands

        #region Menu Commands

        public ReactiveCommand<object> CloseCommand
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
    
        protected virtual void InitializeCommands()
        {
            CloseCommand = ReactiveCommand.Create();
            CloseCommand.Subscribe(x => Application.Current.MainWindow.Close());
        }

        protected virtual void OnSignedIn(object sender, SignedInEventArgs e)
        {
            if (e.LoginObject == null)
                return;

            var trackingViewModel = new TimeTrackerViewModel();
            trackingViewModel.Configure(e.LoginObject);
            WorkArea = trackingViewModel;
        }

        #endregion

    }
}