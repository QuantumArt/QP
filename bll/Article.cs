using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using QA.Validation.Xaml;
using QA.Validation.Xaml.Extensions.Rules;
using QP8.Infrastructure;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class Article : LockableEntityObject
    {
        private enum CopyFilesMode
        {
            ToBackupFolder,
            ToVersionFolder,
            FromVersionFolder
        }

        private string _name = string.Empty;
        private string _alias = string.Empty;
        private List<FieldValue> _fieldValues;
        private List<FieldValue> _liveFieldValues;
        private int _displayContentId;
        private ArticleSchedule _schedule;
        private bool _scheduleLoaded;
        private ArticleWorkflowBind _workflowBinding;
        private readonly InitPropertyValue<IEnumerable<Article>> _aggregatedArticles;
        private readonly InitPropertyValue<IEnumerable<Article>> _liveAggregatedArticles;
        private readonly InitPropertyValue<bool> _isUpdatableWithRelationSecurity;
        private readonly InitPropertyValue<bool> _isRemovableWithRelationSecurity;
        private readonly InitPropertyValue<StatusHistoryListItem> _statusHistoryListItem;
        private readonly InitPropertyValue<List<Article>> _variationArticles;
        private readonly InitPropertyValue<IEnumerable<ArticleVariationListItem>> _variationListItems;
        private readonly InitPropertyValue<IEnumerable<ArticleContextListItem>> _contextListItems;
        private int _parentContentId;

        internal Article()
        {
            _aggregatedArticles = new InitPropertyValue<IEnumerable<Article>>(() => ArticleRepository.LoadAggregatedArticles(this, false));
            _liveAggregatedArticles = new InitPropertyValue<IEnumerable<Article>>(() => ArticleRepository.LoadAggregatedArticles(this, true));
            _variationArticles = new InitPropertyValue<List<Article>>(() => ArticleRepository.LoadVariationArticles(this));
            _variationListItems = new InitPropertyValue<IEnumerable<ArticleVariationListItem>>(LoadVariationListForClient);
            _contextListItems = new InitPropertyValue<IEnumerable<ArticleContextListItem>>(LoadContextListForClient);
            _isUpdatableWithRelationSecurity = new InitPropertyValue<bool>(() => QPContext.IsAdmin || ArticleRepository.CheckRelationSecurity(this, false));
            _isRemovableWithRelationSecurity = new InitPropertyValue<bool>(() => QPContext.IsAdmin || ArticleRepository.CheckRelationSecurity(this, true));
            _statusHistoryListItem = new InitPropertyValue<StatusHistoryListItem>(() => ArticleRepository.GetStatusHistoryItem(Id));
            PredefinedValues = new Dictionary<string, string>();
            VariationsErrorModel = new Dictionary<string, RulesException<Article>>();
            CancelSplit = false;
            UseInVariationUpdate = false;
        }

        internal Article(Content content)
            : this()
        {
            ContentId = content.Id;
            Content = content;
            SetDefaultStatusAndVisibility();
        }

        internal Article(Content content, Dictionary<string, string> predefinedValues)
            : this(content)
        {
            PredefinedValues = predefinedValues;
        }

        public int ContentId { get; set; }

        public bool Visible { get; set; }

        public bool Archived { get; set; }

        public bool Splitted { get; set; }

        [LocalizedDisplayName("Status", NameResourceType = typeof(ArticleStrings))]
        public int StatusTypeId { get; set; }

        [LocalizedDisplayName("DelayPublication", NameResourceType = typeof(ArticleStrings))]
        public bool Delayed { get; set; }

        [LocalizedDisplayName("UniqueId", NameResourceType = typeof(ArticleStrings))]
        public Guid? UniqueId { get; set; }

        public string UniqueIdStr => UniqueId.HasValue ? UniqueId.ToString() : "";

        [LocalizedDisplayName("CancelSplit", NameResourceType = typeof(ArticleStrings))]
        public bool CancelSplit { get; set; }

        [LocalizedDisplayName("LeaveComment", NameResourceType = typeof(ArticleStrings))]
        public string Comment { get; set; }

        public Dictionary<string, string> PredefinedValues { get; set; }

        public override string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    var name = Id.ToString();
                    foreach (var value in FieldValues)
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
            set => _name = value;
        }

        public override bool IsReadOnly => ViewType != ArticleViewType.Normal && ViewType != ArticleViewType.PreviewVersion;

        public string AliasForTree
        {
            get
            {
                if (string.IsNullOrEmpty(_alias))
                {
                    var treeFieldId = ContentRepository.GetTreeFieldId(ContentId);
                    var displayTreeFieldName = FieldRepository.GetById(treeFieldId).Relation.Name;
                    _alias = FieldValues.Single(n => n.Field.Name == displayTreeFieldName).Value;
                }

                return _alias;
            }
            set => _alias = value;
        }

        public int DisplayContentId
        {
            get => _displayContentId == 0 ? ContentId : _displayContentId;
            set => _displayContentId = value;
        }

        public bool UseInVariationUpdate { get; set; }

        public Dictionary<string, RulesException<Article>> VariationsErrorModel { get; set; }

        internal string BackupPath => GetVersionPathInfo(ArticleVersion.CurrentVersionId).Path;

        public override string LockedByAnyoneElseMessage => ArticleStrings.LockedByAnyoneElse;

        public override string CannotAddBecauseOfSecurityMessage => ArticleStrings.CannotAddBecauseOfSecurity;

        public override string CannotUpdateBecauseOfSecurityMessage => ArticleStrings.CannotUpdateBecauseOfSecurity;

        public string DisplayContentName => DisplayContentId == ContentId ? Content?.Name : ContentRepository.GetById(DisplayContentId).Name;

        public bool IsUpdatableWithWorkflow => Workflow.CurrentUserCanUpdateArticles;

        public bool IsPublishableWithWorkflow => Workflow.CurrentUserCanPublishArticles && !WorkflowBinding.IsAssigned;

        public bool IsRemovableWithWorkflow => Workflow.CurrentUserCanRemoveArticles;

        public bool IsUpdatableWithRelationSecurity
        {
            get => _isUpdatableWithRelationSecurity.Value;
            set => _isUpdatableWithRelationSecurity.Value = value;
        }

        public bool IsRemovableWithRelationSecurity
        {
            get => _isRemovableWithRelationSecurity.Value;
            set => _isRemovableWithRelationSecurity.Value = value;
        }

        public StatusHistoryListItem StatusHistoryListItem
        {
            get => _statusHistoryListItem.Value;
            set => _statusHistoryListItem.Value = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.Article;

        public bool UseVariations => Content.VariationField != null;

        public IEnumerable<FieldValue> VariationContextFieldValues
        {
            get { return FieldValues.Where(n => n.Field.UseForContext).OrderBy(n => n.Field.Order); }
        }

        public IEnumerable<FieldValue> VariationEditableFieldValues
        {
            get { return FieldValues.Where(n => !n.Field.UseForContext && !n.Field.UseForVariations); }
        }

        public string VariationContext
        {
            get
            {
                return string.Join(",", VariationContextFieldValues
                    .Where(n => !string.IsNullOrEmpty(n.Value))
                    .Select(n => n.Value)
                );
            }
        }

        public string VariationContextDisplay
        {
            get
            {
                return string.Join(", ", VariationContextFieldValues
                    .Where(n => !string.IsNullOrEmpty(n.Value))
                    .Select(n => ArticleRepository.GetById(int.Parse(n.Value)).Name)
                );
            }
        }

        private IEnumerable<int> SelfAndAggregatedIds
        {
            get
            {
                return
                    Enumerable.Repeat(Id, 1)
                        .Union(AggregatedArticles.Select(n => n.Id))
                        .Union(LiveAggregatedArticles.Select(n => n.Id))
                    ;
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

        public bool IsAggregated
        {
            get { return FieldValues.Any(v => v.Field.Aggregated); }
        }

        public bool IsVariation => !string.IsNullOrEmpty(VariationContext);

        public Content Content { get; set; }

        public StatusType Status { get; set; }

        public override EntityObject Parent => Content;

        public ArticleWorkflowBind WorkflowBinding
        {
            get => _workflowBinding ?? (_workflowBinding = WorkflowRepository.GetArticleWorkflow(this));
            set => _workflowBinding = value;
        }

        [JsonIgnore]
        public List<FieldValue> FieldValues
        {
            get => _fieldValues ?? LoadFieldValues();
            set => _fieldValues = value;
        }

        [JsonIgnore]
        public List<FieldValue> LiveFieldValues
        {
            get => _liveFieldValues ?? LoadLiveFieldValues();
            set => _liveFieldValues = value;
        }

        internal List<FieldValue> LoadLiveFieldValues(bool excludeArchive = false)
        {
            if (_liveFieldValues == null)
            {
                var fields = FieldRepository.GetFullList(DisplayContentId);
                if (_liveFieldValues == null)
                {
                    var data = ArticleRepository.GetData(Id, DisplayContentId, true, excludeArchive);
                    _liveFieldValues = GetFieldValues(data, fields, this, 0, null, excludeArchive);
                }
            }
            return _liveFieldValues;
        }

        internal List<FieldValue> LoadFieldValues(bool excludeArchive = false)
        {
            if (_fieldValues == null)
            {
                var fields = FieldRepository.GetFullList(DisplayContentId);
                if (_fieldValues == null)
                {
                    var data = ArticleRepository.GetData(Id, DisplayContentId, QPContext.IsLive, excludeArchive);
                    _fieldValues = GetFieldValues(data, fields, this, 0, null, excludeArchive);

                }
            }
            return _fieldValues;
        }

        public ArticleViewType ViewType { get; set; } = ArticleViewType.Normal;

        public ArticleSchedule Schedule
        {
            get
            {
                if (_schedule == null && !_scheduleLoaded)
                {
                    _schedule = ScheduleRepository.GetSchedule(this);
                    _scheduleLoaded = true;
                }

                return _schedule;
            }
            set
            {
                _schedule = value;
                _scheduleLoaded = true;
            }
        }

        public WorkflowBind Workflow => WorkflowBinding.IsAssigned ? WorkflowBinding : (WorkflowBind)Content.WorkflowBinding;

        public IEnumerable<Article> AggregatedArticles
        {
            get => _aggregatedArticles.Value;
            set => _aggregatedArticles.Value = value;
        }

        public IEnumerable<Article> LiveAggregatedArticles
        {
            get => _liveAggregatedArticles.Value;
            set => _liveAggregatedArticles.Value = value;
        }

        public List<Article> VariationArticles
        {
            get => _variationArticles.Value;
            set => _variationArticles.Value = value;
        }

        public IEnumerable<ArticleVariationListItem> VariationListItems
        {
            get => _variationListItems.Value;
            set => _variationListItems.Value = value;
        }

        public IEnumerable<ArticleContextListItem> ContextListItems
        {
            get => _contextListItems.Value;
            set => _contextListItems.Value = value;
        }

        public int CollaborativePublishedArticle { get; set; }

        public int ParentContentId
        {
            get => _parentContentId != 0 || CollaborativePublishedArticle == 0 ? _parentContentId : GetContentIdForArticle();
            set => _parentContentId = value;
        }

        public override void Validate()
        {
            var errors = new RulesException<Article>();
            base.Validate(errors);

            ValidateWorkflow(errors);
            ValidateRelationSecurity(errors);
            ValidateFields(errors);
            ValidateAggregated(errors);
            ValidateXaml(errors, QPContext.CurrentCustomerCode);
            ValidateSchedule(errors, Schedule);

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        public static void ValidateXamlById(int articleId, RulesException errors, string customerCode, bool persistChanges)
        {
            var article = ArticleRepository.GetById(articleId);
            var hasChanges = article.ValidateXaml(errors, customerCode ?? QPContext.CurrentCustomerCode);
            if (hasChanges && persistChanges)
            {
                article.Persist(true);
            }
        }

        public static string GetDynamicColumnName(Field field, Dictionary<int, int> relationCounters, bool useFormName = false)
        {
            var relationId = field.RelationId ?? 0;
            if (field.Type.Name == FieldTypeName.Relation && relationId != 0)
            {
                var currentCount = 1;
                if (relationCounters.ContainsKey(relationId))
                {
                    currentCount = relationCounters[relationId];
                    currentCount++;
                }

                relationCounters[relationId] = currentCount;
                var countSuffix = currentCount == 1 ? string.Empty : "_" + currentCount;
                var result = "rel_field_" + relationId + countSuffix;
                if (field.Relation.ExactType == FieldExactTypes.O2MRelation)
                {
                    result += "_r1";
                }

                return result;
            }

            return useFormName ? field.FormName : field.Name;
        }

        public void UpdateFieldValues(Dictionary<string, string> newValues)
        {
            foreach (var pair in VariationEditableFieldValues)
            {
                var value = newValues[pair.Field.FormName];
                if (pair.Field.ExactType == FieldExactTypes.Boolean)
                {
                    value = string.IsNullOrEmpty(value) ? "0" : value;
                    var boolValue = bool.TryParse(value, out var isBool);
                    value = (isBool ? Converter.ToInt32(boolValue) : Converter.ToInt32(value)).ToString();
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

        public static int[] GetClassifierValues(List<FieldValue> fieldValues)
        {
            return fieldValues
                .Where(v => v.Field.IsClassifier)
                .Select(v => Converter.ToNullableInt32(v.Value))
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToArray();
        }

        public void UpdateAggregatedCollection()
        {
            AggregatedArticles = GetClassifierValues(FieldValues).Select(GetAggregatedArticleByClassifier).ToArray();
        }

        public Article GetAggregatedArticleByClassifier(int classifierValue)
        {
            if (classifierValue > 0)
            {
                var aggregated = AggregatedArticles.FirstOrDefault(a => a.ContentId == classifierValue) ?? CreateNew(classifierValue);
                aggregated.Archived = Archived;
                aggregated.ViewType = ViewType;
                return aggregated;
            }

            return null;
        }

        public static Article CreateNew(int contentId) => CreateNew(contentId, null, null, null);

        public static Article CreateNew(int contentId, int? fieldId, int? articleId, bool? isChild)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            var article = content.CreateArticle(GetPredefinedValues(fieldId, articleId, isChild));
            article.UniqueId = Guid.NewGuid();
            article.DefineViewType();

            return article;
        }

        private static Dictionary<string, string> GetPredefinedValues(int? fieldId, int? articleId, bool? isChild)
        {
            var currentId = 0;
            var predefinedValues = new Dictionary<string, string>();
            if (fieldId.HasValue && articleId.HasValue && fieldId.Value != 0 && articleId.Value != 0)
            {
                var field = FieldRepository.GetById(fieldId.Value);
                if (field != null)
                {
                    if (field.ExactType == FieldExactTypes.M2ORelation && field.BackRelationId.HasValue)
                    {
                        currentId = field.BackRelationId.Value;
                    }
                    else if (field.ExactType == FieldExactTypes.M2MRelation && field.ContentLink.Symmetric)
                    {
                        if (field.RelateToContentId != null && field.RelateToContentId.Value == field.ContentId)
                        {
                            currentId = field.Id;
                        }
                        else if (field.M2MBackwardField != null)
                        {
                            currentId = field.M2MBackwardField.Id;
                        }
                    }
                    else if (field.ExactType == FieldExactTypes.O2MRelation && isChild.HasValue && isChild.Value)
                    {
                        currentId = field.Id;
                    }
                }

                if (currentId != 0)
                {
                    predefinedValues.Add("field_" + currentId, articleId.ToString());
                }
            }

            return predefinedValues;
        }

        public static Article CreateNewForSave(int contentId)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            return content.CreateArticle();
        }

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
            ClearM2OArticleFields();
            ClearOldIds();
            ClearFields(clearFieldIds, clearType);

            if (resolveFieldConflicts)
            {
                ResolveUniqueFieldConflictsOnCopy();
            }
        }

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
                Ensure.NotNull(previousArticle, string.Format(ArticleStrings.ArticleNotFound, Id));

                previousArticle.CreateVersion();
                RemoveAggregates(this, previousArticle);
                RemoveVariations();
            }

            FixNonUsedStatus(true);
            ReplaceAllUrlsToPlaceHolders();
            OptimizeForHierarchy();

            var result = ArticleRepository.CreateOrUpdate(this);
            var articleToPrepare = IsNew ? result : previousArticle;
            var codes = new List<string> { IsNew ? NotificationCode.Create : NotificationCode.Update };
            if (!IsNew && previousArticle != null && previousArticle.StatusTypeId != StatusTypeId)
            {
                codes.Add(NotificationCode.ChangeStatus);
            }

            var repo = new NotificationPushRepository();
            repo.PrepareNotifications(articleToPrepare, codes.ToArray(), disableNotifications);
            result.BackupFilesFromCurrentVersion();
            result.CreateDynamicImages();

            if (IsNew && !IsAggregated && !IsVariation)
            {
                result.CopyParentPermissions();
            }

            if (!IsAggregated)
            {
                foreach (var a in AggregatedArticles)
                {
                    a.AggregateToRootArticle(result);
                    a.Persist(true);
                }

                if (!IsVariation)
                {
                    foreach (var a in VariationArticles.Where(n => n.UseInVariationUpdate))
                    {
                        a.VariateTo(result);
                        a.Persist(true);
                    }

                    if (previousArticle != null && previousArticle.StatusTypeId != StatusTypeId)
                    {
                        var message = ReplaceCommentLink(Comment);
                        var systemStatusType = GetSystemStatusType(previousArticle.StatusTypeId, StatusTypeId, ref message);
                        WorkflowRepository.SaveHistoryStatus(Id, systemStatusType, message, QPContext.CurrentUserId);
                    }

                    repo.SendNotifications();
                }
            }

            return result;
        }

        public Article GetVariationByContext(string context)
        {
            if (string.IsNullOrEmpty(context))
            {
                throw new ArgumentException(nameof(context));
            }

            return VariationArticles.SingleOrDefault(n => n.VariationContext == context) ?? CreateVariationWithFieldValues(GetFieldValuesByContext(context));
        }

        /// <summary>
        /// Используется для XAML-валидации, не удалять!
        /// </summary>
        internal RemoteValidationResult ProceedRemoteValidation() => new RemoteValidationResult();

        private void ValidateWorkflow(RulesException errors)
        {
            if (!IsUpdatableWithWorkflow)
            {
                errors.CriticalErrorForModel(IsNew ? ArticleStrings.CannotAddBecauseOfWorkflow : ArticleStrings.CannotUpdateBecauseOfWorkflow);
            }
        }

        private void ValidateRelationSecurity(RulesException errors)
        {
            if (!ArticleRepository.CheckRelationSecurity(this, false))
            {
                errors.CriticalErrorForModel(IsNew
                    ? ArticleStrings.CannotAddBecauseOfRelationSecurity
                    : ArticleStrings.CannotUpdateBecauseOfRelationSecurity);
            }
        }

        private void ValidateFields(RulesException<Article> errors)
        {
            foreach (var pair in FieldValues)
            {
                if (pair.Field.ExactType == FieldExactTypes.M2ORelation && pair.Field.BackRelation != null && pair.Field.BackRelation.Aggregated)
                {
                    continue;
                }

                pair.Validate(errors);
            }

            if (UniqueId == null)
            {
                errors.CriticalErrorForModel(ArticleStrings.GuidWrongFormat);
            }
            else
            {
                var idByGuid = new ArticleRepository().GetIdByGuid(UniqueId.Value);
                if (idByGuid != 0 && idByGuid != Id)
                {
                    errors.CriticalErrorForModel(ArticleStrings.GuidShouldBeUnique);
                }
            }
        }

        private void ValidateAggregated(RulesException<Article> errors)
        {
            foreach (var aggregated in AggregatedArticles)
            {
                var aggregatedFieldId = aggregated.FieldValues.Select(n => n.Field).Single(p => p.Aggregated).Id;
                foreach (var pair in aggregated.FieldValues.Where(p => p.Field.Id != aggregatedFieldId))
                {
                    pair.Validate(errors);
                }

                aggregated.ValidateUnique(errors, aggregatedFieldId);
            }
        }

        private bool ValidateXaml(RulesException errors, string customerCode)
        {
            bool result = false;

            var values = FieldValues
                .Concat(AggregatedArticles.SelectMany(a => a.FieldValues))
                .Where(v => !v.Field.Aggregated)
                .ToDictionary(v => v.Field.FormName, v => v.Field.ExactType == FieldExactTypes.Boolean ? Converter.ToBoolean(v.Value).ToString() : v.Value);

            values[FieldName.ContentItemId] = Id.ToString();
            values[FieldName.StatusTypeId] = StatusTypeId.ToString();

            var aggregatedXamlValidators = AggregatedArticles
                .Where(a => !a.Content.DisableXamlValidation && !string.IsNullOrWhiteSpace(a.Content.XamlValidation))
                .Select(a => a.Content.XamlValidation)
                .ToArray();

            if (!Content.DisableXamlValidation && !string.IsNullOrWhiteSpace(Content.XamlValidation))
            {
                try
                {
                    var valuesState = new Dictionary<string, string>(values);

                    var obj = new ValidationParamObject
                    {
                        Model = values,
                        Validator = Content.XamlValidation,
                        AggregatedValidatorList = aggregatedXamlValidators,
                        DynamicResource = Content.Site.XamlDictionaries,
                        CustomerCode = customerCode,
                        SiteId = Content.SiteId,
                        ContentId = ContentId
                    };

                    var vcontext = ValidationServices.ValidateModel(obj);
                    result = CheckChangesValues(valuesState, values);

                    if (!vcontext.IsValid)
                    {
                        foreach (var ve in vcontext.Result.Errors)
                        {
                            errors.Error(ve.Definition.PropertyName, values[ve.Definition.PropertyName], ve.Message);
                        }
                        foreach (var msg in vcontext.Messages)
                        {
                            errors.ErrorForModel(msg);
                        }
                    }
                }
                catch (Exception exp)
                {
                    errors.ErrorForModel(string.Format(ArticleStrings.CustomValidationFailed, exp.Message));
                }
            }

            return result;
        }

        public bool CheckChangesValues(Dictionary<string, string> stateValues, Dictionary<string, string> values)
        {
            var result = !stateValues.SequenceEqual(values);
            if (result)
            {
                UpdateFieldValues(values);
            }
            return result;
        }

        private static void ValidateSchedule(RulesException<Article> errors, ArticleSchedule item)
        {
            if (item.ScheduleType == ScheduleTypeEnum.OneTimeEvent && !item.WithoutEndDate && item.StartDate >= item.EndDate)
            {
                errors.ErrorFor(n => n.Schedule.EndDate, ArticleStrings.StartEndDates);
            }

            if (item.ScheduleType == ScheduleTypeEnum.Recurring)
            {
                if (!item.Recurring.RepetitionNoEnd && item.Recurring.RepetitionEndDate.Date < item.Recurring.RepetitionStartDate.Date)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.RepetitionStartDate, ArticleStrings.RepetitionStartDateError);
                }

                if (item.Recurring.ScheduleRecurringValue < ScheduleValidationConstants.ScheduleRecurringMinValue || item.Recurring.ScheduleRecurringValue > ScheduleValidationConstants.ScheduleRecurringMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.ScheduleRecurringValue, ArticleStrings.ScheduleRecurringValueError);
                }

                if (item.Recurring.DayOfMonth < ScheduleValidationConstants.DayOfMonthMinValue || item.Recurring.DayOfMonth > ScheduleValidationConstants.DayOfMonthMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.DayOfMonth, ArticleStrings.DayOfMonthError);
                }

                if (item.Recurring.ShowLimitationType == ShowLimitationType.EndTime && item.Recurring.ShowEndTime < item.Recurring.ShowStartTime)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.ShowStartTime, ArticleStrings.ShowStartTimeError);
                }

                if (item.Recurring.DurationValue < ScheduleValidationConstants.DurationMinValue || item.Recurring.DurationValue > ScheduleValidationConstants.DurationMaxValue)
                {
                    errors.ErrorFor(n => n.Schedule.Recurring.DurationValue, ArticleStrings.DurationValueError);
                }
            }
        }

        private void OptimizeForHierarchy()
        {
            FieldValues
                .Where(n => n.Field.ExactType == FieldExactTypes.M2MRelation && n.Field.OptimizeForHierarchy)
                .ToList()
                .ForEach(n => new OptimizeHierarchyHelper(n).Process());
        }

        private static string ReplaceCommentLink(string comment) => !string.IsNullOrEmpty(comment)
            ? Regex.Replace(comment, @"((http|https)://[\w\.-0-9:?=&_]*)", "<a href=\"$1\" target=\"_blank\">$1</a>")
            : string.Empty;

        private static int GetSystemStatusType(int previousStatusTypeId, int currentStatusTypeId, ref string message)
        {
            var historyStatusId = 0;
            IEnumerable<StatusType> statusTypes = StatusTypeRepository.GetList(new[] { previousStatusTypeId, currentStatusTypeId }).ToList();
            var previousStatus = statusTypes.FirstOrDefault(s => s.Id == previousStatusTypeId);
            var currentStatus = statusTypes.FirstOrDefault(s => s.Id == currentStatusTypeId);

            if (currentStatus != null && previousStatus != null)
            {
                if (previousStatus.Weight > currentStatus.Weight)
                {
                    historyStatusId = (int)SystemStatusType.ForcedDemoting;
                    message = $"The article status was demoted from [{previousStatus.Name}] to [{currentStatus.Name}]. Comment: {message}";
                }
                else
                {
                    historyStatusId = (int)SystemStatusType.ForcedPromoting;
                    message = $"The article status was promoted from [{previousStatus.Name}] to [{currentStatus.Name}]. Comment: {message}";
                }
            }

            return historyStatusId;
        }

        public bool IsArticleChangingActionsAllowed(bool? boundToExternal) => Content.IsArticleChangingActionsAllowed(boundToExternal);

        public void DefineViewType()
        {
            if (LockedByAnyoneElse)
            {
                ViewType = ArticleViewType.LockedByOtherUser;
            }
            else if (!IsNew && !IsUpdatable)
            {
                ViewType = ArticleViewType.ReadOnlyBecauseOfSecurity;
            }
            else if (!IsUpdatableWithWorkflow)
            {
                ViewType = ArticleViewType.ReadOnlyBecauseOfWorkflow;
            }
            else if (!IsNew && !CheckRelationSecurity())
            {
                ViewType = ArticleViewType.ReadOnlyBecauseOfRelationSecurity;
            }
            else
            {
                ViewType = ArticleViewType.Normal;
            }
        }

        private bool CheckRelationSecurity() => !GetRelationSecurityFields().Any() || ArticleRepository.CheckRelationSecurity(ContentId, new[] { Id }, false)[Id];

        public IEnumerable<FieldValue> GetRelationSecurityFields()
        {
            return FieldValues.Concat(AggregatedArticles.SelectMany(n => n.FieldValues)).Where(n => n.Field.UseRelationSecurity);
        }

        internal void LoadAggregatedArticles(bool excludeArchive = false)
        {
            foreach (var art in AggregatedArticles)
            {
                art.LoadFieldValues(excludeArchive);
            }
        }

        internal void SetDefaultStatusAndVisibility()
        {
            var isWorkflowAssigned = Content.WorkflowBinding.IsAssigned;
            Visible = isWorkflowAssigned;
            Status = isWorkflowAssigned ? StatusType.GetNone(Content.SiteId) : StatusType.GetPublished(Content.SiteId);
            StatusTypeId = Status.Id;
        }

        /// <summary>
        /// Исправляет статус статьи на допустимый (недопустимый статус может появиться в результате изменения Workflow или Binding)
        /// <param name="fixUnassignedWorkflow">исправлять ли в случае неназначенного Workflow</param>
        /// </summary>
        internal void FixNonUsedStatus(bool fixUnassignedWorkflow)
        {
            if (Workflow.IsAssigned)
            {
                if (!Workflow.UseStatus(Status.Id) && Status.Id != StatusType.GetNone(Content.SiteId).Id)
                {
                    Status = Workflow.GetClosestStatus(Status.Weight);
                    StatusTypeId = Status.Id;
                }
            }
            else if (fixUnassignedWorkflow)
            {
                Status = StatusType.GetPublished(Content.SiteId);
                StatusTypeId = Status.Id;
            }
        }

        internal void ResolveUniqueFieldConflictsOnCopy()
        {
            var constraints = ContentConstraintRepository.GetConstraintsByContentId(ContentId);
            foreach (var constraint in constraints)
            {
                var step = 0;
                var fieldValuesToResolve = constraint.Filter(FieldValues);
                List<FieldValue> currentFieldValues;

                do
                {
                    step++;
                    currentFieldValues = MutateHelper.MutateFieldValues(fieldValuesToResolve, step);
                } while (!ArticleRepository.ValidateUnique(currentFieldValues));

                MergeWithFieldValues(currentFieldValues);
            }
        }

        internal PathInfo GetVersionPathInfo(int newVersionId) => Content.GetVersionPathInfo(newVersionId);

        internal static List<FieldValue> GetFieldValues(DataRow data, IEnumerable<Field> fields, Article article, int versionId = 0, string contentPrefix = null, bool excludeArchive = false)
        {
            if (data == null)
            {
                throw new Exception(string.Format(ArticleStrings.ArticleNotFoundInTheContent, article.Id, article.DisplayContentId));
            }

            var result = new List<FieldValue>();

            var fieldsArr = fields as Field[] ?? fields.ToArray();

            var linkResult = new Dictionary<int, string>();
            var backRelationsResult = new Dictionary<int, string>();


            if (versionId == 0)
            {
                var linkIds = fieldsArr.Where(n => n.RelationType == RelationType.ManyToMany).Select(n => n.GetBaseField(article.Id).LinkId.Value).ToArray();
                linkResult = ArticleRepository.GetLinkedItems(linkIds, article.Id, excludeArchive);
                var backRelationIds = fieldsArr.Where(n => n.RelationType == RelationType.ManyToOne).Select(n => n.GetBaseField(article.Id).BackRelationId.Value).ToArray();
                backRelationsResult = ArticleRepository.GetRelatedItems(backRelationIds, article.Id, excludeArchive);
            }


            foreach (var field in fieldsArr)
            {
                var fullFieldName = string.IsNullOrWhiteSpace(contentPrefix) ? field.Name : $"{contentPrefix}.{field.Name}";
                if (!data.Table.Columns.Contains(fullFieldName))
                {
                    throw new Exception(string.Format(ArticleStrings.FieldNotFound, fullFieldName, article.DisplayContentId));
                }

                object objectValue = null;
                switch (field.RelationType)
                {
                    case RelationType.ManyToMany:
                        var linkId = field.GetBaseField(article.Id).LinkId;
                        if (linkId.HasValue)
                        {
                            objectValue = (versionId == 0)
                                ? linkResult[linkId.Value]
                                : ArticleVersionRepository.GetLinkedItems(versionId, field.Id);
                        }

                        break;
                    case RelationType.ManyToOne:
                        var backRelationId = field.GetBaseField(article.Id).BackRelationId;
                        if (backRelationId.HasValue)
                        {
                            objectValue = versionId == 0
                                ? backRelationsResult[backRelationId.Value]
                                : ArticleVersionRepository.GetRelatedItems(versionId, field.Id);
                        }

                        break;
                    case RelationType.None:
                    case RelationType.OneToMany:
                        objectValue = data[fullFieldName];
                        if (DBNull.Value.Equals(objectValue))
                        {
                            objectValue = null;
                        }

                        break;
                }

                if (field.ExactType == FieldExactTypes.Numeric && field.DecimalPlaces > 0 && versionId != 0)
                {
                    var str = objectValue?.ToString();
                    if (!string.IsNullOrEmpty(str))
                    {
                        objectValue = decimal.Parse(str, CultureInfo.InvariantCulture);
                    }
                }

                if (field.ReplaceUrls && objectValue != null)
                {
                    objectValue = PlaceHolderHelper.ReplacePlaceHoldersToUrls(article.Content.Site, objectValue.ToString());
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

        internal static void LoadFieldValuesForArticles(DataTable data, IEnumerable<Field> fields, IEnumerable<Article> articles, int contentId, bool excludeArchive)
        {
            var articlesArr = articles as Article[] ?? articles.ToArray();
            var ids = articlesArr.Select(n => n.Id).ToArray();
            var itemsForRelations = new Dictionary<int, Dictionary<int, string>>();
            if (data == null)
            {
                throw new Exception(string.Join(",", ids.ToString()));
            }

            var fieldsArr = fields as Field[] ?? fields.ToArray();
            var linkIds = fieldsArr.Where(n => n.LinkId.HasValue).Select(n => n.LinkId.Value).ToArray();
            var backwardIds = fieldsArr.Where(n => n.BackRelationId.HasValue).Select(n => n.BackRelationId.Value).ToArray();
            var linkResult = linkIds.Any()
                ? ArticleRepository.GetLinkedItemsMultiple(linkIds, ids, excludeArchive)
                : new Dictionary<int, Dictionary<int, List<int>>>();
            var backwardResult = backwardIds.Any()
                ? ArticleRepository.GetRelatedItemsMultiple(backwardIds, ids, excludeArchive)
                : new Dictionary<int, Dictionary<int, List<int>>>();

            foreach (var field in fieldsArr)
            {
                if (!data.Columns.Contains(field.Name))
                {
                    throw new Exception(string.Format(ArticleStrings.FieldNotFound, field.Name, contentId));
                }

                switch (field.RelationType)
                {
                    case RelationType.ManyToMany:
                        if (field.LinkId.HasValue)
                        {
                            var dict = linkResult[field.LinkId.Value];
                            var convertedDict = dict.ToDictionary(n => n.Key, m => string.Join(",", m.Value));
                            itemsForRelations.Add(field.Id, convertedDict);
                        }
                        break;
                    case RelationType.ManyToOne:
                        if (field.BackRelationId.HasValue)
                        {
                            var dict = backwardResult[field.BackRelationId.Value];
                            var convertedDict = dict.ToDictionary(n => n.Key, m => string.Join(",", m.Value));
                            itemsForRelations.Add(field.Id, convertedDict);
                        }
                        break;
                }
            }

            foreach (DataRow dr in data.Rows)
            {
                var result = new List<FieldValue>();
                var id = (int)(decimal)dr["content_item_id"];
                var article = articlesArr.SingleOrDefault(n => n.Id == id);

                if (article == null)
                {
                    continue;
                }

                var statusTypeId = (int)(decimal)dr["status_type_id"];
                if (article.StatusTypeId != statusTypeId)
                {
                    article.StatusTypeId = statusTypeId;
                    article.Status = StatusTypeRepository.GetById(statusTypeId);
                }

                foreach (var field in fieldsArr)
                {
                    object objectValue = null;
                    if (field.RelationType == RelationType.ManyToMany || field.RelationType == RelationType.ManyToOne)
                    {
                        var dict = itemsForRelations[field.Id];
                        if (dict != null && dict.TryGetValue(id, out var stringValue))
                        {
                            objectValue = stringValue;
                        }
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
                            objectValue = PlaceHolderHelper.ReplacePlaceHoldersToUrls(article.Content.Site, objectValue.ToString());
                        }
                    }

                    result.Add(new FieldValue { Field = field, ObjectValue = objectValue, Article = article });
                }

                article.FieldValues = result;
            }
        }

        public void ReplaceAllUrlsToPlaceHolders()
        {
            foreach (var item in FieldValues)
            {
                if (item.Field.ReplaceUrlsInDB && item.Field.ReplaceUrls)
                {
                    item.Value = PlaceHolderHelper.ReplaceUrlsToPlaceHolders(Content.Site, item.Value);
                }
            }
        }

        internal bool CheckRelationCondition(string relCondition) => ArticleRepository.CheckRelationCondition(Id, ContentId, relCondition);

        internal void CreateVersion()
        {
            if (Content.UseVersionControl)
            {
                var earliestVersion = ArticleVersionRepository.GetEarliest(Id);
                ArticleVersionRepository.Create(Id);
                var newVersion = ArticleVersionRepository.GetLatest(Id);

                Directory.CreateDirectory(BackupPath);
                Directory.CreateDirectory(newVersion.PathInfo.Path);

                var count = ArticleVersionRepository.GetVersionsCount(Id);
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
                {
                    Folder.ForceDelete(newVersion.PathInfo.Path);
                }
            }
        }

        internal void BackupFilesFromCurrentVersion()
        {
            Directory.CreateDirectory(BackupPath);
            CopyArticleFiles(CopyFilesMode.ToBackupFolder, BackupPath);
        }

        internal void RestoreArticleFilesForVersion(int versionId)
        {
            var versionPath = GetVersionPathInfo(versionId).Path;
            CopyArticleFiles(CopyFilesMode.FromVersionFolder, BackupPath, versionPath);
            foreach (var a in AggregatedArticles)
            {
                a.CopyArticleFiles(CopyFilesMode.FromVersionFolder, a.BackupPath, versionPath);
            }
        }

        internal void RemoveAllVersionFolders()
        {
            foreach (var id in ArticleVersionRepository.GetIds(Id))
            {
                RemoveVersionFolder(id);
            }
        }

        internal static void RemoveAllVersionFolders(Content content, int[] ids)
        {
            foreach (var id in ArticleVersionRepository.GetIds(ids))
            {
                RemoveVersionFolder(content, id);
            }
        }

        internal static void RemoveVersionFolder(Content content, int id)
        {
            Folder.ForceDelete(content.GetVersionPathInfo(id).Path);
        }

        internal void RemoveVersionFolder(int id)
        {
            RemoveVersionFolder(Content, id);
        }

        internal void CreateDynamicImages()
        {
            foreach (var item in FieldValues.Where(n => n.Field.Type.Name == FieldTypeName.Image && !string.IsNullOrEmpty(n.Value)))
            {
                foreach (var field in item.Field.GetDynamicImages())
                {
                    field.DynamicImage.CreateDynamicImage(item.Field.PathInfo.GetPath(item.Value), item.Value);
                }
            }
        }

        /// <summary>
        /// Service method to clear article fields
        /// </summary>
        public void ClearFields(int[] fieldIdsToClear, ArticleClearType clearType)
        {
            if (fieldIdsToClear != null && fieldIdsToClear.Length > 0)
            {
                ClearFields(fv => fieldIdsToClear.Contains(fv.Field.Id), clearType);
            }
        }

        /// <summary>
        /// Service method to clear article fields
        /// </summary>
        public void ClearFields(Func<FieldValue, bool> fieldsToClearPredicate, ArticleClearType clearType)
        {
            foreach (var fieldValue in FieldValues.Where(fieldsToClearPredicate))
            {
                fieldValue.Value = clearType == ArticleClearType.DefaultValue ? fieldValue.Field.DefaultValue : string.Empty;
            }
        }

        private void ClearM2OArticleFields()
        {
            foreach (var fieldValue in FieldValues.Where(n => n.Field.ExactType == FieldExactTypes.M2ORelation))
            {
                fieldValue.Value = string.Empty;
            }
        }

        /// <summary>
        /// Service method to clear old article fields
        /// </summary>
        public void ClearOldIds()
        {
            foreach (var fieldValue in FieldValues.Where(n => n.Field.Name.StartsWith("Old") && n.Field.Name.EndsWith("Id")))
            {
                fieldValue.Value = string.Empty;
            }
        }

        private void AggregateToRootArticle(Article rootArticle)
        {
            Ensure.Not(rootArticle.IsNew, "Root article has to be saved.");
            var aggregatorValue = FieldValues.FirstOrDefault(f => f.Field.Aggregated);

            Ensure.NotNull(aggregatorValue, "There is no aggregated field in article.");
            aggregatorValue.Value = rootArticle.Id.ToString();

            UniqueId = Guid.NewGuid();
            CopyServiceFields(rootArticle);
        }

        private void VariateTo(Article rootArticle)
        {
            Ensure.Not(rootArticle.IsNew, "Root article has to be saved.");
            var variationValue = FieldValues.FirstOrDefault(f => f.Field.UseForVariations);

            Ensure.NotNull(variationValue, "There is no variation field in article.");
            variationValue.Value = rootArticle.Id.ToString();

            CopyServiceFields(rootArticle);
        }

        private void CopyServiceFields(Article fromArticle)
        {
            Visible = fromArticle.Visible;
            Delayed = fromArticle.Delayed;
            CancelSplit = fromArticle.CancelSplit;
            Schedule.CopyFrom(fromArticle.Schedule);
            StatusTypeId = fromArticle.StatusTypeId;
            LastModifiedBy = fromArticle.LastModifiedBy;
        }

        private RulesException ValidateUnique(RulesException errors, int exceptFieldId)
        {
            foreach (var constraint in Content.Constraints)
            {
                var fieldValuesToTest = constraint.Filter(FieldValues);
                if (fieldValuesToTest.Any() && fieldValuesToTest[0].Field.Id != exceptFieldId)
                {
                    if (!ArticleRepository.ValidateUnique(fieldValuesToTest, out var constraintToDisplay, out var conflictingIds))
                    {
                        if (!constraint.IsComplex)
                        {
                            errors.Error(fieldValuesToTest[0].Field.FormName, fieldValuesToTest[0].Value, string.Format(ArticleStrings.NotUniqueValue, constraintToDisplay, conflictingIds));
                        }
                        else
                        {
                            errors.ErrorForModel(string.Format(ArticleStrings.UniqueConstraintViolation, constraintToDisplay, conflictingIds));
                        }
                    }
                }
            }

            return errors;
        }

        public void CopyAggregates(IEnumerable<Article> aggregatedArticles)
        {
            var result = new List<Article>();
            foreach (var a in aggregatedArticles)
            {
                a.AggregateToRootArticle(this);
                result.Add(ArticleRepository.Copy(a));
            }

            AggregatedArticles = result;
        }

        private void CopyParentPermissions()
        {
            var treeField = Content.TreeField;
            if (treeField != null && treeField.CopyPermissionsToChildren)
            {
                var parentId = Converter.ToInt32(FieldValues.Single(n => n.Field.Name == treeField.Name).Value, 0);
                if (parentId != 0)
                {
                    ArticleRepository.CopyPermissions(parentId, Id);
                }
            }
        }

        private void MergeWithFieldValues(IEnumerable<FieldValue> currentFieldValues)
        {
            foreach (var currentValue in currentFieldValues)
            {
                FieldValues.Single(n => Equals(n.Field, currentValue.Field)).Value = currentValue.Value;
            }
        }

        protected override RulesException ValidateUnique(RulesException errors) => ValidateUnique(errors, 0);

        private void CopyArticleFiles(CopyFilesMode mode, string currentVersionPath, string versionPath = "")
        {
            if (Content.UseVersionControl)
            {
                void CopyFile(string src, string dest)
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
                }

                foreach (var item in FieldValues)
                {
                    if (item.Field.UseVersionControl && (item.Field.Type.Name == FieldTypeName.Image || item.Field.Type.Name == FieldTypeName.File))
                    {
                        if (mode == CopyFilesMode.ToBackupFolder)
                        {
                            // не режем откуда, режем куда
                            var source = Path.Combine(item.Field.PathInfo.Path, item.Value);
                            var destination = Path.Combine(currentVersionPath, Path.GetFileName(item.Value));
                            CopyFile(source, destination);
                        }
                        else if (mode == CopyFilesMode.ToVersionFolder)
                        {
                            // режем откуда, режем куда
                            var source = Path.Combine(currentVersionPath, Path.GetFileName(item.Value) ?? string.Empty);
                            var destination = Path.Combine(versionPath, Path.GetFileName(item.Value) ?? string.Empty);
                            CopyFile(source, destination);
                        }
                        else if (mode == CopyFilesMode.FromVersionFolder)
                        {
                            // режем откуда, режем куда
                            var source = Path.Combine(versionPath, Path.GetFileName(item.Value) ?? string.Empty);
                            var destination = Path.Combine(currentVersionPath, Path.GetFileName(item.Value) ?? string.Empty);
                            CopyFile(source, destination);

                            // режем откуда, не режем куда
                            destination = Path.Combine(item.Field.PathInfo.Path, item.Value);
                            CopyFile(source, destination);
                        }
                    }
                }
            }
        }

        private IEnumerable<FieldValue> GetFieldValuesByContext(string context)
        {
            var contextIds = context.Split(",".ToCharArray());
            foreach (var id in contextIds)
            {
                var fieldId = 0;
                foreach (var li in ContextListItems)
                {
                    if (li.Ids.ContainsKey(id))
                    {
                        fieldId = li.FieldId;
                        break;
                    }
                }

                var field = Content.Fields.Single(n => n.Id == fieldId);
                yield return new FieldValue { Field = field, Value = id };
            }
        }

        private Article CreateVariationWithFieldValues(IEnumerable<FieldValue> fieldValues)
        {
            var predefinedValues = fieldValues.ToDictionary(n => n.Field.FormName, n => n.Value);

            //predefinedValues.Add(Content.VariationField.FormName, Id.ToString());
            var result = new Article(Content, predefinedValues);
            VariationArticles.Add(result);
            return result;
        }

        private static void RemoveAggregates(Article currentArticle, Article previousArticle)
        {
            if (currentArticle.FieldValues.Any(n => n.Field.IsClassifier))
            {
                var liveExtensionContents = GetClassifierValues(currentArticle.LiveFieldValues);
                var stageExtensionContents = GetClassifierValues(currentArticle.FieldValues);
                var protectedExtensionContents = liveExtensionContents.Except(stageExtensionContents).ToArray();

                var comparer = new LambdaEqualityComparer<Article>((x, y) => x.Id == y.Id, x => x.Id);
                var deletingAggregated = previousArticle
                    .AggregatedArticles
                    .Except(currentArticle.AggregatedArticles, comparer)
                    .Where(n => !protectedExtensionContents.Contains(n.ContentId))
                    .Select(a => a.Id).ToList();
                ArticleRepository.MultipleDelete(deletingAggregated);
            }

        }

        private void RemoveVariations()
        {
            var baseArticles = VariationArticles.Where(n => !n.UseInVariationUpdate);
            var enumerable = baseArticles as Article[] ?? baseArticles.ToArray();
            var articlesToRemove = enumerable
                .SelectMany(n => n.AggregatedArticles.Select(m => m.Id))
                .Union(enumerable.Select(m => m.Id))
                .ToArray();

            ArticleRepository.MultipleDelete(articlesToRemove);
        }

        private IEnumerable<ArticleVariationListItem> LoadVariationListForClient()
        {
            var fieldIds = VariationContextFieldValues.Select(n => n.Field.Id).ToArray();
            foreach (var article in VariationArticles.Concat(Enumerable.Repeat(this, 1)))
            {
                yield return new ArticleVariationListItem
                {
                    Id = article.Id,
                    FieldValues = article.VariationEditableFieldValues.Union(article.AggregatedArticles.SelectMany(n => n.VariationEditableFieldValues)).ToDictionary(n => n.Field.FormName, n => n.Value),
                    Context = string.Join(",", article.FieldValues
                        .Where(n => fieldIds.Contains(n.Field.Id) && !string.IsNullOrEmpty(n.Value))
                        .OrderBy(n => n.Field.Order)
                        .Select(n => n.Value)
                    )
                };
            }
        }

        private IEnumerable<ArticleContextListItem> LoadContextListForClient()
        {
            var arr = VariationContextFieldValues
                .Where(n => n.Field.RelateToContentId.HasValue)
                .Select(n => new { n.Field.Id, ContentId = n.Field.RelateToContentId.Value })
                .ToArray();

            foreach (var item in arr)
            {
                var result = new ArticleContextListItem
                {
                    HasHierarchy = ContentRepository.HasContentTreeField(item.ContentId),
                    FieldId = item.Id,
                    Ids = ArticleRepository.GetHierarchy(item.ContentId).ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value.ToString())
                };

                yield return result;
            }
        }

        public void LoadCollaborativePublishedArticle(int id)
        {
            CollaborativePublishedArticle =  ArticleRepository.GetArticleIdForCollaborativePublication(id);
        }


    private int GetContentIdForArticle()
        {
            if (_parentContentId == 0 || CollaborativePublishedArticle != 0 )
            {
                _parentContentId = ArticleRepository.GetContentIdForArticle(CollaborativePublishedArticle);
            }
            return _parentContentId;
        }
    }

    public enum ArticleClearType
    {
        EmptyValue = 0,
        DefaultValue = 1
    }
}
