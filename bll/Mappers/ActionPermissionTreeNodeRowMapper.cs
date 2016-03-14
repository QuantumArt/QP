using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Services.DTO;
using System.Data;
using AutoMapper;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Mappers
{
	class ActionPermissionTreeNodeRowMapper : GenericMapper<ActionPermissionTreeNode, DataRow>
	{
		public override void CreateBizMapper()
        {
			Mapper.CreateMap<DataRow, ActionPermissionTreeNode>()
				.ForMember(biz => biz.Id, opt => opt.MapFrom(r => r.Field<int>("ID")))
				.ForMember(biz => biz.Text, opt => opt.MapFrom(FormatText));

		}

		private string FormatText(DataRow row)
		{
			string name = Translator.Translate(row.Field<string>("NAME"));
			string levelName = Translator.Translate(row.Field<string>("PERMISSION_LEVEL_NAME"));
			bool isExplicit = row.Field<bool>("IsExplicit");
			if (string.IsNullOrWhiteSpace(levelName))
				return String.Format("{0} – {1}", name, EntityPermissionStrings.UndefinedPermissionLevel);
			else
				return String.Format("{0} – {1} ({2})", name, levelName, isExplicit ? EntityPermissionStrings.Explicit : EntityPermissionStrings.Implicit);
		}
	}
}
