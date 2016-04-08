using AutoMapper;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;

namespace Quantumart.QP8.BLL.Repository
{
    internal class FieldRepository
    {
        internal static ObjectQuery<FieldDAL> DefaultFieldQuery => QPContext.EFContext.FieldSet.Include("Content").Include("Type").Include("LastModifiedByUser");

        #region Methods
        #region Get
        /// <summary>
        /// Возвращает поле по его идентификатору
        /// </summary>
        /// <param name="fieldId">идентификатор поля</param>
        /// <returns>информация о поле</returns>
        internal static Field GetById(int fieldId)
        {
            var result = GetByIdFromCache(fieldId);
            if (result != null)
            {
                return result;
            }

            return MappersRepository.FieldMapper.GetBizObject(
                DefaultFieldQuery
                .SingleOrDefault(n => n.Id == fieldId)
            );
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

        /// <summary>
        /// Возвращает поле по имени и ID контента
        /// </summary>
        /// <param name="contentId">ID контента</param>
        /// <param name="fieldName">имя поля</param>
        /// <returns>информация о поле</returns>
        internal static Field GetByName(int contentId, string fieldName)
        {
            return MappersRepository.FieldMapper.GetBizObject(
                DefaultFieldQuery
                .SingleOrDefault(n => n.ContentId == contentId && n.Name == fieldName)
            );
        }

        /// <summary>
        /// Возвращает поле по значению поля Order и ID контента
        /// </summary>
        /// <param name="contentId">ID контента</param>
        /// <param name="order">значение поля Order</param>
        /// <returns>информация о поле</returns>
        internal static Field GetByOrder(int contentId, int order)
        {
            return MappersRepository.FieldMapper.GetBizObject(
                DefaultFieldQuery
                .SingleOrDefault(n => n.ContentId == contentId && n.Order == order)
            );
        }

        /// <summary>
        /// Возвращает список полей для использования в списке статей или форме статей
        /// </summary>
        /// <param name="contentId">идентификатор контента</param>
        /// <param name="forList">признак, отображения в списке (не в форме)</param>
        /// <returns>список полей</returns>
        internal static List<Field> GetList(int contentId, bool forList)
        {
            var result = GetListFromCache(contentId);
            if (result != null)
            {
                return result;
            }

            var lambda = (Expression<Func<FieldDAL, bool>>)(n => true);
            if (forList)
            {
                lambda = n => n.ViewInList;
            }

            return MappersRepository.FieldMapper.GetBizList(
                DefaultFieldQuery
                    .Where(n => n.ContentId == contentId)
                    .Where(lambda)
                    .OrderBy(n => n.Order)
                    .ToList()
            );
        }

        private static List<Field> GetListFromCache(int contentId)
        {
            List<Field> result = null;
            var cache = QPContext.GetFieldCache();
            var cache2 = QPContext.GetContentFieldCache();
            if (cache != null && cache2 != null && cache2.ContainsKey(contentId))
            {
                var fieldIds = cache2[contentId];
                var result1 = fieldIds.Select(n => (cache.ContainsKey(n) ? cache[n] : null));
                if (result1.All(n => n != null))
                {
                    result = result1.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// Возвращает список полей для отображения в виде таблицы
        /// </summary>
        /// <param name="cmd">настройки сортировки и пэйджинга</param>
        /// <param name="contentId">идентификатор контента</param>
        /// <returns>список полей</returns>
        internal static ListResult<FieldListItem> GetList(ListCommand cmd, int contentId)
        {
            using (var scope = new QPConnectionScope())
            {
                int totalRecords;
                var options = new FieldPageOptions
                {
                    ContentId = contentId,
                    SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? MappersRepository.FieldListItemRowMapper.TranslateSortExpression(cmd.SortExpression) : null,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize
                };

                var rows = Common.GetFieldsPage(scope.DbConnection, options, out totalRecords);
                return new ListResult<FieldListItem> { Data = MappersRepository.FieldListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        internal static ListResult<FieldListItem> GetListForSelect(ListCommand cmd, int contentId, int[] ids, FieldSelectMode mode)
        {
            using (var scope = new QPConnectionScope())
            {
                int totalRecords;
                var options = new FieldPageOptions
                {
                    ContentId = contentId,
                    SortExpression = !string.IsNullOrWhiteSpace(cmd.SortExpression) ? MappersRepository.FieldListItemRowMapper.TranslateSortExpression(cmd.SortExpression) : null,
                    StartRecord = cmd.StartRecord,
                    PageSize = cmd.PageSize,
                    SelectedIDs = ids,
                    Mode = mode
                };

                var rows = Common.GetFieldsPage(scope.DbConnection, options, out totalRecords);
                return new ListResult<FieldListItem> { Data = MappersRepository.FieldListItemRowMapper.GetBizList(rows.ToList()), TotalRecords = totalRecords };
            }
        }

        /// <summary>
        /// Возвращает список полей по спику id
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Field> GetList(IEnumerable<int> ids)
        {
            IEnumerable<decimal> decIDs = Converter.ToDecimalCollection(ids).Distinct().ToArray();
            return MappersRepository.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => decIDs.Contains(f.Id)).ToList());
        }

        internal static IEnumerable<ListItem> GetSimpleList(IEnumerable<int> ids)
        {
            return GetList(ids).Select(c => new ListItem(c.Id.ToString(), c.Name));
        }

        internal static IEnumerable<Field> GetAll()
        {
            return MappersRepository.FieldMapper.GetBizList(DefaultFieldQuery.OrderBy(n => n.Order).ThenBy(n => n.Id).ToList());
        }

        /// <summary>
        /// Возвращает все поля которые ссылаются на данное поле связью O2M (включая виртуальные)
        /// </summary>
        public static IEnumerable<Field> GetRelatedO2MFields(int fieldId)
        {
            return MappersRepository.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => f.RelationId == fieldId).ToList());
        }

        /// <summary>
        /// Возвращает обратное поле для связи O2M (из реального контента)
        /// </summary>
        public static Field GetByBackRelationId(int fieldId)
        {
            return MappersRepository.FieldMapper.GetBizObject(DefaultFieldQuery.Where(f => f.BackRelationId == fieldId && f.Content.VirtualType == VirtualType.None).SingleOrDefault());
        }

        internal static Field GetTitleField(int contentId)
        {
            var fieldName = ContentRepository.GetTitleName(contentId);
            var field = GetByName(contentId, fieldName);
            if (field == null || !field.ViewInList)
            {
                field = MappersRepository.FieldMapper.GetBizObject(
                    DefaultFieldQuery
                        .Where(n => n.ContentId == contentId && n.ViewInList)
                        .OrderBy(n => n.Order)
                        .FirstOrDefault()
                    );
            }

            return field;
        }

        internal static List<Field> GetFullList(int contentId)
        {
            return GetList(contentId, false);
        }

        internal static Field GetBaseField(int fieldId, int articleId)
        {
            return GetById(GetBaseFieldId(fieldId, articleId));
        }

        internal static int GetBaseFieldId(int fieldId, int articleId)
        {
            using (new QPConnectionScope())
            {
                return Common.GetBaseFieldId(QPConnectionScope.Current.DbConnection, fieldId, articleId);
            }
        }

        internal static DynamicImage GetDynamicImageInfoById(int fieldId)
        {
            var info = DefaultRepository.GetById<DynamicImageFieldDAL>(fieldId);
            return (info == null) ? null : MappersRepository.DynamicImageMapper.GetBizObject(info);
        }

        internal static List<Field> GetDynamicImageFields(int contentId, int imageFieldId)
        {
            return MappersRepository.FieldMapper.GetBizList(DefaultFieldQuery.Where(f => f.ContentId == contentId && f.BaseImageId == imageFieldId).ToList());
        }

        internal static IEnumerable<FieldType> GetAllFieldTypes()
        {
            return MappersRepository.FieldTypeMapper.GetBizList(
                QPContext.EFContext.FieldTypeSet.ToList()
            );
        }

        internal static int GetTextFieldMaxLength(Field dbField)
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

                if (dbField.IsBlob)
                {
                    return Common.GetBlobFieldMaxSymbolLength(scope.DbConnection, dbField.ContentId, dbField.Name);
                }

                return 0;
            }
        }

        internal static int? GetNumericFieldMaxValue(Field dbField)
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

        #endregion

        #region Save
        /// <summary>
        /// Добавляет новое поле
        /// </summary>
        internal static Field Save(Field item, bool explicitOrder = true)
        {
            try
            {
                if (explicitOrder)
                {
                    ChangeMaxOrderTriggerState(false);
                    item.ReorderContentFields();
                }

                var constraint = item.Constraint;
                var dynamicImage = item.DynamicImage;

                DefaultRepository.TurnIdentityInsertOn(EntityTypeCode.Field, item);
                var newItem = DefaultRepository.Save<Field, FieldDAL>(item);
                DefaultRepository.TurnIdentityInsertOff(EntityTypeCode.Field);

                if (explicitOrder)
                {
                    UpdateFieldOrder(newItem.Id, item.Order);
                }

                SaveConstraint(constraint, newItem);
                SaveDynamicImage(dynamicImage, newItem);

                return GetById(newItem.Id);
            }
            finally
            {
                if (explicitOrder)
                {
                    ChangeMaxOrderTriggerState(true);
                }
            }
        }

        private static void UpdateFieldOrder(int fieldId, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateFieldOrder(scope.DbConnection, fieldId, newOrder);
            }
        }

