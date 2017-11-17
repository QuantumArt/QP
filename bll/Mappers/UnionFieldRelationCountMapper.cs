using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Repository.Results;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class UnionFieldRelationCountMapper : GenericMapper<UnionFieldRelationCount, DataRow>
    {
        public override void CreateBizMapper()
        {
            Mapper.CreateMap<DataRow, UnionFieldRelationCount>()
                .ForMember(biz => biz.Count, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<int>("F_COUNT"))))
                .ForMember(biz => biz.UnionFieldId, opt => opt.MapFrom(r => Converter.ToInt32(r.Field<decimal>("UNION_FIELD_ID"))));
        }
    }
}
