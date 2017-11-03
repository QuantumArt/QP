namespace Quantumart.QP8.BLL
{
    public class BackendActionType : BizObject
    {
        /// <summary>
        /// Идентификатор типа действия
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название типа действия
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Название типа действия (на английском)
        /// </summary>
        public string NotTranslatedName { get; set; }

        /// <summary>
        /// Код типа действия
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Идентификатор требуемого уровня доступа
        /// </summary>
        public int RequiredPermissionLevelId { get; set; }

        /// <summary>
        /// Требуемый уровня доступа
        /// </summary>
        public int RequiredPermissionLevel { get; set; }

        /// <summary>
        /// Количество сущностей, к которым применяется действие
        /// </summary>
        public byte ItemsAffected { get; set; }

        /// <summary>
        /// Признак множественного действия
        /// </summary>
        public bool IsMultiple => ItemsAffected > 1;
    }
}
