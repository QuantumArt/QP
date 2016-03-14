using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class BackendActionTypeMapper : GenericMapper<BackendActionType, ActionTypeDAL>
	{

		private bool _DisableTranslations = false;

		public bool DisableTranslations
		{
			get
			{
				return _DisableTranslations;
			}

			set
			{
				_DisableTranslations = value;
			}
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ActionTypeDAL, BackendActionType>()
				.ForMember(biz => biz.Name, opt => opt.MapFrom(data => _DisableTranslations ? data.Name : Translator.Translate(data.Name)))
				.ForMember(biz => biz.NotTranslatedName, opt => opt.MapFrom(data => data.Name))
				.ForMember(biz => biz.RequiredPermissionLevel, opt => opt.MapFrom(data => Converter.ToInt32(data.PermissionLevel.Level)));
		}
	}
}
