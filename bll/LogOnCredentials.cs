using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;
using Quantumart.QP8.Security.Ldap;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

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

        [BindNever]
        public string NtUserName { get; set; }

        [BindNever]
        public QpUser User { get; set; }
        
        public async Task Validate(LdapIdentityManager ldapIdentityManagers, CancellationToken cancellationToken)
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
                if (QPConfiguration.Options.EnableLdapAuthentication)
                {
                    var parts = UserName.Split('\\');                    
                    if (parts.Length == 2 && String.IsNullOrEmpty(NtUserName))
                    {
                        var domain = parts[0];
                        if (!string.Equals(ldapIdentityManagers.CurrentDomain, domain, StringComparison.InvariantCultureIgnoreCase))
                        {
                            errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_DomainNotFound);
                            throw errors;
                        }
                        var userName = parts[1];
                        var signInResult = await ldapIdentityManagers.PasswordSignIn(userName, Password, cancellationToken);
                        if (signInResult.Succeeded)
                        {
                            NtUserName = UserName;
                            UserName = userName;
                            UseAutoLogin = true;
                        }
                        else
                        {
                            switch (signInResult.Status)
                            {
                                case SignInStatus.NotFound:
                                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_NotFound);
                                    break;
                                case SignInStatus.PasswordExpired:
                                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_PasswordExpired);                                    
                                    break;
                                case SignInStatus.AccountExpired:
                                case SignInStatus.IsLockedOut:
                                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_IsLockedOut);                                    
                                    break;
                                case SignInStatus.OperationError:
                                    errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_OperationError);                                    
                                    break;
                                case SignInStatus.NotInitialized:
                                case SignInStatus.Succeeded:
                                default:
                                    break;
                            }
                            throw errors;
                        }                        
                    }
                }
                
                var errorCode = QpAuthenticationErrorNumber.NoErrors;
                User = QPContext.Authenticate(this, ref errorCode, out var message);

                if (errorCode == QpAuthenticationErrorNumber.AccountNotExist)
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
