using System;
using System.Collections.Generic;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL
{
    public class EntityType : BizObject
    {
        /// <summary>
        /// идентификатор типа сущности
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// название типа сущности
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Не переведенное имя
        /// </summary>
        public string NotTranslatedName { get; set; }

        /// <summary>
        /// код типа сущности
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// идентификатор родительской сущности
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// код родительской сущности
        /// </summary>
        public string ParentCode { get; set; }

        /// <summary>
        /// порядковый номер
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// признак наличия дочерних типов сущностей
        /// </summary>
        public bool HasItemNodes { get; set; }

        /// <summary>
        /// признак неактивности
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// идентификатор действия типа "Отмена"
        /// </summary>
        public int? CancelActionId { get; set; }

        /// <summary>
        /// код действия типа "Отмена"
        /// </summary>
        public string CancelActionCode { get; set; }

        /// <summary>
        /// Имя параметра контекста для Custom Action
        /// </summary>
        public string ContextName { get; set; }

        /// <summary>
        /// Id вкладки
        /// </summary>
        public int? TabId { get; set; }

        /// <summary>
        /// Контекстное меню
        /// </summary>
        public ContextMenu ContextMenu { get; set; }

        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Имя поля-идентификатора в таблице сущности
        /// </summary>
        public string IdField { get; set; }

        /// <summary>
        /// Имя поля-идентификатора родителя в таблице сущности
        /// </summary>
        public string ParentIdField { get; set; }

        /// <summary>
        /// Проверка на автосохранение
        /// </summary>
        public static bool CheckToAutosave(string code) => AllowedToAutosaveCodes.Contains(code);

        private static readonly HashSet<string> AllowedToAutosaveCodes = new HashSet<string>(new[]
        {
            EntityTypeCode.Site,
            EntityTypeCode.Content,
            EntityTypeCode.Article,
            EntityTypeCode.Field,
            EntityTypeCode.Notification,
            EntityTypeCode.VisualEditorPlugin,
            EntityTypeCode.VisualEditorCommand,
            EntityTypeCode.VisualEditorStyle,
            EntityTypeCode.StatusType,
            EntityTypeCode.Workflow,
            EntityTypeCode.PageTemplate,
            EntityTypeCode.TemplateObjectFormat,
            EntityTypeCode.PageObjectFormat,
            EntityTypeCode.PageObject,
            EntityTypeCode.TemplateObject,
            EntityTypeCode.VirtualContent,
            EntityTypeCode.Page
        }, StringComparer.InvariantCultureIgnoreCase);
        }
}
