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
        void Start();
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

        public void Start()
        {
            if (dispatcherTimer == null)
                return;

            if (!dispatcherTimer.IsEnabled)
            {
                lastWorkTime = DateTime.Now;
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

            await CollectTrackingData();
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

        private DateTime lastWorkTime;
        private Dictionary<DateTime, DateTime> intervals = new Dictionary<DateTime, DateTime>();

        private async void DispatcherTimerWork(object sender, EventArgs e)
        {          
            await CollectTrackingData();
            lastWorkTime = DateTime.Now;
        }

        //private async Task CollectTrackingDataAsync(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    await Task.Run(() => { CollectTrackingData(cancellationToken); });
        //}

        private async Task CollectTrackingData(CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var snapshotData = snapshotManager.Snapshot(ScreenshotCaptureMode.Full);
                var snapshotTime = snapshotData.Item1;
                var snapshot = snapshotData.Item2;

                if (!intervals.ContainsKey(snapshotTime))
                    lock(intervals)
                        if (!intervals.ContainsKey(snapshotTime))                        
                            intervals.Add(snapshotTime, lastWorkTime);

#if DEBUG
                //for testing only
                if (snapshot != null)
                    snapshot.Save(String.Format("scr_{0}.jpg", DateTime.Now.Ticks));

#endif

                //get all snapshots
                var snapshots = snapshotManager.GetAllSnapshots();

                if (snapshots.Count < 1)
                    return;

                //sending snapshots
                foreach (var snap in snapshots)
                {                    
                    var uploadRequest = new BitHoursUploadRequest
                    {
                        UserInfo = userInfo,
                        ContractId = contractInfo.ContractId,                       
                        Snapshot = snap.Value,
                        StartTime = intervals.ContainsKey(snap.Key) ? intervals[snap.Key] : lastWorkTime,
                        EndTime = snap.Key,                        
                        Memo = contractInfo.Memo
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
