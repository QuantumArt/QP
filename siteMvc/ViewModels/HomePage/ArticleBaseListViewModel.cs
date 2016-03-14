using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using C = Quantumart.QP8.Constants;


namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ArticleBaseListViewModel : ListViewModel
    {
        public static ArticleBaseListViewModel Create(int id, string tabId, int parentId)
        {
            ArticleBaseListViewModel model = ViewModel.Create<ArticleBaseListViewModel>(tabId, parentId);
            return model;
        }
        public override bool AllowMultipleEntitySelection
        {
            get
            {
                return true;
            }
        }
        public override bool LinkOpenNewTab
        {
            get
            {
                return true;
            }
        }
        public override string EntityTypeCode
        {
            get { return C.EntityTypeCode.Article; }
        }
        public override string ActionCode
        {
            get { return C.ActionCode.LockedArticles; }
        }
        public override bool IsListDynamic
        {
            get
            {
                return true;
            }
        }


        public string DataBindingControllerName
        {
            get { return "Home"; }
        }
        public string DataBindingActionName { get; set; }


    }
}