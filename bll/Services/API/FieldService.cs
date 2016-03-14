using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.BLL.Services.API
{
	public class FieldService : ServiceBase
	{


		public FieldService(string connectionString, int userId) : base(connectionString, userId)
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
				var o2m = ContentRepository.GetRelatedO2MFields(contentId);
				var m2m = ContentRepository.GetRelatedM2MFields(contentId);
				var m2o = ContentRepository.GetRelatedM2OFields(contentId);

				return m2o.Union(o2m.Union(m2m));
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
				Field.Die(id, true);
				QPContext.CurrentUserId = 0;
			}
		}
	}
}
