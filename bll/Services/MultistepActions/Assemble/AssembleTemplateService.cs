#if !NET_STANDARD
using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    public sealed class AssembleTemplateService : MultistepActionServiceAbstract
    {
        private AssembleTemplateCommand templateCommand;
        private AssemblePagesCommand pagesCommand;
        private AssembleNotificationsCommand notificationsCommand;

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
                templateCommand = new AssembleTemplateCommand(templateId, template.Name);
                pagesCommand = new AssemblePagesCommand(templateId, template.Name, false, true);
                pagesCommand.Setup();

                notificationsCommand = new AssembleNotificationsCommand(templateId, template.Name, false);
            }
            else
            {
                pagesCommand = new AssemblePagesCommand(templateId, template.Name, false, false);
                pagesCommand.Setup();
            }

            return base.Setup(parentId, templateId, boundToExternal);
        }

        protected override MultistepActionSettings CreateActionSettings(int siteId, int templateId)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            if (site.IsDotNet)
            {
                return new MultistepActionSettings
                {
                    Stages = new[]
                    {
                        templateCommand.GetStageSettings(),
                        pagesCommand.GetStageSettings(),
                        notificationsCommand.GetStageSettings()
                    }
                };
            }

            return new MultistepActionSettings
            {
                Stages = new[]
                {
                    pagesCommand.GetStageSettings()
                }
            };
        }

        protected override MultistepActionServiceContext CreateContext(int siteId, int templateId, bool? boundToExternal)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            if (site.IsDotNet)
            {
                return new MultistepActionServiceContext
                {
                    CommandStates = new[]
                    {
                        templateCommand.GetState(),
                        pagesCommand.GetState(),
                        notificationsCommand.GetState()
                    }
                };
            }

            return new MultistepActionServiceContext
            {
                CommandStates = new[]
                {
                    pagesCommand.GetState()
                }
            };
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
