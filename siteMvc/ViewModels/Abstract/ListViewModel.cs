using System;
using System.Dynamic;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class ListViewModel : ViewModel
    {
        public string TitleFieldName { get; set; }

        public bool ShowAddNewItemButton { get; set; }

        public bool AutoGenerateLink { get; set; }

        public bool GenerateLinkOnTitle { get; set; }

        public bool IsTree { get; set; }

        public virtual bool AllowMultipleEntitySelection { get; set; }

        public virtual bool LinkOpenNewTab => false;

        public virtual bool AllowFilterSelectedEntities => false;

        public bool AllowGlobalSelection { get; set; }

        public bool IsSelect { get; set; }

        public int PageSize { get; set; }

        public int[] SelectedIDs { get; set; }

        public virtual string Filter { get; set; }

        public bool ShowIds { get; set; }

        public bool AutoLoad { get; set; }

        protected ListViewModel()
        {
            TitleFieldName = "Name";
            IsTree = false;
            ShowAddNewItemButton = false;
            IsSelect = false;
            AllowMultipleEntitySelection = true;
            AllowGlobalSelection = true;
            SelectedIDs = new int[0];
            AutoGenerateLink = true;
            GenerateLinkOnTitle = true;
            Filter = string.Empty;
            ShowIds = true;
            AutoLoad = true;
        }

        public string SelectAllId => UniqueId("select_all");

        public string UnselectId => UniqueId("unselect");

        public string ArticlesCountId => UniqueId("articlesCount");

        public virtual string AddNewItemActionCode
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string AddNewItemText
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string ContextMenuCode => EntityTypeCode;

        public virtual bool IsListDynamic => false;

        public virtual string KeyColumnName => string.Empty;

        public virtual string ParentKeyColumnName => string.Empty;

        public virtual bool IsReadOnly => IsVirtual || IsSelect;

        public virtual string ActionCodeForLink => EntityTypeService.GetDefaultActionCodeByEntityTypeCode(EntityTypeCode);

        public override MainComponentType MainComponentType => (IsTree) ? MainComponentType.Tree : MainComponentType.Grid;

        public override string MainComponentId => IsTree ? UniqueId("Tree") : UniqueId("Grid");

        public override ExpandoObject MainComponentOptions
        {
            get
            {
                dynamic result = base.MainComponentOptions;
                result.selectedEntitiesIDs = SelectedIDs;
                if (IsTree)
                {
                    result.allowMultipleNodeSelection = AllowMultipleEntitySelection;
                }
                else
                {
                    result.allowMultipleRowSelection = AllowMultipleEntitySelection;
                }

                result.allowGlobalSelection = AllowGlobalSelection;
                result.contextMenuCode = ContextMenuCode;
                result.filter = Filter;
                result.actionCodeForLink = ActionCodeForLink;
                result.autoGenerateLink = AutoGenerateLink;
                result.selectAllId = SelectAllId;
                result.unselectId = UnselectId;
                result.articlesCountId = ArticlesCountId;
                result.autoLoad = AutoLoad;
                result.allowFilterSelectedEntities = AllowFilterSelectedEntities;

                if (!IsTree)
                {
                    if (IsListDynamic)
                    {
                        result.keyColumnName = KeyColumnName;
                    }

                    result.titleColumnName = TitleFieldName;
                    result.linkOpenNewTab = LinkOpenNewTab;
                    result.parentKeyColumnName = ParentKeyColumnName;
                    result.generateLinkOnTitle = GenerateLinkOnTitle;
                }
                else
                {
                    result.showIds = ShowIds;
                }

                return result;
            }
        }

        public string IconFormat => @"<img src=""{0}/{1}"" title=""{2}"" class=""smallIcon"" />";
    }
}
