namespace Quantumart.QP8.Security
{
    public class QpUser
    {
        /// <summary>
        /// идентификатор пользователя
        /// </summary>
        public int Id { get; set; } = default(int);

        /// <summary>
        /// логин пользователя
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// код клиента
        /// </summary>
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// идентификатор языка
        /// </summary>
        public int LanguageId
        {
            get { return _languageId; }
            set
            {
                _languageId = value;
                CultureName = GetCultureNameByLanguageId(_languageId);
            }
        }

        /// <summary>
        /// название культуры
        /// </summary>
        public string CultureName { get; private set; } = string.Empty;

        /// <summary>
        /// список ролей, доступных пользователю
        /// </summary>
        public string[] Roles { get; set; } = new string[0];

        /// <summary>
        /// Возвращает название языковой культуры по идентификатору языка
        /// </summary>
        /// <param name="languageId">идентификатор языка</param>
        /// <returns>название языковой культуры</returns>
        public static string GetCultureNameByLanguageId(int languageId)
        {
            var cultureName = "neutral";
            switch (languageId)
            {
                case Constants.LanguageId.English:
                    cultureName = "en-us";
                    break;
                case Constants.LanguageId.Russian:
                    cultureName = "ru-ru";
                    break;
                case Constants.LanguageId.Arabic:
                    cultureName = "ar-ar";
                    break;
            }

            return cultureName;
        }

        /// <summary>
        /// Установлен ли Silverlight у пользователя
        /// </summary>
        public bool IsSilverlightInstalled { get; set; }

        private int _languageId;
    }
}
