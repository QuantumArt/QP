namespace Quantumart.QP8.BLL.Helpers
{
	public class WorkflowRuleItem : WorkflowRuleSimpleItem
	{		
		public int StId{get;set;}

		public string RadioChecked{get;set;}				

		public string Description { get; set;}

		public int? Id { get; set; }

		public int Weight { get; set; }
	}

	public class WorkflowRuleSimpleItem
	{
		public string StName { get; set; }

		public int? UserId { get; set; }

		public int? GroupId { get; set; }
	}
}
