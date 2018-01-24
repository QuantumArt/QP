using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.EntityPermissions;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.EntityPermissions
{
    public class PermissionListViewModel : ListViewModel
    {
        protected IPermissionListViewModelSettings Settings;
        protected string ControlerName;
        private Lazy<BLL.Article> _parentArticle;
        private bool _isEnableArticlesPermissionsAccessable;

        public static PermissionListViewModel Create(PermissionInitListResult result, string tabId, int parentId, IPermissionService service, string controlerName)
        {
            var model = Create<PermissionListViewModel>(tabId, parentId);
            model.Settings = service.ListViewModelSettings;
            model.ControlerName = controlerName;
            model._parentArticle = new Lazy<BLL.Article>(() => service.GetParentArticle(parentId));
            model.ShowAddNewItemButton = result.IsAddNewAccessable && !model.IsWindow;
            model._isEnableArticlesPermissionsAccessable = result.IsEnableArticlesPermissionsAccessable;
            model.TitleFieldName = "Id";
            return model;
        }

        public IEnumerable<EntityPermissionListItem> Data { get; set; }

        public string GettingDataControllerName => ControlerName;

        public string GettingDataActionName => "_Index";

        public bool IsPropagateable => Settings.IsPropagateable;

        public bool CanHide => Settings.CanHide;

        public string PermissionEntityTypeCode => Settings.PermissionEntityTypeCode;

        public bool ShowDisableArticlePermissionForContentWarning
        {
            get
            {
                if (PermissionEntityTypeCode == Constants.EntityTypeCode.ArticlePermission)
                {
                    return !ParentArticle.Content.AllowItemsPermission && _isEnableArticlesPermissionsAccessable;
                }

                return false;
            }
        }

        public BLL.Article ParentArticle => PermissionEntityTypeCode == Constants.EntityTypeCode.ArticlePermission ? _parentArticle.Value : null;

        public override string AddNewItemText => EntityPermissionStrings.AddNewPermission;

        public override string EntityTypeCode => Settings.EntityTypeCode;

        public override string ActionCode => Settings.ActionCode;

        public override string AddNewItemActionCode => Settings.AddNewItemActionCode;

        public override bool LinkOpenNewTab => true;
    }
}
