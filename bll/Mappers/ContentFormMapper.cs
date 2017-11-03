using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ContentFormMapper : GenericMapper<ContentForm, ContentFormDAL>
    {
        public override void CreateDalMapper()
        {
            Mapper.CreateMap<ContentForm, ContentFormDAL>()
                .ForMember(x => x.GenerateUpdateScript, opt => opt.MapFrom(src => src.GenerateUpdateScript ? 1 : 0))
                .ForMember(x => x.Content, opt => opt.Ignore())
                .ForMember(x => x.Object, opt => opt.Ignore())
                .ForMember(x => x.Page, opt => opt.Ignore())
                .ForMember(x => x.LockedBy, opt => opt.Ignore())
                .ForMember(x => x.Locked, opt => opt.Ignore());
        }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ContentFormDAL, ContentForm>()
                .ForMember(data => data.GenerateUpdateScript, opt => opt.MapFrom(x => Converter.ToBoolean(x.GenerateUpdateScript, false)))
                .ForMember(data => data.Page, opt => opt.Ignore())
                ;
        }
    }
}
