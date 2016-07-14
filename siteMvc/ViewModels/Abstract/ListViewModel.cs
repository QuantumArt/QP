using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using System;
using System.Dynamic;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public abstract class ListViewModel : ViewModel
    {
        public string TitleFieldName { get; set; }

        public bool ShowAddNewItemButton { get; set; }

        /// <summary>
        /// признак, разрешающий автоматическую генерацию ссылок
        /// </summary>
        public bool AutoGenerateLink { get; set; }

        /// <summary>
        /// признак, определяющий, где генерировать ссылку: в title или в id
        /// </summary>
        public bool GenerateLinkOnTitle { get; set; }

        public bool IsTree { get; set; }

        public virtual bool AllowMultipleEntitySelection { get; set; }

        /// <summary>
        /// Если true - то по клику на Link в гриде открывать новый таб
        /// </summary>
        public virtual bool LinkOpenNewTab { get { return false; } }

        public virtual bool AllowFilterSelectedEntities { get { return false; } }

        public bool AllowGlobalSelection { get; set; }

        public bool IsSelect { get; set; }

        public int PageSize { get; set; }

        public int[] SelectedIDs { get; set; }

        public virtual string Filter { get; set; }

        public bool ShowIds { get; set; }

        public bool AutoLoad { get; set; }

        #region creation
        public ListViewModel()
        {
            this.TitleFieldName = "Name";
            this.IsTree = false;
            this.ShowAddNewItemButton = false;
            this.IsSelect = false;
            this.AllowMultipleEntitySelection = true;
            this.AllowGlobalSelection = true;
            this.SelectedIDs = new int[0];
            this.AutoGenerateLink = true;
            this.GenerateLinkOnTitle = true;
            this.Filter = String.Empty;
            this.ShowIds = true;
            this.AutoLoad = true;
        }
        #endregion

        #region read-only members
        public string SelectAllId
        {
            get
            {
                return UniqueId("select_all");
            }
        }

        public string UnselectId
        {
            get
            {
                return UniqueId("unselect");
            }
        }

        public string ArticlesCountId
        {
            get
            {
                return UniqueId("articlesCount");
            }
        }

        public virtual string AddNewItemActionCode
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string AddNewItemText
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string ContextMenuCode
        {
            get
            {
                return EntityTypeCode;
            }
        }

        public virtual bool IsListDynamic
        {
            get
            {
                return false;
            }
        }

        public virtual string KeyColumnName
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual string ParentKeyColumnName
        {
            get
            {
                return string.Empty;
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                return IsVirtual || IsSelect;
            }
        }

        /// <summary>
        /// код действия, которое запускается при щелчке на гиперссылке
        /// в ячейке с заголовком сущности
        /// </summary>
        public virtual string ActionCodeForLink
        {
            get
            {
                return EntityTypeService.GetDefaultActionCodeByEntityTypeCode(EntityTypeCode);
            }
        }

        public override MainComponentType MainComponentType
        {
            get { return (IsTree) ? MainComponentType.Tree : MainComponentType.Grid; }
        }

        public override string MainComponentId
        {
            get { return IsTree ? UniqueId("Tree") : UniqueId("Grid"); }
        }

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

        public string IconFormat
        {
            get
            {
                return @"<img src=""{0}/{1}"" title=""{2}"" class=""smallIcon"" />";
            }
        }

        #endregion
    }
}
