using System.Data;
using AutoMapper;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class BackendActionStatusMapper : GenericMapper<BackendActionStatus, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, BackendActionStatus>(MemberList.Source)
                .ForMember(biz => biz.Code, opt => opt.MapFrom(row => row.Field<string>("CODE")))
                .ForMember(biz => biz.Visible, opt => opt.MapFrom(row => row.Field<bool>(FieldName.Visible)));
        }
    }
}
