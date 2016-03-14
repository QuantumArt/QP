using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using QA_Assembling;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Assemble
{
	internal class AssemblePagesCommand : IMultistepActionStageCommand
	{
        private const string PageTemplate = "\"{0}/{1}\"";
		private static readonly int ITEMS_PER_STEP = 5;
		private static readonly string SESSION_KEY = "AssemblePagesCommand.ProcessingContext";

        public int AssemblingEntityIdId { get; private set; }
		public string AssemblingEntityName { get; private set; }
		public bool SiteOrTemplate { get; private set; }
        public bool IsDotNet { get; private set; }
        public IQP7Service QP7Service { get; private set; }

        private int itemCount = 0;

		public AssemblePagesCommand(MultistepActionStageCommandState state, bool siteOrTemplate, bool isDotNet) : this(state.Id, null, siteOrTemplate, isDotNet) { }

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
			itemCount = pagesIds.Count();
            QP7Token token = null;

            if (!IsDotNet)
            {
                token = QP7Service.Authenticate();
            }

            HttpContext.Current.Session[SESSION_KEY] = new AssemblePagesCommandContext
            {
                Pages = pagesIds.ToArray(),
                QP7Token = token,
                MissedPages = new Dictionary<string, List<PageInfo>>()
            };
		}    

        internal static void TearDown()
		{
			HttpContext.Current.Session[SESSION_KEY] = null;
		}

		public MultistepActionStageCommandState GetState()
		{
            return new MultistepActionStageCommandState
			{
				Type = BuildSiteStageCommandTypes.BuildPages,
				ParentId = 0,
				Id = AssemblingEntityIdId
			};
		}

		public MultistepStageSettings GetStageSettings()
		{
			return new MultistepStageSettings
			{
				ItemCount = itemCount,
				StepCount = MultistepActionHelper.GetStepCount(itemCount, ITEMS_PER_STEP),
				Name = SiteOrTemplate ? String.Format(SiteStrings.AssemblePagesStageName, (AssemblingEntityName ?? "")):
				String.Format(TemplateStrings.AssemblePagesStageName, (AssemblingEntityName ?? ""))
			};
		}

		#region IMultistepActionStageCommand Members

		public MultistepActionStepResult Step(int step)
		{
			AssemblePagesCommandContext context = HttpContext.Current.Session[SESSION_KEY] as AssemblePagesCommandContext;
			var pages = context.Pages
				.Skip(step * ITEMS_PER_STEP)
				.Take(ITEMS_PER_STEP)
				.ToArray();

			if (pages.Any())
			{
				foreach (var page in pages)
				{
                    if (IsDotNet)
                    {
                        new AssemblePageController(page.Id, QPContext.CurrentCustomerCode).Assemble();
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

            int pageCount = context.MissedPages.Sum(rp => rp.Value.Count);
            var info = new StringBuilder();

            if (pageCount > 0)
            {
                info.AppendFormat(SiteStrings.AssemblePagesResult, pageCount);

                foreach(var item in context.MissedPages)
                {
                    info.AppendFormat(" \"{0}\" : [{1}]", item.Key, string.Join(", ", item.Value.Select(p => string.Format(PageTemplate, p.Template, p.Name))));
                }
            }

            return new MultistepActionStepResult { ProcessedItemsCount = pages.Count(), AdditionalInfo = info.ToString() };
        }

		#endregion

		
	}

	[Serializable]
	public class AssemblePagesCommandContext
	{
		public PageInfo[] Pages { get; set; }
        public QP7Token QP7Token { get; set; }
        public Dictionary<string,List<PageInfo>> MissedPages;
    }
}
