using System;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    public sealed class ClearContentService : RemovingServiceAbstract
    {
        private ClearContentCommand _command;

        public override MessageResult PreAction(int siteId, int contentId)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                return MessageResult.Info(ContentStrings.OperationIsNotAllowedForAggregated);
            }

            if (ContentRepository.GetAggregatedContents(contentId).Any())
            {
                return MessageResult.Info(ContentStrings.OperationIsNotAllowedForRoot);
            }

            return base.PreAction(siteId, contentId);
        }

        public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
        {
            if (ContentRepository.IsAnyAggregatedFields(contentId))
            {
                throw new ActionNotAllowedException(ContentStrings.OperationIsNotAllowedForAggregated);
            }

            if (ContentRepository.GetAggregatedContents(contentId).Any())
            {
                throw new ActionNotAllowedException(ContentStrings.OperationIsNotAllowedForRoot);
            }

            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            if (!content.IsContentChangingActionsAllowed)
            {
                throw new ActionNotAllowedException(ContentStrings.ContentChangingIsProhibited);
            }

            var row = ClearContentRepository.GetContentItemsInfo(contentId);
            var contentName = string.Empty;
            var itemCount = 0;
            if (row != null)
            {
                itemCount = row.Field<int>("ITEMS_COUNT");
                contentName = row.Field<string>("CONTENT_NAME");
            }

            _command = new ClearContentCommand(siteId, contentId, contentName, itemCount);
            return base.Setup(siteId, contentId, boundToExternal);
        }

        protected override MultistepActionSettings CreateActionSettings(int parentId, int id)
        {
            var stageSetting = _command.GetStageSettings();
            return new MultistepActionSettings { Stages = new[] { stageSetting } };
        }

        protected override MultistepActionServiceContext CreateContext(int parentId, int id, bool? boundToExternal)
        {
            var commandState = _command.GetState();
            return new MultistepActionServiceContext { CommandStates = new[] { commandState } };
        }
    }
}
