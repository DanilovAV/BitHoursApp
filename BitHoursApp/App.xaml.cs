using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace BitHoursApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
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
    }
}
