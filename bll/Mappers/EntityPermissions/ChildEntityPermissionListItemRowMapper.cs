using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
    internal class ChildEntityPermissionListItemRowMapper : GenericMapper<ChildEntityPermissionListItem, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, ChildEntityPermissionListItem>()
                .ForMember(biz => biz.Id, opt => opt.MapFrom(row => Converter.ToInt32(row.Field<decimal>("ID"))))
                .ForMember(biz => biz.IsExplicit, opt => opt.MapFrom(row => row.Field<bool>("IsExplicit")))
                .ForMember(biz => biz.Hide, opt => opt.MapFrom(row => row.Field<bool>("Hide")))
                .ForMember(biz => biz.PropagateToItems, opt => opt.MapFrom(row => Converter.ToBoolean(row.Field<decimal>("PropagateToItems"))))                .ForMember(biz => biz.Title, opt => opt.MapFrom(row => row.Field<string>("Title")))
                .ForMember(biz => biz.LevelName, opt =>
                    opt.MapFrom(row => Translator.Translate(row.Field<string>("LevelName")) ?? EntityPermissionStrings.UndefinedPermissionLevel)
                );
        }
    }
}
