using AutoMapper;

namespace Quantumart.QP8.BLL.Services.DTO
{
    public class DTOMapper
    {
        public static void CreateAllMappings()
        {
            Mapper.CreateMap<Folder, EntityTreeItem>()
                .ForMember(data => data.Alias, opt => opt.MapFrom(src => src.OutputName))
                ;
        }
    }
}
