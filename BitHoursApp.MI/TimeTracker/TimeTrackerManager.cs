using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using BitHoursApp.MI.Models;
using BitHoursApp.MI.Snapshots;
using BitHoursApp.MI.WebApi;

namespace BitHoursApp.MI.TimeTracker
{
    public interface ITimeTrackerManager : IDisposable
    {
        Task Start();
        Task Stop();

        bool IsRunning { get; }      
    }

    public class TimeTrackerManager : ITimeTrackerManager
    {
        private readonly ISnapshotsManager snapshotManager;
        private readonly ContractInfo contractInfo;
        private readonly UserInfo userInfo;
        private readonly DispatcherTimer dispatcherTimer;
        private DispatcherTimer dispatcherElapsedTimer;
        private Action callback;

        public TimeTrackerManager(UserInfo userInfo, ContractInfo contractInfo, Action callback)
        {
            this.userInfo = userInfo;
            this.contractInfo = contractInfo;
            this.callback = callback;

            snapshotManager = new SnapshotsManager();

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher.CurrentDispatcher);
            dispatcherTimer.Interval = new TimeSpan(0, TimeTrackerConfiguration.IntervalMins, 0);
            dispatcherTimer.Tick += DispatcherTimerWork;
        }
       
        public async Task Start()
        {
            if (dispatcherTimer == null)
                return;

            if (!dispatcherTimer.IsEnabled)
            {
                var cancellationToken = new CancellationTokenSource();
                cancellationToken.CancelAfter(TimeTrackerConfiguration.CancelAsyncInterval);

                try
                {
                    await CollectTrackingDataAsync(cancellationToken.Token);
                }
                catch (OperationCanceledException oce)
                {
                    //todo: handle it
                    System.Diagnostics.Debug.WriteLine("Collection at the start was cancelled");
                }

                StartElapsedTimer();
                dispatcherTimer.Start();
            }
        }

        public bool IsRunning
        {
            get
            {
                return dispatcherTimer != null && dispatcherTimer.IsEnabled;
            }
        }

        public async Task Stop()
        {
            if (dispatcherTimer == null)
                return;

            if (dispatcherElapsedTimer != null)
                dispatcherElapsedTimer.Stop();

            if (dispatcherTimer.IsEnabled)
                dispatcherTimer.Stop();

            var cancellationToken = new CancellationTokenSource();
            cancellationToken.CancelAfter(TimeTrackerConfiguration.CancelAsyncInterval);

            try
            {                
                await CollectTrackingDataAsync(cancellationToken.Token);
            }
            catch (OperationCanceledException oce)
            {
                //todo: handle it
                System.Diagnostics.Debug.WriteLine("Collection at the stop was cancelled");
            }
        }

        public void Dispose()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimerWork;
            }

            if (dispatcherElapsedTimer != null)
                dispatcherElapsedTimer.Stop();

            if (snapshotManager != null)
                snapshotManager.Dispose();
        }

        private void DispatcherTimerWork(object sender, EventArgs e)
        {
            CollectTrackingData();
        }

        private async Task CollectTrackingDataAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => CollectTrackingData(cancellationToken));
        }

        private async void CollectTrackingData(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var snapshot = snapshotManager.Snapshot(ScreenshotCaptureMode.Full);

                //for testing only
                if (snapshot != null)                
                    snapshot.Save(String.Format("scr_{0}.jpg", DateTime.Now.Ticks));                

                //get all snapshots
                var snapshots = snapshotManager.GetAllSnapshots();

                if (snapshots.Count < 1)
                    return;

                //get last snapshot time
                var lastTimeStamp = snapshots.Max(x => x.Key);

                //try to resend snapshots
                foreach (var snap in snapshots)
                {
                    var elapsedMinutes = lastTimeStamp - snap.Key;
                    
                    var uploadRequest = new BitHoursUploadRequest
                    {
                        ContractId = contractInfo.ContractId,
                        UserId = userInfo.UserId,
                        Snapshot = snap.Value,
                        Status = dispatcherTimer.IsEnabled ? BitHoursUploadRequestStatus.Start : BitHoursUploadRequestStatus.Stop,
                        ElapsedMinutes = (contractInfo.ElapsedTime - elapsedMinutes).Minutes,
                        Memo = "test memo"
                    };

                    var response = await BitHoursApi.Instance.UploadSnapshotAsync(uploadRequest, cancellationToken);

                    if (!response.HasError)
                        snapshotManager.RemoveSnapshot(snap.Key);
                }                
            }
            catch
            {
            }
        }

        private void StartElapsedTimer()
        {
            dispatcherElapsedTimer = new DispatcherTimer(new TimeSpan(0, 0, 1),
                                                            DispatcherPriority.Normal,
                                                            (s, e) =>
                                                            {
                                                                contractInfo.ElapsedTime += TimeSpan.FromSeconds(1);
                                                                
                                                                if (callback != null)
                                                                    callback();
                                                            },
                                                            Dispatcher.CurrentDispatcher);
        }
    }
}
