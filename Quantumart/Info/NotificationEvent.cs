namespace Quantumart.QPublishing.Info
{
    public class NotificationEvent
    {
        public static readonly string Create = "for_create";
        public static readonly string Modify = "for_modify";
        public static readonly string Remove = "for_remove";
        public static readonly string StatusChanged = "for_status_changed";
        public static readonly string StatusPartiallyChanged = "for_status_partially_changed";
        public static readonly string FrontendRequest = "for_frontend";
        public static readonly string DelayedPublication = "for_delayed_publication";
    }
}
