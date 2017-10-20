using AutoMapper;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Mappers
{
	internal class ContainerMapper : GenericMapper<Container, ContainerDAL>
	{
		public override void CreateDalMapper()
		{
			Mapper.CreateMap<Container, ContainerDAL>()				
				.ForMember(x => x.Users, opt => opt.Ignore())
				.ForMember(x => x.Object, opt => opt.Ignore())
				.ForMember(x => x.StartPermissionLevel, opt => opt.Ignore())
				.ForMember(x => x.EndPermissionLevel, opt => opt.Ignore())
				.ForMember(x => x.LockedBy, opt => opt.Ignore())
				.ForMember(x => x.Content, opt => opt.Ignore())
				.ForMember(x => x.Locked, opt => opt.Ignore())
				.AfterMap(SetDalProperties)
				;
		}

		public override void CreateBizMapper()
		{
			Mapper.CreateMap<ContainerDAL, Container>()				
				.ForMember(x => x.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(x => x.Object, opt => opt.Ignore())
				.AfterMap(SetBizProperties)
				;
		}

		private static void SetBizProperties(ContainerDAL dataObject, Container bizObject)
		{
			bizObject.EnableDataCaching = dataObject.Duration.HasValue;
		}

		private static void SetDalProperties(Container bizObject, ContainerDAL dataObject)
		{
			if (!bizObject.ApplySecurity)
			{
				dataObject.UseLevelFiltration = null;
				dataObject.StartLevel = null;
				dataObject.EndLevel = null;
			}
			
			if (!bizObject.AllowOrderDynamic)
			{
			    dataObject.OrderDynamic = null;
			}

		    if (!bizObject.AllowOrderDynamic)
		    {
		        dataObject.OrderDynamic = null;
		    }

		    if (!bizObject.EnableDataCaching)
			{
				dataObject.Duration = null;
			}

			if (bizObject.AllowDynamicContentChanging == false)
			{
				dataObject.DynamicContentVariable = null;
			}
		}
	}
}
