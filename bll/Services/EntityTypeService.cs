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
        public static EntityType GetById(int entityTypeId) => EntityTypeRepository.GetById(entityTypeId);

        /// <summary>
        /// Возвращает тип сущности по ее коду
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>информация о типе сущности</returns>
        public static EntityType GetByCode(string entityTypeCode) =>
            string.IsNullOrWhiteSpace(entityTypeCode) ? null : EntityTypeRepository.GetByCode(entityTypeCode);

        /// <summary>
        /// Возвращает код типа сущности по его Id
        /// </summary>
        public static string GetCodeById(int entityTypeId) => GetById(entityTypeId)?.Code;

        /// <summary>
        /// Возвращает код типа родительской сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>код типа родительской сущности</returns>
        public static string GetParentCodeByCode(string entityTypeCode) =>
            string.IsNullOrWhiteSpace(entityTypeCode) ? null : EntityTypeRepository.GetParentCodeByCode(entityTypeCode);

        /// <summary>
        /// Возвращает код действия по умолчанию для указанного типа сущности
        /// </summary>
        /// <param name="entityTypeCode">код типа сущности</param>
        /// <returns>код действия по умолчанию</returns>
        public static string GetDefaultActionCodeByEntityTypeCode(string entityTypeCode) =>
            string.IsNullOrWhiteSpace(entityTypeCode) ? null : EntityTypeRepository.GetDefaultActionCodeByEntityTypeCode(entityTypeCode);
    }
}
