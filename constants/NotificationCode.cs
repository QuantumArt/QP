namespace Quantumart.QP8.Constants
{
    /// <summary>
    /// Коды уведомлений
    /// </summary>
    public static class NotificationCode
    {
        public const string Create = "for_create";
        public const string Update = "for_modify";
        public const string Delete = "for_remove";
        public const string ChangeStatus = "for_status_changed";
        public const string PartialChangeStatus = "for_status_partially_changed";
        public const string Custom = "for_frontend";
        public const string DelayedPublication = "for_delayed_publication";
    }
}
