using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Assembling;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
	internal class AssembleTemplateCommand: IMultistepActionStageCommand
	{
		public int TemplateId { get; private set; }
		public string TemplateName { get; private set; }

		public AssembleTemplateCommand(MultistepActionStageCommandState state) : this(state.Id, null) { }

		public AssembleTemplateCommand(int templateId, string templateName)
		{
			TemplateId = templateId;
			TemplateName = templateName;
		}

		public MultistepActionStageCommandState GetState()
		{
			return new MultistepActionStageCommandState
			{
				Type = BuildSiteStageCommandTypes.BuildTemplates,
				ParentId = 0,
				Id = TemplateId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = 1,
				StepCount = 1,
				Name = String.Format(TemplateStrings.AssembleTemplateStageName, (TemplateName ?? ""))
			};
		}
		
		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			PageTemplate template = PageTemplateRepository.GetPageTemplatePropertiesById(TemplateId);
			if (template == null)
				throw new ApplicationException(String.Format(TemplateStrings.TemplateNotFound, TemplateId));
			if (!template.SiteIsDotNet)
				throw new ApplicationException(String.Format(SiteStrings.ShouldBeDotNet));
			new AssembleTemplateObjectsController(TemplateId, QPContext.CurrentDbConnectionString).Assemble();

			return new MultistepActionStepResult { ProcessedItemsCount = 1 };
		}

		#endregion
	}
}
