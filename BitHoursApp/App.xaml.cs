using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
        private readonly bool isUpdatable;

        public App()
        {
        }

        public App(bool isUpdatable) : base()
        {
            this.isUpdatable = isUpdatable;
        }

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
                ResourceRegistrator.Initialization();

                CheckUpdates();

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

        private Dictionary<string, X509Certificate2> dynamicAssemblyCertificates = new Dictionary<string, X509Certificate2>();

        protected virtual Task<Assembly> LoadUpdaterAssemblyAsync()
        {
            return Task.Run<Assembly>(() =>
            {
                try
                {
                    var exists = GetUpdaterAssembly();

                    if (exists != null)
                        return exists;

                    var assemblyCertificate = new X509Certificate2(UpdaterInfo.UpdaterCoreAssemblyDll);

                    var executingAssembly = Assembly.GetExecutingAssembly();
                    var executingAssemblyCertificate = new X509Certificate2(executingAssembly.Location);

                    if (!assemblyCertificate.Equals(executingAssemblyCertificate))
                        return null;

                    var assemblyBytes = File.ReadAllBytes(UpdaterInfo.UpdaterCoreAssemblyDll);
                    var assembly = Assembly.Load(assemblyBytes);

                    dynamicAssemblyCertificates.Add(UpdaterInfo.UpdaterCoreAssembly, assemblyCertificate);

                    return assembly;
                }
                catch
                {
                }

                return null;
            });
        }

        protected virtual Assembly GetUpdaterAssembly()
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            var assembly = asms.FirstOrDefault(x => x.GetName().Name == UpdaterInfo.UpdaterCoreAssembly);
            return assembly;
        }

        protected async virtual void CheckUpdates()
        {
            try
            {
                var assembly = await LoadUpdaterAssemblyAsync();

                if (assembly == null)
                    return;

                Type httpApplicationInfoLoaderType = assembly.GetType(UpdaterInfo.HttpApplicationInfoLoader);
                dynamic httpApplicationInfoLoader = Activator.CreateInstance(httpApplicationInfoLoaderType);

                Type appUpdaterType = assembly.GetType(UpdaterInfo.AppUpdater);

                using (dynamic appUpdater = Activator.CreateInstance(appUpdaterType, httpApplicationInfoLoader))
                {
                    var certif = dynamicAssemblyCertificates[UpdaterInfo.UpdaterCoreAssembly];

                    await appUpdater.InitializeAsync(assemblyCertificate: certif);

                    if (appUpdater.CheckIsUpdateAvailable(UpdaterInfo.UpdaterGuid, UpdaterInfo.UpdaterAssembly))
                        await appUpdater.UpdateApplicationAsync(UpdaterInfo.UpdaterGuid, UpdaterInfo.UpdaterAssembly);

                    if (appUpdater.CheckIsUpdateAvailable(ProgramInfo.AssemblyGuid, Assembly.GetExecutingAssembly().Location)
                        && (System.Windows.MessageBox.Show(CommonResourceManager.Instance.GetResourceString("Common_UpdateQuestion"),
                                CommonResourceManager.Instance.GetResourceString("Common_UpdateCaption"), MessageBoxButton.YesNo) == MessageBoxResult.Yes))
                    {
                        ProcessStartInfo info = new ProcessStartInfo(UpdaterInfo.UpdaterAssembly);

                        Process updaterProc = new Process()
                        {
                            StartInfo = info                         
                        };

                        updaterProc.Start();                        
                        Shutdown();
                    }
                }
            }
            //silent mode
            catch
            {
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (MainWindowWpf.Instance != null && MainWindowWpf.Instance.NotifyIcon != null)
                MainWindowWpf.Instance.NotifyIcon.Visible = false;

            base.OnExit(e);
        }
    }
}