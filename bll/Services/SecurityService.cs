using Quantumart.QP8.BLL.Repository;

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
        public bool IsEntityAccessible(string entityTypeCode, int entityId, string actionTypeCode)
        {
            return SecurityRepository.IsEntityAccessible(entityTypeCode, entityId, actionTypeCode);
        }

        public bool IsActionAccessible(string actionCode)
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
