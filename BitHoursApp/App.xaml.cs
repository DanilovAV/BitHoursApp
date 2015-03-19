using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using BitHoursApp.Common;
using BitHoursApp.Common.Resources;
using UserSettings = BitHoursApp.Common.Resources.Properties.Settings;

namespace BitHoursApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application, ISingleInstanceApp
    {       
        #region ISingleInstanceApp Members

        public bool SignalExternalCommandLineArgs(IList<string> args)
        {
            if(MainWindowWpf.Instance.WindowState == WindowState.Minimized)
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

                ResourceRegistrator.Initialization();

                Uri iconUri = GetApplicationIconUri();

                MainWindowWpf.Instance.Icon = BitmapFrame.Create(iconUri);
                MainWindowWpf.Instance.NotifyIcon = GetNotifyIcon();                

                MainWindowWpf.Instance.Show();
                MainWindowWpf.Instance.Activate();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void InitializeSettings()
        {
            if (UserSettings.Default.UpdateSettings)
            {
                UserSettings.Default.Upgrade();
                UserSettings.Default.UpdateSettings = false;
                UserSettings.Default.Save();
            }
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

        protected virtual Uri GetApplicationIconUri()
        {
            return new Uri("pack://application:,,,/favicon.ico", UriKind.RelativeOrAbsolute);
        }

        protected virtual NotifyIcon GetNotifyIcon()
        {
            NotifyIcon notifyIcon = new NotifyIcon();

            using (Stream iconStream = System.Windows.Application.GetResourceStream(GetApplicationIconUri()).Stream)
                notifyIcon.Icon = new System.Drawing.Icon(iconStream);

            return notifyIcon;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MainWindowWpf.Instance != null && MainWindowWpf.Instance.NotifyIcon != null)
                MainWindowWpf.Instance.NotifyIcon.Visible = false;

            base.OnExit(e);
        }
    }
}