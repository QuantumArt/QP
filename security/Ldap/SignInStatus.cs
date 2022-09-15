namespace Quantumart.QP8.Security.Ldap;

public enum SignInStatus
{
    /// <summary>
    /// Авторизация не производилась
    /// </summary>
    NotInitialized,

    /// <summary>
    /// Успешная авторизация
    /// </summary>
    Succeeded,

    /// <summary>
    /// УЗ не найдена в системе
    /// </summary>
    NotFound,

    /// <summary>
    /// Аккаунт спосрочен
    /// </summary>
    AccountExpired,

    /// <summary>
    /// УЗ заблокирована
    /// </summary>
    IsLockedOut,

    /// <summary>
    /// Необходимо обновить пароль
    /// </summary>
    PasswordExpired,

    /// <summary>
    /// Ошибка выполнения, не подходит пароль либо заблокировано групповыми политиками
    /// </summary>
    OperationError
}
