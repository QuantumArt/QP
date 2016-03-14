using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using System.Security;

namespace Quantumart.QP8.BLL.Services
{
	public interface ISecurityService
	{
		bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode);
		bool IsActionAccessible(string actionCode);
		bool IsActionAccessible(string actionCode, out BackendAction action);
		bool IsCurrentUserAdmin();
	}
	public class SecurityService : ISecurityService
	{

		/// <summary>
		/// Определяет есть ли доступ к действию над конкретнам экземпляром сущности для пользователя по entity_type_code и action_type_code
		/// </summary>
		/// <param name="user"></param>
		/// <param name="entityTypeCode"></param>
		/// <param name="entityId"></param>
		/// <param name="actionTypeCode"></param>
		/// <returns></returns>
		public bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode)
		{
			return SecurityRepository.IsEntityAccessible(entityTypeCode, entityId, actionTypeCode);
		}

		public bool IsActionAccessible(string  actionCode)
		{			
			return SecurityRepository.IsActionAccessible(actionCode);
		}

		public bool IsActionAccessible(string actionCode, out BackendAction action)
		{
			return SecurityRepository.IsActionAccessible(actionCode, out action);
		}

		public bool IsCurrentUserAdmin()
		{
			return QPContext.IsAdmin;
		}
	}
}
