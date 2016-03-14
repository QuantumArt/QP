using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;
using System.Data;

namespace Quantumart.QP8.BLL
{
	/// <summary>
	/// Контекст текущего BackendAction
	/// </summary>
	public class BackendActionContext
	{
		public class Entity
		{						
			public int? Id { get; set; }
			public string StringId { get; set; }			
		}

		public static BackendActionContext Current
		{
			get { return current; }
			private set { current = value; }
		}


		#region Creating Thread Singelton
		[ThreadStatic]
		private static BackendActionContext current = null;

		public static void SetCurrent(string actionCode, IEnumerable<string> stringEntiryIDs, int? parentEntityId)
		{
			if (Current == null)
				Current = new BackendActionContext(actionCode, stringEntiryIDs, parentEntityId);
			else
			{
				if (!Current.ActionCode.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase))
					throw new ArgumentException("Attempt to create BackendAction Context with different Action Code.");
			}			
		}

		public static void ResetCurrent()
		{
			Current = null;
		}

		private BackendActionContext(string actionCode, IEnumerable<string> stringEntiryIDs, int? parentEntityId)
		{
			IsChanged = false;
			if (String.IsNullOrWhiteSpace(actionCode))
				throw new ArgumentException("Action Code is empty", "actionCode");

			BackendActionCacheRecord cacheRow = BackendActionRepository.GetActionContextCacheData()
					.SingleOrDefault(a => a.ActionCode.Equals(actionCode, StringComparison.InvariantCultureIgnoreCase));
			if (cacheRow == null)
				throw new ApplicationException("Backend action was not found by code: " + actionCode);

			ActionCode = actionCode;
			ActionTypeCode = cacheRow.ActionTypeCode;
			EntityTypeCode = cacheRow.EntityTypeCode;
			ParentEntityId = parentEntityId;

			Entities = stringEntiryIDs
						.Select(sid => new Entity
						{
							StringId = sid,
							Id = Converter.ToNullableInt32(sid),		
						})
						.ToArray();
		}		
		#endregion		

		#region Properties
		/// <summary>
		/// Код типа действия
		/// </summary>
		public string ActionTypeCode
		{
			get;
			private set;
		}

		/// <summary>
		/// Код действия
		/// </summary>
		public string ActionCode
		{
			get;
			private set;
		}

		/// <summary>
		/// Код типа сущности
		/// </summary>
		public string EntityTypeCode
		{
			get;
			private set;
		}
		
		public Entity[] Entities 
		{ 
			get; 
			private set; 
		}

		public int FromEntityId
		{
			get;
			set;
		}

		public void ResetEntityId(int id)
		{
			IsChanged = true;
			Entities = new[] 
			{
				new Entity
				{
					Id = id,
					StringId = id.ToString()
				}
			};
		}

		public bool IsChanged { get; private set; }

		/// <summary>
		/// Идентификатор сущности
		/// </summary>
		public int? ParentEntityId
		{
			get;
			private set;
		}

		
		#endregion		
	}
}
