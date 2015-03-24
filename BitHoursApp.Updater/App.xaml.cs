using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BitHoursApp.Updater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if (MainWindowWpf.Instance.WindowState == WindowState.Minimized)
                MainWindowWpf.Instance.WindowState = WindowState.Normal;
            else
                MainWindowWpf.Instance.Activate();

            return true;
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;

            InitializeApplication();
            base.OnStartup(e);
        }

        private void InitializeApplication()
        {
            try
            {
                InitializeSettings();           
              
                MainWindowWpf.Instance.Show();
                MainWindowWpf.Instance.Activate();                
                MainWindowWpf.Instance.ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void InitializeSettings()
        {
            //if (UserSettings.Default.UpdateSettings)
            //{
            //    UserSettings.Default.Upgrade();
            //    UserSettings.Default.UpdateSettings = false;
            //    UserSettings.Default.Save();
            //}
        }

        private void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            HandleException(ex);
        }

        private void HandleException(Exception ex)
        {
            if (ex != null)
                System.Windows.MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}