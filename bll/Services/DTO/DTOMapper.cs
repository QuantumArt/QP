using AutoMapper;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class DTOMapper
    {
        public static void CreateAllMappings(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Folder, EntityTreeItem>(MemberList.Source)
                .ForMember(data => data.Alias, opt => opt.MapFrom(src => src.OutputName))
                ;
        }
    }
}
