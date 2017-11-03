using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class ContextBlockViewModel
    {
        public ContextBlockViewModel(int id, string actionCode, string hostId)
        {
            _id = id;
            IsArchive = actionCode == ActionCode.ArchiveArticles;
            _hostId = hostId;
            _relatedContents = new Lazy<IEnumerable<ArticleContextSearchBlockItem>>(() => ContentService.GetContentsForContextSwitching(_id));
        }

        private readonly int _id;
        private readonly string _hostId;
        private readonly Lazy<IEnumerable<ArticleContextSearchBlockItem>> _relatedContents;

        public IEnumerable<ArticleContextSearchBlockItem> RelatedContents => _relatedContents.Value;

        public bool IsArchive { get; }

        public string UniqueId(string id) => HtmlHelperFieldExtensions.UniqueId(id, _hostId);
    }
}
