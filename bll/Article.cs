using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Repository.Articles;
using Quantumart.QP8.BLL.WCF.Notification;
using Quantumart.QP8.Utils;
using QA.Validation.Xaml;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.ListItems;
using System.Text.RegularExpressions;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using QA.Validation.Xaml.Extensions.Rules;

namespace Quantumart.QP8.BLL
{
    public class Article : LockableEntityObject
    {
        #region private fields and types

        private enum CopyFilesMode
        {
            ToBackupFolder,
            ToVersionFolder,
            FromVersionFolder
        }

        private string _name = String.Empty;
        private string _alias = String.Empty;
        private List<FieldValue> _fieldValues;
        private int _displayContentId;
        private ArticleViewType _ViewType = ArticleViewType.Normal;
        private ArticleSchedule _Schedule;
        private bool _ScheduleLoaded;
        private ArticleWorkflowBind _WorkflowBinding;
        private readonly InitPropertyValue<IEnumerable<Article>> _AggregatedArticles;
        private readonly InitPropertyValue<bool> _IsUpdatableWithRelationSecurity;
        private readonly InitPropertyValue<bool> _IsRemovableWithRelationSecurity;
        private readonly InitPropertyValue<StatusHistoryListItem> _StatusHistoryListItem;
        private readonly InitPropertyValue<List<Article>> _VariationArticles;
        private readonly InitPropertyValue<IEnumerable<ArticleVariationListItem>> _VariationListItems;
        private readonly InitPropertyValue<IEnumerable<ArticleContextListItem>> _ContextListItems;

        #endregion

        #region creation

        internal Article()
        {
            _AggregatedArticles = new InitPropertyValue<IEnumerable<Article>>(() => ArticleRepository.LoadAggregatedArticles(this));
            _VariationArticles = new InitPropertyValue<List<Article>>(() => ArticleRepository.LoadVariationArticles(this));
            _VariationListItems = new InitPropertyValue<IEnumerable<ArticleVariationListItem>>(() => this.LoadVariationListForClient());
            _ContextListItems = new InitPropertyValue<IEnumerable<ArticleContextListItem>>(() => this.LoadContextListForClient());
            _IsUpdatableWithRelationSecurity = new InitPropertyValue<bool>(() => QPContext.IsAdmin || ArticleRepository.CheckRelationSecurity(this, false));
            _IsRemovableWithRelationSecurity = new InitPropertyValue<bool>(() => QPContext.IsAdmin || ArticleRepository.CheckRelationSecurity(this, true));
            _StatusHistoryListItem = new InitPropertyValue<StatusHistoryListItem>(() => ArticleRepository.GetStatusHistoryItem(Id));
            PredefinedValues = new Dictionary<string, string>();
            VariationsErrorModel = new Dictionary<string, RulesException<Article>>();
            CancelSplit = false;
            UseInVariationUpdate = false;
        }


        internal Article(Content content) : this()
        {
            ContentId = content.Id;
            Content = content;
            SetDefaultStatusAndVisibility();
        }

        internal Article (Content content, Dictionary<string, string> predefinedValues) : this(content)
        {
            PredefinedValues = predefinedValues;
        }

        #endregion

        #region properties

        #region simple read-write

        public int ContentId { get; set; }

        public bool Visible { get; set; }

        public bool Archived { get; set; }

        public bool Splitted { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof(ArticleStrings))]
        public int StatusTypeId { get; set; }

        [LocalizedDisplayName("DelayPublication", NameResourceType = typeof(ArticleStrings))]
        public bool Delayed { get; set; }

        [LocalizedDisplayName("CancelSplit", NameResourceType = typeof(ArticleStrings))]
        public bool CancelSplit { get; set; }

        [LocalizedDisplayName("LeaveComment", NameResourceType=typeof(ArticleStrings))]
        public string Comment { get; set; }

        public Dictionary<string, string> PredefinedValues { get; set; }

