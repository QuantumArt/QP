using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    public sealed class AssembleSiteService : MultistepActionServiceAbstract
    {
        public override MessageResult PreAction(int dbId, int siteId)
        {
            var site = GetSite(siteId);

            if (site.IsLive)
            {
                return MessageResult.Confirm(SiteStrings.AssembleLiveSiteConfirmation, new[] { siteId });
            }

            return null;
        }

        private CreateFoldersCommand createFoldersCommand;
        private AssembleNotificationsCommand notificationCommand;
        private AssemblePagesCommand pagesCommand;
        private AssembleTemplatesCommand templatesCommand;

        public override MultistepActionSettings Setup(int dbId, int siteId, bool? boundToExternal)
        {
            var site = GetSite(siteId);

            if (site.IsDotNet)
            {
                createFoldersCommand = new CreateFoldersCommand(siteId, site.Name);
                notificationCommand = new AssembleNotificationsCommand(siteId, site.Name, true);

                pagesCommand = new AssemblePagesCommand(siteId, site.Name, true, true);
                pagesCommand.Setup();

                templatesCommand = new AssembleTemplatesCommand(siteId, site.Name);
                templatesCommand.Setup();
            }
            else
            {
                pagesCommand = new AssemblePagesCommand(siteId, site.Name, true, false);
                pagesCommand.Setup();
            }

            return base.Setup(dbId, siteId, boundToExternal);
        }

        protected override MultistepActionSettings CreateActionSettings(int dbId, int siteId)
        {
            var site = GetSite(siteId);

            if (site.IsDotNet)
            {
                return new MultistepActionSettings
                {
                    Stages = new[]
                    {
                        createFoldersCommand.GetStageSettings(),
                        templatesCommand.GetStageSettings(),
                        pagesCommand.GetStageSettings(),
                        notificationCommand.GetStageSettings()
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

        protected override MultistepActionServiceContext CreateContext(int dbId, int siteId, bool? boundToExternal)
        {
            var site = GetSite(siteId);

            if (site.IsDotNet)
            {
                return new MultistepActionServiceContext
                {
                    CommandStates = new[]
                    {
                        createFoldersCommand.GetState(),
                        templatesCommand.GetState(),
                        pagesCommand.GetState(),
                        notificationCommand.GetState()
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
            var site = SiteRepository.GetById(state.Id);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, state.Id));
            }

            switch (state.Type)
            {
                case BuildSiteStageCommandTypes.CreateFolders:
                    return new CreateFoldersCommand(state);
                case BuildSiteStageCommandTypes.BuildTemplates:
                    return new AssembleTemplatesCommand(state);
                case BuildSiteStageCommandTypes.BuildPages:
                    return new AssemblePagesCommand(state, true, site.IsDotNet);
                case BuildSiteStageCommandTypes.BuildNotifications:
                    return new AssembleNotificationsCommand(state, true);
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

        private Site GetSite(int siteId)
        {
            var site = SiteRepository.GetById(siteId);
            if (site == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            return site;
        }
    }

    internal class BuildSiteStageCommandTypes
    {
        public const int CreateFolders = 1;
        public const int BuildTemplates = 2;
        public const int BuildPages = 3;
        public const int BuildNotifications = 4;
    }
}
