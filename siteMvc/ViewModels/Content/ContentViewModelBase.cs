using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
    public abstract class ContentViewModelBase : EntityViewModel
    {
        public readonly string ClassBlock = "class";

        public new BLL.Content Data
        {
            get => (BLL.Content)EntityData;
            set => EntityData = value;
        }

        public List<ListItem> Groups
        {
            get { return Data.Site.ContentGroups.Select(n => new ListItem(n.Id.ToString(), n.Name)).ToList(); }
        }

        public bool GroupChanged { get; set; }

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentOptions;
                result.groupChanged = GroupChanged;
                return result;
            }
        }

        public IEnumerable<ListItem> GetContentsForParent()
        {
            var contents = ContentService.GetContentsForParentContent(Data.SiteId, Data.Id).ToArray();
            return new[] { new ListItem(string.Empty, ContentStrings.SelectParentContent) }.Concat(contents);
        }

        public SelectOptions SelectParentOptions => new SelectOptions { ReadOnly = !Data.IsNew };

        public SelectOptions SelectGroupOptions => new SelectOptions
        {
            EntityDataListArgs = new EntityDataListArgs
            {
                EntityTypeCode = Constants.EntityTypeCode.ContentGroup,
                ParentEntityId = Data.SiteId,
                EntityId = Data.Id,
                AddNewActionCode = Constants.ActionCode.AddNewContentGroup,
                ReadActionCode = Constants.ActionCode.ContentGroupProperties
            }
        };
    }
}
