using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BitHoursApp.Updater.Core;
using BitHoursApp.Updater.Properties;

namespace BitHoursApp.Updater
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
        }

        #region Properties

        private int currentProgress;
        public int CurrentProgress
        {
            get
            {
                return currentProgress;
            }
            private set
            {
                if (currentProgress == value)
                    return;

                currentProgress = value;
                RaisePropertyChanged(() => CurrentProgress);
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get
            {
                return statusMessage;
            }
            set
            {
                if (statusMessage == value)
                    return;

                statusMessage = value;
                RaisePropertyChanged(() => StatusMessage);
            }
        }

        public string Caption
        {
            get
            {
                return String.Format("{0} v.{1}", Resources.ApplicationCaption, Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }
        }

        private string currentFile;
        public string CurrentFile
        {
            get
            {
                return currentFile;
            }
            set
            {

                if (currentFile == value)
                    return;

                currentFile = value;
                RaisePropertyChanged(() => CurrentFile);
            }
        }

        #endregion

        public async virtual void InitializeAsync()
        {
            var appLoader = new HttpApplicationInfoLoader();

            try
            {
                if (IsAppRunning())
                {
                    await Task.Delay(5000);

                    if (IsAppRunning())
                    {
                        StatusMessage = Resources.Error_ManyApps;
                        return;
                    }
                }

                using (var appUpdater = new AppUpdater(appLoader))
                {
                    appUpdater.OperationChanged += OnOperationChanged;
                    appUpdater.ProgressChanged += OnProgressChanged;
                    appUpdater.UnzippingFileChanged += OnUnzippingFileChanged;
                    appUpdater.CopyingFileChanged += OnCopyingFileChanged;

                    await appUpdater.InitializeAsync();

                    if (!appUpdater.CheckIsUpdateAvailable(BitHoursAppUpdaterPaths.MainApplicationGuid, BitHoursAppUpdaterPaths.MainApplicationProccess))
                    {
                        StatusMessage = Resources.Error_NoUpdate;
                        return;
                    }

                    await appUpdater.UpdateApplicationAsync(BitHoursAppUpdaterPaths.MainApplicationGuid, BitHoursAppUpdaterPaths.MainApplicationProccess);

                    StatusMessage = Resources.Message_Operation_Completed;

                    RunApplication();
                }
            }
            catch (UpdaterException ex)
            {
                HandleUpdaterError(ex);
            }
            catch (FileNotFoundException)
            {
                StatusMessage = Resources.Error_AssemblyNotFound;
            }
            catch
            {
                StatusMessage = Resources.Error_Unexpected;
            }
        }

        private bool IsAppRunning()
        {
            Process[] appProcesses = Process.GetProcessesByName(BitHoursAppUpdaterPaths.MainApplicationProccess);

            FileInfo appFile = new FileInfo(BitHoursAppUpdaterPaths.MainApplicationProccess);

            foreach (var proc in appProcesses)
            {
                if (appFile.FullName == proc.MainModule.FileName)
                    return true;
            }

            return false;
        }

        private void RunApplication()
        {
            SingleInstance<App>.Cleanup();

            try
            {
                if (File.Exists(BitHoursAppUpdaterPaths.MainApplicationProccess))
                {
                    var info = new ProcessStartInfo(BitHoursAppUpdaterPaths.MainApplicationProccess);

                    var appProcess = new Process()
                    {
                        StartInfo = info
                    };

                    appProcess.Start();
                }
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }

        private void HandleUpdaterError(UpdaterException ex)
        {
            switch (ex.ErrorCode)
            {
                case UpdaterError.AppInfoLoadingFailed:
                    StatusMessage = Resources.Error_AppInfoLoadingFailed;
                    break;

                case UpdaterError.ComputingHashingFailed:
                    StatusMessage = Resources.Error_ComputingHashingFailed;
                    break;

                case UpdaterError.DeserializingFailed:
                    StatusMessage = Resources.Error_DeserializingFailed;
                    break;

                case UpdaterError.DownloadingFailed:
                    StatusMessage = Resources.Error_DownloadingFailed;
                    break;

                case UpdaterError.InvalidAppInfoPath:
                    StatusMessage = Resources.Error_InvalidAppInfoPath;
                    break;

                case UpdaterError.InvalidHash:
                    StatusMessage = Resources.Error_InvalidHash;
                    break;

                case UpdaterError.InvalidXmlFormat:
                    StatusMessage = Resources.Error_InvalidXmlFormat;
                    break;

                case UpdaterError.UnzippingFailed:
                    StatusMessage = Resources.Error_UnzippingFailed;
                    break;

                case UpdaterError.VerifyingFailed:
                    StatusMessage = Resources.Error_VerifyingFailed;
                    break;

                case UpdaterError.CopyingFailed:
                    StatusMessage = Resources.Error_VerifyingFailed;
                    break;

                default:
                    StatusMessage = Resources.Error_Unexpected;
                    break;
            }
        }

        private void OnUnzippingFileChanged(object sender, UpdateProcessingFileChangedEventArgs e)
        {
            StatusMessage = String.Format("{0} {1}", Resources.Message_Operation_Unzipping, e.FileName);
        }

        private void OnCopyingFileChanged(object sender, UpdateProcessingFileChangedEventArgs e)
        {
            StatusMessage = String.Format("{0} {1}", Resources.Message_Operation_Copying, e.FileName);
        }

        private void OnProgressChanged(object sender, UpdaterProgressChangedEventArgs e)
        {
            CurrentProgress = e.Progress;
        }

        private void OnOperationChanged(object sender, UpdaterOperationChangedEventArgs e)
        {
            CurrentProgress = 0;

            switch (e.OperationStatus)
            {
                case OperationStatus.Getting:
                    StatusMessage = Resources.Message_Operation_Getting;
                    break;
                case OperationStatus.Computing:
                    StatusMessage = Resources.Message_Operation_Computing;
                    break;
                case OperationStatus.Downloading:
                    StatusMessage = Resources.Message_Operation_Downloading;
                    break;
                case OperationStatus.Unzipping:
                    StatusMessage = Resources.Message_Operation_Unzipping;
                    break;
                case OperationStatus.Copying:
                    StatusMessage = Resources.Message_Operation_Copying;
                    break;
                default:
                    StatusMessage = String.Empty;
                    break;
            }
        }
    }
}