using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BitHoursApp.Common.Resources;
using BitHoursApp.Common.Utils;
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
                ValidateEmail();
            }
        }

        private string emailValidationError;
        public string EmailValidationError
        {
            get
            {
                return emailValidationError;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref emailValidationError, value);
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
                ValidatePassword();
            }
        }

        private string passwordValidationError;
        public string PasswordValidationError
        {
            get
            {
                return passwordValidationError;
            }
            private set
            {
                this.RaiseAndSetIfChanged(ref passwordValidationError, value);
                ResetErrorText();
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

        private Visibility capsLockWarningVisibility;
        public Visibility CapsLockWarningVisibility
        {
            get
            {
                return capsLockWarningVisibility;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref capsLockWarningVisibility, value);
            }
        }

        private string errorText;
        public string ErrorText
        {
            get
            {
                return errorText;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref errorText, value);
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
            ResetErrorText();

            IsLogging = true;

            var response = await BitHoursApi.Instance.LoginAsync(Email, PasswordBox.Password);

            if (response != null)
            {
                if (response.HasError)
                {
                    ErrorText = CommonResourceManager.Instance.GetEnumResource(response.ErrorCode);

#if DEBUG

                    if (!String.IsNullOrEmpty(response.Error))
                        ErrorText += response.Error;

#endif
                }
                else if (response.Result != null)
                    OnSignedIn(response.Result.data);
            }

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

        #region Validation

        private void ValidateEmail()
        {
            EmailValidationError = !RegexUtils.IsValidEmail(Email)
                ? CommonResourceManager.Instance.GetResourceString("Error_EmailBadFormat")
                : String.Empty;

        }

        private void ValidatePassword()
        {
            PasswordValidationError = String.IsNullOrEmpty(PasswordBox.Password)
                ? CommonResourceManager.Instance.GetResourceString("Error_PasswordEmpty")
                : String.Empty;
        }

        #endregion

        private void ResetErrorText()
        {
            ErrorText = String.Empty;
        }

        public void RefreshCapsLockState()
        {
            CapsLockWarningVisibility = Console.CapsLock ? Visibility.Visible : Visibility.Hidden;
        }

        protected virtual void InitializeCommands()
        {
            var canLogin = this.WhenAny(x => x.Email, x => x.PasswordBox, x => x.IsLogging,
                                    (x1, x2, x3) => RegexUtils.IsValidEmail(x1.Value) && x2.Value != null &&
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