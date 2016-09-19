using System.Linq;
using Quantumart.QP8.BLL.Mappers;
using Quantumart.QP8.BLL.Models.XmlDbUpdate;

namespace Quantumart.QP8.BLL.Repository.XmlDbUpdate
{
    public class XmlDbUpdateActionsLogRepository
    {
        public bool IsExist(string hash)
        {
            return QPContext.EFContext.XML_DB_UPDATE_ACTIONS.Count(entry => entry.Hash == hash) > 0;
        }

        public int Insert(XmlDbUpdateActionsLogModel entry)
        {
            var context = QPContext.EFContext;
            var entity = MappersRepository.XmlDbUpdateActionsLogMapper.GetDalObject(entry);
            context.XML_DB_UPDATE_ACTIONS.AddObject(entity);
            context.SaveChanges();

            return entity.Id;
        }
    }
}
