﻿using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ArticleListViewModel : ListViewModel
    {
		public DataTable Data { get; set; }

        public string ContentName { get; set; }

        public int ContentId { get; set; }

        public string ControllerName { get; set; }

        public bool ShowArchive { get; set; }

		public bool AutoCheckChildren { get; set; }

		public string CustomFilter { get; set; }

		public IEnumerable<BLL.Field> DisplayFields { get; set; }

        public string GetDataActionName
        {
            get
            {
                if (IsSelect)
                {
                    return AllowMultipleEntitySelection ? "_MultipleSelect" : "_Select";
                }

                return ShowArchive ? "_ArchiveIndex" : "_Index";
            }
        }

        public ArticleListViewModel()
        {
            ControllerName = "Article";
            IsViewChangable = true;
            ShowArchive = false;
			CustomFilter = "";
			AutoCheckChildren = false;
        }

		public static ArticleListViewModel Create(ArticleResultBase result, int parentEntityId, string tabId)
        {
			var model = Create<ArticleListViewModel>(tabId, parentEntityId);
            model.ContentId = parentEntityId;
			model.Init(result);
			return model;
        }

		public static ArticleListViewModel Create(ArticleResultBase result, int parentEntityId, string tabId, bool allowMultipleEntitySelection, bool isSelect, int[] ids)
        {
			var model = Create(result, parentEntityId, tabId);
			model.AllowMultipleEntitySelection = allowMultipleEntitySelection;
            model.IsSelect = isSelect;
			model.SelectedIDs = ids;
            return model;
        }

		public void Init(ArticleResultBase result)
		{
			ContentName = result.ContentName;

			IsVirtual = result.IsVirtual;
			ShowAddNewItemButton = result.IsUpdatable && result.IsAddNewAccessable && !IsWindow && !result.ContentDisableChangingActions;

			var listResult = result as ArticleInitListResult;
			if (listResult != null)
			{
				TitleFieldName = listResult.TitleFieldName;
				PageSize = listResult.PageSize;
				DisplayFields = listResult.DisplayFields;
			}

			var treeResult = result as ArticleInitTreeResult;
			if (treeResult != null)
			{
				IsTree = true;
				CustomFilter = treeResult.Filter;
				AutoCheckChildren = treeResult.AutoCheckChildren;
			}
		}

        #region overrides
        public override bool IsReadOnly
        {
            get
            {
                return base.IsReadOnly || ShowArchive;
            }
        }

        public override string EntityTypeCode
        {
            get
            {
                if (ShowArchive)
                {
                    return C.EntityTypeCode.ArchiveArticle;
                }

                if (IsVirtual)
                {
                    return C.EntityTypeCode.VirtualArticle;
                }

                return C.EntityTypeCode.Article;
            }
        }

        public override string ActionCode
        {
            get
            {
                if (ShowArchive)
                {
                    return C.ActionCode.ArchiveArticles;
                }

                if (IsVirtual)
                {
                    return C.ActionCode.VirtualArticles;
                }

                return C.ActionCode.Articles;
            }
        }

        public override string AddNewItemText
        {
            get
            {
                return ArticleStrings.Link_AddNewArticle;
            }
        }

        public override string AddNewItemActionCode
        {
            get
            {
                return C.ActionCode.AddNewArticle;
            }
        }

        public override bool IsListDynamic
        {
            get
            {
                return true;
            }
        }

        public override string KeyColumnName
        {
            get
            {
                return C.FieldName.CONTENT_ITEM_ID;
            }
        }

		public override string Filter
		{
			get
			{
                var filter = string.Format("c.archive = {0}", Convert.ToInt32(ShowArchive));
                return CustomFilter.Contains(filter) ? CustomFilter : SqlFilterComposer.Compose(CustomFilter, filter);
            }
		}

		public override ExpandoObject MainComponentOptions
		{
			get
			{
				dynamic result = base.MainComponentOptions;
				result.isVirtual = IsVirtual;
				result.autoCheckChildren = AutoCheckChildren;
                result.treeFieldId = GetTreeFieldId();
				result.delayAutoLoad = true;
				return result;
			}
		}
        public int GetTreeFieldId()
        {
            if (ContentService.Read(ContentId).TreeField != null)
            {
                return ContentService.Read(ContentId).TreeField.Id;
            }
            return 0;
        }

        public Dictionary<string, object> TreeHthmlAttributes
		{
			get
			{
				var result = new Dictionary<string, object>();
				if (AllowMultipleEntitySelection)
				{
					result.AddCssClass(HtmlHelpersExtensions.CHECK_BOX_TREE_CLASS_NAME);
				}

				return result;
			}
		}

        public override bool AllowFilterSelectedEntities { get { return true; } }
        #endregion
    }
}