using Microsoft.Practices.Unity;
using Quantumart.QP8.Scheduler.API;
using Quantumart.QP8.BLL.Services.UserSynchronization;
using System.Diagnostics;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.Scheduler.Users
{
	public class SchedulerUsersConfiguration : UnityContainerExtension
	{
		public const string UsersService = "qp8.users";

		protected override void Initialize()
		{
            Container.RegisterType<IUserService, UserService>();
            Container.RegisterService(UsersService, "QP8 Users Synchronization", "Синхронизация пользователей QP8 с Active Directory");
			Container.RegisterProcessor<UsersProcessor>(UsersService, "UserSynchronizationSchedule");
			Container.RegisterType<IUserSynchronizationService, UserSynchronizationService>(new InjectionFactory(c => UserSynchronizationServiceFactory.GetService(c.Resolve<TraceSource>())));			
		}
	}
}