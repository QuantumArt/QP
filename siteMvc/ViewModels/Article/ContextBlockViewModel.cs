using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
	public class ContextBlockViewModel
	{
		public ContextBlockViewModel(int id, string actionCode, string hostId)
		{
			this.id = id;
			this.actionCode = actionCode;
			this.isArchive = actionCode == ActionCode.ArchiveArticles;
			this.hostId = hostId;
			_relatedContents = new Lazy<IEnumerable<ArticleContextSearchBlockItem>>(() => ContentService.GetContentsForContextSwitching(this.id));
		}

		private readonly int id;
		private readonly string actionCode;
		private readonly string hostId;
		private readonly Lazy<IEnumerable<ArticleContextSearchBlockItem>> _relatedContents;
		private readonly bool isArchive;


		public IEnumerable<ArticleContextSearchBlockItem> RelatedContents { get { return _relatedContents.Value; } }

		public bool IsArchive { get { return isArchive; } }

		public string UniqueId(string id)
		{
			return HtmlHelperFieldExtensions.UniqueId(id, hostId);
		}
	}
}