using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services
{
    /// <summary>
    /// Интерфейс сервиса блока поиска по статьям
    /// </summary>
    public interface IArticleSearchService
    {
        /// <summary>
        /// Возвращает список "текстовых" полей (по которым возможет полнотекстовый поиск)
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns></returns>
        IEnumerable<ArticleSearchableField> GetFullTextSearchableFieldList(int contentId);

        IEnumerable<IGrouping<string, ArticleSearchableField>> GetFullTextSearchableFieldGroups(int contentId);

        /// <summary>
        /// Возвращает cписок полей по которым возможен поиск
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        IEnumerable<ArticleSearchableField> GetSearchableFieldList(int contentId);

        IEnumerable<IGrouping<string, ArticleSearchableField>> GetSearchableFieldFieldGroups(int contentId);

        /// <summary>
        /// Получить поле по его ID
        /// </summary>
        /// <param name="fieldID"></param>
        /// <returns></returns>
        Field GetFieldByID(int fieldID);

        /// <summary>
        /// Возвращает список всех пользователей
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetAllUsersList();

        /// <summary>
        /// Получить список статусов сайта по ID контента
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        IEnumerable<StatusType> GetStatusListByContentId(int contentId);

        IEnumerable<ListItem> GetSimpleList(Field field, IEnumerable<int> selectedArticleIDs);

        /// <summary>
        /// Возвращает список настроек фильтров по умолчанию для списка статей
        /// </summary>
        /// <param name="actionCode"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        IEnumerable<RelationSearchBlockState> GetDefaultFilterStates(string actionCode, int contentId);
    }

    /// <summary>
    /// Реализация сервиса блока поиска по статьям
    /// </summary>
    public class ArticleSearchService : IArticleSearchService
    {
        private readonly IArticleSearchRepository articleSearchRepository;

        public ArticleSearchService(IArticleSearchRepository articleSearchRepository)
        {
            Contract.Requires(articleSearchRepository != null);

            this.articleSearchRepository = articleSearchRepository;
        }

        public IEnumerable<ListItem> GetSimpleList(Field field, IEnumerable<int> selectedArticleIDs) => ArticleRepository.GetSimpleList(
            new SimpleListQuery()
            {
                ParentEntityId = field.RelateToContentId.Value,
                ListId = field.Id,
                SelectionMode = ListSelectionMode.OnlySelectedItems,
                SelectedEntitiesIds = selectedArticleIDs.ToArray(),
                Filter = MapperFacade.CustomFilterMapper.GetBizList(field?.ExternalRelationFilter).ToArray()
            }
            );

        /// <summary>
        /// Возвращает список "текстовых" полей (по которым возможет полнотекстовый поиск)
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns></returns>
        public IEnumerable<ArticleSearchableField> GetFullTextSearchableFieldList(int contentId)
        {
            var fields = articleSearchRepository.GetAllArticleFields(contentId);
            var result = GetArticleFieldList(fields, false, f => ArticleFieldSearchType.FullText);

            var contentIds = ContentRepository.GetReferencedAggregatedContentIds(contentId);
            var aggregatedFields = (from cid in contentIds
                from f in articleSearchRepository.GetAllArticleFields(cid)
                select f).ToArray();

            var ids = fields.Concat(aggregatedFields).Where(f =>
                f.Type.Name == FieldTypeName.String ||
                f.Type.Name == FieldTypeName.Textbox ||
                f.Type.Name == FieldTypeName.VisualEdit).Select(f => f.Id);

            if (ids.Any())
            {
                result = new[]
                {
                    new ArticleSearchableField
                    {
                        Name = ArticleStrings.SearchBlock_AllTextFields,
                        ID = string.Join(",", ids),
                        ColumnName = null,
                        ArticleFieldSearchType = ArticleFieldSearchType.FullText,
                        IsAll = true
                    }
                }.Concat(result);
            }

            result = new[]
            {
                new ArticleSearchableField
                {
                    Name = ArticleStrings.SearchBlock_AllFields,
                    ID = "",
                    ColumnName = null,
                    ArticleFieldSearchType = ArticleFieldSearchType.FullText,
                    IsAll = true
                }
            }.Concat(result);

            result = result.Concat(GetArticleFieldList(aggregatedFields, true, GetFieldSearchType));

            return result;
        }

        public IEnumerable<IGrouping<string, ArticleSearchableField>> GetFullTextSearchableFieldGroups(int contentId) => (from f in GetFullTextSearchableFieldList(contentId)
            group f by f.GroupName
            into g
            select g).ToArray();

        /// <summary>
        /// Возвращает cписок полей по которым возможен поиск
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public IEnumerable<ArticleSearchableField> GetSearchableFieldList(int contentId)
        {
            var contentIds = ContentRepository.GetReferencedAggregatedContentIds(contentId).Concat(new[] { contentId }).ToArray();

            return GetArticleSystemFieldList()
                .Concat
                (
                    from cid in contentIds
                    from f in GetArticleFieldList(cid, cid != contentId)
                    select f
                )
                .ToArray();
        }

        public IEnumerable<IGrouping<string, ArticleSearchableField>> GetSearchableFieldFieldGroups(int contentId) => (from f in GetSearchableFieldList(contentId)
            group f by f.GroupName
            into g
            select g).ToArray();

        private IEnumerable<ArticleSearchableField> GetArticleFieldList(IEnumerable<Field> fields, bool fillGroup, Func<Field, ArticleFieldSearchType> getFieldSearchType)
        {
            return fields
                .Where(f => !fillGroup || !f.Aggregated)
                .Select(f => new ArticleSearchableField
                {
                    ID = f.Id.ToString(),
                    ContentId = fillGroup ? f.ContentId.ToString() : null,
                    Name = f.DisplayName,
                    ColumnName = f.Name,
                    GroupName = fillGroup ? f.Content.Name : null,
                    ArticleFieldSearchType = getFieldSearchType(f),
                    IsAll = false
                });
        }

        private IEnumerable<ArticleSearchableField> GetArticleFieldList(int contentId, bool fillGroup)
        {
            if (fillGroup)
            {
                return GetArticleFieldList(articleSearchRepository.GetAllArticleFields(contentId), fillGroup, GetFieldSearchType);
            }

            return GetContentRelatedFields(contentId);
        }

        private IEnumerable<ArticleSearchableField> GetContentRelatedFields(int contentId)
        {
            var fieldMap = articleSearchRepository.GetAllArticleRelatedFields(contentId);

            foreach (var entry in fieldMap)
            {
                var field = entry.Key;
                var relatedFields = entry.Value;

                yield return new ArticleSearchableField
                {
                    ID = field.Id.ToString(),
                    ContentId = null,
                    GroupName = null,
                    ReferenceFieldId = null,
                    Name = field.DisplayName,
                    ColumnName = field.Name,
                    ArticleFieldSearchType = GetFieldSearchType(field)
                };

                foreach (var relatedField in relatedFields)
                {
                    yield return new ArticleSearchableField
                    {
                        ID = relatedField.Id.ToString(),
                        ContentId = relatedField.ContentId.ToString(),
                        GroupName = null,
                        ReferenceFieldId = field.Id.ToString(),
                        Name = field.DisplayName + "." + relatedField.DisplayName,
                        ColumnName = relatedField.Name,
                        ArticleFieldSearchType = GetFieldSearchType(relatedField)
                    };
                }
            }
        }

        private IEnumerable<ArticleSearchableField> GetArticleSystemFieldList()
        {
            return ServiceField.CreateAll() // служебные поля
                .Select(f => new ArticleSearchableField
                {
                    ID = f.ID.ToString(),
                    Name = f.Name,
                    ColumnName = f.ColumnName,
                    ArticleFieldSearchType = f.ArticleFieldSearchType
                });
        }

        public Field GetFieldByID(int fieldID) => articleSearchRepository.GetFieldById(fieldID);

        /// <summary>
        /// Определяет тип поиска по полю
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private ArticleFieldSearchType GetFieldSearchType(Field field)
        {
            if (field.ExactType == FieldExactTypes.StringEnum)
            {
                return ArticleFieldSearchType.StringEnum;
            }

            if (field.Type.Name == FieldTypeName.String ||
                field.Type.Name == FieldTypeName.File ||
                field.Type.Name == FieldTypeName.Image ||
                field.Type.Name == FieldTypeName.Textbox ||
                field.Type.Name == FieldTypeName.VisualEdit ||
                field.Type.Name == FieldTypeName.DynamicImage)
            {
                return ArticleFieldSearchType.Text;
            }

            if (field.IsClassifier)
            {
                return ArticleFieldSearchType.Classifier;
            }

            if (field.Type.Name == FieldTypeName.Date)
            {
                return ArticleFieldSearchType.DateRange;
            }

            if (field.Type.Name == FieldTypeName.DateTime)
            {
                return ArticleFieldSearchType.DateTimeRange;
            }

            if (field.Type.Name == FieldTypeName.Time)
            {
                return ArticleFieldSearchType.TimeRange;
            }

            if (field.Type.Name == FieldTypeName.Numeric)
            {
                return ArticleFieldSearchType.NumericRange;
            }

            if (field.Type.Name == FieldTypeName.Boolean)
            {
                return ArticleFieldSearchType.Boolean;
            }

            if (field.RelationType == RelationType.ManyToMany)
            {
                return ArticleFieldSearchType.M2MRelation;
            }

            if (field.RelationType == RelationType.ManyToOne)
            {
                return ArticleFieldSearchType.M2ORelation;
            }

            if (field.RelationType == RelationType.OneToMany)
            {
                return ArticleFieldSearchType.O2MRelation;
            }

            return ArticleFieldSearchType.None;
        }

        public IEnumerable<User> GetAllUsersList() => articleSearchRepository.GetAllUsersList();

        /// <summary>
        /// Получить список статусов сайта по ID контента
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public IEnumerable<StatusType> GetStatusListByContentId(int contentId)
        {
            var content = articleSearchRepository.GetContentById(contentId);
            if (content == null)
            {
                return Enumerable.Empty<StatusType>();
            }

            return articleSearchRepository.GetStatusList(content.SiteId);
        }

        /// <summary>
        /// Возвращает список настроек фильтров по умолчанию для списка статей
        /// </summary>
        /// <param name="actionCode"></param>
        /// <param name="contentId"></param>
        /// <returns></returns>
        public IEnumerable<RelationSearchBlockState> GetDefaultFilterStates(string actionCode, int contentId)
        {
            var action = BackendActionRepository.GetByCode(actionCode);
            var entityCode = action.EntityType.Code;
            if (
                (
                    action.ActionType.Code.Equals(ActionTypeCode.List, StringComparison.CurrentCultureIgnoreCase) ||
                    action.ActionType.Code.Equals(ActionTypeCode.Select, StringComparison.CurrentCultureIgnoreCase) ||
                    action.ActionType.Code.Equals(ActionTypeCode.MultipleSelect, StringComparison.CurrentCultureIgnoreCase)
                )
                &&
                (
                    entityCode.Equals(EntityTypeCode.Article, StringComparison.CurrentCultureIgnoreCase) ||
                    entityCode.Equals(EntityTypeCode.VirtualArticle, StringComparison.CurrentCultureIgnoreCase)
                )
            )
            {
                var defFilter = UserRepository.GetById(QPContext.CurrentUserId).ContentDefaultFilters
                    .Where(f => f.ContentId.HasValue && f.ArticleIDs.Any())
                    .ToDictionary(f => f.ContentId.Value);
                if (defFilter.Any())
                {
                    var articles =
                        ArticleRepository.GetList(defFilter.SelectMany(f => f.Value.ArticleIDs).ToList())
                            .GroupBy(a => a.ContentId)
                            .ToDictionary(g => g.Key, g => g.Select(a => new EntityListItem { Id = a.Id, Name = a.Name }));

                    return ContentRepository.GetById(contentId).Fields
                        .Where(f =>
                            (f.ExactType == FieldExactTypes.M2MRelation || f.ExactType == FieldExactTypes.O2MRelation)
                            && f.UseForDefaultFiltration
                            && f.RelateToContentId.HasValue && defFilter.ContainsKey(f.RelateToContentId.Value)
                        )
                        .Select(f => new RelationSearchBlockState
                        {
                            SearchType = GetFieldSearchType(f),
                            FieldId = f.Id,
                            ContentId = f.ContentId,
                            FieldColumnName = f.Name,
                            FieldName = f.DisplayName,
                            FieldGroup = f.Content.Name,
                            SelectedEntities = articles[f.RelateToContentId.Value]
                        })
                        .ToArray();
                }

                return Enumerable.Empty<RelationSearchBlockState>();
            }

            return Enumerable.Empty<RelationSearchBlockState>();
        }
    }
}
