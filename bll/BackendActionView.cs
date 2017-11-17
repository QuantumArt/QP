using System.Web.Script.Serialization;

namespace Quantumart.QP8.BLL
{
    public class BackendActionView
    {
        /// <summary>
        /// идентификатор действия
        /// </summary>
        [ScriptIgnore]
        public int ActionId { get; set; }

        /// <summary>
        /// идентификатор типа представления
        /// </summary>
        [ScriptIgnore]
        public int ViewTypeId { get; set; }

        /// <summary>
        /// тип представления
        /// </summary>
        public ViewType ViewType { get; set; }

        /// <summary>
        /// признак, разрещающий предотвращение поведения поумолчанию
        /// </summary>
        public bool PreventDefaultBehavior { get; set; }

        /// <summary>
        /// URL действия контроллера
        /// </summary>
        public string ControllerActionUrl { get; set; }
    }
}
