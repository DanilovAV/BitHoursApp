using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BitHoursApp.Common.Resources;
using BitHoursApp.Common.Utils;
using BitHoursApp.MI.WebApi;
using BitHoursApp.Mvvm;
using UserSettings = BitHoursApp.Common.Resources.Properties.Settings;
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
                var isFirstInit = passwordBox == null;

                this.RaiseAndSetIfChanged(ref passwordBox, value);

                InitializeDefaults();

                if (!isFirstInit)
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

        public bool IsValid
        {
            get
            {
                return PasswordBox != null && String.IsNullOrEmpty(EmailValidationError) && String.IsNullOrEmpty(PasswordValidationError);
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
                else if (response.Result != null && response.Result.success)
                    OnSignedIn(response.Result.data, response.SessionId);
            }

            IsLogging = false;
        }

        private void OnSignedIn(BitHoursLoginObject loginObject, string sessionId)
        {
            UserSettings.Default.DefaultEmail = Email;
            UserSettings.Default.Save();

            var args = new SignedInEventArgs(loginObject, sessionId);

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

            RaisePropertyChanged(() => IsValid);
        }

        private void ValidatePassword()
        {
            PasswordValidationError = String.IsNullOrEmpty(PasswordBox.Password)
                ? CommonResourceManager.Instance.GetResourceString("Error_PasswordEmpty")
                : String.Empty;

            RaisePropertyChanged(() => IsValid);
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
            var canLogin = this.WhenAny(x => x.IsValid, x => x.Value);

            LoginCommand = ReactiveCommand.Create(canLogin);
            LoginCommand.Subscribe(x => Login());
        }

        private bool isDefaultsInitialized = false;

        protected virtual void InitializeDefaults()
        {
            if (isDefaultsInitialized)
                return;

            isDefaultsInitialized = true;

            if (RegexUtils.IsValidEmail(UserSettings.Default.DefaultEmail))
                Email = UserSettings.Default.DefaultEmail;
        }
    }

    public class SignedInEventArgs : EventArgs
    {
        private readonly BitHoursLoginObject loginObject;
        private readonly string sessionId;

        public SignedInEventArgs(BitHoursLoginObject loginObject, string sessionId)
        {
            this.loginObject = loginObject;
            this.sessionId = sessionId;
        }

        public BitHoursLoginObject LoginObject { get { return loginObject; } }

        public string SessionId { get { return sessionId; } }
    }
}