using System.Linq;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Repository.XmlDbUpdate
{
    public class XmlDbUpdateLogRepository
    {
        public bool IsExist(string hash)
        {
            return QPContext.EFContext.XML_DB_UPDATE.Count(entry => entry.Hash == hash) > 0;
        }

        public int Insert(XmlDbUpdateLogModel entry)
        {
            var context = QPContext.EFContext;
            var entity = MapperFacade.XmlDbUpdateLogMapper.GetDalObject(entry);
            context.XML_DB_UPDATE.AddObject(entity);
            context.SaveChanges();

            return entity.Id;
        }
    }
}
