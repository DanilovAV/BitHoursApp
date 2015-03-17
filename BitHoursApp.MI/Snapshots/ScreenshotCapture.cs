using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BitHoursApp.MI.Snapshots
{
    public static class ScreenshotCapture
    {
        #region Public static methods

        public static Image TakeScreenshot(ScreenshotCaptureMode captureMode)
        {
            try
            {
                return WindowsCapture(captureMode);
            }
            catch (Exception)
            {
                return OsXCapture(captureMode);
            }
        }

        #endregion

        #region  Private static methods

        private static Image OsXCapture(ScreenshotCaptureMode captureMode)
        {
            bool onlyPrimaryScreen = captureMode == ScreenshotCaptureMode.OnlyPrimaryScreen;

            var data = ExecuteCaptureProcess(
                "screencapture",
                string.Format("{0} -T0 -tpng -S -x", onlyPrimaryScreen ? "-m" : ""));
            return data;
        }

        private static Image ExecuteCaptureProcess(string execModule, string parameters)
        {
            var imageFileName = Path.Combine(Path.GetTempPath(), string.Format("screenshot_{0}.jpg", Guid.NewGuid()));

            var process = Process.Start(execModule, string.Format("{0} {1}", parameters, imageFileName));
            if (process == null)
            {
                throw new InvalidOperationException(string.Format("Executable of '{0}' was not found", execModule));
            }
            process.WaitForExit();

            if (!File.Exists(imageFileName))
            {
                throw new InvalidOperationException(string.Format("Failed to capture screenshot using {0}", execModule));
            }

            try
            {
                return Image.FromFile(imageFileName);
            }
            finally
            {
                File.Delete(imageFileName);
            }
        }

        private static Image WindowsCapture(ScreenshotCaptureMode captureMode)
        {
            switch (captureMode)
            {
                case ScreenshotCaptureMode.OnlyPrimaryScreen:
                    return ScreenCapture(Screen.PrimaryScreen);

                case ScreenshotCaptureMode.ActiveWindow:
                    return ActiveWindowScreenCapture();

                default:
                    break;
            }

            var bitmaps = (Screen.AllScreens.OrderBy(s => s.Bounds.Left).Select(ScreenCapture)).ToArray();

            return CombineBitmap(bitmaps);
        }

        private static Bitmap ScreenCapture(Screen screen)
        {
            var bounds = screen.Bounds;

            if (screen.Bounds.Width / screen.WorkingArea.Width > 1 || screen.Bounds.Height / screen.WorkingArea.Height > 1)
            {
                // Trick  to restore original bounds of screen.
                bounds = new Rectangle(
                    0,
                    0,
                    screen.WorkingArea.Width + screen.WorkingArea.X,
                    screen.WorkingArea.Height + screen.WorkingArea.Y);
            }

            var pixelFormat = new Bitmap(1, 1, Graphics.FromHwnd(IntPtr.Zero)).PixelFormat;

            var bitmap = new Bitmap(bounds.Width, bounds.Height, pixelFormat);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    bounds.X,
                    bounds.Y,
                    0,
                    0,
                    bounds.Size,
                    CopyPixelOperation.SourceCopy);
            }

            return bitmap;
        }

        private static Bitmap ActiveWindowScreenCapture()
        {           
            IntPtr hWnd = User32.GetForegroundWindow();

            if (hWnd != null)
            {
                User32.RECT windowRect = new User32.RECT();
                User32.GetWindowRect(hWnd, ref windowRect);

                int windowWidth = Math.Abs(windowRect.right - windowRect.left);
                int windowHeight = Math.Abs(windowRect.bottom - windowRect.top);

                var bounds = new Rectangle(
                   0,
                   0,
                   windowWidth,
                   windowHeight);

                var pixelFormat = new Bitmap(1, 1, Graphics.FromHwnd(IntPtr.Zero)).PixelFormat;

                var bitmap = new Bitmap(windowWidth, windowHeight, pixelFormat);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(
                        windowRect.left,
                        windowRect.top,
                        0,
                        0,
                        bounds.Size,
                        CopyPixelOperation.SourceCopy);
                }

                return bitmap;
            }

            //capture anything if error is occured
            return ScreenCapture(Screen.PrimaryScreen);            
        }

        private static Image CombineBitmap(ICollection<Image> images)
        {
            Image finalImage = null;

            try
            {
                var width = 0;
                var height = 0;

                foreach (var image in images)
                {
                    width += image.Width;
                    height = image.Height > height ? image.Height : height;
                }

                finalImage = new Bitmap(width, height);

                using (var g = Graphics.FromImage(finalImage))
                {
                    g.Clear(Color.Black);

                    var offset = 0;
                    foreach (var image in images)
                    {
                        g.DrawImage(image,
                            new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();
                throw ex;
            }
            finally
            {
                foreach (var image in images)
                    image.Dispose();
            }

            return finalImage;
        }

        #endregion

        #region WinApi

        private static class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
        }

        #endregion

    }

    public enum ScreenshotCaptureMode
    {
        Full,
        OnlyPrimaryScreen,
        ActiveWindow
    }
}