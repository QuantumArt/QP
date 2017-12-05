using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ArticleServices;
using Quantumart.QP8.BLL.Services.ContentServices;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels.Article
{
    public class ArticleViewModel : LockableEntityViewModel
    {
        public new BLL.Article Data
        {
            get => (BLL.Article)EntityData;
            set => EntityData = value;
        }

        private readonly InitPropertyValue<string> _variationModel;
        private readonly InitPropertyValue<string> _contextModel;
        private readonly InitPropertyValue<string> _errorModel;

        public ArticleViewModel()
        {
            _variationModel = new InitPropertyValue<string>(() => JsonConvert.SerializeObject(Data.VariationListItems));
            _contextModel = new InitPropertyValue<string>(() => JsonConvert.SerializeObject(Data.ContextListItems));

            _errorModel = new InitPropertyValue<string>(() => JsonConvert.SerializeObject(Data.VariationsErrorModel.Select(n => new
            {
                Context = n.Key,
                Errors = n.Value.Errors.Select(m => new
                {
                    Name = m.PropertyName,
                    m.Message
                })
            })));
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

        public bool IsReadOnly => Data.ViewType != ArticleViewType.Normal && Data.ViewType != ArticleViewType.PreviewVersion || Data.IsAggregated || IsChangingActionsProhibited;

        public bool IsChangingActionsProhibited => !Data.IsArticleChangingActionsAllowed(BoundToExternal);

        public bool ShowLockInfo => (Data.ViewType == ArticleViewType.Normal || Data.ViewType == ArticleViewType.LockedByOtherUser) && !Data.IsAggregated && !IsChangingActionsProhibited;

        public bool ShowArchive => Data.ViewType == ArticleViewType.Archived;

        public override string CaptureLockActionCode => Constants.ActionCode.CaptureLockArticle;

        public List<ListItem> ScheduleTypes => new List<ListItem>
        {
            new ListItem(ScheduleTypeEnum.Invisible.ToString(), ArticleStrings.Invisible),
            new ListItem(ScheduleTypeEnum.OneTimeEvent.ToString(), ArticleStrings.OneTimeEvent, true),
            new ListItem(ScheduleTypeEnum.Recurring.ToString(), ArticleStrings.Recurring, true),
            new ListItem(ScheduleTypeEnum.Visible.ToString(), ArticleStrings.Visible)
        };

        public List<ListItem> ScheduleRecurringTypes => new List<ListItem>
        {
            new ListItem(ScheduleRecurringType.Daily.ToString(), ArticleStrings.Daily),
            new ListItem(ScheduleRecurringType.Weekly.ToString(), ArticleStrings.Weekly, true),
            new ListItem(ScheduleRecurringType.Monthly.ToString(), ArticleStrings.Monthly, "daySpecBy"),
            new ListItem(ScheduleRecurringType.Yearly.ToString(), ArticleStrings.Yearly, "daySpecBy")
        };

        public List<ListItem> DaySpecifyingTypes => new List<ListItem>
        {
            new ListItem(DaySpecifyingType.Date.ToString(), ArticleStrings.DaySpecifyingByDate, true),
            new ListItem(DaySpecifyingType.DayOfWeek.ToString(), ArticleStrings.DaySpecifyingByDayOfWeek, true)
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
            new ListItem(WeekOfMonth.FirstWeek.ToString(), ArticleStrings.FirstWeek),
            new ListItem(WeekOfMonth.SecondWeek.ToString(), ArticleStrings.SecondWeek),
            new ListItem(WeekOfMonth.ThirdWeek.ToString(), ArticleStrings.ThirdWeek),
            new ListItem(WeekOfMonth.FourthWeek.ToString(), ArticleStrings.FourthWeek),
            new ListItem(WeekOfMonth.LastWeek.ToString(), ArticleStrings.LastWeek)
        };

        public List<ListItem> DaysOfWeek => new List<ListItem>
        {
            new ListItem(DayOfWeek.Monday.ToString(), ArticleStrings.Monday),
            new ListItem(DayOfWeek.Tuesday.ToString(), ArticleStrings.Tuesday),
            new ListItem(DayOfWeek.Wednesday.ToString(), ArticleStrings.Wednesday),
            new ListItem(DayOfWeek.Thursday.ToString(), ArticleStrings.Thursday),
            new ListItem(DayOfWeek.Friday.ToString(), ArticleStrings.Friday),
            new ListItem(DayOfWeek.Saturday.ToString(), ArticleStrings.Saturday),
            new ListItem(DayOfWeek.Sunday.ToString(), ArticleStrings.Sunday),
            new ListItem(DayOfWeek.Weekday.ToString(), ArticleStrings.Weekday),
            new ListItem(DayOfWeek.Weekend.ToString(), ArticleStrings.Weekend)
        };

        public List<ListItem> ShowLimitationTypes => new List<ListItem>
        {
            new ListItem(ShowLimitationType.EndTime.ToString(), ArticleStrings.LimitedByTime, true),
            new ListItem(ShowLimitationType.Duration.ToString(), ArticleStrings.LimitedByDuration, true)
        };

        public List<ListItem> DurationUnits => new List<ListItem>
        {
            new ListItem(ShowDurationUnit.Minutes.ToString(), ArticleStrings.MinutesLimitationUnit),
            new ListItem(ShowDurationUnit.Hours.ToString(), ArticleStrings.HoursLimitationUnit),
            new ListItem(ShowDurationUnit.Days.ToString(), ArticleStrings.DaysLimitationUnit),
            new ListItem(ShowDurationUnit.Weeks.ToString(), ArticleStrings.WeeksLimitationUnit),
            new ListItem(ShowDurationUnit.Months.ToString(), ArticleStrings.MonthsLimitationUnit),
            new ListItem(ShowDurationUnit.Years.ToString(), ArticleStrings.YearsLimitationUnit)
        };

        public string WorkflowWarning => IsNew ? ArticleStrings.CannotAddBecauseOfWorkflow : ArticleStrings.CannotUpdateBecauseOfWorkflow;

        public string RelationSecurityWarning => ArticleStrings.CannotUpdateBecauseOfRelationSecurity;

        public string VariationModel
        {
            get => _variationModel.Value;
            set => _variationModel.Value = value;
        }

        public string ContextModel => _contextModel.Value;

        public string ErrorModel => _errorModel.Value;

        public string CurrentContext { get; set; }

        public bool? BoundToExternal { get; set; }

        public override string EntityTypeCode
        {
            get
            {
                if (ShowArchive)
                {
                    return Constants.EntityTypeCode.ArchiveArticle;
                }

                if (IsVirtual)
                {
                    return Constants.EntityTypeCode.VirtualArticle;
                }

                return Constants.EntityTypeCode.Article;
            }
        }

        public override string ActionCode
        {
            get
            {
                if (IsNew)
                {
                    return Constants.ActionCode.AddNewArticle;
                }

                if (ShowArchive)
                {
                    return Constants.ActionCode.ViewArchiveArticle;
                }

                if (IsVirtual)
                {
                    return Constants.ActionCode.ViewVirtualArticle;
                }

                return Constants.ActionCode.EditArticle;
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

            if (st.Weight != Data.Workflow.MaxStatus.Weight && Data.Workflow.IsAsync && Data.Workflow.CurrentUserHasWorkflowMaxWeight && (Data.Splitted || !Data.Splitted && Data.StatusTypeId == Data.Workflow.MaxStatus.Id))
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

        internal static RelationListResult GetListForRelation(BLL.Field field, string value, int articleId)
        {
            var baseField = field.GetBaseField(articleId);
            var contentId = baseField.RelateToContentId ?? 0;
            var fieldId = baseField.Id;
            var selectedArticleIDs = Converter.ToInt32Collection(value, ',');
            var filter = baseField.GetRelationFilter(articleId);
            var itemCount = ArticleService.Count(contentId, filter, false);
            var isListOverflow = field.MaxDataListItemCount == 0 || itemCount > field.MaxDataListItemCount;
            var mode = isListOverflow ? ListSelectionMode.OnlySelectedItems : ListSelectionMode.AllItems;
            var list = new List<ListItem>();
            if (!isListOverflow || selectedArticleIDs.Length != 0)
            {
                list = ArticleService.SimpleList(contentId, articleId, fieldId, mode, selectedArticleIDs, filter);
            }

            return new RelationListResult { IsListOverflow = isListOverflow, Items = list };
        }

        internal static IEnumerable<ListItem> GetAggregatableContentsForClassifier(BLL.Field classifier, string excludeValue) => ArticleService.GetAggregetableContentsForClassifier(classifier, excludeValue);

        internal static BLL.Content GetContentById(int? contentId) => contentId.HasValue ? ContentService.Read(contentId.Value) : null;

        public void DoCustomBinding()
        {
            Data.VariationListItems = JsonConvert.DeserializeObject<List<ArticleVariationListItem>>(VariationModel);
        }

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
