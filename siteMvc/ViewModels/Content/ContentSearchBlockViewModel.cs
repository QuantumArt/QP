using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.Content
{
    public class ContentSearchBlockViewModel
    {
        public ContentSearchBlockViewModel(int id, string actionCode, string hostId)
        {
            _id = id;
            _actionCode = actionCode;
            _hostId = hostId;
            _contentGroups = new Lazy<IEnumerable<ListItem>>(() => ContentService.GetSiteContentGroupsForFilter(_id));
            _sites = new Lazy<IEnumerable<ListItem>>(SiteService.GetAllSites);
        }

        private readonly int _id;
        private readonly string _actionCode;
        private readonly string _hostId;
        private readonly Lazy<IEnumerable<ListItem>> _contentGroups;
        private readonly Lazy<IEnumerable<ListItem>> _sites;

        public IEnumerable<ListItem> ContentGroups => _contentGroups.Value;

        public IEnumerable<ListItem> Sites => _sites.Value;

        public bool ShowSiteList => _actionCode.Equals(ActionCode.MultipleSelectContent, StringComparison.InvariantCultureIgnoreCase);

        public bool ShowGroupList => !ShowSiteList;

        public string UniqueId(string id) => HtmlHelperFieldExtensions.UniqueId(id, _hostId);
    }
}
