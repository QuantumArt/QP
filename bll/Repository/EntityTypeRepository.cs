using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository
{
    internal class EntityTypeRepository
    {
        private static IQueryable<EntityTypeDAL> DefaultEntityTypeQuery => QPContext
            .EFContext
            .EntityTypeSet
            .Include(x => x.Parent)
            .Include(x => x.CancelAction)
            .Include(x => x.ContextMenu);

        /// <summary>
        /// Возвращает тип сущности по ее идентификатору
        /// </summary>
        /// <param name="entityTypeId">идентификатор типа сущности</param>
        /// <returns>информация о типе сущности</returns>
        internal static EntityType GetById(int entityTypeId)
        {
            return MapperFacade.EntityTypeMapper.GetBizObject(DefaultEntityTypeQuery.Single(et => et.Id == entityTypeId));
        }

        /// <summary>
        /// Возвращает тип сущности по ее коду
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>информация о типе сущности</returns>
        internal static EntityType GetByCode(string entityTypeCode)
        {
            var entityType = MapperFacade.EntityTypeMapper.GetBizObject(DefaultEntityTypeQuery.Single(et => et.Code == entityTypeCode));
            return entityType;
        }

        internal static List<EntityTypeDAL> GetDbList()
        {
            return EntityTypeCache.GetEntityTypes(
                QPContext.EFContext, QPContext.CurrentCustomerCode, QPContext.CurrentUserId
            );
        }

        internal static List<EntityType> GetList()
        {
            return MapperFacade.EntityTypeMapper.GetBizList(GetDbList());
        }

        internal static IEnumerable<EntityType> GetListByCodes(IEnumerable<string> entityCodes)
        {
            return GetList().Where(e => entityCodes.Contains(e.Code, StringComparer.InvariantCultureIgnoreCase)).ToArray();
        }

        /// <summary>
        /// Возвращает код типа родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>код типа родительской сущности</returns>
        internal static string GetParentCodeByCode(string entityTypeCode)
        {
            return GetDbList().SingleOrDefault(et => et.Code == entityTypeCode)?.Parent?.Code;
        }


        /// <summary>
        /// Возвращает код действия по умолчанию для указанного типа сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>код действия по умолчанию</returns>
        internal static string GetDefaultActionCodeByEntityTypeCode(string entityTypeCode)
        {
            return GetDbList().SingleOrDefault(et => et.Code == entityTypeCode)?.DefaultAction?.Code;
        }
    }
}
