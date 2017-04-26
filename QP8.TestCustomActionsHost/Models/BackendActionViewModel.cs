namespace QP8.TestCustomActionsHost.Models
{
    public class BackendActionViewModel
    {

        public BackendActionViewModel()
        {
            Title = "Test integration";
            ActionCode = "edit_article";
            EntityTypeCode = "article";
            EntityId = 106401;
            ParentEntityId = 339;
        }

        public string HostUid { get; set; }

        public string Title { get; set; }

        public string ActionCode { get; set; }

        public string EntityTypeCode { get; set; }

        public int EntityId { get; set; }

        public int ParentEntityId { get; set; }
    }
}
