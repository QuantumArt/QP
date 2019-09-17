#if !NET_STANDARD
using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    public sealed class AssembleTemplateService : MultistepActionServiceAbstract
    {

        public override MessageResult PreAction(int parentId, int templateId)
        {
            var site = SiteRepository.GetById(parentId);
            if (site == null)
            {
                return MessageResult.Error(string.Format(SiteStrings.SiteNotFound, parentId), new[] { parentId });
            }
            if (site.IsLive)
            {
                return MessageResult.Confirm(TemplateStrings.AssembleLiveSiteTemplateConfirmation, new[] { parentId });
            }

            return null;
        }

        public override MultistepActionSettings Setup(int parentId, int templateId, bool? boundToExternal)
        {
            var site = SiteRepository.GetById(parentId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, parentId));
            }

            var template = PageTemplateRepository.GetPageTemplatePropertiesById(templateId);

            if (site.IsDotNet)
            {
                var templateCommand = new AssembleTemplateCommand(templateId, template.Name);
                Commands.Add(templateCommand);
                var pagesCommand = new AssemblePagesCommand(templateId, template.Name, false, true);
                pagesCommand.Setup();
                Commands.Add(pagesCommand);

                var notificationsCommand = new AssembleNotificationsCommand(templateId, template.Name, false);
                Commands.Add(notificationsCommand);
            }
            else
            {
                var pagesCommand = new AssemblePagesCommand(templateId, template.Name, false, false);
                pagesCommand.Setup();
                Commands.Add(pagesCommand);
            }

            return base.Setup(parentId, templateId, boundToExternal);
        }

        protected override string ContextSessionKey => "BuildSiteService.ProcessingContext";

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            var site = SiteRepository.GetByTemplateId(state.Id);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, state.ParentId));
            }

            switch (state.Type)
            {
                case BuildSiteStageCommandTypes.BuildTemplates:
                    return new AssembleTemplateCommand(state);
                case BuildSiteStageCommandTypes.BuildPages:
                    return new AssemblePagesCommand(state, false, site.IsDotNet);
                case BuildSiteStageCommandTypes.BuildNotifications:
                    return new AssembleNotificationsCommand(state, false);
                default:
                    throw new ApplicationException("Undefined Site Building Stage Command Type: " + state.Type);
            }
        }

        public override void TearDown()
        {
            AssembleTemplatesCommand.TearDown();
            AssemblePagesCommand.TearDown();
            base.TearDown();
        }
    }
}
#endif
