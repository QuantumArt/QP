namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Номера ошибка QP-аутентификации
    /// </summary>
    public static class QpAuthenticationErrorNumber
    {
        public static readonly int NoErrors = -1;
        public static readonly int UnknownError = 0;
        public static readonly int AccountNotExist = 1;
        public static readonly int AccountBlocked = 2;
        public static readonly int WrongPassword = 3;
        public static readonly int WindowsAccountNotAssociatedQpUser = 5;
        public static readonly int AutoLoginDisabled = 6;
        public static readonly int IntegratedAccountsDisabled = 7;
    }
}
