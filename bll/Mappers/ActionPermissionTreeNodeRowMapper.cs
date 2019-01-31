using System.Data;
using AutoMapper;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Mappers
{
    internal class ActionPermissionTreeNodeRowMapper : GenericMapper<ActionPermissionTreeNode, DataRow>
    {
        public override void CreateBizMapper(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<DataRow, ActionPermissionTreeNode>(MemberList.Source)
                .ForMember(biz => biz.Id, opt => opt.MapFrom(r => r.Field<int>("ID")))
                .ForMember(biz => biz.Text, opt => opt.MapFrom(r => FormatText(r)))
            ;
        }

        private string FormatText(DataRow row)
        {
            var name = Translator.Translate(row.Field<string>("NAME"));
            var levelName = Translator.Translate(row.Field<string>("PERMISSION_LEVEL_NAME"));
            var isExplicit = row.Field<bool>("IsExplicit");
            if (string.IsNullOrWhiteSpace(levelName))
            {
                return string.Format("{0} – {1}", name, EntityPermissionStrings.UndefinedPermissionLevel);
            }

            return string.Format("{0} – {1} ({2})", name, levelName, isExplicit ? EntityPermissionStrings.Explicit : EntityPermissionStrings.Implicit);
        }
    }
}
