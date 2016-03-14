using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Данные описывающие астосохраненную сущьность
	/// </summary>
	public class AutosavedEntityRecordHeader
	{
		public long RecordId { get; set; }
		public string ActionCode { get; set; }
		public string EntityTypeCode { get; set; }
		public int EntityId { get; set; }
		public int ParentEntityId { get; set; }
		public string ParentEntityTypeCode { get; set; }
		public long? ModifiedTicks { get; set; }

		public DateTime? Modified 
		{ 
			get 
			{ 
				return ModifiedTicks.HasValue ? new DateTime(ModifiedTicks.Value) : new Nullable<DateTime>(); 
			} 
		}
		public bool IsNew { get { return (EntityId == 0); } }
	}
}
