using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;

namespace Quantumart.QP8.BLL.Repository.ArticleRepositories
{
    /// <summary>
    /// Интерфейс репозитория для получения данных для блока поиска по статьям
    /// </summary>
    public interface IArticleSearchRepository
    {
        /// <summary>
        /// Возвращает список всех полей статьи
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        IEnumerable<Field> GetAllArticleFields(int contentId);

        Dictionary<Field, IEnumerable<Field>> GetAllArticleRelatedFields(int contentId);

        /// <summary>
        /// Получить поле по ID
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        Field GetFieldByID(int fieldId);

        /// <summary>
        /// Получить LinkedID поля по его ID
        /// </summary>
        /// <param name="fieldId"></param>
        /// <returns></returns>
        int? GetFieldLinkedID(int fieldId);

        /// <summary>
        /// Возвращает список всех пользователей
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetAllUsersList();

        /// <summary>
        /// Получить контент по его ID
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        Content GetContentById(int contentId);

        /// <summary>
        /// Получить список статусов по ID сайта
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        IEnumerable<StatusType> GetStatusList(int siteId);
    }

    /// <summary>
    /// Реализация репозитория для получения данных для блока поиска по статьям
    /// </summary>
    public class ArticleSearchRepository : IArticleSearchRepository
    {
        public IEnumerable<Field> GetAllArticleFields(int contentId) => FieldRepository.GetFullList(contentId);

        public Dictionary<Field, IEnumerable<Field>> GetAllArticleRelatedFields(int contentId)
        {
            return FieldRepository.GetFullList(contentId).ToDictionary(f => f, GetRelatedFields);
        }

        private IEnumerable<Field> GetRelatedFields(Field field)
        {
            if (field.RelationType == RelationType.OneToMany && field.RelationId.HasValue)
            {
                var relationField = FieldRepository.GetById(field.RelationId.Value);
                return GetAllArticleFields(relationField.ContentId).Where(f => f.UseInChildContentFilter);
            }

            return new Field[0];
        }

        public Field GetFieldByID(int fieldId) => FieldRepository.GetById(fieldId);

        public int? GetFieldLinkedID(int fieldId)
        {
            var field = GetFieldByID(fieldId);
            if (field == null)
            {
                return null;
            }

            return field.LinkId ?? field.BackRelationId;
        }

        public IEnumerable<User> GetAllUsersList() => UserRepository.GetAllUsersList();

        public Content GetContentById(int contentId) => ContentRepository.GetById(contentId);

        public IEnumerable<StatusType> GetStatusList(int siteId) => StatusTypeRepository.GetStatusList(siteId);
    }
}
