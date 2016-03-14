using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;
using Quantumart.QP8.Validators;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Configuration;
using System.IO;
using Quantumart.QP8.BLL.Helpers;


namespace Quantumart.QP8.BLL
{
	public class LogOnCredentials
	{
		private string _UserName;

		[LocalizedDisplayName("UserName", NameResourceType = typeof(LogOnStrings))]	
		public string UserName
		{
			get
			{
				return (UseAutoLogin) ? NtUserName : _UserName;
			}
			set
			{
				_UserName = value;
			}
		}

		[LocalizedDisplayName("Password", NameResourceType = typeof(LogOnStrings))]	
		public string Password
		{
			get;
			set;
		}

		[LocalizedDisplayName("CustomerCode", NameResourceType = typeof(LogOnStrings))]	
		public string CustomerCode
		{
			get;
			set;
		}

		public bool UseAutoLogin
		{
			get;
			set;
		}

		public string NtUserName
		{
			get;
			set;
		}

		public bool? IsSilverlightInstalled { get; set; }

		public QPUser User
		{
			get;
			set;
		}


		public void Validate()
		{

			var errors = new RulesException<LogOnCredentials>();

			if (!UseAutoLogin)
			{
				if (String.IsNullOrEmpty(UserName))
				{
					errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_NotEnteredLogin);
				}

				if (String.IsNullOrEmpty(Password))
				{
					errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_NotEnteredPassword);
				}
			}

			if (String.IsNullOrEmpty(CustomerCode))
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
				QPContext.CurrentCustomerCode = CustomerCode;
				using (var scope = new QPConnectionScope())
				{

					ApplicationInfoHelper appSrv = new ApplicationInfoHelper();
					string dbVer = appSrv.GetCurrentDBVersion();
					string appVer = appSrv.GetCurrentBackendVersion();
					if (!appSrv.VersionsEqual(dbVer, appVer))
					{
						errors.ErrorForModel(String.Format(LogOnStrings.VersionsAreDifferent, dbVer, appVer));
					}
				}
				QPContext.CurrentCustomerCode = null;
			}

			if (errors.IsEmpty)
			{
				int errorCode = Constants.QPAuthenticationErrorNumber.NoErrors; 
				string message;

				User = QPContext.Authenticate(this, ref errorCode, out message);
				if (User != null)
					User.IsSilverlightInstalled = this.IsSilverlightInstalled.HasValue && this.IsSilverlightInstalled.Value;

				if (errorCode == Constants.QPAuthenticationErrorNumber.NoErrors)
				{

				}
				else if (errorCode == Constants.QPAuthenticationErrorNumber.AccountNotExist)
				{
					errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountNotExist);
				}
				else if (errorCode == Constants.QPAuthenticationErrorNumber.AccountBlocked)
				{
					errors.ErrorFor(n => n.UserName, LogOnStrings.ErrorMessage_AccountBlocked);
				}
				else if (errorCode == Constants.QPAuthenticationErrorNumber.WrongPassword)
				{
					errors.ErrorFor(n => n.Password, LogOnStrings.ErrorMessage_WrongPassword);
				}
				else if (errorCode == Constants.QPAuthenticationErrorNumber.WindowsAccountNotAssociatedQPUser)
				{
					errors.ErrorForModel(LogOnStrings.ErrorMessage_WindowsAccountNotAssociatedQPUser);
				}
				else if (errorCode == Constants.QPAuthenticationErrorNumber.AutoLoginDisabled)
				{
					errors.ErrorForModel(LogOnStrings.ErrorMessage_AutoLoginDisabled);
				}
				else
				{
					errors.ErrorForModel(LogOnStrings.ErrorMessage_UnknownAuthenticationError + ": " + message);
				}
			}
			
			if (!errors.IsEmpty)
				throw errors;
		}
	}
}