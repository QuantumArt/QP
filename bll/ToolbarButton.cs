using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL
{
	public class ToolbarButton
	{
		/// <summary>
		/// идентификатор действия
		/// </summary>
		[ScriptIgnore]
		public int ActionId
		{
			get;
			set;
		}

		/// <summary>
		/// код действия
		/// </summary>
		public string ActionCode
		{
			get;
			set;
		}

		/// <summary>
		/// код типа действия
		/// </summary>
		public string ActionTypeCode
		{
			get;
			set;
		}

		/// <summary>
		/// идентификатор родительского действия
		/// </summary>
		[ScriptIgnore]
		public int ParentActionId
		{
			get;
			set;
		}

		/// <summary>
		/// код родительского действия
		/// </summary>
		public string ParentActionCode
		{
			get;
			set;
		}

		/// <summary>
		/// текст кнопки
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// количество сущностей, к которым применяется действие
		/// </summary>
		public byte ItemsAffected
		{
			get;
			set;
		}

		/// <summary>
		/// порядковый номер
		/// </summary>
		[ScriptIgnore]
		public int Order
		{
			get;
			set;
		}

		/// <summary>
		/// название файла пиктограммы кнопки
		/// </summary>
		public string Icon
		{
			get;
			set;
		}

		/// <summary>
		/// название файла пиктограммы неактивной кнопки
		/// </summary>
		public string IconDisabled
		{
			get;
			set;
		}
		
		public bool IsCommand
		{
			get;
			set;
		}
	}
}
