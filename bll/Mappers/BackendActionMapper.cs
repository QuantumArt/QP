using System.Linq;
using AutoMapper;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class BackendActionMapper : GenericMapper<BackendAction, BackendActionDAL>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BackendActionDAL, BackendAction>(MemberList.Source)
                .ForMember(biz => biz.NextFailedActionCode, opt => opt.MapFrom(data => data.NextFailedAction != null ? data.NextFailedAction.Code : null))
                .ForMember(biz => biz.NextSuccessfulActionCode, opt => opt.MapFrom(data => data.NextSuccessfulAction != null ? data.NextSuccessfulAction.Code : null))
                .ForMember(biz => biz.ExcludeCodes, opt => opt.MapFrom(data => data.Excludes != null ? data.Excludes.Select(n => n.Code).ToArray() : null))
                .ForMember(biz => biz.Name, opt => opt.MapFrom(data => Translator.Translate(data.Name)))
                .ForMember(biz => biz.NotTranslatedName, opt => opt.MapFrom(data => Translator.Translate(data.Name)))
                .ForMember(biz => biz.ShortName, opt => opt.MapFrom(data => Translator.Translate(data.ShortName)))
                .ForMember(biz => biz.ConfirmPhrase, opt => opt.MapFrom(data => Translator.Translate(data.ConfirmPhrase)))
                .ForMember(biz => biz.TabId, opt => opt.MapFrom(data => Converter.ToNullableInt32(data.TabId)));
        }

        public override void CreateDalMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<BackendAction, BackendActionDAL>(MemberList.Destination)
                .ForMember(data => data.ActionType, opt => opt.Ignore())
                .ForMember(data => data.EntityType, opt => opt.Ignore())
                .ForMember(data => data.CustomActions, opt => opt.Ignore())
                .ForMember(data => data.ToolbarButtons, opt => opt.Ignore())
                .ForMember(data => data.ContextMenuItems, opt => opt.Ignore())
                .ForMember(data => data.DefaultViewType, opt => opt.Ignore())
                .ForMember(data => data.NextFailedAction, opt => opt.Ignore())
                .ForMember(data => data.NextSuccessfulAction, opt => opt.Ignore())
                .ForMember(data => data.ParentAction, opt => opt.Ignore())
                .ForMember(data => data.ParentPreFailedActions, opt => opt.Ignore())
                .ForMember(data => data.ParentPreSuccessfulActions, opt => opt.Ignore())
                .ForMember(data => data.ExcludedBy, opt => opt.Ignore())
                .ForMember(data => data.Excludes, opt => opt.Ignore())
                .ForMember(data => data.Views, opt => opt.Ignore());
        }
    }
}
