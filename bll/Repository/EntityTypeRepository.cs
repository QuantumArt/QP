using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Linq;

namespace Quantumart.QP8.BLL.Repository
{
    internal class EntityTypeRepository
    {
        private static ObjectQuery<EntityTypeDAL> DefaultEntityTypeQuery
        {
            get
            {
                return QPContext.EFContext.EntityTypeSet
                .Include("Parent")
                .Include("CancelAction")
                .Include("ContextMenu");
            }
        }

        /// <summary>
        /// Возвращает тип сущности по ее идентификатору
        /// </summary>
        /// <param name="entityTypeId">идентификатор типа сущности</param>
        /// <returns>информация о типе сущности</returns>
        internal static EntityType GetById(int entityTypeId)
        {
            EntityType entityType = MappersRepository.EntityTypeMapper.GetBizObject(DefaultEntityTypeQuery.Single(et => et.Id == entityTypeId));
            return entityType;
        }

        /// <summary>
        /// Возвращает тип сущности по ее коду
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>информация о типе сущности</returns>
        internal static EntityType GetByCode(string entityTypeCode)
        {
            var entityType = MappersRepository.EntityTypeMapper.GetBizObject(DefaultEntityTypeQuery.Single(et => et.Code == entityTypeCode));
            return entityType;
        }

        private static Lazy<IEnumerable<EntityType>> entityTypesCache = new Lazy<IEnumerable<EntityType>>(() => LoadEntityTypes());

        private static IEnumerable<EntityType> LoadEntityTypes()
        {
            var mapper = MappersRepository.EntityTypeMapper;
            mapper.DisableTranslations = true;

            var result = mapper.GetBizList(DefaultEntityTypeQuery.ToList());
            mapper.DisableTranslations = false;

            return result;
        }

        internal static IEnumerable<EntityType> GetList()
        {
            return entityTypesCache.Value;
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
            var entityType = QPContext.EFContext.EntityTypeSet.Include("Parent").SingleOrDefault(et => et.Code == entityTypeCode);
            return entityType.Parent != null ? entityType.Parent.Code : null;
        }

        /// <summary>
        /// Возвращает код действия по умолчанию для указанного типа сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>код действия по умолчанию</returns>
        internal static string GetDefaultActionCodeByEntityTypeCode(string entityTypeCode)
        {
            var entityType = QPContext.EFContext.EntityTypeSet.Include("DefaultAction").SingleOrDefault(et => et.Code == entityTypeCode);
            return entityType.DefaultAction != null ? entityType.DefaultAction.Code : null;
        }
    }
}
