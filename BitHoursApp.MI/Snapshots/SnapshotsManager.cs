using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitHoursApp.MI.Snapshots
{
    public interface ISnapshotsManager : IDisposable
    {
        Tuple<DateTime, Image> Snapshot(ScreenshotCaptureMode snapshotMode, DateTime? time = null);

        void RemoveSnapshot(DateTime time);

        Image GetSnapshot(DateTime time);
        Dictionary<DateTime, Image> GetAllSnapshots();
    }

    /// <summary>
    /// Snapshot manager (thread-safe)
    /// </summary>
    public class SnapshotsManager : ISnapshotsManager
    {
        private readonly Dictionary<DateTime, Image> snapshots = new Dictionary<DateTime, Image>();
      
        public SnapshotsManager()
        {
        }

        public Tuple<DateTime, Image> Snapshot(ScreenshotCaptureMode snapshotMode, DateTime? time = null)
        {
            try
            {
                var snapshot = ScreenshotCapture.TakeScreenshot(snapshotMode);
                time = time ?? DateTime.Now;

                if (snapshot != null)
                    lock (snapshots)
                        snapshots.Add(time.Value, snapshot);

                return new Tuple<DateTime, Image>(time.Value, snapshot);
            }
            catch
            {
            }

            return null;
        }

        public Image GetSnapshot(DateTime time)
        {
            if (snapshots.ContainsKey(time))
            {
                lock (snapshots)
                {
                    if (snapshots.ContainsKey(time))
                        return snapshots[time];
                }
            }

            return null;
        }

        public Dictionary<DateTime, Image> GetAllSnapshots()
        {
            lock (snapshots)
            {
                return snapshots.ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public void RemoveSnapshot(DateTime time)
        {
            if (snapshots.ContainsKey(time))
            {
                lock (snapshots)
                {
                    if (snapshots.ContainsKey(time))
                        snapshots.Remove(time);
                }
            }
        }

        public void Dispose()
        {
            if (snapshots != null)
            {
                foreach (var snapshot in snapshots.ToList())
                {
                    RemoveSnapshot(snapshot.Key);
                    snapshot.Value.Dispose();
                }
            }
        }
    }  
}