using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System.Data.Objects;
using Quantumart.QP8.BLL.Facades;

namespace Quantumart.QP8.BLL.Repository
{
	internal class BackendActionTypeRepository
	{
		private static ObjectQuery<ActionTypeDAL> DefaultActionTypeQuery
		{
			get
			{
				return QPContext.EFContext.ActionTypeSet
					.Include("PermissionLevel");
			}
		}

		private static Lazy<IEnumerable<BackendActionType>> actionTypesCache = new Lazy<IEnumerable<BackendActionType>>(() => LoadActionTypes());

		private static IEnumerable<BackendActionType> LoadActionTypes()
		{
			var mapper = MapperFacade.BackendActionTypeMapper;
			mapper.DisableTranslations = true;
			var result = mapper.GetBizList(DefaultActionTypeQuery.ToList());
			mapper.DisableTranslations = false;
			return result;
		}

		internal static IEnumerable<BackendActionType> GetList()
		{
			return actionTypesCache.Value;
		}

		/// <summary>
		/// Возвращает тип действия по его идентификатору
		/// </summary>
		/// <param name="actionTypeId">идентификатор типа действия</param>
		/// <returns>тип действия</returns>
		internal static BackendActionType GetById(int id)
		{
			BackendActionType actionType = MapperFacade.BackendActionTypeMapper.GetBizObject(DefaultActionTypeQuery.Single(r => r.Id == id));
			return actionType;
		}		

		/// <summary>
		/// Возвращает тип действия по его коду
		/// </summary>
		/// <param name="code">код типа действия</param>
		/// <returns>тип действия</returns>
		internal static BackendActionType GetByCode(string code)
		{
			BackendActionType actionType = MapperFacade.BackendActionTypeMapper.GetBizObject(DefaultActionTypeQuery.Single(r => r.Code == code));
			return actionType;
		}

		/// <summary>
		/// Возвращает код типа действия по коду действия
		/// </summary>
		/// <param name="actionCode">код действия</param>
		/// <returns>код типа действия</returns>
		internal static string GetCodeByActionCode(string actionCode)
		{
			return QPContext.EFContext.BackendActionSet.Include("ActionType")
				.Single(a => a.Code == actionCode)
				.ActionType.Code;
				//GetActionTypeCodeByActionCode(actionCode);
		}
	}
}
