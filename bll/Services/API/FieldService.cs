using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL.Services.API
{
    public class FieldService : ServiceBase
    {
        public FieldService(string connectionString, int userId) : base(connectionString, userId)
        {
        }

        public FieldService(int userId) : base(userId)
        {
        }

        public Field Read(int id)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return FieldRepository.GetById(id);
            }
        }

        public IEnumerable<Field> List(int contentId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                return FieldRepository.GetFullList(contentId);
            }
        }

        public IEnumerable<Field> ListRelated(int contentId)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                var o2mField = ContentRepository.GetRelatedO2MFields(contentId);
                var m2mField = ContentRepository.GetRelatedM2MFields(contentId);
                var m2oField = ContentRepository.GetRelatedM2OFields(contentId);

                return m2oField.Union(o2mField.Union(m2mField));
            }
        }

        public Field Save(Field field, bool explicitOrder)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = UserId;
                var result = field.PersistWithBackward(explicitOrder);
                QPContext.CurrentUserId = 0;
                return result;
            }
        }

        public void Delete(int id)
        {
            using (new QPConnectionScope(ConnectionString))
            {
                QPContext.CurrentUserId = UserId;
                Field.Die(id);
                QPContext.CurrentUserId = 0;
            }
        }
    }
}
