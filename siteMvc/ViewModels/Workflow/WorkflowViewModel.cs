using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using C = Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Helpers;


namespace Quantumart.QP8.WebMvc.ViewModels.Workflow
{
	public class WorkflowViewModel : EntityViewModel
	{
		private IWorkflowService _service;
		public override string EntityTypeCode
		{
			get { return C.EntityTypeCode.Workflow; }
		}

		public override string ActionCode
		{
			get { return IsNew ? C.ActionCode.AddNewWorkflow : C.ActionCode.WorkflowProperties; }
		}

		public new Quantumart.QP8.BLL.Workflow Data
		{
			get
			{
				return (Quantumart.QP8.BLL.Workflow)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		[LocalizedDisplayName("Contents", NameResourceType = typeof(WorkflowStrings))]
		public IEnumerable<int> ActiveContentsIds { get; set; }

		[LocalizedDisplayName("Contents", NameResourceType = typeof(WorkflowStrings))]
		public IEnumerable<ListItem> ActiveContentsListItems { get; set; }

		public static WorkflowViewModel Create(Quantumart.QP8.BLL.Workflow workflow, string tabId, int parentId, IWorkflowService service)
		{			
			var model = EntityViewModel.Create<WorkflowViewModel>(workflow, tabId, parentId);
			model._service = service;
			return model.Init();
		}

		public EntityDataListArgs EntityDataListArgs { get; set; }

		public override void Validate(ModelStateDictionary modelState)
		{
			modelState.Clear();
			if(ActiveStatuses.Count == 0)
				modelState.AddModelError("ActiveStatuses", WorkflowStrings.StatusNotSelected);
			base.Validate(modelState);
		}

		private InitPropertyValue<IEnumerable<Quantumart.QP8.BLL.StatusType>> _statuses;

		private IEnumerable<Quantumart.QP8.BLL.StatusType> Statuses
		{
			get { return _statuses.Value.OrderBy(x => x.Weight); }
			set { _statuses.Value = value; }
		}


		public string StatusWeightsDictionary
		{
			get { return new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(Statuses.ToDictionary(x => x.Id.ToString(), x => x.Weight)); }
		}

		public IEnumerable<ListItem> AllStatusListItems
		{
			get 
			{
			return Statuses.Where(s => s.Weight > 0).Select(s => new ListItem
				{
					Value = s.Id.ToString(),
					Text = s.Name
				}).ToArray();
			}
		}

		public string Workflows_WorkflowRulesDisplay { get; set; }

		[LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
		public IList<QPCheckedItem> ActiveStatuses { get; set; }

		private InitPropertyValue<IEnumerable<Quantumart.QP8.BLL.Content>> _contents;

		public IEnumerable<Quantumart.QP8.BLL.Content> Contents
		{
			get { return _contents.Value; }
			set { _contents.Value = value; }
		}

		public IEnumerable<object> WorkflowRulesDisplay
		{
			get
			{				
				return Data.WorkflowRules.OrderBy(x => x.StatusType.Weight).Select(x => new {Id = x.Id, StName = x.StatusType != null ? x.StatusType.Name : string.Empty,
					StId = x.SuccessorStatusId, RadioChecked = x.UserId.HasValue? "User": "Group", x.UserId, x.GroupId, x.Description, Weight = x.StatusType.Weight, Invalid = x.IsInvalid					
				});
			}
		}

		[LocalizedDisplayName("User", NameResourceType = typeof(WorkflowStrings))]
		public string O2MDefaultValue { get; set; }

		public QPSelectListItem DefaultValueListItem
		{
			get
			{
				return O2MDefaultValue != null ?
					new QPSelectListItem { Value = "0", Text = "Select User", Selected = true } :
					null;
			}
		}

		private WorkflowViewModel Init()
		{
			_statuses = new InitPropertyValue<IEnumerable<Quantumart.QP8.BLL.StatusType>>(() => _service.GetStatusTypesBySiteId(Data.SiteId));
			_contents = new InitPropertyValue<IEnumerable<Quantumart.QP8.BLL.Content>>(() => _service.GetContentsBySiteId(Data.SiteId));

			ActiveContentsIds = Data.Id == 0 ? new List<int>() : _service.GetBindedContetnsIds(Data.Id);
			ActiveContentsListItems = Contents.Where(x => ActiveContentsIds.Contains(x.Id))
				.Select(x => new ListItem { Value = x.Id.ToString(), Selected = true, Text = x.Name });
			ActiveStatuses = Data.WorkflowRules != null ? Data.WorkflowRules.Select(x => new QPCheckedItem { Value = x.StatusType.Id.ToString() }).ToList()
				: new List<QPCheckedItem>();

			EntityDataListArgs = new EntityDataListArgs
			{
				EntityTypeCode = C.EntityTypeCode.Content,
				ParentEntityId = Data.SiteId,
				SelectActionCode = C.ActionCode.MultipleSelectContentForWorkflow,
				Filter = "cwb.workflow_id is null or cwb.workflow_id = " + Data.Id,
				ListId = -1 * System.DateTime.Now.Millisecond,
				MaxListHeight = 200,
				MaxListWidth = 350,
				ShowIds = true
			};
			return this;
		}

		private List<WorkflowRuleItem> _WorkflowRules;

		internal void DoCustomBinding()
		{
			_WorkflowRules = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<List<WorkflowRuleItem>>(Workflows_WorkflowRulesDisplay);			
			Data.DoCustomBinding(_WorkflowRules);
		}
	}
}