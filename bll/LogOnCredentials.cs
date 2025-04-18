using Microsoft.AspNetCore.Mvc.ModelBinding;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;
using Quantumart.QP8.Security.Ldap;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using NLog;
using Quantumart.QP8.BLL.Services.KeyCloak;
using Quantumart.QP8.Configuration.Enums;

namespace Quantumart.QP8.BLL
{
    public class LogOnCredentials
    {
        private string _userName;
        private readonly RulesException<LogOnCredentials> _errors = new();

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
        public bool IsSso { get; set; }

        [BindNever]
        public bool IsPopup { get; set; }

        [BindNever]
        public QpUser User { get; set; }

        public void Validate(ILdapIdentityManager ldapIdentityManagers)
        {
            if (!UseAutoLogin && !IsSso)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_NotEnteredLogin);
                }

                if (string.IsNullOrEmpty(Password))
                {
                    _errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_NotEnteredPassword);
                }
            }

            if (string.IsNullOrEmpty(CustomerCode))
            {
                _errors.ErrorFor(n => n.CustomerCode, LogOnStrings.ErrorMessage_NotEnteredCustomerCode);
            }

            if (_errors.IsEmpty)
            {
                if (!QPContext.CheckCustomerCode(CustomerCode))
                {
                    _errors.ErrorFor(n => n.CustomerCode, LogOnStrings.ErrorMessage_CustomerCodeNotExist);
                    throw _errors;
                }

                if (QPConfiguration.Options.ExternalAuthentication.Type == ExternalAuthenticationType.ActiveDirectory)
                {
                    var parts = UserName.Split('\\');
                    if (parts.Length == 2 && String.IsNullOrEmpty(NtUserName))
                    {
                        var domain = parts[0];
                        if (!string.Equals(ldapIdentityManagers.CurrentDomain, domain, StringComparison.OrdinalIgnoreCase))
                        {
                            _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_DomainNotFound);
                            throw _errors;
                        }
                        var userName = parts[1];
                        var signInResult = ldapIdentityManagers.PasswordSignIn(userName, Password);
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
                                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_NotFound);
                                    break;
                                case SignInStatus.PasswordExpired:
                                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_PasswordExpired);
                                    break;
                                case SignInStatus.AccountExpired:
                                case SignInStatus.IsLockedOut:
                                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_IsLockedOut);
                                    break;
                                case SignInStatus.OperationError:
                                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_Ldap_OperationError);
                                    break;
                                case SignInStatus.NotInitialized:
                                case SignInStatus.Succeeded:
                                default:
                                    break;
                            }
                            throw _errors;
                        }
                    }
                }

                var errorCode = QpAuthenticationErrorNumber.NoErrors;
                User = QPContext.Authenticate(this, ref errorCode, out var message);

                if (errorCode == QpAuthenticationErrorNumber.AccountNotExist)
                {
                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountNotExist);
                }
                else if (errorCode == QpAuthenticationErrorNumber.AccountBlocked)
                {
                    _errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountBlocked);
                }
                else if (errorCode == QpAuthenticationErrorNumber.WrongPassword)
                {
                    _errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_WrongPassword);
                }
                else if (errorCode == QpAuthenticationErrorNumber.WindowsAccountNotAssociatedQpUser)
                {
                    _errors.ErrorForModel(LogOnStrings.ErrorMessage_WindowsAccountNotAssociatedQPUser);
                }
                else if (errorCode == QpAuthenticationErrorNumber.AutoLoginDisabled)
                {
                    _errors.ErrorForModel(LogOnStrings.ErrorMessage_AutoLoginDisabled);
                }
                else if (errorCode == QpAuthenticationErrorNumber.IntegratedAccountsDisabled)
                {
                    _errors.ErrorForModel(LogOnStrings.ErrorMessage_IntegratedAccountsDisabled);
                }
                else if (errorCode != QpAuthenticationErrorNumber.NoErrors)
                {
                    _errors.ErrorForModel(LogOnStrings.ErrorMessage_UnknownAuthenticationError + ": " + message);
                }
            }

            if (!_errors.IsEmpty)
            {
                throw _errors;
            }
        }

        public async Task<bool> CheckSsoEnabled(ISsoAuthService ssoAuthService)
        {
            bool result = await ssoAuthService.CheckSsoEnabled(CustomerCode);

            if (!result)
            {
                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_Disabled);
            }

            return result;
        }

        public async Task ValidateSso(ISsoAuthService ssoAuthService,
            Logger logger,
            string state,
            string originalState,
            string code,
            string error,
            string verifier)
        {
            if (!await CheckSsoEnabled(ssoAuthService))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                logger.ForErrorEvent()
                    .Message("Unable to authenticate with KeyCloak")
                    .Property("Error", error)
                    .Log();

                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_DefaultMessage);

                return;
            }

            if (string.IsNullOrWhiteSpace(state) || !state.Equals(originalState, StringComparison.OrdinalIgnoreCase))
            {
                logger.ForErrorEvent()
                    .Message("Request state does not match with saved one")
                    .Property("ReceivedState", state)
                    .Property("SavedState", originalState)
                    .Log();

                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_Retry);

                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                logger.ForErrorEvent()
                    .Message("KeyCloak authentication code is empty")
                    .Log();

                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_Retry);

                return;
            }

            if (string.IsNullOrWhiteSpace(verifier))
            {
                logger.ForErrorEvent()
                    .Message("Verification code is empty")
                    .Log();

                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_Retry);

                return;
            }

            try
            {
                SsoAuthResult result = await ssoAuthService.CheckUserAuth(code, verifier);

                if (!result.IsSuccess)
                {
                    logger.ForErrorEvent()
                        .Message("Failed to authenticate user with KeyCloak")
                        .Property("Error", result.Error)
                        .Log();

                    _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_DefaultMessage);
                }
                else
                {
                    UserName = result.UserName;
                    NtUserName = result.UserName;
                    UseAutoLogin = true;
                }
            }
            catch (Exception e)
            {
                logger.ForErrorEvent()
                    .Exception(e)
                    .Message("Error while authenticating KeyCloak")
                    .Log();

                _errors.ErrorForModel(LogOnStrings.ErrorMessage_SSO_DefaultMessage);
            }
        }
    }
}
