using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL
{
	public class WorkflowRule: EntityObject
	{
		public int? GroupId { get; set; }

		public int? UserId { get; set; }

		public int RuleOrder { get; set; }

		public int WorkflowId { get; set; }

		public int? PredecessorPermissionId { get; set; }
		
  		public int? SuccessorPermissionId { get; set; }

		public int? SuccessorStatusId { get; set; }

		public StatusType StatusType { get; set; }

		public bool IsInvalid { get; set; }
	}
}
