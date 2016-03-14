using System;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Security
{
    public class QPUser
    {
        private int _id = 0;
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _name = "";
        /// <summary>
        /// логин пользователя
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _customerCode = "";
        /// <summary>
        /// код клиента
        /// </summary>
        public string CustomerCode
        {
            get { return _customerCode; }
            set { _customerCode = value; }
        }

		private int _languageId = 0;
		/// <summary>
		/// идентификатор языка
		/// </summary>
		public int LanguageId
		{
			get { return _languageId; }
			set { 
                _languageId = value;
                _cultureName = GetCultureNameByLanguageId(_languageId);
            }
		}

		private string _cultureName = "";
		/// <summary>
		/// название культуры
		/// </summary>
		public string CultureName
		{
			get { return _cultureName; }
		}

        private string[] _roles = new string[0];
        /// <summary>
        /// список ролей, доступных пользователю
        /// </summary>
        public string[] Roles
        {
            get { return _roles; }
            set { _roles = value; }
        }

        /// <summary>
        /// Возвращает название языковой культуры по идентификатору языка
        /// </summary>
        /// <param name="languageId">идентификатор языка</param>
        /// <returns>название языковой культуры</returns>
        public static string GetCultureNameByLanguageId(int languageId)
        {
            string cultureName = "neutral";

            if (languageId == Constants.LanguageId.English)
            {
                cultureName = "en-us";
            }
            else if (languageId == Constants.LanguageId.Russian)
            {
                cultureName = "ru-ru";
            }
            else if (languageId == Constants.LanguageId.Arabic)
            {
                cultureName = "ar-ar";
            }

            return cultureName;
        }

		/// <summary>
		/// Установлен ли Silverlight у пользователя
		/// </summary>
		public bool IsSilverlightInstalled { get; set; }
    }
}
