using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Rebuild
{
    public sealed class RebuildVirtualContentsService : MultistepActionServiceAbstract
    {

        public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new ApplicationException(string.Format(SiteStrings.SiteNotFound, siteId));
            }

            List<Content.TreeItem> rebuildedViewSubContents;
            var helper = new VirtualContentHelper();
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(contentId))
            {
                rebuildedViewSubContents = helper.TraverseForUpdateVirtualSubContents(content);
            }

            var rebuildViewsCommand = new RebuildVirtualContentViewsCommand(contentId, content.Name, rebuildedViewSubContents);
            rebuildViewsCommand.Setup();
            Commands.Add(rebuildViewsCommand);

            var rebuildUserQueryCommand = new RebuildUserQueryCommand(contentId, content.Name, rebuildedViewSubContents);
            rebuildUserQueryCommand.Setup();
            Commands.Add(rebuildUserQueryCommand);

            return base.Setup(siteId, contentId, boundToExternal);
        }
        protected override string ContextSessionKey => HttpContextSession.RebuildUserQueryProcessingContext;

        protected override IMultistepActionStageCommand CreateCommand(MultistepActionStageCommandState state)
        {
            switch (state.Type)
            {
                case RebuildVirtualContentsCommandTypes.RebuildViews:
                    return new RebuildVirtualContentViewsCommand(state);
                case RebuildVirtualContentsCommandTypes.RebuildUserQueries:
                    return new RebuildUserQueryCommand(state);
                default:
                    throw new ApplicationException("Undefined Rebuilding Virtual Contents Stage Command Type: " + state.Type);
            }
        }

        public override void TearDown()
        {
            RebuildVirtualContentViewsCommand.TearDown();
            RebuildUserQueryCommand.TearDown();
            base.TearDown();
        }
    }

    internal class RebuildVirtualContentsCommandTypes
    {
        public const int RebuildViews = 1;
        public const int RebuildUserQueries = 2;
    }
}
