using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Base
{
    public abstract class MultistepActionStageCommandBase : IMultistepActionStageCommand
    {
        protected static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        public int ContentId { get; private set; }

        public int ItemCount { get; private set; }

        public int[] Ids { get; private set; }

        public bool? BoundToExternal { get; private set; }

        public int ItemsPerStep { get; private set; }

        public S3Options S3Options { get; private set; }

        public List<MessageResult> Messages { get; }

        protected MultistepActionStageCommandBase()
        {
            Messages = HttpContext.Session.GetValue<List<MessageResult>>(HttpContextSession.MultistepActionStageCommandSettings) ?? new List<MessageResult>();
            S3Options = new S3Options();
        }

        protected MultistepActionStageCommandBase(int contentId, int itemCount, int[] ids)
        {
            ContentId = contentId;
            Ids = ids;
            ItemCount = itemCount;
        }

        public void Initialize(MultistepActionStageCommandState state)
        {
            ContentId = state.ParentId;
            ItemCount = state.Ids.Count;
            Ids = state.Ids.ToArray();
            BoundToExternal = state.BoundToExternal;
            ItemsPerStep = state.ItemsPerStep;
            S3Options = state.S3Options;
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            ParentId = ContentId,
            Id = 0,
            Ids = Ids.ToList(),
            ExtensionContentIds = ContentRepository.GetReferencedAggregatedContentIds(ContentId, Ids).ToList(),
            BoundToExternal = BoundToExternal,
            ItemsPerStep = ItemsPerStep,
            S3Options = S3Options
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = ItemCount,
            StepCount = MultistepActionHelper.GetStepCount(ItemCount, ItemsPerStep),
            Name = MultistepActionStrings.ResourceManager.GetString(GetType().Name)
        };

        public MultistepActionStepResult Step(int step)
        {
            var ids = Ids.Skip(step * ItemsPerStep).Take(ItemsPerStep).ToArray();
            var result = Step(ids);
            if (result != null)
            {
                Messages.Add(result);
                HttpContext.Session.SetValue(HttpContextSession.MultistepActionStageCommandSettings, Messages);
            }

            string additionalInfo = null;
            if (Messages.Any())
            {
                additionalInfo = string.Join("; ", Messages.Select(m => m.Text));
            }

            return new MultistepActionStepResult { ProcessedItemsCount = ids.Length, AdditionalInfo = additionalInfo };
        }

        protected abstract MessageResult Step(int[] ids);

        public static void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.MultistepActionStageCommandSettings);
        }
    }
}
