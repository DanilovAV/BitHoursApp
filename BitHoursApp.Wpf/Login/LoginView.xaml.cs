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

        public void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LoginViewModel;
            var psb = sender as PasswordBox;

            if (vm != null)
                vm.PasswordBox = psb;
        }
    }
}