        public static void UpdateContentFieldsOrders(int contentId, int currentOrder, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.UpdateContentFieldsOrder(scope.DbConnection, currentOrder, newOrder, contentId);
            }
        }

        private static void SaveDynamicImage(DynamicImage dynamicImage, Field newItem)
        {
            if (dynamicImage != null)
            {
                dynamicImage.Id = newItem.Id;
                var dynamicImageDal = MappersRepository.DynamicImageMapper.GetDalObject(dynamicImage);
                dynamicImageDal = DefaultRepository.SimpleSave(dynamicImageDal);
                newItem.DynamicImage = MappersRepository.DynamicImageMapper.GetBizObject(dynamicImageDal);
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

        #endregion

        #region Update
        /// <summary>
        /// Обновляет информацию о поле
        /// </summary>
        internal static Field Update(Field item)
        {
            try
            {
                var preUpdateField = GetById(item.Id);

                ChangeMaxOrderTriggerState(false);
                ChangeM2MDefaultTriggerState(false);

                var constraint = item.Constraint;
                var dynamicImage = item.DynamicImage;

                item.ReorderContentFields();

                UpdateBackwardFields(item, preUpdateField);

                UpdateRelationData(item, preUpdateField);

                var newItem = DefaultRepository.Update<Field, FieldDAL>(item);

                UpdateFieldOrder(newItem.Id, item.Order);

                UpdateConstraint(constraint, newItem);

                UpdateDynamicImage(dynamicImage, newItem);

                return GetById(newItem.Id);
            }
            finally
            {
                ChangeMaxOrderTriggerState(true);
                ChangeM2MDefaultTriggerState(true);
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
                    Common.ChangeContentDataForRelation(scope.DbConnection, newItem.Id, newItem.BackRelationId.Value);
                }
            }

            // M2O -> M2M или новый связанный контент для M2M
            if (preUpdateField.ExactType == FieldExactTypes.M2ORelation && newItem.ExactType == FieldExactTypes.M2MRelation ||
                preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.M2MRelation && preUpdateField.LinkId != newItem.LinkId)
            {
                using (var scope = new QPConnectionScope())
                {
                    Common.ChangeContentDataForRelation(scope.DbConnection, newItem.Id, newItem.LinkId.Value);
                }
            }

            // O2M -> M2M
            if (preUpdateField.ExactType == FieldExactTypes.O2MRelation && newItem.ExactType == FieldExactTypes.M2MRelation)
            {
                using (var scope = new QPConnectionScope())
                {
                    Common.O2MToM2MTranferData(scope.DbConnection, newItem.Id, newItem.LinkId.Value);
                }

            }
            // M2M -> O2M
            else if (preUpdateField.ExactType == FieldExactTypes.M2MRelation && newItem.ExactType == FieldExactTypes.O2MRelation)
            {
                using (var scope = new QPConnectionScope())
                {
                    Common.M2MToO2MTranferData(scope.DbConnection, newItem.Id, preUpdateField.LinkId.Value);
                }
            }
        }

        private static void UpdateBackwardFields(Field newItem, Field preUpdateField)
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
                SetFieldM2MDefValue(f.Id, Enumerable.Empty<int>().ToArray());
                f.PersistWithVirtualRebuild();
            }
        }

        private static void UpdateDynamicImage(DynamicImage dymamicImage, Field newItem)
        {
            if (dymamicImage != null)
            {
                var dymamicImageDal = MappersRepository.DynamicImageMapper.GetDalObject(dymamicImage);
                if (dymamicImage.IsNew)
                {
                    dymamicImage.Id = newItem.Id;
                    dymamicImageDal = DefaultRepository.SimpleSave(dymamicImageDal);
                }
                else
                {
                    dymamicImageDal = DefaultRepository.SimpleUpdate(dymamicImageDal);
                }

                newItem.DynamicImage = MappersRepository.DynamicImageMapper.GetBizObject(dymamicImageDal);
            }
            else
            {
                var dymamicImageDal = DefaultRepository.GetById<DynamicImageFieldDAL>(newItem.Id);
                if (dymamicImageDal != null)
                {
                    DefaultRepository.SimpleDelete<DynamicImageFieldDAL>(dymamicImageDal.EntityKey);
                }
            }
        }

        private static void UpdateConstraint(ContentConstraint constraint, Field newItem)
        {
            if (constraint != null)
            {
                BindFieldToContstraint(newItem, constraint);
                if (constraint.IsNew)
                    ContentConstraintRepository.Save(constraint);
                else
                    ContentConstraintRepository.Update(constraint);
            }
        }

        /// <summary>
        /// Обновляет значения поля StringEnum в существующих статьях, в соответствии со составом перечисления
        /// </summary>
        /// <param name="field"></param>
        internal static void CorrectEnumInContentData(Field field)
        {
            if (!field.IsNew && field.ExactType == FieldExactTypes.StringEnum && field.StringEnumItems.Any())
            {
                var enumValues = field.StringEnumItems.Select(v => v.Value);
                using (var scope = new QPConnectionScope())
                {
                    Common.CorrectEnumInContentData(scope.DbConnection, field.Id, enumValues, field.StringDefaultValue);
                }
            }
        }
        #endregion

        #region Remove
        /// <summary>
        /// Удаляет поле по его идентификатору
        /// </summary>
        /// <param name="id">идентификатор поля</param>
        internal static void Delete(int id)
        {
            DefaultRepository.Delete<FieldDAL>(id);
        }

        /// <summary>
        /// Удалить версии линков M2M
        /// </summary>
        public static void RemoveLinkVersions(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.RemoveLinkVersions(scope.DbConnection, fieldId);
            }
        }
        #endregion

        #region Checkings
        internal static bool Exists(int id)
        {
            return QPContext.EFContext.FieldSet.Any(n => n.Id == id);
        }

        private static bool ContentNetNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.NetName == link.NetLinkName && n.SiteId == link.Content.SiteId);
        }

        private static bool LinkNetNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Any(n => n.NetLinkName == link.NetLinkName && n.LinkId != link.LinkId && n.Content.SiteId == link.Content.SiteId);
        }

        internal static bool NetNameExists(ContentLink link)
        {
            return ContentNetNameExists(link) || LinkNetNameExists(link);
        }

        internal static bool NetPluralNameExists(ContentLink link)
        {
            return ContentNetPluralNameExists(link) || LinkNetPluralNameExists(link);
        }

        private static bool ContentNetPluralNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentSet.Any(n => n.NetPluralName == link.NetPluralLinkName && n.SiteId == link.Content.SiteId);
        }

        private static bool LinkNetPluralNameExists(ContentLink link)
        {
            return QPContext.EFContext.ContentToContentSet.Include("Content").Any(n => n.NetPluralLinkName == link.NetPluralLinkName && n.LinkId != link.LinkId && n.Content.SiteId == link.Content.SiteId);
        }

        internal static bool LinqBackPropertyNameExists(Field field)
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

        internal static bool LinqPropertyNameExists(Field field)
        {
            if (field == null)
                throw new ArgumentException("field");

            // Существуют ли в контенте данного поля, c таким же именем прямого Linq-свойства
            var existFieldInContent = QPContext.EFContext.FieldSet
                .Any(f => f.NetName == field.LinqPropertyName && f.Id != field.Id && f.ContentId == field.ContentId);

            // Существуют ли поля ссылающиеся на контент данного поля и с именем обратного Linq-свойства равным имени прямого Linq-свойства данного поля
            var relateContentFields = field.Content.Fields.Select(f => (decimal)f.Id).ToArray();
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

        public static bool CheckNumericValuesAsKey(Field field, Field dbField)
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

        /// <summary>
        /// Существуют ли множественные связи по данному полю.
        /// </summary>
        public static bool DoPluralLinksExist(Field dbField)
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

        /// <summary>
        /// Определяет, существуют ли значения поля в БД, не входящие в перечисление
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static bool IsNonEnumFieldValueExist(Field field)
        {
            var enumValues = field.StringEnumItems.Select(v => v.Value);
            return QPContext.EFContext.ContentDataSet.Any(d => d.FieldId == field.Id && !enumValues.Contains(d.Data));
        }

        internal static bool HasAnyAggregators(int fieldId)
        {
            return QPContext.EFContext.FieldSet
                .Where(f => f.Id == fieldId)
                .Select(f => f.Aggregators.Any())
                .Single();
        }
        #endregion

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

        internal static void ChangeCreateFieldsTriggerState(bool enable)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "ti_create_fields", enable);
            }
        }


        internal static void ChangeMaxOrderTriggerState(bool enable)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "ti_set_max_order", enable);
            }
        }

        internal static void ChangeContentAttributeTriggerState(bool enable)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "ti_content_attribute_m2m_default_value", enable);
            }
        }

        internal static void ChangeInsertFieldTriggerState(bool enable)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "ti_insert_field", enable);
            }
        }

        internal static void ChangeM2MDefaultTriggerState(bool enable)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ChangeTriggerState(scope.DbConnection, "tu_content_attribute_m2m_default_value", enable);
            }
        }

        internal static VisualEditFieldParams GetVisualEditFieldParams(int fieldId)
        {
            using (var scope = new QPConnectionScope())
            {
                var dt = Common.GetVisualEditFieldParams(scope.DbConnection, fieldId);
                if (dt.Rows.Count == 0)
                {
                    throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, fieldId));
                }

                return Mapper.Map<IDataReader, IEnumerable<VisualEditFieldParams>>(dt.CreateDataReader()).Single();
            }
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
        internal static IEnumerable<ListItem> GetAggregatableContentListItemsForClassifier(Field classifier, string excludeValue = null)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return AggregatedContentListItems(classifier, excludeValue)
                    .Select(c => new ListItem(c.Id.ToString(), c.Name))
                    .ToArray();
            }

            return Enumerable.Empty<ListItem>();
        }

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        internal static int[] GetAggregatableContentIdsForClassifier(Field classifier)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return AggregatedContentListItems(classifier, null)
                    .Select(c => c.Id)
                    .ToArray();
            }

            return new int[] { };
        }

        private static ContentListItem[] AggregatedContentListItems(Field classifier, string excludeValue)
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
                    var result = CommonSecurity.CheckContentSecurity(scope.DbConnection, siteId, ids, QPContext.CurrentUserId,
                        PermissionLevel.Modify);
                    items = items.Where(n => result[n.Id] || n.Id == excludeId).ToArray();
                }
            }

            return items;
        }

        /// <summary>
        /// Возвращает список контентов для классификатора
        /// </summary>
        internal static IEnumerable<Content> GetAggregatableContentsForClassifier(Field classifier)
        {
            if (classifier != null && classifier.IsClassifier)
            {
                return MappersRepository.ContentMapper.GetBizList(QPContext.EFContext.FieldSet
                    .Where(f => f.Id == classifier.Id)
                    .SelectMany(f => f.Aggregators)
                    .Select(f => f.Content)
                    .ToList()
                    );
            }

            return Enumerable.Empty<Content>();
        }

        internal static IEnumerable<Field> GetChildList(int fieldId)
        {
            return MappersRepository.FieldMapper.GetBizList(
                DefaultFieldQuery
                    .Where(f => f.ParentFieldId == fieldId)
                    .ToList()
            );
        }

        internal static void ClearTreeOrder(int id)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.ClearFieldTreeOrder(scope.DbConnection, id);
            }
        }
        #endregion


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

            var newField = field.PersistWithVirtualRebuild(true);
            return newField;
        }

        internal static bool NameExists(Field field)
        {
            return QPContext.EFContext.FieldSet.Any(x => x.Name == field.Name && x.ContentId == field.ContentId);
        }

        internal static void MoveFieldOrders(int contentId, int newOrder)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.MoveFieldOrders(scope.DbConnection, contentId, newOrder);
            }
        }

        internal static IEnumerable<Article> GetActiveArticlesForM2MField(int fieldId)
        {
            List<int> activeIds;
            using (var scope = new QPConnectionScope())
            {
                activeIds = Common.GetActiveArticlesIdsForM2mField(scope.DbConnection, fieldId).ToList();
            }

            var entities = QPContext.EFContext;
            return MappersRepository.ArticleMapper.GetBizList(entities.ArticleSet.Include("Content").Where(x => activeIds.Contains((int)x.Id)).ToList());
        }

        internal static bool FieldHasArticles(int contentId)
        {
            var entities = QPContext.EFContext;
            return entities.ArticleSet.Any(x => x.ContentId == contentId);
        }

        internal static void SetFieldM2MDefValue(int fieldId, int[] defaultArticles)
        {
            using (var scope = new QPConnectionScope())
            {
                Common.SetFieldM2MDefValue(scope.DbConnection, defaultArticles, fieldId);
            }
        }

        internal static IEnumerable<DataRow> GetRelationsBetweenAttributes(int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
        {
            using (var scope = new QPConnectionScope())
            {
                return Common.GetRelationsBetweenAttributes(scope.DbConnection, sourceSiteId, destinationSiteId, contentIds, forVirtualContents, byNewContents);
            }
        }

        internal static string GetRelationsBetweenAttributesXML(int sourceSiteId, int destinationSiteId, string contentIds, bool? forVirtualContents, bool byNewContents)
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
    }
}
