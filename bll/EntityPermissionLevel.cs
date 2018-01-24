using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Уровень доступа к сущности
    /// </summary>
    public class EntityPermissionLevel
    {
        private static readonly string _list = "List";
        private static readonly string _fullAccess = "Full Access";

        public int Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public static EntityPermissionLevel GetList() => GetEntityPermissionLevelByName(_list);

        public static EntityPermissionLevel GetFullAccess() => GetEntityPermissionLevelByName(_fullAccess);

        private static EntityPermissionLevel GetEntityPermissionLevelByName(string name) => PageTemplateRepository.GetPermissionLevelByName(name);
    }
}
