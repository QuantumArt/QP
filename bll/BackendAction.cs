using System.Collections.Generic;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class BackendAction : BizObject
    {
        /// <summary>
        /// идентификатор действия
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// идентификатор типа действия
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// идентификатор типа сущности
        /// </summary>
        public int EntityTypeId { get; set; }

        /// <summary>
        /// название действия
        /// </summary>
        public string Name { get; set; }

        public string NotTranslatedName { get; set; }

        /// <summary>
        /// краткое название действия
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// код действия
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// URL действия контроллера
        /// </summary>
        public string ControllerActionUrl { get; set; }

        /// <summary>
        /// дополнительный URL действия контроллера
        /// </summary>
        public string AdditionalControllerActionUrl { get; set; }

        /// <summary>
        /// текст предупреждения, которое выводится
        /// перед запуском действия
        /// </summary>
        [LocalizedDisplayName("ConfirmPhrase", NameResourceType = typeof(CustomActionStrings))]
        [MaxLengthValidator(1000, MessageTemplateResourceName = "ConfirmPhraseLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public string ConfirmPhrase { get; set; }

        /// <summary>
        /// признак того, что для выполнения действия требуется пользовательский интерфейс
        /// </summary>
        [LocalizedDisplayName("IsInterface", NameResourceType = typeof(CustomActionStrings))]
        public bool IsInterface { get; set; }

        [LocalizedDisplayName("HasPreAction", NameResourceType = typeof(CustomActionStrings))]
        public bool HasPreAction { get; set; }

        [LocalizedDisplayName("HasSettings", NameResourceType = typeof(CustomActionStrings))]
        public bool HasSettings { get; set; }

        /// <summary>
        /// признак того, что для выполнения действия требуется всплывающее окно
        /// </summary>
        [LocalizedDisplayName("IsWindow", NameResourceType = typeof(CustomActionStrings))]
        public bool IsWindow { get; set; }

        /// <summary>
        /// ширина окна
        /// </summary>
        [LocalizedDisplayName("WindowWidth", NameResourceType = typeof(CustomActionStrings))]
        public int? WindowWidth { get; set; }

        /// <summary>
        /// высота окна
        /// </summary>
        [LocalizedDisplayName("WindowHeight", NameResourceType = typeof(CustomActionStrings))]
        public int? WindowHeight { get; set; }

        /// <summary>
        /// представление, выбранное по умолчанию
        /// </summary>
        public ViewType DefaultViewType { get; set; }

        /// <summary>
        /// разрешает поиск
        /// </summary>
        public bool AllowSearch { get; set; }

        /// <summary>
        /// разрешает предварительный просмотр
        /// </summary>
        public bool AllowPreview { get; set; }

        /// <summary>
        /// идентификатор родительского действия
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// тип действия
        /// </summary>
        public BackendActionType ActionType { get; set; }

        /// <summary>
        /// тип сущности
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// список представлений
        /// </summary>
        public List<BackendActionView> Views { get; set; }

        /// <summary>
        /// идентификатор действия, которое должно быть произведено после успешного выполнения текущего действия
        /// </summary>
        public int? NextSuccessfulActionId { get; set; }

        /// <summary>
        /// код действия, которое должно быть произведено после успешного выполнения текущего действия
        /// </summary>
        public string NextSuccessfulActionCode { get; set; }

        /// <summary>
        /// идентификатор действия, которое должно быть произведено после неуспешного выполнения текущего действия
        /// </summary>
        public int? NextFailedActionId { get; set; }

        /// <summary>
        /// код действия, которое должно быть произведено после неуспешного выполнения текущего действия
        /// </summary>
        public string NextFailedActionCode { get; set; }

        /// <summary>
        /// Признак того, что действие является пользовательским
        /// </summary>
        public bool IsCustom { get; set; }

        /// <summary>
        /// Признак того, что операция длительная и многошаговая
        /// </summary>
        public bool IsMultistep { get; set; }

        public IEnumerable<ToolbarButton> ToolbarButtons { get; set; }

        public IEnumerable<ContextMenuItem> ContextMenuItems { get; set; }

        public int? TabId { get; set; }

        public string[] ExcludeCodes { get; set; }

        /// <summary>
        /// Лимит объектов при превышении которого действие обрабатывается как многошаговое
        /// </summary>
        public int? EntityLimit { get; set; }
    }
}
