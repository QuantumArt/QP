using AutoMapper;
using Quantumart.QP8.BLL.Facades;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Mappers.EntityPermissions
{
	internal class WorkflowPermissionMapper : GenericMapper<EntityPermission, WorkflowPermissionDAL>
	{
		public override void CreateBizMapper()
		{
			Mapper.CreateMap<WorkflowPermissionDAL, EntityPermission>()
				.ForMember(biz => biz.Parent, opt => opt.MapFrom(data => MapperFacade.WorkflowMapper.GetBizObject(data.Workflow)))
				.ForMember(biz => biz.ParentEntityId, opt => opt.MapFrom(data => Converter.ToInt32(data.WorkflowId)));
		}

		public override void CreateDalMapper()
		{
			Mapper.CreateMap<EntityPermission, WorkflowPermissionDAL>()
				.ForMember(data => data.Workflow, opt => opt.Ignore())
				.ForMember(data => data.WorkflowId, opt => opt.MapFrom(biz => Converter.ToDecimal(biz.ParentEntityId)))

				.ForMember(data => data.LastModifiedByUser, opt => opt.Ignore())
				.ForMember(data => data.Group, opt => opt.Ignore())
				.ForMember(data => data.PermissionLevel, opt => opt.Ignore())
				.ForMember(data => data.User, opt => opt.Ignore());
		}
	}
}
