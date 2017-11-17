using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Audit
{
    public sealed class ActionLogAreaViewModel : AreaViewModel
    {
        public static ActionLogAreaViewModel Create(string tabId, int parentId) => Create<ActionLogAreaViewModel>(tabId, parentId);

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.ActionLog;

        public string GridElementId => UniqueId("Grid");

        public string FilterElementId => UniqueId("LogFilter");

        public IEnumerable<QPSelectListItem> ActionTypeList { get; internal set; }

        public IEnumerable<QPSelectListItem> ActionList { get; internal set; }

        public MvcHtmlString ActionTypeListJson
        {
            get
            {
                return MvcHtmlString.Create(
                    new JavaScriptSerializer().Serialize(
                        ActionTypeList.Select(at => new
                        {
                            value = at.Value,
                            text = at.Text
                        }).ToArray()
                    )
                );
            }
        }

        public MvcHtmlString ActionListJson
        {
            get
            {
                return MvcHtmlString.Create(
                    new JavaScriptSerializer().Serialize(
                        ActionList.Select(at => new
                        {
                            value = at.Value,
                            text = at.Text
                        }).ToArray()
                    )
                );
            }
        }

        public IEnumerable<QPSelectListItem> EntityTypeList
        {
            get;
            internal set;
        }

        public MvcHtmlString EntityTypeListJson
        {
            get
            {
                return MvcHtmlString.Create(
                    new JavaScriptSerializer().Serialize(
                        EntityTypeList.Select(at => new
                        {
                            value = at.Value,
                            text = at.Text
                        }).ToArray()
                    )
                );
            }
        }

        public IEnumerable<QPSelectListItem> FilterList => new[]
        {
            new QPSelectListItem{Text = AuditStrings.ActionName,Value = ((int)FilteredColumnsEnum.ActionName).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.ActionTypeName,Value = ((int)FilteredColumnsEnum.ActionTypeName).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.EntityTypeName,Value = ((int)FilteredColumnsEnum.EntityTypeName).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.EntityStringId,Value = ((int)FilteredColumnsEnum.EntityStringId).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.EntityTitle,Value = ((int)FilteredColumnsEnum.EntityTitle).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.ParentEntityId,Value = ((int)FilteredColumnsEnum.ParentEntityId).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.ExecutionTime,Value = ((int)FilteredColumnsEnum.ExecutionTime).ToString(),Selected = false},
            new QPSelectListItem{Text = AuditStrings.UserLogin,Value = ((int)FilteredColumnsEnum.UserLogin).ToString(),Selected = false}
        };
    }
}
