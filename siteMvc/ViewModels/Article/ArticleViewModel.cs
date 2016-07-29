using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using C = Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{

    public class ArticleViewModel : LockableEntityViewModel
    {
        public const int MaxDataListItemCount = 10;

        public class RelationListResult
        {
            public RelationListResult()
            {
                Items = new List<ListItem>();
            }

            public IEnumerable<ListItem> Items { get; set; }

            public bool IsListOverflow { get; set; }
        }

        public new BLL.Article Data
        {
            get
            {
                return (BLL.Article)EntityData;
            }

            set
            {
                EntityData = value;
            }
        }

        #region creation

        private readonly InitPropertyValue<string> _variationModel;
        private readonly InitPropertyValue<string> _contextModel;
        private readonly InitPropertyValue<string> _errorModel;

        public ArticleViewModel()
        {
            _variationModel = new InitPropertyValue<string>(() => new JavaScriptSerializer().Serialize(Data.VariationListItems));
            _contextModel = new InitPropertyValue<string>(() => new JavaScriptSerializer().Serialize(Data.ContextListItems));
            _errorModel = new InitPropertyValue<string>(() => new JavaScriptSerializer().Serialize(
                Data.VariationsErrorModel.Select(n => new
                {
                    Context = n.Key,
                    Errors = n.Value.Errors.Select(m => new
                    {
                        Name = m.PropertyName, m.Message
                    })
                })
            ));
        }

        public static ArticleViewModel Create(BLL.Article data, string tabId, int parentEntityId, bool? boundToExternal)
        {
            var model = Create<ArticleViewModel>(data, tabId, parentEntityId);
            model.BoundToExternal = boundToExternal;
            model.IsVirtual = data.Content.IsVirtual;
            return model;
        }

        public static ArticleViewModel Create(BLL.Article data, int parentEntityId, string tabId, string successfulActionCode, bool? boundToExternal)
        {
            var model = Create(data, tabId, parentEntityId, boundToExternal);
            model.SuccesfulActionCode = successfulActionCode;
            return model;
        }

        #endregion

        public string ContentName => Data.DisplayContentName;

        public bool IsReadOnly => ((Data.ViewType != C.ArticleViewType.Normal) && (Data.ViewType != C.ArticleViewType.PreviewVersion))
                                  || Data.IsAggregated
                                  || IsChangingActionsProhibited;

        public bool IsChangingActionsProhibited => !Data.IsArticleChangingActionsAllowed(BoundToExternal);

        public bool ShowLockInfo => ((Data.ViewType == C.ArticleViewType.Normal) || (Data.ViewType == C.ArticleViewType.LockedByOtherUser))
                                    && !Data.IsAggregated
                                    && !IsChangingActionsProhibited;

        public bool ShowArchive => Data.ViewType == C.ArticleViewType.Archived;

        public override string CaptureLockActionCode => C.ActionCode.CaptureLockArticle;

        public List<ListItem> ScheduleTypes => new List<ListItem>
        {
            new ListItem(C.ScheduleTypeEnum.Invisible.ToString(), ArticleStrings.Invisible),
            new ListItem(C.ScheduleTypeEnum.OneTimeEvent.ToString(), ArticleStrings.OneTimeEvent, true),
            new ListItem(C.ScheduleTypeEnum.Recurring.ToString(), ArticleStrings.Recurring, true),
            new ListItem(C.ScheduleTypeEnum.Visible.ToString(), ArticleStrings.Visible)
        };

        public List<ListItem> ScheduleRecurringTypes => new List<ListItem>
        {
            new ListItem(C.ScheduleRecurringType.Daily.ToString(), ArticleStrings.Daily),
            new ListItem(C.ScheduleRecurringType.Weekly.ToString(), ArticleStrings.Weekly, true),
            new ListItem(C.ScheduleRecurringType.Monthly.ToString(), ArticleStrings.Monthly, "daySpecBy"),
            new ListItem(C.ScheduleRecurringType.Yearly.ToString(), ArticleStrings.Yearly, "daySpecBy")
        };

        public List<ListItem> DaySpecifyingTypes => new List<ListItem>
        {
            new ListItem(C.DaySpecifyingType.Date.ToString(), ArticleStrings.DaySpecifyingByDate, true),
            new ListItem(C.DaySpecifyingType.DayOfWeek.ToString(), ArticleStrings.DaySpecifyingByDayOfWeek, true)                    
        };

        public List<ListItem> Months
        {
            get
            {
                return DateTimeFormatInfo.CurrentInfo?.MonthNames
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .Select((m, i) => new ListItem((i + 1).ToString(), m))
                    .ToList();
            }
        }

        public List<ListItem> WeeksOfMonth => new List<ListItem>
        {
            new ListItem(C.WeekOfMonth.FirstWeek.ToString(), ArticleStrings.FirstWeek),
            new ListItem(C.WeekOfMonth.SecondWeek.ToString(), ArticleStrings.SecondWeek),
            new ListItem(C.WeekOfMonth.ThirdWeek.ToString(), ArticleStrings.ThirdWeek),
            new ListItem(C.WeekOfMonth.FourthWeek.ToString(), ArticleStrings.FourthWeek),
            new ListItem(C.WeekOfMonth.LastWeek.ToString(), ArticleStrings.LastWeek)
        };

        public List<ListItem> DaysOfWeek => new List<ListItem>
        {
            new ListItem(C.DayOfWeek.Monday.ToString(), ArticleStrings.Monday),
            new ListItem(C.DayOfWeek.Tuesday.ToString(), ArticleStrings.Tuesday),
            new ListItem(C.DayOfWeek.Wednesday.ToString(), ArticleStrings.Wednesday),
            new ListItem(C.DayOfWeek.Thursday.ToString(), ArticleStrings.Thursday),
            new ListItem(C.DayOfWeek.Friday.ToString(), ArticleStrings.Friday),
            new ListItem(C.DayOfWeek.Saturday.ToString(), ArticleStrings.Saturday),
            new ListItem(C.DayOfWeek.Sunday.ToString(), ArticleStrings.Sunday),
            new ListItem(C.DayOfWeek.Weekday.ToString(), ArticleStrings.Weekday),
            new ListItem(C.DayOfWeek.Weekend.ToString(), ArticleStrings.Weekend)
        };

        public List<ListItem> ShowLimitationTypes => new List<ListItem>
        {
            new ListItem(C.ShowLimitationType.EndTime.ToString(), ArticleStrings.LimitedByTime , true),
            new ListItem(C.ShowLimitationType.Duration.ToString(), ArticleStrings.LimitedByDuration, true)
        };

        public List<ListItem> DurationUnits => new List<ListItem>
        {
            new ListItem(C.ShowDurationUnit.Minutes.ToString(), ArticleStrings.MinutesLimitationUnit),
            new ListItem(C.ShowDurationUnit.Hours.ToString(), ArticleStrings.HoursLimitationUnit),
            new ListItem(C.ShowDurationUnit.Days.ToString(), ArticleStrings.DaysLimitationUnit),
            new ListItem(C.ShowDurationUnit.Weeks.ToString(), ArticleStrings.WeeksLimitationUnit),
            new ListItem(C.ShowDurationUnit.Months.ToString(), ArticleStrings.MonthsLimitationUnit),
            new ListItem(C.ShowDurationUnit.Years.ToString(), ArticleStrings.YearsLimitationUnit)					
        };


        public string WorkflowWarning => (IsNew) ? ArticleStrings.CannotAddBecauseOfWorkflow : ArticleStrings.CannotUpdateBecauseOfWorkflow;

        public string RelationSecurityWarning => ArticleStrings.CannotUpdateBecauseOfRelationSecurity;

        public string VariationModel
        {
            get { return _variationModel.Value; }
            set { _variationModel.Value = value; }
        }

        public string ContextModel => _contextModel.Value;

        public string ErrorModel => _errorModel.Value;

        public string CurrentContext { get; set; }

        public bool? BoundToExternal { get; set; }

        #region overrides

        public override string EntityTypeCode
        {
            get
            {
                if (ShowArchive)
                    return C.EntityTypeCode.ArchiveArticle;
                if (IsVirtual)
                    return C.EntityTypeCode.VirtualArticle;
                return C.EntityTypeCode.Article;
            }
        }


        public override string ActionCode
        {
            get
            {
                if (IsNew)
                    return C.ActionCode.AddNewArticle;
                if (ShowArchive)
                    return C.ActionCode.ViewArchiveArticle;
                if (IsVirtual)
                    return C.ActionCode.ViewVirtualArticle;
                return C.ActionCode.EditArticle;
            }
        }

        internal ListItem GetStatusListItem(BLL.StatusType st)
        {
            var result = new ListItem
            {
                Text = st.Name,
                Value = st.Id.ToString(),
                HasDependentItems = true
            };

            if ((st.Weight != Data.Workflow.MaxStatus.Weight) && Data.Workflow.IsAsync && Data.Workflow.CurrentUserHasWorkflowMaxWeight
                && (Data.Splitted || (!Data.Splitted && (Data.StatusTypeId == Data.Workflow.MaxStatus.Id)))
                )
            {
                result.DependentItemIDs = new[] { "cancelSplitPanel" };
            }
            if (Data.StatusTypeId != st.Id)
            {
                if (result.DependentItemIDs != null)
                {
                    result.DependentItemIDs[result.DependentItemIDs.Length - 1] = "comment";
                }
                else
                {
                    result.DependentItemIDs = new[] { "comment" };
                }
            }
            return result;
        }

        public IEnumerable<ListItem> AvailableStatuses => Data.Workflow.AvailableStatuses.Select(GetStatusListItem);

        #endregion

        #region Data Lists
        /// <summary>
        /// Возвращает список контентов для Relation-полей
        /// </summary>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="articleId"></param>
        /// <returns></returns>
        internal static RelationListResult GetListForRelation(BLL.Field field, string value, int articleId)
        {
            var baseField = field.GetBaseField(articleId);
            var contentId = baseField.RelateToContentId ?? 0;
            var fieldId = baseField.Id;
            var selectedArticleIDs = Converter.ToInt32Collection(value, ',');
            var filter = baseField.GetRelationFilter(articleId);

            var itemCount = ArticleService.Count(contentId, filter);
            var isListOverflow = (itemCount > MaxDataListItemCount);
            var mode = isListOverflow ? C.ListSelectionMode.OnlySelectedItems : C.ListSelectionMode.AllItems;

            var list = new List<ListItem>();
            if (!isListOverflow || (selectedArticleIDs.Length != 0))
                list = ArticleService.SimpleList(contentId, articleId, fieldId, mode, selectedArticleIDs, filter);

            return new RelationListResult { IsListOverflow = isListOverflow, Items = list };
        }

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        /// <param name="classifier"></param>
        /// <param name="excludeValue"></param>
        /// <returns></returns>
        internal static IEnumerable<ListItem> GetAggregatableContentsForClassifier(BLL.Field classifier, string excludeValue)
        {
            return ArticleService.GetAggregetableContentsForClassifier(classifier, excludeValue);
        }

        /// <summary>
        /// Возвращает контент по ID
        /// </summary>
        /// <returns></returns>
        internal static BLL.Content GetContentById(int? contentId)
        {
            return contentId.HasValue ? ContentService.Read(contentId.Value) : null;
        }

        public void DoCustomBinding()
        {
            Data.VariationListItems = new JavaScriptSerializer().Deserialize<List<ArticleVariationListItem>>(VariationModel);
        }

        #endregion

        public string RemoveVariationCode
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(@"<div class=""variationInfoItem removeItem"">");
                sb.Append(@"<span class=""linkButton actionLink"">");
                sb.Append(@"<span class=""icon deselectAll"">");
                sb.Append(@"<img src=""/backend/Content/Common/0.gif"">");
                sb.Append(@"</span><a class=""js removeVariation"" href=""javascript:void(0);"">");
                sb.AppendFormat(@"<span class=""text"">{0}</span>", ArticleStrings.RemoveCurrentVariation);
                sb.Append(@"</a></span></div>");
                return sb.ToString();
            }
        }

    }
}