using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContentGroupMapper : GenericMapper<ContentGroup, ContentGroupDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<ContentGroup, ContentGroupDAL>()
				.ForMember(data => data.Site, opt => opt.Ignore())
				.ForMember(data => data.Contents, opt => opt.Ignore())
			;			
		}
	}
}
