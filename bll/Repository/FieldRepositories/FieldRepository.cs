using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.DAL.Entities;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Repository.FieldRepositories
{
    public class FieldRepository : IFieldRepository
    {
        internal static IQueryable<FieldDAL> DefaultFieldQuery => QPContext.EFContext.FieldSet.Include("Content").Include("Type").Include("LastModifiedByUser");

        internal static IQueryable<FieldDAL> DefaultLightFieldQuery => QPContext.EFContext.FieldSet.Include("Type").Include("LastModifiedByUser");

        Field IFieldRepository.GetById(int fieldId) => GetById(fieldId);

        Field IFieldRepository.GetByName(int contentId, string fieldName) => GetByName(contentId, fieldName);

        IList<Field> IFieldRepository.GetByNames(int contentId, IList<string> fieldNames)
        {
            var list = MapperFacade.FieldMapper.GetBizList(DefaultLightFieldQuery.Where(f => f.ContentId == contentId && fieldNames.Contains(f.Name)).ToList());
            ApplyContentToFields(list, contentId);
            return list;
        }

        Field IFieldRepository.GetByOrder(int contentId, int order)
        {
            return MapperFacade.FieldMapper.GetBizObject(DefaultFieldQuery.SingleOrDefault(f => f.ContentId == contentId && f.Order == order));
        }

        IEnumerable<Field> IFieldRepository.GetRelatedO2MFields(int fieldId)
        {
            return MapperFacade.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => f.RelationId == fieldId).ToList());
        }

        Field IFieldRepository.GetByBackRelationId(int fieldId)
        {
            return MapperFacade.FieldMapper.GetBizObject(DefaultFieldQuery.SingleOrDefault(f => f.BackRelationId == fieldId && f.Content.VirtualType == VirtualType.None));
        }

        IEnumerable<Field> IFieldRepository.GetChildList(int fieldId)
        {
            return MapperFacade.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => f.ParentFieldId == fieldId).ToList());
        }

        bool IFieldRepository.HasAnyAggregators(int fieldId)
        {
            return QPContext.EFContext.FieldSet.Where(f => f.Id == fieldId).Select(f => f.Aggregators.Any()).Single();
        }

        VisualEditFieldParams IFieldRepository.GetVisualEditFieldParams(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetVisualEditFieldParams(scope.DbConnection, fieldId);
                if (dt.Rows.Count == 0)
                {
                    throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, fieldId));
                }

                return Mapper.Map<DataRow, VisualEditFieldParams>(dt.Rows[0]);
            }
        }

        IEnumerable<Article> IFieldRepository.GetActiveArticlesForM2MField(int fieldId)
        {
            IList<int> activeIds;
            using (var scope = new QPConnectionScope())
            {
                activeIds = Common.GetActiveArticlesIdsForM2MField(scope.DbConnection, fieldId).ToList();
            }

            var entities = QPContext.EFContext;
            return MapperFacade.ArticleMapper.GetBizList(entities.ArticleSet.Include("Content").Where(x => activeIds.Contains((int)x.Id)).ToList());
        }

        int IFieldRepository.GetTextFieldMaxLength(Field dbField)
        {
            if (dbField.IsNew)
            {
                return 0;
            }

            using (var scope = new QPConnectionScope())
            {
                if (dbField.ExactType == FieldExactTypes.String)
                {
                    return Common.GetStringFieldMaxLength(scope.DbConnection, dbField.ContentId, dbField.Name);
                }

                return dbField.IsBlob ? Common.GetBlobFieldMaxSymbolLength(scope.DbConnection, dbField.ContentId, dbField.Name) : 0;
            }
        }

        bool IFieldRepository.IsNonEnumFieldValueExist(Field field)
        {
            var enumValues = field.StringEnumItems.Select(v => v.Value);
            return QPContext.EFContext.ContentDataSet.Any(d => d.FieldId == field.Id && !enumValues.Contains(d.Data));
        }

        int? IFieldRepository.GetNumericFieldMaxValue(Field dbField)
        {
            if (dbField.IsNew)
            {
                return null;
            }

            using (var scope = new QPConnectionScope())
            {
                return Common.GetNumericFieldMaxValue(scope.DbConnection, dbField.ContentId, dbField.Name);
            }
        }

        bool IFieldRepository.LinqBackPropertyNameExists(Field field)
        {
            if (field == null)
            {
                throw new ArgumentException("field");
            }

            if (field.RelateToContentId == null)
            {
                throw new ApplicationException("RelateToContentId is null");
            }

            // Существуют ли в связанном контенте такое поля, у которых имя прямого Linq-свойства равно имени обратного в данном поле
            var existFieldInRelatedContent = QPContext.EFContext.FieldSet.Any(f => f.NetName == field.LinqBackPropertyName && f.Id != field.Id && f.ContentId == field.RelateToContentId);

            // Существуют ли поля ссылающиеся на тот же контент что и данное поле и с таким же именем обратного Linq-свойства
            var relateContentFields = field.Relation.Content.Fields.Select(f => (decimal)f.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var baseM2OFields = field.Content.Fields.Where(f => f.TypeId == FieldTypeCodes.M2ORelation).Select(f => (decimal)f.BackRelationId).ToArray();
            var existSameNetBackNameFields = QPContext.EFContext.FieldSet.Any(
                f => f.TypeId == FieldTypeCodes.Relation &&
                    f.Id != field.Id &&
                    f.NetBackName == field.LinqBackPropertyName &&
                    f.RelationId.HasValue &&
                    relateContentFields.Contains(f.RelationId.Value)
                    && !baseM2OFields.Contains(f.Id)
            );

            return existFieldInRelatedContent || existSameNetBackNameFields;
        }

        bool IFieldRepository.LinqPropertyNameExists(Field field)
        {
            if (field == null)
            {
                throw new ArgumentException("field");
            }

            // Существуют ли в контенте данного поля, c таким же именем прямого Linq-свойства
            var existFieldInContent = QPContext.EFContext.FieldSet.Any(f => f.NetName == field.LinqPropertyName && f.Id != field.Id && f.ContentId == field.ContentId);

            // Существуют ли поля ссылающиеся на контент данного поля и с именем обратного Linq-свойства равным имени прямого Linq-свойства данного поля
            var relateContentFields = field.Content.Fields.Select(f => (decimal)f.Id).ToArray();

            // ReSharper disable once PossibleInvalidOperationException
            var baseM2OFields = field.Content.Fields.Where(f => f.TypeId == FieldTypeCodes.M2ORelation).Select(f => (decimal)f.BackRelationId.Value).ToArray();
            var existSameNetBackNameFields = QPContext.EFContext.FieldSet.Any(
                f => f.TypeId == FieldTypeCodes.Relation &&
                    f.Id != field.Id &&
                    f.Id != field.BackRelationId &&
                    f.NetBackName == field.LinqPropertyName &&
                    f.RelationId.HasValue &&
                    relateContentFields.Contains(f.RelationId.Value)
                    && !baseM2OFields.Contains(f.Id)
            );

            return existFieldInContent || existSameNetBackNameFields;
        }

        bool IFieldRepository.DoPluralLinksExist(Field dbField)
        {
            if (dbField.IsNew)
            {
                return false;
            }

            if (dbField.LinkId == null)
            {
                throw new ApplicationException("Поле не имеет ожидаемой ссылки на контент.");
            }

            using (var scope = new QPConnectionScope())
            {
                return Common.DoPluralLinksExist(scope.DbConnection, dbField.ContentId, dbField.LinkId.Value);
            }
        }

        bool IFieldRepository.LinkNetNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Any(n => n.NetLinkName == link.NetLinkName && n.LinkId != link.LinkId && n.Content.SiteId == link.Content.SiteId);
        }

        bool IFieldRepository.NetNameExists(ContentLink link) => ContentNetNameExists(link) || ((IFieldRepository)this).LinkNetNameExists(link);

        bool IFieldRepository.NetPluralNameExists(ContentLink link) => ContentNetPluralNameExists(link) || LinkNetPluralNameExists(link);

        void IFieldRepository.RemoveLinkVersions(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveLinkVersions(scope.DbConnection, fieldId);
            }
        }

        void IFieldRepository.Delete(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeCleanEmptyLinksTriggerState(scope.DbConnection, false);
                        ChangeRemoveFieldTriggerState(scope.DbConnection, false);
                        ChangeReorderFieldsTriggerState(scope.DbConnection, false);
                        ChangeDeleteContentLinkTriggerState(scope.DbConnection, false);

                    }

                    var field = GetById(id);
                    field.ReorderContentFields(true);
                    var isVirtual = field.Content.IsVirtual;
                    FieldDAL dal = GetDal(field);

                    DefaultRepository.Delete<FieldDAL>(id);

                    if (!isVirtual)
                    {
                        Common.DropColumn(scope.DbConnection, dal);
                        DropLinkTablesAndViews(scope, dal);
                    }
                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeCleanEmptyLinksTriggerState(scope.DbConnection, true);
                        ChangeRemoveFieldTriggerState(scope.DbConnection, true);
                        ChangeReorderFieldsTriggerState(scope.DbConnection, true);
                        ChangeDeleteContentLinkTriggerState(scope.DbConnection, true);
                    }

                }
            }
        }

        internal static void DropLinkTablesAndViews(int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                var m2ms = QPContext.EFContext.FieldSet
                    .Include(n => n.ContentToContent)
                    .Where(n => n.ContentId == contentId && n.LinkId != null)
                    .ToArray();

                foreach (var item in m2ms)
                {
                    DropLinkTablesAndViews(scope, item);
                }
            }
        }

        private static void DropLinkTablesAndViews(QPConnectionScope scope, FieldDAL dal)
        {
            if (dal.LinkId != null)
            {
                var anotherRealFieldsExists = QPContext.EFContext.FieldSet.Include(n => n.Content)
                    .Any(n => n.LinkId == dal.LinkId && n.Content.VirtualType == 0 && n.Id != dal.Id);

                if (!anotherRealFieldsExists)
                {
                    DefaultRepository.SimpleDelete(dal.ContentToContent);
                    Common.DropLinkView(scope.DbConnection, dal.ContentToContent);
                    if (QPContext.DatabaseType == DatabaseType.Postgres)
                    {
                        Common.DropLinkTables(scope.DbConnection, dal.ContentToContent);
                    }
                }
            }
        }

        void IFieldRepository.ClearTreeOrder(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ClearFieldTreeOrder(scope.DbConnection, id);
            }
        }

        Field IFieldRepository.CreateNew(Field item, bool explicitOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeMaxOrderTriggerState(scope.DbConnection, false);
                        ChangeInsertFieldTriggerState(scope.DbConnection, false);
                        ChangeM2MDefaultTriggerState(scope.DbConnection, false);
                    }

                    if (explicitOrder)
                    {
                        item.ReorderContentFields();
                    }
                    else
                    {
                        var maxOrder = (int)QPContext.EFContext.FieldSet.Where(n => n.ContentId == item.ContentId)
                            .Select(n => n.Order).DefaultIfEmpty(0).Max();
                        item.Order = maxOrder + 1;
                    }

                    var constraint = item.Constraint;
                    var dynamicImage = item.DynamicImage;

                    DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Field, item);
                    var newItem = DefaultRepository.Save<Field, FieldDAL>(item);
                    var field = GetDal(newItem);
                    Common.AddColumn(scope.DbConnection, field);

                    DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Field);

                    SaveConstraint(constraint, newItem);
                    SaveDynamicImage(dynamicImage, newItem);

                    return GetById(newItem.Id);
                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeMaxOrderTriggerState(scope.DbConnection, true);
                        ChangeInsertFieldTriggerState(scope.DbConnection, true);
                        ChangeM2MDefaultTriggerState(scope.DbConnection, true);
                    }

                }
            }
        }

        private static FieldTypeDAL GetType(int id)
        {
            return QPContext.EFContext.FieldTypeSet.FirstOrDefault(n => n.Id == id);
        }

        private static ContentToContentDAL GetContentToContent(int? linkId)
        {
            return (linkId.HasValue) ?
                QPContext.EFContext.ContentToContentSet.FirstOrDefault(n => n.LinkId == linkId.Value) : null;
        }

        private static FieldDAL GetDal(Field newItem)
        {
            var field = MapperFacade.FieldMapper.GetDalObject(newItem);
            field.Type = GetType(newItem.TypeId);
            field.ContentToContent = GetContentToContent(newItem.LinkId);
            return field;
        }

        Field IFieldRepository.Update(Field item)
        {
            using (var scope = new QPConnectionScope())
            {
                try
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeMaxOrderTriggerState(scope.DbConnection, false);
                        ChangeM2MDefaultTriggerState(scope.DbConnection, false);
                        ChangeCleanEmptyLinksTriggerState(scope.DbConnection, false);
                        ChangeInsertFieldTriggerState(scope.DbConnection, false);
                        ChangeUpdateFieldTriggerState(scope.DbConnection, false);
                    }

                    var preUpdateField = GetById(item.Id);
                    var constraint = item.Constraint;
                    var dynamicImage = item.DynamicImage;

                    item.ReorderContentFields();

                    UpdateBackwardFields(item, preUpdateField);

                    UpdateRelationData(item, preUpdateField);

                    var newItem = DefaultRepository.Update<Field, FieldDAL>(item);
                    var oldDal = GetDal(preUpdateField);
                    var newDal = GetDal(newItem);
                    Common.UpdateColumn(scope.DbConnection, oldDal, newDal);

                    UpdateFieldOrder(newItem.Id, item.Order);

                    UpdateConstraint(constraint, newItem);

                    UpdateDynamicImage(dynamicImage, newItem);

                    return GetById(newItem.Id);
                }
                finally
                {
                    if (QPContext.DatabaseType == DatabaseType.SqlServer)
                    {
                        ChangeMaxOrderTriggerState(scope.DbConnection, true);
                        ChangeM2MDefaultTriggerState(scope.DbConnection, true);
                        ChangeCleanEmptyLinksTriggerState(scope.DbConnection, true);
                        ChangeInsertFieldTriggerState(scope.DbConnection, true);
                        ChangeUpdateFieldTriggerState(scope.DbConnection, true);
                    }
                }
            }
        }

        void IFieldRepository.SetFieldM2MDefValue(int fieldId, int[] defaultArticles)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetFieldM2MDefValue(scope.DbConnection, defaultArticles, fieldId);
            }
        }

        void IFieldRepository.CorrectEnumInContentData(Field field)
        {
            if (!field.IsNew && field.ExactType == FieldExactTypes.StringEnum && field.StringEnumItems.Any())
            {
                var enumValues = field.StringEnumItems.Select(v => v.Value).ToList();
                using (var scope = new QPConnectionScope())
                {
                    Common.CorrectEnumInContentData(scope.DbConnection, field.Id, enumValues, field.StringDefaultValue);
                }
            }
        }

        Field IFieldRepository.GetBaseField(int fieldId, int articleId) => GetById(((IFieldRepository)this).GetBaseFieldId(fieldId, articleId));

        int IFieldRepository.GetBaseFieldId(int fieldId, int articleId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetBaseFieldId(QPConnectionScope.Current.DbConnection, fieldId, articleId);
            }
        }

        List<Field> IFieldRepository.GetDynamicImageFields(int contentId, int imageFieldId)
        {
            return MapperFacade.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => f.ContentId == contentId && f.BaseImageId == imageFieldId).ToList());
        }

        void IFieldRepository.UpdateContentFieldsOrders(int contentId, int currentOrder, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateContentFieldsOrder(scope.DbConnection, currentOrder, newOrder, contentId);
            }
        }

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        IEnumerable<Content> IFieldRepository.GetAggregatableContentsForClassifier(Field classifier)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return MapperFacade.ContentMapper.GetBizList(QPContext.EFContext.FieldSet
                    .Where(f => f.Id == classifier.Id)
                    .SelectMany(f => f.Aggregators)
                    .Select(f => f.Content)
                    .ToList()
                );
            }

            return Enumerable.Empty<Content>();
        }

        bool IFieldRepository.FieldHasArticles(int contentId)
        {
            var entities = QPContext.EFContext;
            return entities.ArticleSet.Any(x => x.ContentId == contentId);
        }

        bool IFieldRepository.CheckNumericValuesAsKey(Field field, Field dbField)
        {
            if (field.IsNew)
            {
                return true;
            }

            if (field.RelateToContentId == null)
            {
                throw new ApplicationException("Поле не имеет ожидаемой ссылки на контент.");
            }

            using (var scope = new QPConnectionScope())
            {
                return Common.CheckNumericValuesAsO2MForeingKey(scope.DbConnection, field.ContentId, dbField.Name, field.RelateToContentId.Value);
            }
        }

        bool IFieldRepository.NameExists(Field field)
        {
            return QPContext.EFContext.FieldSet.Any(x => x.Name == field.Name && x.ContentId == field.ContentId);
        }

        public static Field GetById(int fieldId)
        {
            return GetByIdFromCache(fieldId) ?? MapperFacade.FieldMapper.GetBizObject(DefaultFieldQuery.SingleOrDefault(f => f.Id == fieldId));
        }

        public static Field GetByName(int contentId, string fieldName)
        {
            return MapperFacade.FieldMapper.GetBizObject(DefaultFieldQuery.SingleOrDefault(f => f.ContentId == contentId && f.Name == fieldName));
        }

        public static IEnumerable<Field> GetList(IEnumerable<int> ids)
        {
            var decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MapperFacade.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        private static void ApplyContentToFields(IEnumerable<Field> fields, int contentId)
        {
            var content = ContentRepository.GetById(contentId);
            foreach (var item in fields)
            {
                item.Content = content;
            }
        }

        internal static List<Field> GetList(int contentId, bool forList)
        {
            var result = GetListFromCache(contentId);
            if (result != null)
            {
                return result;
            }

            var lambda = (Expression<Func<FieldDAL, bool>>)(f => true);
            if (forList)
            {
                lambda = n => n.ViewInList;
            }

            var list =  MapperFacade.FieldMapper.GetBizList(
                DefaultLightFieldQuery.Where(n => n.ContentId == contentId).Where(lambda).OrderBy(n => n.Order).ToList()
            );
            ApplyContentToFields(list, contentId);
            return list;
        }

        internal static ListResult<FieldListItem> GetList(ListCommand cmd, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                var options = new FieldPageOptions
                {
                    ContentId = contentId,
                    SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? MapperFacade.FieldListItemRowMapper.TranslateSortExpression(cmd.SortExpression) : null,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize
                };

                var rows = Common.GetFieldsPage(scope.DbConnection, options, out var totalRecords);
                return new ListResult<FieldListItem> { Data = MapperFacade.FieldListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        private static List<Field> GetListFromCache(int contentId)
        {
            IList<Field> result = null;
            var cache = QPContext.GetFieldCache();
            var cache2 = QPContext.GetContentFieldCache();
            if (cache != null && cache2 != null && cache2.ContainsKey(contentId))
            {
                var fieldIds = cache2[contentId];
                var tempFieldIds = fieldIds.Select(n => cache.ContainsKey(n) ? cache[n] : null).ToList();
                if (tempFieldIds.All(n => n != null))
                {
                    result = tempFieldIds;
                }
            }

            return result?.ToList();
        }

        internal static ListResult<FieldListItem> GetListForSelect(ListCommand cmd, int contentId, int[] ids, FieldSelectMode mode)
        {
            using (var scope = new QPConnectionScope())
            {
                var options = new FieldPageOptions
                {
                    ContentId = contentId,
                    SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? MapperFacade.FieldListItemRowMapper.TranslateSortExpression(cmd.SortExpression) : null,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize,
                    SelectedIDs = ids,
                    Mode = mode
                };

                var rows = Common.GetFieldsPage(scope.DbConnection, options, out var totalRecords);
                return new ListResult<FieldListItem> { Data = MapperFacade.FieldListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        internal static IEnumerable<Field> GetAll()
        {
            return MapperFacade.FieldMapper.GetBizList(DefaultFieldQuery.OrderBy(n => n.Order).ThenBy(n => n.Id).ToList());
        }

        internal static Field GetTitleField(int contentId)
        {
            var fieldName = ContentRepository.GetTitleName(contentId);
            var field = GetByName(contentId, fieldName);
            if (field == null || !field.ViewInList)
            {
                field = MapperFacade.FieldMapper.GetBizObject(DefaultFieldQuery.Where(n => n.ContentId == contentId && n.ViewInList).OrderBy(n => n.Order).FirstOrDefault());
            }

            return field;
        }

        internal static List<Field> GetFullList(int contentId) => GetList(contentId, false);

        internal static DynamicImage GetDynamicImageInfoById(int fieldId)
        {
            var info = DefaultRepository.GetById<DynamicImageFieldDAL>(fieldId);
            return info == null ? null : MapperFacade.DynamicImageMapper.GetBizObject(info);
        }

        internal static IEnumerable<FieldType> GetAllFieldTypes() => MapperFacade.FieldTypeMapper.GetBizList(QPContext.EFContext.FieldTypeSet.ToList());

        internal static IEnumerable<ListItem> GetBaseFieldsForM2O(int contentId, int fieldId)
        {
            var siteId = ContentRepository.GetById(contentId).SiteId;
            var result = new List<ListItem>();
            using (var scope = new QPConnectionScope())
            {
                foreach (DataRow row in Common.GetBaseFieldsForM2O(scope.DbConnection, contentId, fieldId).Rows)
                {
                    var parts = new List<string>();
                    if (siteId != (int)(decimal)row["site_id"])
                    {
                        parts.Add((string)row["site_name"]);
                    }

                    if (contentId != (int)(decimal)row["content_id"])
                    {
                        parts.Add((string)row["content_name"]);
                    }

                    parts.Add((string)row["attribute_name"]);
                    var currentFieldId = (int)(decimal)row["attribute_id"];
                    var resultPart = new ListItem { Text = string.Join(".", parts), Value = currentFieldId.ToString() };
                    if (currentFieldId == fieldId)
                    {
                        resultPart.Selected = true;
                    }

                    result.Add(resultPart);
                }
            }

            return result;
        }

        private static void UpdateFieldOrder(int fieldId, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateFieldOrder(scope.DbConnection, fieldId, newOrder);
            }
        }

        private static void SaveDynamicImage(DynamicImage dynamicImage, Field newItem)
        {
            if (dynamicImage != null)
            {
                dynamicImage.Id = newItem.Id;
                var dynamicImageDal = MapperFacade.DynamicImageMapper.GetDalObject(dynamicImage);
                dynamicImageDal = DefaultRepository.SimpleSave(dynamicImageDal);
                newItem.DynamicImage = MapperFacade.DynamicImageMapper.GetBizObject(dynamicImageDal);
            }
        }

        private static void SaveConstraint(ContentConstraint constraint, Field newItem)
        {
            if (constraint != null)
            {
                BindFieldToContstraint(newItem, constraint);
                ContentConstraintRepository.Save(constraint);
            }
        }

        /// <summary>
        /// Обновляет данные связей при изменении типов связей или связанных контентов
        /// </summary>
        private static void UpdateRelationData(Field newItem, Field preUpdateField)
        {
            // M2M -> M2O или новое базовое поле для M2O
            if (preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.M2ORelation ||
                preUpdateField.ExactType == FieldExactTypes.M2ORelation && newItem.ExactType == FieldExactTypes.M2ORelation && preUpdateField.BackRelationId != newItem.BackRelationId)
            {
                using (var scope = new QPConnectionScope())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Common.ChangeContentDataForRelation(scope.DbConnection, newItem.Id, newItem.BackRelationId.Value);
                }
            }

            // M2O -> M2M или новый связанный контент для M2M
            if (preUpdateField.ExactType == FieldExactTypes.M2ORelation && newItem.ExactType == FieldExactTypes.M2MRelation ||
                preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.M2MRelation && preUpdateField.LinkId != newItem.LinkId)
            {
                using (var scope = new QPConnectionScope())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Common.ChangeContentDataForRelation(scope.DbConnection, newItem.Id, newItem.LinkId.Value);
                }
            }

            // O2M -> M2M
            if (preUpdateField.ExactType == FieldExactTypes.O2MRelation && newItem.ExactType == FieldExactTypes.M2MRelation)
            {
                using (var scope = new QPConnectionScope())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Common.O2MtoM2MTranferData(scope.DbConnection, newItem.Id, newItem.LinkId.Value);
                }
            }

            // M2M -> O2M
            else if (preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.O2MRelation)
            {
                using (var scope = new QPConnectionScope())
                {
                    // ReSharper disable once PossibleInvalidOperationException
                    Common.M2MtoO2MTranferData(scope.DbConnection, newItem.Id, preUpdateField.LinkId.Value);
                }
            }
        }

        private void UpdateBackwardFields(Field newItem, Field preUpdateField)
        {
            // если тип поменялся с O2M на что-то другое (не M2M), либо поменялся связанный контент
            if (preUpdateField.ExactType == FieldExactTypes.O2MRelation && preUpdateField.O2MBackwardField != null && !preUpdateField.O2MBackwardField.IsNew)
            {
                if (newItem.ExactType != FieldExactTypes.O2MRelation && newItem.ExactType != FieldExactTypes.M2MRelation ||
                    newItem.ExactType == FieldExactTypes.O2MRelation && preUpdateField.RelateToContentId != newItem.RelateToContentId)
                {
                    preUpdateField.O2MBackwardField.Die();
                }
            }

            // если тип поменялся с O2M на M2M
            if (preUpdateField.ExactType == FieldExactTypes.O2MRelation && newItem.ExactType == FieldExactTypes.M2MRelation && preUpdateField.O2MBackwardField != null && !preUpdateField.O2MBackwardField.IsNew)
            {
                var f = preUpdateField.O2MBackwardField;
                f.ExactType = FieldExactTypes.M2MRelation;
                f.BackRelationId = null;
                f.LinkId = newItem.LinkId;
                f.DefaultValue = null;
                f.PersistWithVirtualRebuild();
            }

            // если тип поменялся с M2M на O2M
            if (preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.O2MRelation && preUpdateField.M2MBackwardField != null && !preUpdateField.M2MBackwardField.IsNew)
            {
                var f = preUpdateField.M2MBackwardField;
                f.ExactType = FieldExactTypes.M2ORelation;
                f.BackRelationId = newItem.Id;
                f.LinkId = null;
                ((IFieldRepository)this).SetFieldM2MDefValue(f.Id, Enumerable.Empty<int>().ToArray());
                f.PersistWithVirtualRebuild();
            }
        }

        private static void UpdateDynamicImage(DynamicImage dymamicImage, Field newItem)
        {
            if (dymamicImage != null)
            {
                var dymamicImageDal = MapperFacade.DynamicImageMapper.GetDalObject(dymamicImage);
                if (dymamicImage.IsNew)
                {
                    dymamicImage.Id = newItem.Id;
                    dymamicImageDal = DefaultRepository.SimpleSave(dymamicImageDal);
                }
                else
                {
                    dymamicImageDal = DefaultRepository.SimpleUpdate(dymamicImageDal);
                }

                newItem.DynamicImage = MapperFacade.DynamicImageMapper.GetBizObject(dymamicImageDal);
            }
            else
            {
                var context = QPContext.EFContext;
                var dynamicImageFieldDAL = DefaultRepository.GetById<DynamicImageFieldDAL>(newItem.Id, context);
                if (dynamicImageFieldDAL != null)
                {
                    DefaultRepository.SimpleDelete(dynamicImageFieldDAL, context);
                }
            }
        }

        private static void UpdateConstraint(ContentConstraint constraint, Field newItem)
        {
            if (constraint != null)
            {
                BindFieldToContstraint(newItem, constraint);
                if (constraint.IsNew)
                {
                    ContentConstraintRepository.Save(constraint);
                }
                else
                {
                    ContentConstraintRepository.Update(constraint);
                }
            }
        }

        internal static bool Exists(int id)
        {
            return QPContext.EFContext.FieldSet.Any(n => n.Id == id);
        }

        private static bool ContentNetNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.NetName == link.NetLinkName && n.SiteId == link.Content.SiteId);
        }

        private static bool LinkNetPluralNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Any(n => n.NetPluralLinkName == link.NetPluralLinkName && n.LinkId != link.LinkId && n.Content.SiteId == link.Content.SiteId);
        }

        private static bool ContentNetPluralNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.NetPluralName == link.NetPluralLinkName && n.SiteId == link.Content.SiteId);
        }

        private static void BindFieldToContstraint(Field item, ContentConstraint constraint)
        {
            constraint.ContentId = item.ContentId;
            foreach (var r in constraint.Rules)
            {
                if (r.FieldId == 0)
                {
                    r.FieldId = item.Id;
                }
            }
        }

        internal static void ChangeMaxOrderTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_set_max_order", enable);
        }

        internal static void ChangeInsertFieldTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_insert_field", enable);
        }

        internal static void ChangeUpdateFieldTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "tu_update_field", enable);
        }

        internal static void ChangeRemoveFieldTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "td_remove_field", enable);
        }

        internal static void ChangeReorderFieldsTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "td_reorder_fields", enable);
        }

        internal static void ChangeCleanEmptyLinksTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "tu_content_attribute_clean_empty_links", enable);
            Common.ChangeTriggerState(cnn, "td_content_attribute_clean_empty_links", enable);
        }

        internal static void ChangeM2MDefaultTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "tu_content_attribute_m2m_default_value", enable);
            Common.ChangeTriggerState(cnn, "ti_content_attribute_m2m_default_value", enable);
        }

        internal static void ChangeInsertContentLinkTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "ti_content_to_content", enable);
        }

        internal static void ChangeDeleteContentLinkTriggerState(DbConnection cnn, bool enable)
        {
            Common.ChangeTriggerState(cnn, "td_content_to_content", enable);
        }


        public class ContentListItem
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public decimal SiteId { get; set; }
        }

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        internal static IEnumerable<ListItem> GetAggregatableContentListItemsForClassifier(Field classifier, string excludeValue = null, int permissionLevel = PermissionLevel.Modify)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return AggregatedContentListItems(classifier, excludeValue, permissionLevel).Select(c => new ListItem(c.Id.ToString(), c.Name)).ToArray();
            }

            return Enumerable.Empty<ListItem>();
        }

        internal static int[] GetAggregatableContentIdsForClassifier(Field classifier)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return AggregatedContentListItems(classifier, null, PermissionLevel.List).Select(c => c.Id).ToArray();
            }

            return new int[] { };
        }

        private static IEnumerable<ContentListItem> AggregatedContentListItems(Field classifier, string excludeValue, int permissionLevel)
        {
            var items = QPContext.EFContext.FieldSet
                .Where(f => f.Id == classifier.Id)
                .SelectMany(f => f.Aggregators)
                .Select(a => new ContentListItem { Id = (int)a.Content.Id, Name = a.Content.Name, SiteId = a.Content.SiteId })
                .Distinct()
                .OrderBy(c => c.Name)
                .ToArray();

            if (!QPContext.IsAdmin && classifier.UseTypeSecurity)
            {
                var ids = items.Select(n => n.Id).ToArray();
                var siteId = items.Select(n => (int)n.SiteId).First();
                var excludeId = Converter.ToInt32(excludeValue, 0);
                using (var scope = new QPConnectionScope())
                {
                    var result = CommonSecurity.CheckContentSecurity(scope.DbConnection, siteId, ids, QPContext.CurrentUserId, permissionLevel);
                    items = items.Where(n => result[n.Id] || n.Id == excludeId).ToArray();
                }
            }

            return items;
        }

        internal static Field Copy(Field field)
        {
            field.MutateNames();
            field.Constraint = null;

            field.UseForTree = false;
            field.UseForVariations = false;
            field.Aggregated = false;

            if (field.DynamicImage != null)
            {
                field.DynamicImage.Id = 0;
            }

            field.Id = 0;
            if (field.ContentLink != null)
            {
                field.ContentLink.LinkId = 0;
                field.ContentLink.MutateNames();
                field.SaveContentLink();
            }

            return field.PersistWithVirtualRebuild(true);
        }

        internal static void MoveFieldOrders(int contentId, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.MoveFieldOrders(scope.DbConnection, contentId, newOrder);
            }
        }

        internal static IEnumerable<DataRow> GetRelationsBetweenAttributes(int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetRelationsBetweenAttributes(scope.DbConnection, sourceSiteId, destinationSiteId, contentIds, forVirtualContents, byNewContents);
            }
        }

        internal static string GetRelationsBetweenAttributesXml(int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
        {
            using (var scope = new QPConnectionScope())
            {
                var rows = Common.GetRelationsBetweenAttributes(scope.DbConnection, sourceSiteId, destinationSiteId, contentIds, forVirtualContents, byNewContents);
                return MultistepActionHelper.GetXmlFromDataRows(rows, "attribute");
            }
        }

        internal static void CopyCommandFieldBind(string relationsBetweenAttributes)
        {
            using (new QPConnectionScope())
            {
                Common.CopyCommandFieldBind(QPConnectionScope.Current.DbConnection, relationsBetweenAttributes);
            }
        }

        internal static void CopyStyleFieldBind(string relationsBetweenAttributes)
        {
            using (new QPConnectionScope())
            {
                Common.CopyStyleFieldBind(QPConnectionScope.Current.DbConnection, relationsBetweenAttributes);
            }
        }

        internal static void CopyContentsAttributes(int oldSiteId, int newSiteId, string newContentIds, bool isContentsVirtual)
        {
            using (new QPConnectionScope())
            {
                Common.CopyContentsAttributes(QPConnectionScope.Current.DbConnection, oldSiteId, newSiteId, newContentIds, isContentsVirtual);
            }
        }

        internal static void CopyDynamicImageAttributes(string relationsBetweenAttributesXml)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.CopyDynamicImageAttributes(scope.DbConnection, relationsBetweenAttributesXml);
            }
        }

        internal static void UpdateAttributesOrder(int destinationSiteId, string relationsBetweenAttributesXml, string newContents)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateAttributesOrder(scope.DbConnection, destinationSiteId, relationsBetweenAttributesXml, newContents);
            }
        }

        internal static void UpdateAttributes(int sourceSiteId, int destinationSiteId, string relationsBetweenAttributesXml, string contentIds)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateAttributes(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenAttributesXml, contentIds);
            }
        }

        internal static void UpdateAttributeLinkIdAndDefaultValue(int sourceSiteId, int destinationSiteId, string relationsBetweenLinksXml)
        {
            using (new QPConnectionScope())
            {
                Common.UpdateAttributeLinkIdAndDefaultValue(QPConnectionScope.Current.DbConnection, sourceSiteId, destinationSiteId, relationsBetweenLinksXml);
            }
        }

        private static Field GetByIdFromCache(int fieldId)
        {
            Field result = null;
            var cache = QPContext.GetFieldCache();
            if (cache != null && cache.ContainsKey(fieldId))
            {
                result = cache[fieldId];
            }

            return result;
        }
    }
}
