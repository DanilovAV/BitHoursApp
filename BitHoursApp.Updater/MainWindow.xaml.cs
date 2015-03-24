using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BitHoursApp.Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowWpf : WindowBase
    {
        public MainWindowWpf()
            : this(new MainViewModel())
        {
        }

        public MainViewModel ViewModel { get; private set; }

        private MainWindowWpf(MainViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;         
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
    }
}