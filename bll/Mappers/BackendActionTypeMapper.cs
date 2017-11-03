using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class BackendActionTypeMapper : GenericMapper<BackendActionType, ActionTypeDAL>
    {
        public bool DisableTranslations { get; set; }

        public override void CreateBizMapper()
        {
            Mapper.CreateMap<ActionTypeDAL, BackendActionType>()
                .ForMember(biz => biz.Name, opt => opt.MapFrom(data => DisableTranslations ? data.Name : Translator.Translate(data.Name)))
                .ForMember(biz => biz.NotTranslatedName, opt => opt.MapFrom(data => data.Name))
                .ForMember(biz => biz.RequiredPermissionLevel, opt => opt.MapFrom(data => Converter.ToInt32(data.PermissionLevel.Level)));
        }
    }
}
