namespace Quantumart.QP8.WebMvc.Infrastructure.Enums
{
    /// <summary>
    /// Режим работы фильтра
    /// </summary>
    public enum ExceptionResultMode
    {
        /// <summary>
        /// Возвращает ошибку в формате для интерфейсных qp-action
        /// </summary>
        UiAction,

        /// <summary>
        /// Возвращает ошибку в формате для неинтерфейсных qp-action
        /// </summary>
        OperationAction,

        /// <summary>
        /// Возвращает ошибку в формате JSend
        /// </summary>
        JSendResponse
    }
}
