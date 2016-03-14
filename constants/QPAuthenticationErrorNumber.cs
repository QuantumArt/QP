using System;

namespace Quantumart.QP8.Constants
{
	/// <summary>
	/// Номера ошибка QP-аутентификации
	/// </summary>
	public static class QPAuthenticationErrorNumber
	{
		public static readonly int NoErrors = -1;
        public static readonly int UnknownError = 0;
        public static readonly int AccountNotExist = 1;
        public static readonly int AccountBlocked = 2;
        public static readonly int WrongPassword = 3;
        public static readonly int WindowsAccountNotAssociatedQPUser = 5;
        public static readonly int AutoLoginDisabled = 6;
	}
}
