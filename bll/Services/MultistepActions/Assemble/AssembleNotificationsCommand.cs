using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using QA_Assembling;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
	internal class AssembleNotificationsCommand : IMultistepActionStageCommand
	{
		public bool SiteOrTemplate { get; private set; }
		public int AssemblingEntityId { get; private set; }
		public string AssemblingEntityName { get; private set; }

		public AssembleNotificationsCommand(MultistepActionStageCommandState state, bool siteOrTemplate) : this(state.Id, null, siteOrTemplate) { }

		public AssembleNotificationsCommand(int assemblingEntityId, string assemblingEntityName, bool siteOrTemplate)
		{
			SiteOrTemplate = siteOrTemplate;
			AssemblingEntityId = assemblingEntityId;
			AssemblingEntityName = assemblingEntityName;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = BuildSiteStageCommandTypes.BuildNotifications,
				ParentId = 0,
				Id = AssemblingEntityId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = 1,
				StepCount = 1,
				Name = SiteOrTemplate ? 
				String.Format(SiteStrings.AssembleNotificationStageName, (AssemblingEntityName ?? "")):
				String.Format(TemplateStrings.AssembleNotificationsStageName, (AssemblingEntityName ?? ""))
			};
		}

		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			IEnumerable<int> notificationIds = SiteOrTemplate ? AssembleRepository.GetSiteFormatId(AssemblingEntityId) : AssembleRepository.GetTemplateFormatId(AssemblingEntityId);
			if (notificationIds.Any())
			{
				foreach (int id in notificationIds)
				{
					new AssembleFormatController(id, AssembleMode.Notification, QPContext.CurrentCustomerCode).Assemble();
				}
			}
			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}

		#endregion
	}
}
