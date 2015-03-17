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
using BitHoursApp.Wpf.ViewModels;

namespace BitHoursApp.Wpf.Views
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        public LoginViewModel ViewModel
        {
            get { return DataContext as LoginViewModel; }
        }

        public void OnPasswordChanged(object sender, RoutedEventArgs e)
        {      
            if (ViewModel != null)
                ViewModel.PasswordBox = pbPassword;
        }

        private void LoginWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.RefreshCapsLockState();
        }

        private void LoginWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.RefreshCapsLockState();
                ViewModel.PasswordBox = pbPassword;
            }
        }
    }
}