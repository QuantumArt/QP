using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class EntityTypeMapper : GenericMapper<EntityType, EntityTypeDAL>
    {
        public bool DisableTranslations { get; set; }

        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<EntityTypeDAL, EntityType>(MemberList.Source)

                //---------------
                .ForMember(biz => biz.ParentCode, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Code : null))
                .ForMember(biz => biz.CancelActionCode, opt => opt.MapFrom(src => src.CancelAction != null ? src.CancelAction.Code : null))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(data => DisableTranslations ? data.Name : Translator.Translate(data.Name)))
                .ForMember(biz => biz.NotTranslatedName, opt => opt.MapFrom(data => data.Name))
                .ForMember(biz => biz.TabId, opt => opt.MapFrom(data => Converter.ToNullableInt32(data.TabId)));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<EntityType, EntityTypeDAL>(MemberList.Destination)
                .ForMember(data => data.Source, opt => opt.Ignore())
                .ForMember(data => data.IdField, opt => opt.Ignore())
                .ForMember(data => data.ParentIdField, opt => opt.Ignore());
        }
    }
}
