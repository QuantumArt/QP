namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
    public sealed class RemovedEntitiesAreaViewModel : AreaViewModel
    {
        public static RemovedEntitiesAreaViewModel Create(string tabId, int parentId)
        {
            var model = Create<RemovedEntitiesAreaViewModel>(tabId, parentId);
            return model;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.RemovedEntities;

        public string GridElementId => UniqueId("Grid");
    }
}
