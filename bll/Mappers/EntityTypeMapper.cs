using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;
using AutoMapper;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class EntityTypeMapper : GenericMapper<EntityType, EntityTypeDAL>
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
			Mapper.CreateMap<EntityTypeDAL, EntityType>()				
				//---------------
				.ForMember(biz => biz.ParentCode, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Code : null))
				.ForMember(biz => biz.CancelActionCode, opt => opt.MapFrom(src => src.CancelAction != null ? src.CancelAction.Code : null))
				.ForMember(biz => biz.Name, opt => opt.MapFrom(data => _DisableTranslations ? data.Name : Translator.Translate(data.Name)))
				.ForMember(biz => biz.NotTranslatedName, opt => opt.MapFrom(data => data.Name))
				.ForMember(biz => biz.TabId, opt => opt.MapFrom(data => Converter.ToNullableInt32(data.TabId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<EntityType, EntityTypeDAL>()
				.ForMember(data => data.Source, opt => opt.Ignore())
				.ForMember(data => data.IdField, opt => opt.Ignore())
				.ForMember(data => data.ParentIdField, opt => opt.Ignore());
		}
	}
}
