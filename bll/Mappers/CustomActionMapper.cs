using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class CustomActionMapper : GenericMapper<CustomAction, CustomActionDAL>
	{		
		public override void CreateDalMapper()
        {
			Mapper.CreateMap<CustomAction, CustomActionDAL>()
				.ForMember(data => data.Action, opt => opt.Ignore())
				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Sites, opt => opt.Ignore())
				.ForMember(data => data.Contents, opt => opt.Ignore());
        }

	}
}
