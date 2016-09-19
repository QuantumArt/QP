using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class ContentViewModelBase : EntityViewModel
    {
        public readonly string ClassBlock = "class";

        public new BLL.Content Data
        {
            get
            {
                return (BLL.Content)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        public List<ListItem> Groups
        {
            get
            {
                return Data.Site.ContentGroups.Select(n => new ListItem(n.Id.ToString(), n.Name)).ToList();
            }
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

        /// <summary>
        /// Возвращает список контентов доступных для выбора в качестве родительского
        /// </summary>
        public IEnumerable<ListItem> GetContentsForParent()
        {
            var contents = ContentService.GetContentsForParentContent(Data.SiteId, Data.Id).ToArray();
            return new[] { new ListItem("", ContentStrings.SelectParentContent) }.Concat(contents);
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
