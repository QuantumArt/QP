using System.Data.Entity;
using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Repository.XmlDbUpdate
{
    public class XmlDbUpdateActionsLogRepository : IXmlDbUpdateActionsLogRepository
    {
        public bool IsExist(string hash)
        {
            return QPContext.EFContext.XML_DB_UPDATE_ACTIONS.Count(entry => entry.Hash == hash) > 0;
        }

        public int Insert(XmlDbUpdateActionsLogModel entry)
        {
            var context = QPContext.EFContext;
            var entity = MapperFacade.XmlDbUpdateActionsLogMapper.GetDalObject(entry);
            context.Entry(entity).State = EntityState.Added;
            context.SaveChanges();

            return entity.Id;
        }
    }
}
