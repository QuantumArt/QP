using System.Security.Principal;

namespace Quantumart.QP8.Security
{
    public class QpIdentity : IIdentity
    {
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// логин пользователя
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// код клиента
        /// </summary>
        public string CustomerCode { get; }

        /// <summary>
        /// название типа аутентификации
        /// </summary>
        public string AuthenticationType { get; }

        /// <summary>
        /// признак успешной атентификации
        /// </summary>
        public bool IsAuthenticated { get; }

        /// <summary>
		/// идентификатор языка
		/// </summary>
		public int LanguageId { get; }

        /// <summary>
		/// название культуры
		/// </summary>
		public string CultureName { get; }

        /// <summary>
		/// Установлен ли Silverlight у пользователя
		/// </summary>
		public bool IsSilverlightInstalled { get; private set; }

        /// <summary>
        /// Конструирует объект QPIdentity
        /// </summary>
		public QpIdentity(int id, string name, string customerCode, string type, bool isAuthentificated, int languageId, string cultureName, bool isSilverlightInstalled)
        {
            Id = id;
            Name = name;
            CustomerCode = customerCode;
            AuthenticationType = type;
            IsAuthenticated = isAuthentificated;
            LanguageId = languageId;
            CultureName = cultureName;
            IsSilverlightInstalled = isSilverlightInstalled;
        }
    }
}
