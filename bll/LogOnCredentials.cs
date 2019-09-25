using System;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;

namespace Quantumart.QP8.BLL
{
    public class LogOnCredentials
    {
        private string _userName;

        [Display(Name = "UserName", ResourceType = typeof(LogOnStrings))]
        public string UserName
        {
            get => UseAutoLogin ? NtUserName : _userName;
            set => _userName = value;
        }

        [Display(Name = "Password", ResourceType = typeof(LogOnStrings))]
        public string Password { get; set; }

        [Display(Name = "CustomerCode", ResourceType = typeof(LogOnStrings))]
        public string CustomerCode { get; set; }

        public bool UseAutoLogin { get; set; }

        public string NtUserName { get; set; }

        public bool? IsSilverlightInstalled { get; set; }

        public QpUser User { get; set; }

        public void Validate()
        {
            var errors = new RulesException<LogOnCredentials>();
            if (!UseAutoLogin)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_NotEnteredLogin);
                }

                if (string.IsNullOrEmpty(Password))
                {
                    errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_NotEnteredPassword);
                }
            }

            if (string.IsNullOrEmpty(CustomerCode))
            {
                errors.ErrorFor(n => n.CustomerCode, LogOnStrings.ErrorMessage_NotEnteredCustomerCode);
            }

            if (errors.IsEmpty)
            {
                if (!QPContext.CheckCustomerCode(CustomerCode))
                {
                    errors.ErrorFor(n => n.CustomerCode, LogOnStrings.ErrorMessage_CustomerCodeNotExist);
                }
            }

            if (errors.IsEmpty)
            {
                var errorCode = QpAuthenticationErrorNumber.NoErrors;
                User = QPContext.Authenticate(this, ref errorCode, out var message);

                if (User != null)
                {
                    User.IsSilverlightInstalled = IsSilverlightInstalled.HasValue && IsSilverlightInstalled.Value;
                }
                else if (errorCode == QpAuthenticationErrorNumber.AccountNotExist)
                {
                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountNotExist);
                }
                else if (errorCode == QpAuthenticationErrorNumber.AccountBlocked)
                {
                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountBlocked);
                }
                else if (errorCode == QpAuthenticationErrorNumber.WrongPassword)
                {
                    errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_WrongPassword);
                }
                else if (errorCode == QpAuthenticationErrorNumber.WindowsAccountNotAssociatedQpUser)
                {
                    errors.ErrorForModel(LogOnStrings.ErrorMessage_WindowsAccountNotAssociatedQPUser);
                }
                else if (errorCode == QpAuthenticationErrorNumber.AutoLoginDisabled)
                {
                    errors.ErrorForModel(LogOnStrings.ErrorMessage_AutoLoginDisabled);
                }
                else if (errorCode != QpAuthenticationErrorNumber.NoErrors)
                {
                    errors.ErrorForModel(LogOnStrings.ErrorMessage_UnknownAuthenticationError + ": " + message);
                }
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }
    }
}
