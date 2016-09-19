using Quantumart.QP8.BLL.Models.XmlDbUpdate;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers.XmlDbUpdate
{
    internal sealed class XmlDbUpdateLogMapper : GenericMapper<XmlDbUpdateLogModel, XmlDbUpdateLogEntity>
    {
        public XmlDbUpdateLogMapper()
        {
            CreateDalMapper();
        }
    }
}
