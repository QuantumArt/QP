using System;
using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    public sealed class RemoveContentService : RemovingServiceAbstract
    {
        public override MultistepActionSettings Setup(int siteId, int contentId, bool? boundToExternal)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new ApplicationException(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            if (!content.IsAccessible(ActionTypeCode.Remove))
            {
                throw new ApplicationException(ArticleStrings.CannotRemoveBecauseOfSecurity);
            }

            var violationMessages = new List<string>();
            content.ValidateForRemove(violationMessages);
            if (violationMessages.Count > 0)
            {
                throw new ApplicationException(string.Join(Environment.NewLine, violationMessages));
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
            Commands.Add(new RemoveContentCommand(siteId, contentId, contentName));
            return base.Setup(siteId, contentId, boundToExternal);
        }
    }
}
