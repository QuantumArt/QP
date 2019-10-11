using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.Assembling;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
    internal class AssemblePagesCommand : IMultistepActionStageCommand
    {
        private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

        private const string PageTemplate = "\"{0}/{1}\"";
        private const int ItemsPerStep = 5;

        public int AssemblingEntityIdId { get; }

        public string AssemblingEntityName { get; }

        public bool SiteOrTemplate { get; }

        public bool IsDotNet { get; }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public IQP7Service QP7Service { get; }

        private int _itemCount;

        public AssemblePagesCommand(MultistepActionStageCommandState state, bool siteOrTemplate, bool isDotNet)
            : this(state.Id, null, siteOrTemplate, isDotNet)
        {
        }

        public AssemblePagesCommand(int siteId, string sitetName, bool siteOrTemplate, bool isDotNet)
        {
            AssemblingEntityIdId = siteId;
            AssemblingEntityName = sitetName;
            SiteOrTemplate = siteOrTemplate;
            IsDotNet = isDotNet;
            QP7Service = new QP7Service();
        }

        internal void Setup()
        {
            IEnumerable<PageInfo> pagesIds = SiteOrTemplate ? AssembleRepository.GetSitePages(AssemblingEntityIdId) : AssembleRepository.GetTemplatePages(AssemblingEntityIdId);
            _itemCount = pagesIds.Count();

            QP7Token token = null;
            if (!IsDotNet)
            {
                token = QP7Service.Authenticate();
            }

            HttpContext.Session.SetValue(
                HttpContextSession.AssemblePagesCommandProcessingContext,
                new AssemblePagesCommandContext
                {
                    Pages = pagesIds.ToArray(),
                    QP7Token = token,
                    MissedPages = new Dictionary<string, List<PageInfo>>()
                });
        }

        internal static void TearDown()
        {
            HttpContext.Session.Remove(HttpContextSession.AssemblePagesCommandProcessingContext);
        }

        public MultistepActionStageCommandState GetState() => new MultistepActionStageCommandState
        {
            Type = BuildSiteStageCommandTypes.BuildPages,
            ParentId = 0,
            Id = AssemblingEntityIdId
        };

        public MultistepStageSettings GetStageSettings() => new MultistepStageSettings
        {
            ItemCount = _itemCount,
            StepCount = MultistepActionHelper.GetStepCount(_itemCount, ItemsPerStep),
            Name = SiteOrTemplate ? string.Format(SiteStrings.AssemblePagesStageName, AssemblingEntityName ?? string.Empty) : string.Format(TemplateStrings.AssemblePagesStageName, AssemblingEntityName ?? string.Empty)
        };

        public MultistepActionStepResult Step(int step)
        {
            var context = HttpContext.Session.GetValue<AssemblePagesCommandContext>(HttpContextSession.AssemblePagesCommandProcessingContext);

            var pages = context.Pages
                .Skip(step * ItemsPerStep)
                .Take(ItemsPerStep)
                .ToArray();

            if (pages.Any())
            {
                foreach (var page in pages)
                {
                    if (IsDotNet)
                    {
                        new AssemblePageController(page.Id, QPContext.CurrentDbConnectionString).Assemble();
                        AssembleRepository.UpdatePageStatus(page.Id, QPContext.CurrentUserId);
                    }
                    else
                    {
                        var result = QP7Service.AssemblePage(page.Id, context.QP7Token);

                        if (!string.IsNullOrEmpty(result))
                        {
                            var pagesForResult = new List<PageInfo>();

                            if (context.MissedPages.ContainsKey(result))
                            {
                                pagesForResult = context.MissedPages[result];
                            }
                            else
                            {
                                context.MissedPages[result] = pagesForResult;
                            }

                            pagesForResult.Add(page);
                        }
                    }
                }
            }

            var pageCount = context.MissedPages.Sum(rp => rp.Value.Count);
            var info = new StringBuilder();
            if (pageCount > 0)
            {
                info.AppendFormat(SiteStrings.AssemblePagesResult, pageCount);
                foreach (var item in context.MissedPages)
                {
                    info.AppendFormat(" \"{0}\" : [{1}]", item.Key, string.Join(", ", item.Value.Select(p => string.Format(PageTemplate, p.Template, p.Name))));
                }
            }

            return new MultistepActionStepResult { ProcessedItemsCount = pages.Length, AdditionalInfo = info.ToString() };
        }
    }

    [Serializable]
    public class AssemblePagesCommandContext
    {
        public PageInfo[] Pages { get; set; }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public QP7Token QP7Token { get; set; }

        public Dictionary<string, List<PageInfo>> MissedPages;
    }
}
