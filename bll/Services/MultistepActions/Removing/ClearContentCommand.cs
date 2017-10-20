using System;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Removing
{
    internal class ClearContentCommand : IMultistepActionStageCommand
    {
        private const int ItemsPerStep = 100;

        public int ContentId { get; }

        public int SiteId { get; }

        public int ItemCount { get; }

        public string ContentName { get; }

        public ClearContentCommand(MultistepActionStageCommandState state) : this(state.ParentId, state.Id, null, 0) { }

        public ClearContentCommand(int siteId, int contentId, string contentName, int itemCount)
        {
            SiteId = siteId;
            ContentId = contentId;
            ContentName = contentName;
            ItemCount = itemCount;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = RemovingStageCommandTypes.ClearContent,
            ParentId = SiteId,
            Id = ContentId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
            Name = string.Format(ContentStrings.ClearContentStageName, ContentName ?? string.Empty)
        };

        public bool IsLastStep(int step) => step == GetStageSettings().StepCount - 1;

        public MultistepActionStepResult Step(int step)
        {
            var content = ContentRepository.GetById(ContentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, ContentId));
            }

            if (!content.IsContentChangingActionsAllowed)
            {
                throw new ActionNotAllowedException(ContentStrings.ContentChangingIsProhibited);
            }

            ClearContentRepository.RemoveContentItems(ContentId, ItemsPerStep);
            if (IsLastStep(step))
            {
                ClearContentRepository.ClearO2MRelations(ContentId);
            }

            return new MultistepActionStepResult { ProcessedItemsCount = ItemsPerStep };
        }
    }
}
