using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.WebMvc.Infrastructure.Models
{
    public class XmlDbUpdateRecordedAction
    {
        private readonly InitPropertyValue<BackendAction> _backendAction;

        public XmlDbUpdateRecordedAction()
        {
            _backendAction = new InitPropertyValue<BackendAction>(() => BackendActionService.GetByCode(Code));
        }

        public string[] Ids { get; set; }

        public int ParentId { get; set; }

        public int ResultId { get; set; }

        public Guid[] UniqueId { get; set; }

        public Guid ResultUniqueId { get; set; }

        public int ChildId { get; set; }

        public string ChildIds { get; set; }

        public string ChildLinkIds { get; set; }

        public string VirtualFieldIds { get; set; }

        public int BackwardId { get; set; }

        public int Lcid { get; set; }

        public DateTime Executed { get; set; }

        public string ExecutedBy { get; set; }

        public string Code { get; set; }

        public string CustomActionCode { get; set; }

        public Dictionary<string, StringValues> Form { get; set; }

        public BackendAction BackendAction => _backendAction.Value;

        public int ActionId { get; set; }

        public string ActionCode { get; set; }

        public string FieldIds { get; set; }

        public string LinkIds { get; set; }

        public int NewLinkId { get; set; }

        public string NewChildFieldIds { get; set; }

        public string NewChildLinkIds { get; set; }

        public string NewCommandIds { get; set; }

        public string NewRulesIds { get; set; }

        public int NotificationFormatId { get; set; }

        public int DefaultFormatId { get; set; }

        public int UserId { get; set; }

        public int GroupId { get; set; }
    }
}
