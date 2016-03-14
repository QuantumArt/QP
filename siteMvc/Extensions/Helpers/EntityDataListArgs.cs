using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	/// <summary>
	/// Свойства списка сущностей
	/// </summary>
	public class EntityDataListArgs
	{
		public EntityDataListArgs()
		{
			EntityTypeCode = String.Empty;
			ParentEntityId = null;
			EntityId = 0;
			ListId = 0;
			AddNewActionCode = ActionCode.None;
			ReadActionCode = ActionCode.None;
			SelectActionCode = ActionCode.None;
			MaxListHeight = 0;
			MaxListWidth = 0;
			ShowIds = false;
			Filter = String.Empty;
			IsCollapsable = false;
			Collapsed = false;
			EnableCopy = true;
			ReadDataOnInsert = false;
		}

		/// <summary>
		/// код типа сущности
		/// </summary>
		public string EntityTypeCode { get; set; }

		/// <summary>
		/// идентификатор родительской сущности
		/// </summary>
		public int? ParentEntityId { get; set; }

		/// <summary>
		/// идентификатор сущности
		/// </summary>
		public int? EntityId { get; set; }

		/// <summary>
		/// дополнительный параметр для идентификации списка
		/// </summary>
		public int ListId { get; set; }

		/// <summary>
		/// код действия, которое открывает форму добавления сущности
		/// </summary>
		public string AddNewActionCode { get; set; }

		/// <summary>
		/// код действия, которое открывает форму редактирования сущности
		/// </summary>
		public string ReadActionCode { get; set; }

		/// <summary>
		/// код действия, которое открывает окно выбора сущностей
		/// </summary>
		public string SelectActionCode { get; set; }

		/// <summary>
		/// максимальная ширина списка
		/// </summary>
		public int MaxListWidth { get; set; }

		/// <summary>
		/// максимальная высота списка
		/// </summary>
		public int MaxListHeight { get; set; }

		/// <summary>
		/// показывать ли ID в списке сущностей
		/// </summary>
		public bool ShowIds { get; set; }

		/// <summary>
		/// фильтр сущностей
		/// </summary>		
		public string Filter { get; set; }

		/// <summary>
		/// Является ли список сворачиваемым
		/// </summary>
		public bool IsCollapsable { get; set; }

		/// <summary>
		/// Свернут ли по - умолчанию
		/// </summary>
		public bool Collapsed { get; set; }

		/// <summary>
		/// Разрешено ли копирование
		/// </summary>
		public bool EnableCopy { get; set; }

		/// <summary>
		/// Нужно ли перечитывать данные
		/// </summary>
		public bool ReadDataOnInsert { get; set; }
	
	}
}