        /// <summary>
        /// Имя статьи
        /// </summary>
        public override string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                {
                    string name = Id.ToString();
                    foreach (FieldValue value in FieldValues)
                    {
                        if (value.Field.ViewInList)
                        {
                            name = value.Value;
                            break;
                        }
                    }

                    _name = name;
                }

                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Только для чтения
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return ViewType != ArticleViewType.Normal && ViewType != ArticleViewType.PreviewVersion;
            }
        }

        /// <summary>
        /// Псевдоним статьи
        /// </summary>
        public string AliasForTree
        {
            get
            {
                if (String.IsNullOrEmpty(_alias))
                {
                    int treeFieldId = ContentRepository.GetTreeFieldId(ContentId);
                    string displayTreeFieldName = FieldRepository.GetById(treeFieldId).Relation.Name;
                    _alias = FieldValues.Single(n => n.Field.Name == displayTreeFieldName).Value;
                }

                return _alias;
            }
            set
            {
                _alias = value;
            }
        }


        /// <summary>
        /// ID контента, в котором показывается статья
        /// </summary>
        public int DisplayContentId
        {
            get
            {
                return (_displayContentId == 0) ? ContentId : _displayContentId;
            }
            set
            {
                _displayContentId = value;
            }
        }

        public bool UseInVariationUpdate { get; set; }

        public Dictionary<string, RulesException<Article>> VariationsErrorModel { get; set; }




        #endregion

        #region simple read-only

        internal string BackupPath
        {
            get
            {
                return GetVersionPathInfo(ArticleVersion.CurrentVersionId).Path;
            }
        }

        public override string LockedByAnyoneElseMessage
        {
            get
            {
                return ArticleStrings.LockedByAnyoneElse;
            }
        }

        public override string CannotAddBecauseOfSecurityMessage
        {
            get
            {
                return ArticleStrings.CannotAddBecauseOfSecurity;
            }
        }

        public override string CannotUpdateBecauseOfSecurityMessage
        {
            get
            {
                return ArticleStrings.CannotUpdateBecauseOfSecurity;
            }
        }

        /// <summary>
        /// Имя контента, в котором показывается статья
        /// </summary>
        public string DisplayContentName
        {
            get
            {
                if (DisplayContentId == ContentId)
                    return (Content != null) ? Content.Name : null;
                else
                    return ContentRepository.GetById(DisplayContentId).Name;
            }
        }

        public bool IsUpdatableWithWorkflow
        {
            get
            {
                return Workflow.CurrentUserCanUpdateArticles;
            }
        }

        public bool IsPublishableWithWorkflow
        {
            get
            {
                return Workflow.CurrentUserCanPublishArticles && !WorkflowBinding.IsAssigned;
            }
        }

        public bool IsRemovableWithWorkflow
        {
            get
            {
                return Workflow.CurrentUserCanRemoveArticles; // || (Status.Name == "None" && ArticleRepository.ArticleStatusHasNotBeenChanged(Id)
            }
        }

        public bool IsUpdatableWithRelationSecurity
        {
            get
            {
                return _IsUpdatableWithRelationSecurity.Value;
            }

            set
            {
                _IsUpdatableWithRelationSecurity.Value = value;
            }
        }

        public bool IsRemovableWithRelationSecurity
        {
            get
            {
                return _IsRemovableWithRelationSecurity.Value;
            }

            set
            {
                _IsRemovableWithRelationSecurity.Value = value;
            }
        }

        public StatusHistoryListItem StatusHistoryListItem
        {
            get
            {
                return _StatusHistoryListItem.Value;
            }

            set
            {
                _StatusHistoryListItem.Value = value;
            }
        }

        /// <summary>
        /// Код сущности
        /// </summary>
        public override string EntityTypeCode
        {
            get
            {
                return Constants.EntityTypeCode.Article;
            }
        }

        public bool UseVariations
        {
            get
            {
                return Content.VariationField != null;
            }
        }


        public IEnumerable<FieldValue> VariationContextFieldValues
        {
            get
            {
                return FieldValues.Where(n => n.Field.UseForContext).OrderBy(n => n.Field.Order);
            }
        }

        public IEnumerable<FieldValue> VariationEditableFieldValues
        {
            get
            {
                return FieldValues.Where(n => !n.Field.UseForContext && !n.Field.UseForVariations);
            }
        }

        public string VariationContext
        {
            get
            {
                return String.Join(",", VariationContextFieldValues
                    .Where(n => !String.IsNullOrEmpty(n.Value))
                    .Select(n => n.Value)
                );
            }
        }
        public string VariationContextDisplay
        {
            get
            {
                return String.Join(", ", VariationContextFieldValues
                    .Where(n => !String.IsNullOrEmpty(n.Value))
                    .Select(n => ArticleRepository.GetById(Int32.Parse(n.Value)).Name)
                );
            }
        }

        private int[] SelfAndAggregatedIds
        {
            get
            {
                return Enumerable.Repeat(Id, 1).Union(
                    AggregatedArticles.Select(n => n.Id)
                ).ToArray();
            }
        }

        public int[] SelfAndChildIds
        {
            get
            {
                return SelfAndAggregatedIds.Union(
                    VariationArticles.SelectMany(n => n.SelfAndAggregatedIds)
                ).ToArray();

            }
        }

        /// <summary>
        /// Является ли статья агрегированной
        /// </summary>
        public bool IsAggregated
        {
            get
            {
                return FieldValues.Any(v => v.Field.Aggregated);
            }
        }

        /// <summary>
        /// Является ли статья вариацией
        /// </summary>
        public bool IsVariation
        {
            get
            {
                return !String.IsNullOrEmpty(VariationContext);
            }
        }

        private static string UploadUrlPlaceHolder
        {
            get { return "<%=upload_url%>"; }
        }

        private static string SiteUrlPlaceHolder
        {
            get { return "<%=site_url%>"; }
        }





        #endregion

        #region references

        public Content Content { get; set; }

        public StatusType Status { get; set; }

        public override EntityObject Parent
        {
            get
            {
                return Content;
            }
        }


        /// <summary>
        /// Отдельный Workflow статьи (если есть)
        /// </summary>
        public ArticleWorkflowBind WorkflowBinding
        {
            get
            {
                if (_WorkflowBinding == null)
                {
                    _WorkflowBinding = WorkflowRepository.GetArticleWorkflow(this);
                }
                return _WorkflowBinding;
            }
            set
            {
                _WorkflowBinding = value;
            }
        }

        /// <summary>
        /// Список значений полей
        /// </summary>
        [ScriptIgnore]
        public List<FieldValue> FieldValues
        {
            get
            {
                if (_fieldValues == null)
                    _fieldValues = GetFieldValues();
                return _fieldValues;
            }
            set { _fieldValues = value; }
        }


        /// <summary>
        /// Режим отображения статьи
        /// </summary>
        public ArticleViewType ViewType
        {
            get { return _ViewType; }
            set { _ViewType = value; }
        }

        /// <summary>
        /// Расписание статьи
        /// </summary>
        public ArticleSchedule Schedule
        {
            get
            {
                if (_Schedule == null && !_ScheduleLoaded)
                {
                    _Schedule = ScheduleRepository.GetSchedule(this);
                    _ScheduleLoaded = true;
                }
                return _Schedule;
            }
            set
            {
                _Schedule = value;
                _ScheduleLoaded = true;
            }
        }

        /// <summary>
        /// Фактический Workflow статьи (с учетом Article Workflow)
        /// </summary>
        public WorkflowBind Workflow
        {
            get
            {
                return (WorkflowBinding.IsAssigned) ? (WorkflowBind)WorkflowBinding : (WorkflowBind)Content.WorkflowBinding;
            }
        }

        /// <summary>
        /// Связанные агрегированные статьи
        /// </summary>
        public IEnumerable<Article> AggregatedArticles
        {
            get { return _AggregatedArticles.Value; }
            set { _AggregatedArticles.Value = value; }
        }

        /// <summary>
        /// Связанные статьи-вариации
        /// </summary>
        public List<Article> VariationArticles
        {
            get { return _VariationArticles.Value; }
            set { _VariationArticles.Value = value; }
        }

        /// <summary>
        /// Связанные статьи-вариации (версия для сериализации в JSON)
        /// </summary>
        public IEnumerable<ArticleVariationListItem> VariationListItems
        {
            get { return _VariationListItems.Value; }
            set { _VariationListItems.Value = value; }
        }

        /// <summary>
        /// Возможные контексты с информацией о родителях (версия для сериализации в JSON)
        /// </summary>
        public IEnumerable<ArticleContextListItem> ContextListItems
        {
            get { return _ContextListItems.Value; }
            set { _ContextListItems.Value = value; }
        }
        #endregion

        #endregion

        #region methods

        #region public
        /// <summary>
        /// Осуществляет валидацию статьи, в случае неудачи генерируется суммарное исключение
        /// </summary>
        public override void Validate()
        {
            var errors = new RulesException<Article>();

            base.Validate(errors);

            ValidateWorkflow(errors);

            ValidateRelationSecurity(errors);

            foreach (FieldValue pair in FieldValues)
            {
                if (pair.Field.ExactType == FieldExactTypes.M2ORelation && pair.Field.BackRelation != null && pair.Field.BackRelation.Aggregated)
                    continue;
                pair.Validate(errors);
            }

            // Валидация агрегированных статей
            ValidateAggregated(errors);

            // Пользовательская валидация на основе Xaml описаний
            // валидация производится одновременно и для основной статьи и для агрегированных статей
            ValidateCustom(errors);

            ValidateSchedule(errors, Schedule);

            if (!errors.IsEmpty)
                throw errors;
        }

        private void ValidateVariations(RulesException<Article> baseArticleErrors)
        {
            foreach (Article varArticle in this.VariationArticles)
            {
                var varErrors = new RulesException<Article>();
                foreach (FieldValue pair in varArticle.VariationEditableFieldValues)
                {
                    pair.Validate(varErrors);
                }
                varArticle.ValidateUnique(varErrors);
                varArticle.ValidateCustom(varErrors);
                varArticle.ValidateAggregated(varErrors);

                string context = varArticle.VariationContextDisplay;

                foreach (var error in varErrors.Errors)
                {
                    baseArticleErrors.ErrorForModel(String.Format("{0} ({1}): {2}", ArticleStrings.Context, context, error.Message));
                }
                if (varErrors.Errors.Any())
                {
                    VariationsErrorModel.Add(varArticle.VariationContext, varErrors);
                }
            }
        }

        private void ValidateAggregated(RulesException<Article> errors)
        {
            foreach (Article aggregated in this.AggregatedArticles)
            {
                int aggregatedFieldId = aggregated.FieldValues.Select(n => n.Field).Single(p => p.Aggregated).Id;
                foreach (FieldValue pair in aggregated.FieldValues.Where(p => p.Field.Id != aggregatedFieldId))
                {
                    pair.Validate(errors);
                }
                aggregated.ValidateUnique(errors, aggregatedFieldId);
            }
        }

        /// <summary>
        /// Пользовательская валидация на основе Xaml описаний
        /// валидация производится одновременно и для основной статьи и для агрегированных статей
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateCustom(RulesException<Article> errors)
        {
            Dictionary<string, string> values = FieldValues
                .Concat(AggregatedArticles.SelectMany(a => a.FieldValues))
                .Where(v => !v.Field.Aggregated)
                .ToDictionary(v => v.Field.FormName,
                    v => (v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value)
                );

            // Добавялем id и status type id
            values[Content.ContentItemIdPropertyName] = base.Id.ToString();
            values[Content.StatusTypeIdPropertyName] = StatusTypeId.ToString();

            string[] aggregatedXamlValidators = AggregatedArticles
                .Where(a => !a.Content.DisableXamlValidation && !String.IsNullOrWhiteSpace(a.Content.XamlValidation))
                .Select(a => a.Content.XamlValidation)
                .ToArray();

            if (!Content.DisableXamlValidation && !String.IsNullOrWhiteSpace(Content.XamlValidation))
            {
                try
                {
                    ValidationContext vcontext = ValidationServices.ValidateModel(values, Content.XamlValidation, aggregatedXamlValidators, Content.Site.XamlDictionaries);
                    if (!vcontext.IsValid)
                    {
                        foreach (ValidationError ve in vcontext.Result.Errors)
                        {
                            errors.Error(ve.Definition.PropertyName, values[ve.Definition.PropertyName], ve.Message);
                        }
                        foreach(string msg in vcontext.Messages)
                        {
                            errors.ErrorForModel(msg);
                        }
                    }
                }
                catch (Exception exp)
                {
                    errors.ErrorForModel(String.Format(ArticleStrings.CustomValidationFailed, exp.Message));
                }
            }
        }

        public static void TestRemoteValidation()
        {
            RemoteValidationContext rvc = new RemoteValidationContext();
        }

        public static string GetDynamicColumnName(Field field, Dictionary<int, int> relationCounters, bool useFormName = false)
        {
            int relationId = (field.RelationId.HasValue) ? field.RelationId.Value : 0;
            if (field.Type.Name == Constants.FieldTypeName.Relation && relationId != 0)
            {
                int currentCount = 1;
                if (relationCounters.ContainsKey(relationId))
                {
                    currentCount = relationCounters[relationId];
                    currentCount++;
                }
                relationCounters[relationId] = currentCount;
                string countSuffix = (currentCount == 1) ? "" : "_" + currentCount;

                string result = "rel_field_" + relationId + countSuffix;
                if (field.Relation.ExactType == FieldExactTypes.O2MRelation)
                {
                    result += "_r1";
                }
                return result;
            }
            else
            {
                return (useFormName) ? field.FormName : field.Name;
            }
        }

        public void UpdateFieldValues(Dictionary<string, string> newValues)
        {
            foreach (FieldValue pair in VariationEditableFieldValues)
            {
                var value = newValues[pair.Field.FormName];
                if (pair.Field.ExactType == FieldExactTypes.Boolean)
                {
                    value = String.IsNullOrEmpty(value) ? "0" : value;
                    bool isBool = false;
                    bool boolValue = Boolean.TryParse(value, out isBool);
                    value = ((isBool) ? Utils.Converter.ToInt32(boolValue) : Utils.Converter.ToInt32(value)).ToString();
                }
                pair.UpdateValue(value);
            }
        }

        public void UpdateFieldValuesWithAggregated(Dictionary<string, string> newValues)
        {
            UpdateFieldValues(newValues);
            UpdateAggregatedCollection();
            foreach (var aggArticle in AggregatedArticles)
            {
                aggArticle.UpdateFieldValues(newValues);
            }
        }

        public void UpdateAggregatedCollection()
        {
            IEnumerable<int> classifierValues = this.FieldValues
                .Where(v => v.Field.IsClassifier)
                .Select(v => Converter.ToNullableInt32(v.Value))
                .Where(v => v.HasValue)
                .Select(v => v.Value);

            IEnumerable<Article> newAggregatedArticlesCollection = classifierValues.Select(cv => GetAggregatedArticleByClassifier(cv)).ToArray();
            AggregatedArticles = newAggregatedArticlesCollection;
        }

        /// <summary>
        /// Возвращает существующую или новую агрегированную статью
        /// </summary>
        /// <param name="classifierValue"></param>
        /// <returns></returns>
        public Article GetAggregatedArticleByClassifier(int classifierValue)
        {
            if (classifierValue > 0)
            {
                // так как у статьи не может быть более одной агрегированной статьи из агрегированного контента, то достаточно условия a.ContentId == classifierValue
                Article aggregated = AggregatedArticles.Where(a => a.ContentId == classifierValue).FirstOrDefault() ?? CreateNew(classifierValue);
                aggregated.Archived = Archived;
                aggregated.ViewType = ViewType;
                return aggregated;
            }
            else
                return null;
        }

        /// <summary>
        /// Генерирует пустую статью для показа
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns>пустая статья</returns>
        public static Article CreateNew(int contentId)
        {
            return CreateNew(contentId, null, null, null);
        }

        public static Article CreateNew(int contentId, int? fieldId, int? articleId, bool? isChild)
        {
            Content content = ContentRepository.GetById(contentId);
            if (content == null)
                throw new Exception(String.Format(ContentStrings.ContentNotFound, contentId));
            Article article = content.CreateArticle(GetPredefinedValues(fieldId, articleId, isChild));
            article.DefineViewType();
            return article;
        }

        private static Dictionary<string, string> GetPredefinedValues(int? fieldId, int? articleId, bool? isChild)
        {
            Dictionary<string, string> predefinedValues = new Dictionary<string, string>();
            int currentId = 0;
            if (fieldId.HasValue && articleId.HasValue && fieldId.Value != 0 && articleId.Value != 0)
            {
                Field field = FieldRepository.GetById(fieldId.Value);
                if (field != null)
                {
                    if (field.ExactType == FieldExactTypes.M2ORelation && field.BackRelationId.HasValue)
                    {
                        currentId = field.BackRelationId.Value;
                    }
                    else if (field.ExactType == FieldExactTypes.M2MRelation && field.ContentLink.Symmetric)
                    {
                        if (field.RelateToContentId.Value == field.ContentId)
                        {
                            currentId = field.Id;
                        }
                        else if (field.M2MBackwardField != null)
                        {
                            currentId = field.M2MBackwardField.Id;
                        }
                    }

                    else if (field.ExactType == FieldExactTypes.O2MRelation && (isChild.HasValue && isChild.Value)) {
                        currentId = field.Id;
                    }
                }
                if (currentId != 0)
                    predefinedValues.Add("field_" + currentId, articleId.ToString());
            }
            return predefinedValues;
        }

        /// <summary>
        /// Генерирует пустую статью для сохранения
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns>пустая статья</returns>
        public static Article CreateNewForSave(int contentId)
        {
            Content content = ContentRepository.GetById(contentId);
            if (content == null)
                throw new Exception(String.Format(ContentStrings.ContentNotFound, contentId));
            Article article = content.CreateArticle();
            return article;
        }

        #region PrepareForCopy
        public void PrepareForCopy()
        {
            PrepareForCopy(true, false, null, ArticleClearType.EmptyValue);
        }

        public void PrepareForCopy(int[] clearFieldIds, ArticleClearType clearType)
        {
            PrepareForCopy(true, false, clearFieldIds, clearType);
        }

        public void PrepareForCopy(bool checkAggregated, bool resolveFieldConflicts)
        {
            PrepareForCopy(checkAggregated, resolveFieldConflicts, null, ArticleClearType.EmptyValue);
        }

        public void PrepareForCopy(bool checkAggregated, bool resolveFieldConflicts, int[] clearFieldIds, ArticleClearType clearType)
        {
            LoadFieldValues();
            if (checkAggregated)
            {
                foreach (var art in AggregatedArticles)
                {
                    art.PrepareForCopy(false, resolveFieldConflicts, clearFieldIds, clearType);
                }
            }
            Id = 0;
            LockedByUser = null;
            LockedBy = 0;
            SetDefaultStatusAndVisibility();
            ClearM2OFields();
            ClearOldIds();
            ClearFields(clearFieldIds, clearType);
            if (resolveFieldConflicts)
                ResolveFieldConflicts();
        }
        #endregion

        internal void LockForUpdate()
        {
            ArticleRepository.LockForUpdate(Id);
        }


        public Article Persist(bool disableNotifications)
        {

            Article previousArticle = null;

            if (!IsNew && !IsAggregated && !IsVariation)
            {
                LockForUpdate();

                previousArticle = ArticleRepository.GetById(Id);
                if (previousArticle == null)
                    throw new Exception(String.Format(ArticleStrings.ArticleNotFound, Id));

                previousArticle.CreateVersion();

                RemoveAggregates(this, previousArticle);

                RemoveVariations();
            }

            FixNonUsedStatus(true);

            ReplaceAllUrlsToPlaceHolders();
            OptimizeForHierarchy();
            Article result = (IsNew) ? ArticleRepository.Save(this) : ArticleRepository.Update(this);
            result.BackupCurrentFiles();
            result.CreateDynamicImages();

            if (IsNew && !IsAggregated && !IsVariation)
            {
                result.CopyParentPermissions();
            }

            if (!IsAggregated)
            {

                foreach (var a in AggregatedArticles)
                {
                    a.AggregateTo(result);
                    a.Persist(disableNotifications);
                }

                if (!IsVariation)
                {
                    foreach (var a in VariationArticles.Where(n => n.UseInVariationUpdate))
                    {
                        a.VariateTo(result);
                        a.Persist(disableNotifications);
                    }

                    if (IsNew)
                        result.SendNotificationOneWay(NotificationCode.Create, disableNotifications);
                    else if (previousArticle.StatusTypeId != StatusTypeId)
                    {
                        result.SendNotificationOneWay(String.Format("{0};{1}", NotificationCode.Update, NotificationCode.ChangeStatus), disableNotifications);
                        string message = ReplaceCommentLink(Comment);
                        int systemStatusType = GetSystemStatusType(previousArticle.StatusTypeId, StatusTypeId, ref message);
                        WorkflowRepository.SaveHistoryStatus(Id, systemStatusType, message, QPContext.CurrentUserId);
                    }
                    else
                        result.SendNotificationOneWay(NotificationCode.Update, disableNotifications);

                }
            }
            return result;
        }

        private void OptimizeForHierarchy()
        {
            FieldValues
                .Where(n => n.Field.ExactType == FieldExactTypes.M2MRelation && n.Field.OptimizeForHierarchy)
                .ForEach(n => new OptimizeHierarchyHelper(n).Process());
        }

        public Article GetVariationByContext(string context)
        {
            if (String.IsNullOrEmpty(context))
                throw new ArgumentException("context is empty");
            else
            {
                Article result = VariationArticles.SingleOrDefault(n => n.VariationContext == context);
                if (result == null)
                {
                    result = CreateVariationWithFieldValues(GetFieldValuesByContext(context));
                }
                return result;
            }
        }

        private string ReplaceCommentLink(string comment) {
            if (!String.IsNullOrEmpty(comment))
                return Regex.Replace(comment, @"((http|https)://[\w\.-0-9:?=&_]*)", "<a href=\"$1\" target=\"_blank\">$1</a>");
                else
                return String.Empty;
        }
        private int GetSystemStatusType(int previousStatusTypeId, int currentStatusTypeId, ref string message) {
            int historyStatusId;
            IEnumerable<StatusType> statusTypes = StatusTypeRepository.GetList(new int[] { previousStatusTypeId, currentStatusTypeId }).ToList();
            StatusType previousStatus = statusTypes.Where(s => s.Id == previousStatusTypeId).FirstOrDefault();
            StatusType currentStatus = statusTypes.Where(s => s.Id == currentStatusTypeId).FirstOrDefault();

            if (previousStatus.Weight > currentStatus.Weight)
            {
                historyStatusId = (int)Constants.SystemStatusType.ForcedDemoting;
                message = String.Format("The article status was demoted from [{0}] to [{1}]. Comment: {2}", previousStatus.Name, currentStatus.Name, message);
            }
            else
            {
                historyStatusId = (int)Constants.SystemStatusType.ForcedPromoting;
                message = String.Format("The article status was promoted from [{0}] to [{1}]. Comment: {2}", previousStatus.Name, currentStatus.Name, message);
            }

            return historyStatusId;
        }

        /// <summary>
        /// Проверяет можно ли для статей контента выполнять изменяющие операции
        /// </summary>
        /// <param name="content"></param>
        /// <param name="boundToExternal"></param>
        /// <returns></returns>
        public bool IsArticleChangingActionsAllowed(bool? boundToExternal)
        {
            return Content.IsArticleChangingActionsAllowed(boundToExternal);
        }
        #endregion

        #region internal

        public void DefineViewType()
        {
            var ids = new int[] { Id };
            if (this.LockedByAnyoneElse)
                ViewType = ArticleViewType.LockedByOtherUser;
            else if (!IsNew && !IsUpdatable)
                ViewType = ArticleViewType.ReadOnlyBecauseOfSecurity;
            else if (!this.IsUpdatableWithWorkflow)
                ViewType = ArticleViewType.ReadOnlyBecauseOfWorkflow;
            else if (!IsNew && !CheckRelationSecurity())
                ViewType = ArticleViewType.ReadOnlyBecauseOfRelationSecurity;
            else
                ViewType = ArticleViewType.Normal;
        }

        private bool CheckRelationSecurity()
        {
            if (GetRelationSecurityFields().Any())
                return ArticleRepository.CheckRelationSecurity(ContentId, new int[] { Id }, false)[Id];
            else
                return true;
        }

        public IEnumerable<FieldValue> GetRelationSecurityFields()
        {
            return FieldValues.Concat(AggregatedArticles.SelectMany(n => n.FieldValues)).Where(n => n.Field.UseRelationSecurity);
        }

        /// <summary>
        /// Принудительная загрузка значений полей
        /// </summary>
        internal void LoadFieldValues()
        {
            var result = FieldValues;
        }

        /// <summary>
        /// Принудительная загрузка агрегированных статей
        /// /// </summary>
        internal void LoadAggregatedArticles()
        {
            foreach (var art in AggregatedArticles)
                art.LoadFieldValues();
        }


        /// <summary>
        /// Инициализирует дефолтный статус и Visible
        /// </summary>
        internal void SetDefaultStatusAndVisibility()
        {
            bool isWorkflowAssigned = Content.WorkflowBinding.IsAssigned;
            Visible = isWorkflowAssigned;
            Status = (isWorkflowAssigned) ? StatusType.GetNone(Content.SiteId) : StatusType.GetPublished(Content.SiteId);
            StatusTypeId = Status.Id;
        }

        /// <summary>
        /// Исправляет статус статьи на допустимый (недопустимый статус может появиться в результате изменения Workflow или Binding)
        /// <param name="fixNotAssignedWorkflow">исправлять ли в случае неназначенного Workflow</param>
        /// </summary>
        internal void FixNonUsedStatus(bool fixNotAssignedWorkflow)
        {
            if (Workflow.IsAssigned)
            {
                if (!Workflow.UseStatus(Status.Id) && Status.Id != StatusType.GetNone(Content.SiteId).Id)
                {
                    Status = Workflow.GetClosestStatus(Status.Weight);
                    StatusTypeId = Status.Id;
                }
            }
            else if (fixNotAssignedWorkflow)
            {
                Status = StatusType.GetPublished(Content.SiteId);
                StatusTypeId = Status.Id;
            }
        }

        /// <summary>
        /// Pазрешает конфликты уникальности при копировании
        /// </summary>
        internal void ResolveFieldConflicts()
        {
            IEnumerable<ContentConstraint> constraints = ContentConstraintRepository.GetConstraintsByContentId(ContentId);
            foreach (ContentConstraint constraint in constraints)
            {
                List<FieldValue> fieldValuesToResolve = constraint.Filter(FieldValues);
                List<FieldValue> currentFieldValues;
                int step = 0;
                do
                {
                    step++;
                    currentFieldValues = MutateHelper.MutateFieldValues(fieldValuesToResolve, step);
                }

                while (!ArticleRepository.ValidateUnique(currentFieldValues));
                MergeWithFieldValues(currentFieldValues);
            }
        }

        /// <summary>
        /// Получение библиотеки для версии статьи
        /// </summary>
        /// <param name="newVersionId">ID версии</param>
        /// <returns>библиотека</returns>
        internal PathInfo GetVersionPathInfo(int newVersionId)
        {
            return Content.GetVersionPathInfo(newVersionId);
        }

        /// <summary>
        /// Получение списка значений полей
        /// </summary>
        /// <param name="data">DataRow значение</param>
        /// <param name="fields">список полей</param>
        /// <param name="article">ссылка на статью</param>
        /// <param name="versionId">ID версии (необязателен)</param>
        /// <returns>список значений полей</returns>
        internal static List<FieldValue> GetFieldValues(DataRow data, IEnumerable<Field> fields, Article article, int versionId = 0, string contentPrefix=null)
        {
            if (data == null) throw new Exception(String.Format(ArticleStrings.ArticleNotFoundInTheContent, article.Id, article.DisplayContentId));
            List<FieldValue> result = new List<FieldValue>();
            foreach (Field field in fields)
            {
                string fullFieldName = String.IsNullOrWhiteSpace(contentPrefix) ? field.Name : String.Format("{0}.{1}", contentPrefix, field.Name);
                if (!data.Table.Columns.Contains(fullFieldName)) throw new Exception(String.Format(ArticleStrings.FieldNotFound, fullFieldName, article.DisplayContentId));

                object objectValue = null;

                if (field.RelationType == RelationType.ManyToMany)
                {
                    if (!article.IsNew)
                    {
                        if (versionId != 0)
                            objectValue = ArticleVersionRepository.GetLinkedItems(versionId, field.Id);
                        else
                            objectValue = ArticleRepository.GetLinkedItems(field.GetBaseField(article.Id).LinkId.Value, article.Id);
                    }

                    else
                    {
                        objectValue = string.Join(",", field.DefaultArticleIds.ToArray());
                    }
                }
                else if (field.RelationType == RelationType.ManyToOne)
                {
                    if (versionId != 0)
                        objectValue = ArticleVersionRepository.GetRelatedItems(versionId, field.Id);
                    else
                        objectValue = ArticleRepository.GetRelatedItems(field.GetBaseField(article.Id).BackRelationId.Value, article.Id);
                }
                else
                {
                    objectValue = data[fullFieldName];
                    if (DBNull.Value.Equals(objectValue))
                    {
                        objectValue = null;
                    }
                }

                if (field.ReplaceUrls && objectValue != null)
                {
                    objectValue = article.ReplacePlaceHoldersToUrls(objectValue.ToString());
                }

                if (article.IsNew)
                {
                    if (field.IsDateTime && field.Required && objectValue == null)
                    {
                        objectValue = Converter.ToDbDateTimeString(DateTime.Now);
                    }

                    if (article.PredefinedValues.ContainsKey(field.FormName))
                    {
                        objectValue = article.PredefinedValues[field.FormName];
                    }
                }

                result.Add(new FieldValue { Field = field, ObjectValue = objectValue, Article = article });
            }
            return result;
        }

        /// <summary>
        /// Получение списка значений полей
        /// </summary>
        /// <param name="data">DataRow значение</param>
        /// <param name="fields">список полей</param>
        /// <param name="article">ссылка на статью</param>
        /// <param name="versionId">ID версии (необязателен)</param>
        /// <returns>список значений полей</returns>
        internal static void LoadFieldValuesForArticles(DataTable data, IEnumerable<Field> fields, IEnumerable<Article> articles, int contentId)
        {
            int[] ids = articles.Select(n => n.Id).ToArray();
            Dictionary<int, Dictionary<int, string>> itemsForRelations = new Dictionary<int, Dictionary<int, string>>();
            if (data == null) throw new Exception(String.Join(",", ids.ToString()));
            foreach (Field field in fields)
            {
                if (!data.Columns.Contains(field.Name)) throw new Exception(String.Format(ArticleStrings.FieldNotFound, field.Name, contentId));

                if (field.RelationType == RelationType.ManyToMany)
                {
                    itemsForRelations.Add(field.Id, ArticleRepository.GetLinkedItemsMultiple(field.LinkId.Value, ids));
                }
                else if (field.RelationType == RelationType.ManyToOne)
                {
                    itemsForRelations.Add(field.Id, ArticleRepository.GetRelatedItemsMultiple(field.BackRelationId.Value, ids));
                }
            }

            foreach (DataRow dr in data.Rows)
            {
                List<FieldValue> result = new List<FieldValue>();
                int id = (int)(decimal)dr["content_item_id"];
                Article article = articles.Single(n => n.Id == id);

                int statusTypeId = (int)(decimal)dr["status_type_id"];
                if (article.StatusTypeId != statusTypeId)
                {
                    article.StatusTypeId = statusTypeId;
                    article.Status = StatusTypeRepository.GetById(statusTypeId);
                }

                foreach (Field field in fields)
                {
                    object objectValue = null;
                    if (field.RelationType == RelationType.ManyToMany || field.RelationType == RelationType.ManyToOne)
                    {
                        var dict = itemsForRelations[field.Id];
                        if (dict != null && dict.ContainsKey(id))
                            objectValue = dict[id];
                    }
                    else
                    {
                        objectValue = dr[field.Name];

                        if (DBNull.Value.Equals(objectValue))
                        {
                            objectValue = null;
                        }

                        if (field.ReplaceUrls && objectValue != null)
                        {
                            objectValue = article.ReplacePlaceHoldersToUrls(objectValue.ToString());
                        }
                    }

                    result.Add(new FieldValue { Field = field, ObjectValue = objectValue, Article = article });
                }
                article.FieldValues = result;
            }

        }

        private string ReplacePlaceHoldersToUrls(string value)
        {
            return PlaceHolderHelper.ReplacePlaceHoldersToUrls(Content.Site, value);
        }

        private string ReplaceUrlsToPlaceHolders(string value)
        {
            return PlaceHolderHelper.ReplaceUrlsToPlaceHolders(Content.Site, value);
        }

        public void ReplaceAllUrlsToPlaceHolders()
        {
            foreach (FieldValue item in FieldValues)
            {
                if (item.Field.ReplaceUrls)
                {
                    item.Value = ReplaceUrlsToPlaceHolders(item.Value);
                }
            }
        }

        internal INotificationService GetNotificationServiceClient()
        {
            return new NotificationSimpleServiceClient();
        }


        /// <summary>
        /// Отправляет асинхронное уведомление
        /// </summary>
        /// <param name="code">код события</param>
        internal void SendNotificationOneWay(string code, bool disableNotifications)
        {
            if (!disableNotifications && this.Content.HasNotifications(code))
                using (var client = GetNotificationServiceClient())
                {
                    client.SendNotificationOneWay(QPContext.CurrentDbConnectionString, Id, code);
                }
        }


        /// <summary>
        /// Отправляет синхронное уведомление
        /// </summary>
        /// <param name="code">код события</param>
        internal void SendNotification(string code, bool disableNotifications)
        {
            if (!disableNotifications)
                SendNotification(code, QPContext.CurrentDbConnectionString, false);
        }

        internal void SendNotification(string code, string connectionString, bool useDirect)
        {
            if (useDirect || this.Content.HasNotifications(code))
                using (var client = GetNotificationServiceClient())
                {
                    client.SendNotification(connectionString, Id, code);
                }
        }

        internal bool CheckRelationCondition(string relCondition)
        {
            return ArticleRepository.CheckRelationCondition(Id, ContentId, relCondition);
        }


        /// <summary>
        /// Создает версию статьи
        /// </summary>
        internal void CreateVersion()
        {
            if (Content.UseVersionControl)
            {
                ArticleVersion earliestVersion = ArticleVersionRepository.GetEarliest(Id);
                ArticleVersionRepository.Create(Id);
                ArticleVersion newVersion = ArticleVersionRepository.GetLatest(Id);

                Directory.CreateDirectory(BackupPath);
                Directory.CreateDirectory(newVersion.PathInfo.Path);

                int count = ArticleVersionRepository.GetVersionsCount(Id);
                if (count == Content.MaxNumOfStoredVersions)
                {
                    Folder.ForceDelete(earliestVersion.PathInfo.Path);
                }

                CopyArticleFiles(CopyFilesMode.ToVersionFolder, BackupPath, newVersion.PathInfo.Path);
                foreach (var a in AggregatedArticles)
                {
                    a.CopyArticleFiles(CopyFilesMode.ToVersionFolder, a.BackupPath, newVersion.PathInfo.Path);
                }

                if (Directory.Exists(newVersion.PathInfo.Path) && !Directory.EnumerateFiles(newVersion.PathInfo.Path, "*.*", SearchOption.AllDirectories).Any())
                    Folder.ForceDelete(newVersion.PathInfo.Path);
            }


        }

        /// <summary>
        /// Сохраняет файлы текущей версии в специальной папке
        /// </summary>
        internal void BackupCurrentFiles()
        {
            Directory.CreateDirectory(BackupPath);
            CopyArticleFiles(CopyFilesMode.ToBackupFolder, BackupPath);
        }

        /// <summary>
        /// Восстанавливает файлы версии статьи
        /// </summary>
        /// <param name="id">ID версии</param>
        internal void RestoreCurrentFiles(int id)
        {
            string versionPath = GetVersionPathInfo(id).Path;
            CopyArticleFiles(CopyFilesMode.FromVersionFolder, BackupPath, versionPath);
            foreach (var a in AggregatedArticles)
            {
                a.CopyArticleFiles(CopyFilesMode.FromVersionFolder, a.BackupPath, versionPath);
            }
        }

        /// <summary>
        /// Удаляет все папки версий для статьи
        /// </summary>
        internal void RemoveAllVersionFolders()
        {
            foreach (int id in ArticleVersionRepository.GetIds(Id))
                RemoveVersionFolder(id);
        }

        /// <summary>
        /// Удаляет папку версии статьи
        /// </summary>
        /// <param name="id">ID версии</param>
        internal void RemoveVersionFolder(int id)
        {
            Folder.ForceDelete(Content.GetVersionPathInfo(id).Path);
        }

        /// <summary>
        /// Создает динамические изображения для статьи
        /// </summary>
        internal void CreateDynamicImages()
        {
            foreach (FieldValue item in FieldValues.Where(n => n.Field.Type.Name == FieldTypeName.Image && !String.IsNullOrEmpty(n.Value)))
            {
                foreach (Field field in item.Field.GetDynamicImages())
                {
                    field.DynamicImage.CreateDynamicImage(item.Field.PathInfo.GetPath(item.Value), item.Value);
                }

            }
        }

        #region Clear fields
        internal void ClearM2OFields()
        {
            foreach (var fieldValue in FieldValues.Where(n => n.Field.ExactType == FieldExactTypes.M2ORelation))
            {
                fieldValue.Value = "";
            }
        }

        public void ClearOldIds()
        {
            foreach (var fieldValue in FieldValues.Where(n => n.Field.Name.StartsWith("Old") && n.Field.Name.EndsWith("Id")))
            {
                fieldValue.Value = "";
            }
        }

        /// <summary>
        /// Очищает значения указанных полей для статьи
        /// </summary>
        /// <param name="fieldValuepredicate">Предикат для выбора полей</param>
        /// <param name="clearType">Тип очистки</param>
        public void ClearFields(Func<FieldValue, bool> fieldValuepredicate, ArticleClearType clearType)
        {
            foreach (var fieldValue in FieldValues.Where(fieldValuepredicate))
            {
                if (clearType == ArticleClearType.DefaultValue)
                {
                    fieldValue.Value = fieldValue.Field.DefaultValue;
                }
                else
                {
                    fieldValue.Value = string.Empty;
                }
            }
        }

        /// <summary>
        /// Очищает значения указанных полей для статьи
        /// </summary>
        /// <param name="fieldIds">Идентификаторы полей</param>
        /// <param name="clearType">Тип очистки</param>
        public void ClearFields(int[] fieldIds, ArticleClearType clearType)
        {
            if (fieldIds != null && fieldIds.Length > 0)
            {
                ClearFields(fv => fieldIds.Contains(fv.Field.Id), clearType);
            }
        }

        /// <summary>
        /// Очищает значения указанных полей для статьи
        /// </summary>
        /// <param name="fieldNames">Названия полей</param>
        /// <param name="clearType">Тип очистки</param>
        public void ClearFields(string[] fieldNames, ArticleClearType clearType)
        {
            if (fieldNames != null && fieldNames.Length > 0)
            {
                ClearFields(fv => fieldNames.Contains(fv.Field.Name), clearType);
            }
        }
        #endregion

        /// <summary>
        /// Связывает статью как агрегированную с корневой статьей
        /// </summary>
        /// <param name="rootArticle"></param>
        internal void AggregateTo(Article rootArticle)
        {
            if (rootArticle.IsNew)
                throw new ApplicationException("Root article has to be saved.");

            FieldValue aggregatorValue = this.FieldValues.FirstOrDefault(f => f.Field.Aggregated);
            if (aggregatorValue == null)
                throw new ApplicationException("There is no aggregated field in article.");
            aggregatorValue.Value = rootArticle.Id.ToString();


            CopyServiceFields(rootArticle);
        }

        internal void VariateTo(Article rootArticle)
        {
            if (rootArticle.IsNew)
                throw new ApplicationException("Root article has to be saved.");

            FieldValue variationValue = this.FieldValues.FirstOrDefault(f => f.Field.UseForVariations);
            if (variationValue == null)
                throw new ApplicationException("There is no variation field in article.");
            variationValue.Value = rootArticle.Id.ToString();

            CopyServiceFields(rootArticle);
        }

        internal void CopyServiceFields(Article fromArticle)
        {
            Visible = fromArticle.Visible;
            Delayed = fromArticle.Delayed;
            CancelSplit = fromArticle.CancelSplit;
            Schedule.CopyFrom(fromArticle.Schedule);
            StatusTypeId = fromArticle.StatusTypeId;
            LastModifiedBy = fromArticle.LastModifiedBy;
        }

        internal void ValidateUnique(RulesException errors, int exceptFieldId = 0)
        {
            foreach (ContentConstraint constraint in Content.Constraints)
            {
                List<FieldValue> fieldValuesToTest = constraint.Filter(FieldValues);
                if (fieldValuesToTest[0].Field.Id != exceptFieldId)
                {
                    string conflictingIds, constraintToDisplay;
                    if (!ArticleRepository.ValidateUnique(fieldValuesToTest, out constraintToDisplay, out conflictingIds))
                    {
                        if (!constraint.IsComplex)
                            errors.Error(fieldValuesToTest[0].Field.FormName, fieldValuesToTest[0].Value, String.Format(ArticleStrings.NotUniqueValue, constraintToDisplay, conflictingIds));
                        else
                            errors.ErrorForModel(String.Format(ArticleStrings.UniqueConstraintViolation, constraintToDisplay, conflictingIds));
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет новые агрегированные статьи
        /// </summary>
        /// <param name="aggregatedArticles"></param>
        public void CopyAggregates(IEnumerable<Article> aggregatedArticles)
        {
            List<Article> result = new List<Article>();
            foreach (var a in aggregatedArticles)
            {
                a.AggregateTo(this);
                result.Add(ArticleRepository.Copy(a));
            }
            AggregatedArticles = result;
        }

        internal void CopyParentPermissions()
        {
            Field treeField = Content.TreeField;
            if (treeField != null && treeField.CopyPermissionsToChildren)
            {
                int parentId = Converter.ToInt32(FieldValues.Single(n => n.Field.Name == treeField.Name).Value, 0);
                if (parentId != 0)
                {
                    ArticleRepository.CopyPermissions(parentId, Id);
                }
            }
        }
        #endregion

        #region private

        private void MergeWithFieldValues(List<FieldValue> currentFieldValues)
        {
            foreach (FieldValue currentValue in currentFieldValues)
            {
                FieldValues.Single(n => n.Field == currentValue.Field).Value = currentValue.Value;
            }
        }

        private List<FieldValue> GetFieldValues()
        {
            DataRow data = ArticleRepository.GetData(Id, DisplayContentId);
            List<Field> fields = FieldRepository.GetFullList(DisplayContentId);
            return GetFieldValues(data, fields);
        }

        private List<FieldValue> GetFieldValues(DataRow data, List<Field> fields)
        {
            return Article.GetFieldValues(data, fields, this);
        }

        private void ValidateWorkflow(RulesException<Article> errors)
        {
            if (!IsUpdatableWithWorkflow)
                errors.CriticalErrorForModel(IsNew ? ArticleStrings.CannotAddBecauseOfWorkflow : ArticleStrings.CannotUpdateBecauseOfWorkflow);
        }

        private void ValidateRelationSecurity(RulesException<Article> errors)
        {
            if (!ArticleRepository.CheckRelationSecurity(this, false))
            {
                if (IsNew)
                    errors.CriticalErrorForModel(ArticleStrings.CannotAddBecauseOfRelationSecurity);
                else
                    errors.CriticalErrorForModel(ArticleStrings.CannotUpdateBecauseOfRelationSecurity);
            }
        }

        private void ValidateSchedule(RulesException<Article> errors, ArticleSchedule item)
        {
            if (item.ScheduleType == ScheduleTypeEnum.OneTimeEvent && !item.WithoutEndDate && item.StartDate >= item.EndDate)
                errors.ErrorFor(n => n.Schedule.EndDate, ArticleStrings.StartEndDates);
            // Recurring Mode
            if (item.ScheduleType == ScheduleTypeEnum.Recurring)
            {
                // Дата окончания повторений должна быть больше Даты начала повторений
                if (!item.Recurring.RepetitionNoEnd &&
                    item.Recurring.RepetitionEndDate.Date < item.Recurring.RepetitionStartDate.Date)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.RepetitionStartDate, ArticleStrings.RepetitionStartDateError);
                }

                if (item.Recurring.ScheduleRecurringValue < ScheduleValidationConstants.ScheduleRecurringMinValue ||
                    item.Recurring.ScheduleRecurringValue > ScheduleValidationConstants.ScheduleRecurringMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.ScheduleRecurringValue, ArticleStrings.ScheduleRecurringValueError);
                }

                if (item.Recurring.DayOfMonth < ScheduleValidationConstants.DayOfMonthMinValue ||
                    item.Recurring.DayOfMonth > ScheduleValidationConstants.DayOfMonthMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.DayOfMonth, ArticleStrings.DayOfMonthError);
                }

                if (item.Recurring.ShowLimitationType == ShowLimitationType.EndTime &&
                    item.Recurring.ShowEndTime < item.Recurring.ShowStartTime)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.ShowStartTime, ArticleStrings.ShowStartTimeError);
                }

                if (item.Recurring.DurationValue < ScheduleValidationConstants.DurationMinValue ||
                    item.Recurring.DurationValue > ScheduleValidationConstants.DurationMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.DurationValue, ArticleStrings.DurationValueError);
                }
            }
        }

        protected override void ValidateUnique(RulesException errors)
        {
            ValidateUnique(errors, 0);
        }

        private void CopyArticleFiles(CopyFilesMode mode, string currentVersionPath, string versionPath = "")
        {
            if (Content.UseVersionControl)
            {
                Action<string, string> copyFile = (src, dest) =>
                {
                    if (File.Exists(dest))
                    {
                        File.SetAttributes(dest, FileAttributes.Normal);
                    }
                    if (File.Exists(src))
                    {
                        File.Copy(src, dest, true);
                        File.SetAttributes(dest, FileAttributes.Normal);
                    }
                };

                foreach (FieldValue item in FieldValues)
                {
                    if (item.Field.UseVersionControl && (item.Field.Type.Name == FieldTypeName.Image || item.Field.Type.Name == FieldTypeName.File))
                    {

                        if (mode == CopyFilesMode.ToBackupFolder)
                        {
                            // не режем откуда, режем куда
                            string source = Path.Combine(item.Field.PathInfo.Path, item.Value);
                            string destination = Path.Combine(currentVersionPath, Path.GetFileName(item.Value));
                            copyFile(source, destination);
                        }
                        else if (mode == CopyFilesMode.ToVersionFolder)
                        {
                            // режем откуда, режем куда
                            string source = Path.Combine(currentVersionPath, Path.GetFileName(item.Value));
                            string destination = Path.Combine(versionPath, Path.GetFileName(item.Value));
                            copyFile(source, destination);
                        }
                        else if (mode == CopyFilesMode.FromVersionFolder)
                        {
                            // режем откуда, режем куда
                            string source = Path.Combine(versionPath, Path.GetFileName(item.Value));
                            string destination = Path.Combine(currentVersionPath, Path.GetFileName(item.Value));
                            copyFile(source, destination);
                            // режем откуда, не режем куда
                            destination = Path.Combine(item.Field.PathInfo.Path, item.Value);
                            copyFile(source, destination);
                        }
                    }
                }
            }
        }


        private IEnumerable<FieldValue> GetFieldValuesByContext(string context)
        {
            var contextIds = context.Split(",".ToCharArray());
            foreach (string id in contextIds)
            {
                int fieldId = 0;
                foreach (ArticleContextListItem li in ContextListItems)
                {
                    if (li.Ids.ContainsKey(id))
                    {
                        fieldId = li.FieldId;
                        break;
                    }
                }
                Field field = Content.Fields.Single(n => n.Id == fieldId);
                yield return new FieldValue { Field = field, Value = id };
            }
        }

        private Article CreateVariationWithFieldValues(IEnumerable<FieldValue> fieldValues)
        {
            var predefinedValues = fieldValues.ToDictionary(n => n.Field.FormName, n => n.Value);
            //predefinedValues.Add(Content.VariationField.FormName, Id.ToString());
            Article result = new Article(Content, predefinedValues);
            VariationArticles.Add(result);
            return result;
        }



        private static void RemoveAggregates(Article currentArticle, Article previousArticle)
        {
            LambdaEqualityComparer<Article> comparer = new LambdaEqualityComparer<Article>((x, y) => x.Id == y.Id, x => x.Id);
            // Какие агрегированные статьи удалить
            IEnumerable<Article> deletingAggregated = previousArticle.AggregatedArticles.Except(currentArticle.AggregatedArticles, comparer).ToArray();

            ArticleRepository.MultipleDelete(deletingAggregated.Select(a => a.Id));
        }

        private void RemoveVariations()
        {
            var baseArticles = VariationArticles.Where(n => !n.UseInVariationUpdate);
            int[] articlesToRemove = baseArticles
                .SelectMany(n => n.AggregatedArticles.Select(m => m.Id))
                .Union(baseArticles.Select(m => m.Id))
                .ToArray();
            ArticleRepository.MultipleDelete(articlesToRemove);
        }

        private IEnumerable<ArticleVariationListItem> LoadVariationListForClient()
        {
            int[] fieldIds = VariationContextFieldValues
                .Select(n => n.Field.Id)
                .ToArray()
            ;

            foreach (var article in VariationArticles.Concat(Enumerable.Repeat(this, 1)))
            {
                yield return new ArticleVariationListItem()
                {
                    Context = String.Join(",", article.FieldValues
                        .Where(n => fieldIds.Contains(n.Field.Id) && !String.IsNullOrEmpty(n.Value))
                        .OrderBy(n => n.Field.Order)
                        .Select(n => n.Value)
                    ),

                    Id = article.Id,

                    FieldValues = article.VariationEditableFieldValues.Union(
                            article.AggregatedArticles.SelectMany(n => n.VariationEditableFieldValues)
                        )
                        .ToDictionary(n => n.Field.FormName, n => n.Value)
                };
            }
        }

        private IEnumerable<ArticleContextListItem> LoadContextListForClient()
        {
            var arr = VariationContextFieldValues
                .Select(n => new { Id = n.Field.Id, ContentId = n.Field.RelateToContentId.Value })
                .ToArray();

            foreach (var item in arr)
            {
                var result = new ArticleContextListItem();
                result.HasHierarchy = ContentRepository.HasContentTreeField(item.ContentId);
                result.FieldId = item.Id;
                result.Ids = ArticleRepository.GetHierarchy(item.ContentId).ToDictionary(k => k.Key.ToString(), k => k.Value.ToString());
                yield return result;
            }
        }

        #endregion

        #endregion
    }

    #region enums
    public enum ArticleClearType
    {
        EmptyValue = 0,
        DefaultValue = 1
    }
    #endregion
}
