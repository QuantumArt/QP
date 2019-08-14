#if !NET_STANDARD
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    internal class AssembleTemplatesCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const int ItemsPerStep = 1;

        public int SiteId { get; }

        public string SiteName { get; }

        private int _itemCount;

        public AssembleTemplatesCommand(MultistepActionStageCommandState state)
            : this(state.Id, null)
        {
        }

        public AssembleTemplatesCommand(int siteId, string sitetName)
        {
            SiteId = siteId;
            SiteName = sitetName;
        }

        internal void Setup()
        {
            var templateIds = AssembleRepository.GetSiteTemplatesId(SiteId);
            _itemCount = templateIds.Count();

            HttpContext.Session.SetValue(
                HttpContextSession.AssembleTemplatesCommandProcessingContext,
                new AssembleTemplatesCommandContext
                {
                    TemplateIds = templateIds.ToArray()
                });
        }

        internal static void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.AssembleTemplatesCommandProcessingContext);
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = BuildSiteStageCommandTypes.BuildTemplates,
            ParentId = 0,
            Id = SiteId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = _itemCount,
            StepCount = MultistepActionHelper.GetStepCount(_itemCount, ItemsPerStep),
            Name = string.Format(SiteStrings.AssembleTemplatesStageName, SiteName ?? string.Empty)
        };

        public MultistepActionStepResult Step(int step)
        {
            var context = HttpContext.Session.GetValue<AssembleTemplatesCommandContext>(HttpContextSession.AssembleTemplatesCommandProcessingContext);

            IEnumerable<int> templateIds = context.TemplateIds
                .Skip(step * ItemsPerStep)
                .Take(ItemsPerStep)
                .ToArray();

            if (templateIds.Any())
            {
                foreach (var id in templateIds)
                {
                    new AssembleTemplateObjectsController(id, QPContext.CurrentDbConnectionString).Assemble();
                }
            }

            return new MultistepActionStepResult { ProcessedItemsCount = ItemsPerStep };
        }
    }

    [Serializable]
    public class AssembleTemplatesCommandContext
    {
        public int[] TemplateIds { get; set; }
    }
}
#endif
