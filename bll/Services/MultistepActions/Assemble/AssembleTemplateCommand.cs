using System;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    internal class AssembleTemplateCommand : IMultistepActionStageCommand
    {
        public int TemplateId { get; }
        public string TemplateName { get; }

        public AssembleTemplateCommand(MultistepActionStageCommandState state)
            : this(state.Id, null)
        {
        }

        public AssembleTemplateCommand(int templateId, string templateName)
        {
            TemplateId = templateId;
            TemplateName = templateName;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = BuildSiteStageCommandTypes.BuildTemplates,
            ParentId = 0,
            Id = TemplateId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = 1,
            StepCount = 1,
            Name = string.Format(TemplateStrings.AssembleTemplateStageName, TemplateName ?? "")
        };

        #region IMultistepActionStageCommand Members

        public MultistepActionStepResult Step(int step)
        {
            var template = PageTemplateRepository.GetPageTemplatePropertiesById(TemplateId);
            if (template == null)
            {
                throw new ApplicationException(string.Format(TemplateStrings.TemplateNotFound, TemplateId));
            }
            if (!template.SiteIsDotNet)
            {
                throw new ApplicationException(string.Format(SiteStrings.ShouldBeDotNet));
            }

            new AssembleTemplateObjectsController(TemplateId, QPContext.CurrentDbConnectionString).Assemble();

            return new MultistepActionStepResult { ProcessedItemsCount = 1 };
        }

        #endregion
    }
}
