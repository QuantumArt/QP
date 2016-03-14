using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services
{
	public class EntityTypeService
	{
		/// <summary>
		/// Возвращает тип сущности по ее идентификатору
		/// </summary>
		/// <param name="entityTypeId">идентификатор типа сущности</param>
		/// <returns>информация о типе сущности</returns>
		public static EntityType GetById(int entityTypeId)
		{
			return EntityTypeRepository.GetById(entityTypeId);
		}

		/// <summary>
		/// Возвращает тип сущности по ее коду
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <returns>информация о типе сущности</returns>
		public static EntityType GetByCode(string entityTypeCode)
		{
			if (String.IsNullOrWhiteSpace(entityTypeCode))
				return null;
			else
				return EntityTypeRepository.GetByCode(entityTypeCode);
		}

		/// <summary>
		/// Возвращает код типа сущности по его Id
		/// </summary>
		/// <param name="?"></param>
		/// <returns></returns>
		public static string GetCodeById(int entityTypeId)
		{
			EntityType rt = GetById(entityTypeId);
			return rt != null ? rt.Code : null;
		}

		/// <summary>
		/// Возвращает код типа родительской сущности
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <returns>код типа родительской сущности</returns>
		public static string GetParentCodeByCode(string entityTypeCode)
		{
			if (String.IsNullOrWhiteSpace(entityTypeCode))
				return null;
			else
				return EntityTypeRepository.GetParentCodeByCode(entityTypeCode);
		}

		/// <summary>
		/// Возвращает код действия по умолчанию для указанного типа сущности
		/// </summary>
		/// <param name="entityTypeCode">код типа сущности</param>
		/// <returns>код действия по умолчанию</returns>
		public static string GetDefaultActionCodeByEntityTypeCode(string entityTypeCode)
		{
			// TODO: Реализовать кэширование, когда будет выработана общая стратегия кэширования
			if (String.IsNullOrWhiteSpace(entityTypeCode))
				return null;
			else
				return EntityTypeRepository.GetDefaultActionCodeByEntityTypeCode(entityTypeCode);
		}
	}
}
