﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BitHoursApp.Wpf;
using BitHoursApp.Wpf.ViewModels;

namespace BitHoursApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowWpf
    {
        public MainWindowWpf()
            : this(new MainViewModel())
        {
        }

        private MainWindowWpf(MainViewModel vm)
        {
            DataContext = vm;
            this.StateChanged += OnStateChanged;
            InitializeComponent();
        }

        private static MainWindowWpf instance;

        public static MainWindowWpf Instance
        {
            get
            {
                return instance ?? (instance = new MainWindowWpf());
            }
        }

        private NotifyIcon notifyIcon;
        public NotifyIcon NotifyIcon
        {
            get
            {
                return notifyIcon;
            }
            set
            {
                if (object.ReferenceEquals(notifyIcon, value))
                    return;

                //unsubscribe
                if (notifyIcon != null)
                    notifyIcon.MouseDoubleClick -= OnNotifyMouseDoubleClick;

                notifyIcon = value;

                //subscribe
                if (notifyIcon != null)
                    notifyIcon.MouseDoubleClick += OnNotifyMouseDoubleClick;
            }
        }

        private void OnNotifyMouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (NotifyIcon == null)
                return;

            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                NotifyIcon.BalloonTipTitle = "Minimize sucessful";
                NotifyIcon.BalloonTipText = "Minimized the app ";
                NotifyIcon.Text = "BitHours";
                NotifyIcon.ShowBalloonTip(400);
                NotifyIcon.Visible = true;
            }
            else if (this.WindowState == WindowState.Normal)
            {
                NotifyIcon.Visible = false;
                this.ShowInTaskbar = true;
            }
        }
    }
}