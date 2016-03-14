using System;
using System.Text;
using System.Security;
using System.Security.Principal;

namespace Quantumart.QP8.Security
{
    public class QPIdentity : IIdentity
    {
        private int _id = 0;
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public int Id
        {
            get { return _id; }
        }

        private string _name = "";
        /// <summary>
        /// логин пользователя
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        private string _customerCode = "";
        /// <summary>
        /// код клиента
        /// </summary>
        public string CustomerCode
        {
            get { return _customerCode; }
        }

        private string _authenticationType = "";
        /// <summary>
        /// название типа аутентификации
        /// </summary>
        public string AuthenticationType
        {
            get { return _authenticationType; }
        }

        private bool _isAuthentificated = false;
        /// <summary>
        /// признак успешной атентификации
        /// </summary>
        public bool IsAuthenticated
        {
            get { return _isAuthentificated; }
        }

		private int _languageId = 0;
		/// <summary>
		/// идентификатор языка
		/// </summary>
		public int LanguageId
		{
			get { return _languageId; }
		}

		private string _cultureName = "";
		/// <summary>
		/// название культуры
		/// </summary>
		public string CultureName
		{
			get { return _cultureName; }
		}

		/// <summary>
		/// Установлен ли Silverlight у пользователя
		/// </summary>
		public bool IsSilverlightInstalled { get; private set; }

        /// <summary>
        /// Конструирует объект QPIdentity
        /// </summary>
        /// <param name="name">логин пользователя</param>
        /// <param name="type">тип аутентификации</param>
        /// <param name="id">идентификатор пользователя</param>
        /// <param name="customerCode">код клиента</param>
		public QPIdentity(int id, string name, string customerCode, string type, bool isAuthentificated, 
			int languageId, string cultureName, bool isSilverlightInstalled)
        {
            _id = id;
            _name = name;
            _customerCode = customerCode;
            _authenticationType = type;
            _isAuthentificated = isAuthentificated;
			_languageId = languageId;
			_cultureName = cultureName;
			IsSilverlightInstalled = isSilverlightInstalled;
        }
    }
}
