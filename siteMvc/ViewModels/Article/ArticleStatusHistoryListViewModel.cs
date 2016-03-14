using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ArticleStatusHistoryListViewModel : ListViewModel
    {
        public static ArticleStatusHistoryListViewModel Create(string tabId, int parentId)
        {
            ArticleStatusHistoryListViewModel model = ViewModel.Create<ArticleStatusHistoryListViewModel>(tabId, parentId);
            return model;
        }

        public string DataBindingActionName { get { return "_StatusHistoryList"; } }

        public string DataBindingControllerName
        {
            get { return "Article"; }
        }

        public override string ActionCode
        {
            get { return C.ActionCode.ArticleStatus; }
        }

        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Article; }
        }
    }
}