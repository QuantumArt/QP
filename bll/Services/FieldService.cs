using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Helpers;


namespace Quantumart.QP8.BLL.Services
{
	public class FieldService
	{

		public static Field New(int contentId, int? fieldId)
		{
			Content content = ContentRepository.GetById(contentId);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, contentId)); 
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
			Field field = FieldRepository.GetById(id);
			if (field == null)
				throw new Exception(String.Format(FieldStrings.FieldNotFound, id));
			if (!field.IsUpdatable)
				field.IsReadOnly = true;
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
				throw new ArgumentNullException("item");
			if (item.ExactType == FieldExactTypes.O2MRelation && item.RelateToContentId == item.ContentId && !item.Content.HasTreeField && !item.UseForVariations)
			{
				item.UseForTree = true;
			}
			return SaveOrUpdate(item);
		}

		public static Field Update(Field item)
		{
			if (item == null)
				throw new ArgumentNullException("item");
			if (!FieldRepository.Exists(item.Id))
				throw new Exception(String.Format(FieldStrings.FieldNotFound, item.Id));
			return SaveOrUpdate(item);
		}

        private static Field SaveOrUpdate(Field item)
        {
			return item.PersistWithBackward(true);
        }
		
		public static MessageResult Remove(int id)
		{
			Field item = FieldRepository.GetById(id);
			if (item == null)
				throw new ApplicationException(String.Format(FieldStrings.FieldNotFound, id));

            var violationMessages = item.Die();

			if (violationMessages.Any())
				return MessageResult.Error(String.Join(Environment.NewLine, violationMessages), new[] { id });
			else
				return null;
			 
		}

		public static MessageResult MultipleRemove(int[] IDs)
		{
			CheckIdResult<Field> result = CheckIdResult<Field>.Create(IDs, ActionTypeCode.Remove);
			List<MessageResult> removeMsgResults = new List<MessageResult>();
			foreach (var id in result.ValidIds)
			{
				var violationMessages = Field.Die(id);
				if (violationMessages.Any())
				{
					removeMsgResults.Add(MessageResult.Error(String.Join(Environment.NewLine, violationMessages), new[] { id }));
				}
			}			
			if(removeMsgResults.Count > 0)
			{
				var checkResult = result.GetServiceResult();
				List<int> ids = new List<int>(checkResult != null ? checkResult.FailedIds : new int[0]);
				foreach (var msgr in removeMsgResults)
				{
					ids.AddRange(msgr.FailedIds);
				}
				string msg = String.Concat(checkResult != null ? checkResult.Text : String.Empty, Environment.NewLine, String.Join("", removeMsgResults.Select(r => r.Text)));

				return MessageResult.Error(msg, ids.Distinct().ToArray());
			}
			else
				return result.GetServiceResult();
		}

		/// <summary>
		/// Инициализация списка полей
		/// </summary>
		public static FieldInitListResult InitList(int contentId)
		{
			Content content = ContentRepository.GetById(contentId);
			if (content == null)
				throw new Exception(String.Format(ContentStrings.ContentNotFound, contentId));
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
            Field field = FieldRepository.GetById(id);
			if (field == null)
				throw new Exception(String.Format(FieldStrings.FieldNotFound, id));
            if (articleId != null)
                field = field.GetBaseField((int)articleId);
            return field.PathInfo;       
        }


		private static Dictionary<FieldExactTypes, IEnumerable<FieldExactTypes>> fieldTypeConversionRules =
			#region fieldTypeConversionRules initialization
			new Dictionary<FieldExactTypes, IEnumerable<FieldExactTypes>>
			{
				{
					FieldExactTypes.String, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.File,
						FieldExactTypes.Image,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
						FieldExactTypes.StringEnum,
					}
				},

				{
					FieldExactTypes.Numeric, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Numeric,
						FieldExactTypes.Boolean,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
						FieldExactTypes.O2MRelation,
						FieldExactTypes.Classifier
					}
				},

				{
					FieldExactTypes.Classifier, new FieldExactTypes[]
					{
						FieldExactTypes.Numeric,
						FieldExactTypes.Classifier
					}
				},

				{
					FieldExactTypes.Boolean, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Numeric,
						FieldExactTypes.Boolean,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.Date, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Date,
						FieldExactTypes.Time,
						FieldExactTypes.DateTime,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.Time, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Date,
						FieldExactTypes.Time,
						FieldExactTypes.DateTime,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.DateTime, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Date,
						FieldExactTypes.Time,
						FieldExactTypes.DateTime,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.File, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.File,
						FieldExactTypes.Image,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.Image, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.File,
						FieldExactTypes.Image,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.Textbox, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.VisualEdit, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
					}
				},

				{
					FieldExactTypes.O2MRelation, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Numeric,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
						FieldExactTypes.O2MRelation,
						FieldExactTypes.M2MRelation,
					}
				},

                {
				    FieldExactTypes.M2ORelation, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Numeric,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
                        FieldExactTypes.M2ORelation,
					}
				},

				{
					FieldExactTypes.M2MRelation, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.Numeric,
						FieldExactTypes.Textbox,
						FieldExactTypes.VisualEdit,
						FieldExactTypes.O2MRelation,
						FieldExactTypes.M2MRelation,
					}
				}, 

				{
					FieldExactTypes.DynamicImage, new FieldExactTypes[]
					{
						FieldExactTypes.String,
						FieldExactTypes.DynamicImage,
					}
				},

				{
					FieldExactTypes.StringEnum, new FieldExactTypes[]
					{
						FieldExactTypes.StringEnum,
						FieldExactTypes.String						
					}
				}
			};
			#endregion
		/// <summary>
		/// Получить подмножество типов поля конвертация в которые разрешена из текущего типа поля
		/// </summary>
		/// <param name="fieldType"></param>
		/// <returns></returns>
		public static IEnumerable<FieldExactTypes> GetAcceptableExactFieldTypes(FieldExactTypes fieldType)
		{
			if(fieldType == FieldExactTypes.Undefined)
				return Enum.GetValues(typeof(FieldExactTypes)).Cast<int>().Select(v => (FieldExactTypes)v);
			else
				return fieldTypeConversionRules[fieldType];
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
			if (classifier.IsNew)
				return Enumerable.Empty<ListItem>();
			else
				return FieldRepository.GetAggregatableContentListItemsForClassifier(classifier);
		}

		public static IEnumerable<ListItem> GetFieldsForTreeOrder(int contentId, int id)
		{
			var result = ContentService.GetRelateableFields(contentId, id).ToList();
			result.Insert(0, new ListItem { 
				Text = FieldStrings.SelectField, 
				Value = string.Empty 
			});
			return result;
		}

		public static FieldCopyResult Copy(int id, int? forceId, int? forceLinkId, int[] forceVirtualFieldIds, int[] forceChildFieldIds, int[] forceChildLinkIds)
		{
			FieldCopyResult result = new FieldCopyResult();
			Field field = FieldRepository.GetById(id);
			if (field == null)
				throw new Exception(String.Format(FieldStrings.FieldNotFound, id));

			if (!field.Content.Site.IsUpdatable || !field.IsAccessible(ActionTypeCode.Read))
				result.Message = MessageResult.Error(ContentStrings.CannotCopyBecauseOfSecurity);

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
					field.ForceId = forceId.Value;
				field.ForceVirtualFieldIds = forceVirtualFieldIds;

				if (field.ContentLink != null && forceLinkId.HasValue)
					field.ContentLink.ForceLinkId = forceLinkId.Value;
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

        public static Field GetById(int fieldId)
        {
            return FieldRepository.GetById(fieldId);
        }
    }
}
