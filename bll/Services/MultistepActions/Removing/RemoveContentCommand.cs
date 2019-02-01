#if !NET_STANDARD
using System;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    /// <summary>
    /// Команда этапа удаления контента
    /// </summary>
    internal class RemoveContentCommand : IMultistepActionStageCommand
    {
        public int ContentId { get; }

        public int SiteId { get; }

        public string ContentName { get; }

        public RemoveContentCommand(MultistepActionStageCommandState state)
            : this(state.ParentId, state.Id, null)
        {
        }

        public RemoveContentCommand(int siteId, int contentId, string contentName)
        {
            SiteId = siteId;
            ContentId = contentId;
            ContentName = contentName;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = RemovingStageCommandTypes.RemoveContent,
            ParentId = SiteId,
            Id = ContentId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = 1,
            StepCount = 1,
            Name = string.Format(ContentStrings.RemoveContentStageName, ContentName ?? string.Empty)
        };

        public MultistepActionStepResult Step(int step)
        {
            var content = ContentRepository.GetById(ContentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, ContentId));
            }

            content.DieWithoutValidation();
            return new MultistepActionStepResult { ProcessedItemsCount = 1 };
        }
    }
}
#endif
