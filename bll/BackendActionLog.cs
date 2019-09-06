using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Запись лога выполнения Action
    /// </summary>
    public class BackendActionLog
    {
        #region DB Properties

        public int Id { get; set; }
        public int? UserId { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "ExecutionTime", ResourceType = typeof(AuditStrings))]
        public DateTime ExecutionTime { get; set; }

        public string ActionTypeCode { get; set; }
        public string ActionCode { get; set; }
        public string EntityTypeCode { get; set; }
        public int? EntityId { get; set; }

        [Display(Name = "IsApi", ResourceType = typeof(AuditStrings))]
        public bool IsApi { get; set; }

        [Display(Name = "EntityStringId", ResourceType = typeof(AuditStrings))]
        public string EntityStringId { get; set; }

        [Display(Name = "ParentEntityId", ResourceType = typeof(AuditStrings))]
        public int? ParentEntityId { get; set; }

        [Display(Name = "EntityTitle", ResourceType = typeof(AuditStrings))]
        public string EntityTitle { get; set; }

        #endregion

        #region Text Properties

        [Display(Name = "UserLogin", ResourceType = typeof(AuditStrings))]
        public string UserLogin { get; set; }

        [Display(Name = "ActionTypeName", ResourceType = typeof(AuditStrings))]
        public string ActionTypeName { get; set; }

        [Display(Name = "ActionName", ResourceType = typeof(AuditStrings))]
        public string ActionName { get; set; }

        [Display(Name = "EntityTypeName", ResourceType = typeof(AuditStrings))]
        public string EntityTypeName { get; set; }

        #endregion

        public static IEnumerable<BackendActionLog> CreateLogs(BackendActionContext actionContext, IBackendActionLogRepository repository)
        {
            var ids = actionContext.Entities.Where(ent => ent.Id.HasValue).Select(ent => ent.Id.Value).ToArray();
            var titles = repository.GetEntityTitles(actionContext.EntityTypeCode, actionContext.ParentEntityId, ids).ToDictionary(n => n.Value, m => m.Text);

            return actionContext.Entities
                .Select(ent => new BackendActionLog
                {
                    EntityId = ent.Id,
                    EntityStringId = ent.StringId,
                    EntityTitle = ent.Id.HasValue && titles.ContainsKey(ent.Id.Value.ToString())
                        ? titles[ent.Id.Value.ToString()].Left(255)
                        : null,
                    ActionCode = actionContext.ActionCode,
                    ActionTypeCode = actionContext.ActionTypeCode,
                    EntityTypeCode = actionContext.EntityTypeCode,
                    ParentEntityId = actionContext.ParentEntityId,
                    ExecutionTime = DateTime.Now,
                    UserId = QPContext.CurrentUserId
                })
                .ToArray();
        }
    }
}
