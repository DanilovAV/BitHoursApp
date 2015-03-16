using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BitHoursApp.MI.WebApi;
using BitHoursApp.Mvvm;
using ReactiveUI;

namespace BitHoursApp.Wpf.ViewModels
{
    public interface ILoginViewModel
    {
        event EventHandler<SignedInEventArgs> SignedIn;
    }

    public class LoginViewModel : ViewModelBase, ILoginViewModel
    {
        public LoginViewModel()
        {
            InitializeCommands();
        }

        public event EventHandler<SignedInEventArgs> SignedIn;

        #region Properties

        private string email;
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref email, value);
            }
        }

        private PasswordBox passwordBox;
        public PasswordBox PasswordBox
        {
            get
            {
                return passwordBox;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref passwordBox, value);
            }
        }


        private bool isLogging;
        public bool IsLogging
        {
            get
            {
                return isLogging;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref isLogging, value);
            }

        }

        #endregion

        #region Commands

        public ReactiveCommand<object> LoginCommand
        {
            get;
            private set;
        }

        #endregion

        #region Commands implementation

        private async void Login()
        {
            IsLogging = true;

            var response = await BitHoursApi.Instance.LoginAsync(Email, PasswordBox.Password);

            if (response != null && response.Result != null)
                OnSignedIn(response.Result.data);

            IsLogging = false;
        }

        private void OnSignedIn(BitHoursLoginObject loginObject)
        {
            var args = new SignedInEventArgs(loginObject);

            var handler = SignedIn;

            if (handler != null)
                handler(this, args);
        }

        #endregion

        protected virtual void InitializeCommands()
        {
            var canLogin = this.WhenAny(x => x.Email, x => x.PasswordBox, x => x.IsLogging,
                                    (x1, x2, x3) => !String.IsNullOrWhiteSpace(x1.Value) && x2.Value != null &&
                                                        !String.IsNullOrWhiteSpace(x2.Value.Password) &&
                                                            !x3.Value);

            LoginCommand = ReactiveCommand.Create(canLogin);
            LoginCommand.Subscribe(x => Login());
        }
    }

    public class SignedInEventArgs : EventArgs
    {
        private readonly BitHoursLoginObject loginObject;

        public SignedInEventArgs(BitHoursLoginObject loginObject)
        {
            this.loginObject = loginObject;
        }

        public BitHoursLoginObject LoginObject { get { return loginObject; } }
    }
}