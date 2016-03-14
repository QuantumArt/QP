using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Quantumart.QP8;
using Quantumart.QP8.DAL;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Mappers;

namespace Quantumart.QP8.BLL
{
	public class BackendActionType : BizObject
	{
		/// <summary>
		/// идентификатор типа действия
		/// </summary>
		public int Id {get; set;}
		
		/// <summary>
		/// название типа действия
		/// </summary>
		public string Name {get; set;}		

		public string NotTranslatedName { get; set; }

		/// <summary>
		/// код типа действия
		/// </summary>

		public string Code { get; set; }
		

		/// <summary>
		/// идентификатор требуемого уровня доступа
		/// </summary>
		public int RequiredPermissionLevelId {get; set;}		

		/// <summary>
		/// Требуемый уровня доступа
		/// </summary>
		public int RequiredPermissionLevel {get; set;}		

		/// <summary>
		/// количество сущностей, к которым применяется действие
		/// </summary>
		public byte ItemsAffected {get; set;}		
		/// <summary>
		/// Признак множественного действия
		/// </summary>
		public bool IsMultiple
		{
			get 
			{
				return (this.ItemsAffected > 1);
			}
		}
	}
}
