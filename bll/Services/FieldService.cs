using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Quantumart.QP8.BLL.Services
{
    public class FieldService
    {
        public static Field New(int contentId, int? fieldId)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            var field = new Field(content).Init();
            if (fieldId.HasValue)
            {
                field.Order = FieldRepository.GetById(fieldId.Value).Order;
            }

            return field;
        }

        public static Field NewForSave(int contentId)
        {
            return New(contentId, null);
        }

        public static Field VirtualRead(int id)
        {
            return Read(id);
        }

        public static Field Read(int id)
        {
            var field = FieldRepository.GetById(id);
            if (field == null)
            {
                throw new Exception(string.Format(FieldStrings.FieldNotFound, id));
            }

            if (!field.IsUpdatable)
            {
                field.IsReadOnly = true;
            }

            return field;
        }

        public static Field ReadForUpdate(int id)
        {
            return Read(id);
        }

        public static Field ReadForVisualEditor(int id)
        {
            return Read(id);
        }

        public static Field Save(Field item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ExactType == FieldExactTypes.O2MRelation && item.RelateToContentId == item.ContentId && !item.Content.HasTreeField && !item.UseForVariations)
            {
                item.UseForTree = true;
            }

            return SaveOrUpdate(item);
        }

        public static Field Update(Field item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (!FieldRepository.Exists(item.Id))
            {
                throw new Exception(string.Format(FieldStrings.FieldNotFound, item.Id));
            }

            return SaveOrUpdate(item);
        }

        private static Field SaveOrUpdate(Field item)
        {
            return item.PersistWithBackward(true);
        }

        public static MessageResult Remove(int id)
        {
            var item = FieldRepository.GetById(id);
            if (item == null)
            {
                throw new ApplicationException(string.Format(FieldStrings.FieldNotFound, id));
            }

            var violationMessages = item.Die().ToList();
            if (violationMessages.Any())
            {
                return MessageResult.Error(string.Join(Environment.NewLine, violationMessages), new[] { id });
            }

            return null;
        }

        public static MessageResult MultipleRemove(int[] ds)
        {
            var result = CheckIdResult<Field>.Create(ds, ActionTypeCode.Remove);
            var removeMsgResults = new List<MessageResult>();
            foreach (var id in result.ValidIds)
            {
                var violationMessages = Field.Die(id).ToList();
                if (violationMessages.Any())
                {
                    removeMsgResults.Add(MessageResult.Error(string.Join(Environment.NewLine, violationMessages), new[] { id }));
                }
            }

            if (removeMsgResults.Count > 0)
            {
                var checkResult = result.GetServiceResult();
                var ids = new List<int>(checkResult != null ? checkResult.FailedIds : new int[0]);
                foreach (var msgr in removeMsgResults)
                {
                    ids.AddRange(msgr.FailedIds);
                }

                var msg = string.Concat(checkResult != null ? checkResult.Text : string.Empty, Environment.NewLine, string.Join("", removeMsgResults.Select(r => r.Text)));
                return MessageResult.Error(msg, ids.Distinct().ToArray());
            }

            return result.GetServiceResult();
        }

        /// <summary>
        /// Инициализация списка полей
        /// </summary>
        public static FieldInitListResult InitList(int contentId)
        {
            var content = ContentRepository.GetById(contentId);
            if (content == null)
            {
                throw new Exception(string.Format(ContentStrings.ContentNotFound, contentId));
            }

            return new FieldInitListResult
            {
                ParentName = content.Name,
                IsAddNewAccessable = SecurityRepository.IsActionAccessible(ActionCode.AddNewField) && SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, ActionTypeCode.Update)
            };
        }


        /// <summary>
        /// Загрузка данных в список полей
        /// </summary>
        public static ListResult<FieldListItem> List(int contentId, ListCommand cmd)
        {
            return FieldRepository.GetList(cmd, contentId);
        }

        public static ListResult<FieldListItem> ListForExport(ListCommand cmd, int contentId, int[] ids)
        {
            return FieldRepository.GetListForSelect(cmd, contentId, ids, FieldSelectMode.ForExport);
        }

        public static ListResult<FieldListItem> ListForExportExpanded(ListCommand cmd, int contentId, int[] ids)
        {
            return FieldRepository.GetListForSelect(cmd, contentId, ids, FieldSelectMode.ForExportExpanded);
        }

        public static IEnumerable<Field> GetList(int[] ids)
        {
            return FieldRepository.GetList(ids);
        }


        /// <summary>
        /// Получение информации о библиотеке поля для просмотра/скачки файла
        /// </summary>
        /// <param name="id">ID поля</param>
        /// <param name="articleId">ID статьи</param>
        /// <returns>информация о библиотеке</returns>
        public static PathInfo GetPathInfo(int id, int? articleId = null)
        {
            var field = FieldRepository.GetById(id);
            if (field == null)
            {
                throw new Exception(string.Format(FieldStrings.FieldNotFound, id));
            }

            if (articleId != null)
            {
                field = field.GetBaseField((int)articleId);
            }

            return field.PathInfo;
        }

        private static readonly Dictionary<FieldExactTypes, IEnumerable<FieldExactTypes>> FieldTypeConversionRules = new Dictionary<FieldExactTypes, IEnumerable<FieldExactTypes>>
        {
            { FieldExactTypes.String, new[] { FieldExactTypes.String, FieldExactTypes.File, FieldExactTypes.Image, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit, FieldExactTypes.StringEnum } },
            { FieldExactTypes.Numeric, new[] { FieldExactTypes.String, FieldExactTypes.Numeric, FieldExactTypes.Boolean, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit, FieldExactTypes.O2MRelation, FieldExactTypes.Classifier } },
            { FieldExactTypes.Classifier, new[] { FieldExactTypes.Numeric, FieldExactTypes.Classifier } },
            { FieldExactTypes.Boolean, new[] { FieldExactTypes.String, FieldExactTypes.Numeric, FieldExactTypes.Boolean, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.Date, new[] { FieldExactTypes.String, FieldExactTypes.Date, FieldExactTypes.Time, FieldExactTypes.DateTime, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.Time, new[] { FieldExactTypes.String, FieldExactTypes.Date, FieldExactTypes.Time, FieldExactTypes.DateTime, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.DateTime, new[] { FieldExactTypes.String, FieldExactTypes.Date, FieldExactTypes.Time, FieldExactTypes.DateTime, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.File, new[] { FieldExactTypes.String, FieldExactTypes.File, FieldExactTypes.Image, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.Image, new[] { FieldExactTypes.String, FieldExactTypes.File, FieldExactTypes.Image, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.Textbox, new[] { FieldExactTypes.String, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.VisualEdit, new[] { FieldExactTypes.String, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit } },
            { FieldExactTypes.O2MRelation, new[] { FieldExactTypes.String, FieldExactTypes.Numeric, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit, FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation } },
            { FieldExactTypes.M2ORelation, new[] { FieldExactTypes.String, FieldExactTypes.Numeric, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit, FieldExactTypes.M2ORelation } },
            { FieldExactTypes.M2MRelation, new[] { FieldExactTypes.String, FieldExactTypes.Numeric, FieldExactTypes.Textbox, FieldExactTypes.VisualEdit, FieldExactTypes.O2MRelation, FieldExactTypes.M2MRelation } },
            { FieldExactTypes.DynamicImage, new[] { FieldExactTypes.String, FieldExactTypes.DynamicImage } },
            { FieldExactTypes.StringEnum, new[] { FieldExactTypes.StringEnum, FieldExactTypes.String } }
        };

        /// <summary>
        /// Получить подмножество типов поля, конвертация в которые, разрешена из текущего типа поля
        /// </summary>
        public static IEnumerable<FieldExactTypes> GetAcceptableExactFieldTypes(FieldExactTypes fieldType)
        {
            return fieldType == FieldExactTypes.Undefined
                ? Enum.GetValues(typeof(FieldExactTypes)).Cast<FieldExactTypes>()
                : FieldTypeConversionRules[fieldType];
        }

        /// <summary>
        /// Получить список типов полей
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<FieldType> GetAllFieldTypes()
        {
            return FieldRepository.GetAllFieldTypes();
        }

        public static IEnumerable<ListItem> GetBaseFieldsForM2O(int contentId, int fieldId)
        {
            return FieldRepository.GetBaseFieldsForM2O(contentId, fieldId);
        }

        public static IEnumerable<VisualEditorCommand> GetDefaultVisualEditorCommands()
        {
            return VisualEditorRepository.GetDefaultCommands();
        }

        public static IEnumerable<VisualEditorCommand> GetResultVisualEditorCommands(int fieldId, int siteId)
        {
            return VisualEditorRepository.GetResultCommands(fieldId, siteId);
        }

        public static IEnumerable<VisualEditorStyle> GetResultStyles(int fieldId, int siteId)
        {
            return VisualEditorRepository.GetResultStyles(fieldId, siteId);
        }

        public static IEnumerable<ListItem> GetAggregetableContentsForClassifier(Field classifier)
        {
            return classifier.IsNew
                ? Enumerable.Empty<ListItem>()
                : FieldRepository.GetAggregatableContentListItemsForClassifier(classifier);
        }

        public static IEnumerable<ListItem> GetFieldsForTreeOrder(int contentId, int id)
        {
            var result = ContentService.GetRelateableFields(contentId, id).ToList();
            result.Insert(0, new ListItem
            {
                Text = FieldStrings.SelectField,
                Value = string.Empty
            });

            return result;
        }

        public static FieldCopyResult Copy(int id, int? forceId, int? forceLinkId, int[] forceVirtualFieldIds, int[] forceChildFieldIds, int[] forceChildLinkIds)
        {
            var result = new FieldCopyResult();
            var field = FieldRepository.GetById(id);
            if (field == null)
            {
                throw new Exception(string.Format(FieldStrings.FieldNotFound, id));
            }

            if (!field.Content.Site.IsUpdatable || !field.IsAccessible(ActionTypeCode.Read))
            {
                result.Message = MessageResult.Error(ContentStrings.CannotCopyBecauseOfSecurity);
            }
            else if (field.ExactType == FieldExactTypes.M2ORelation)
            {
                result.Message = MessageResult.Error(FieldStrings.UnableToCopyM2O);
            }
            else if (field.ExactType == FieldExactTypes.Classifier)
            {
                result.Message = MessageResult.Error(FieldStrings.UnableToCopyClassifier);
            }
            else if (field.ExactType == FieldExactTypes.O2MRelation && field.Aggregated)
            {
                result.Message = MessageResult.Error(FieldStrings.UnableToCopyAggregator);
            }
            else
            {
                if (forceId.HasValue)
                {
                    field.ForceId = forceId.Value;
                }

                field.ForceVirtualFieldIds = forceVirtualFieldIds;
                if (field.ContentLink != null && forceLinkId.HasValue)
                {
                    field.ContentLink.ForceLinkId = forceLinkId.Value;
                }

                field.ForceChildFieldIds = forceChildFieldIds;
                field.ForceChildLinkIds = forceChildLinkIds;
                field.LoadVeBindings();

                var resultField = FieldRepository.Copy(field);
                result.Id = resultField.Id;
                result.LinkId = resultField.LinkId;
                result.VirtualFieldIds = resultField.NewVirtualFieldIds;
                result.ChildFieldIds = resultField.ResultChildFieldIds;
                result.ChildLinkIds = resultField.ResultChildLinkIds;
            }

            return result;
        }
    }
}
