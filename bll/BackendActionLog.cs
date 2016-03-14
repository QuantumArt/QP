using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL.Repository;
using System.Data;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;

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
		[LocalizedDisplayName("ExecutionTime", NameResourceType = typeof(AuditStrings))]
		public DateTime ExecutionTime { get; set; }
		public string ActionTypeCode { get; set; }
		public string ActionCode { get; set; }
		public string EntityTypeCode { get; set; }
		public int? EntityId { get; set; }
		[LocalizedDisplayName("IsApi", NameResourceType = typeof(AuditStrings))]
		public bool IsApi { get; set; }
		[LocalizedDisplayName("EntityStringId", NameResourceType = typeof(AuditStrings))]
		public string EntityStringId { get; set; }
		[LocalizedDisplayName("ParentEntityId", NameResourceType = typeof(AuditStrings))]
		public int? ParentEntityId { get; set; }
		[LocalizedDisplayName("EntityTitle", NameResourceType = typeof(AuditStrings))]
		public string EntityTitle { get; set; } 
		#endregion	
	
		#region Text Properties	
		[LocalizedDisplayName("UserLogin", NameResourceType = typeof(AuditStrings))]
		public string UserLogin { get; set; }
		[LocalizedDisplayName("ActionTypeName", NameResourceType = typeof(AuditStrings))]
		public string ActionTypeName { get; set; }
		[LocalizedDisplayName("EntityTypeName", NameResourceType = typeof(AuditStrings))]
		public string EntityTypeName { get; set; } 
		#endregion
	
		public static IEnumerable<BackendActionLog> CreateLogs(BackendActionContext actionContext, IBackendActionLogRepository repository)
		{
			var titles = repository.GetEntityTitles(actionContext.EntityTypeCode,
							 actionContext.Entities
								 .Where(ent => ent.Id.HasValue)
								 .Select(ent => ent.Id.Value)
								 .ToArray()
					 )
					 .Select(r => new
					 {
						 Id = Converter.ToNullableInt32(r["ID"]),
						 Title = r["TITLE"].ToString()
					 })
					 .ToDictionary(r => r.Id, r => r.Title);
			

			return actionContext.Entities
					.Select(ent => new BackendActionLog 
					{
						EntityId = ent.Id,
						EntityStringId = ent.StringId,
						EntityTitle = ent.Id.HasValue && titles.ContainsKey(ent.Id.Value)
							? titles[ent.Id.Value] 
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
