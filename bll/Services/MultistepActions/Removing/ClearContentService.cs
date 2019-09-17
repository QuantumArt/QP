#if !NET_STANDARD
using System;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    public sealed class ClearContentService : RemovingServiceAbstract
    {
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

            Commands.Add(new ClearContentCommand(siteId, contentId, contentName, itemCount));
            return base.Setup(siteId, contentId, boundToExternal);
        }

    }
}
#endif
