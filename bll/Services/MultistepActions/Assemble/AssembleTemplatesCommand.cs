using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Assembling;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
	internal class AssembleTemplatesCommand : IMultistepActionStageCommand
	{
		private static readonly int ITEMS_PER_STEP = 1;
		private static readonly string SESSION_KEY = "AssembleTemplatesCommand.ProcessingContext";

		public int SiteId { get; private set; }
		public string SiteName { get; private set; }

		private int itemCount = 0;

		public AssembleTemplatesCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

		public AssembleTemplatesCommand(int siteId, string sitetName)
		{
			SiteId = siteId;
			SiteName = sitetName;
		}

		internal void Setup()
		{
			IEnumerable<int> templateIds = AssembleRepository.GetSiteTemplatesId(SiteId);
			itemCount = templateIds.Count();
			HttpContext.Current.Session[SESSION_KEY] = new AssembleTemplatesCommandContext
			{
				TemplateIds = templateIds.ToArray()
			};
		}

		internal static void TearDown()
		{
			HttpContext.Current.Session[SESSION_KEY] = null;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = BuildSiteStageCommandTypes.BuildTemplates,
				ParentId = 0,
				Id = SiteId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = itemCount,
				StepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP),
				Name = String.Format(SiteStrings.AssembleTemplatesStageName, (SiteName ?? ""))
			};
		}

		

		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			AssembleTemplatesCommandContext context = HttpContext.Current.Session[SESSION_KEY] as AssembleTemplatesCommandContext;
			IEnumerable<int> templateIds = context.TemplateIds
				.Skip(step * ITEMS_PER_STEP)
				.Take(ITEMS_PER_STEP)
				.ToArray();
			if(templateIds.Any())
			{
				foreach(int id in templateIds)
				{
					new AssembleTemplateObjectsController(id, QPContext.CurrentCustomerCode).Assemble();
				}
			}
			return new MultistepActionStepResult { ProcessedItemsCount = ITEMS_PER_STEP };
		}

		#endregion

		
	}

	[Serializable]
	public class AssembleTemplatesCommandContext
	{
		public int[] TemplateIds { get; set; }
	}

}
