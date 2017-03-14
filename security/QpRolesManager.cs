namespace Quantumart.QP8.Security
{
    public class QpRolesManager
    {
        public static string[] GetRolesForUser(bool isAdmin)
        {
            if (isAdmin)
            {
                return new[]
                {
                    "Admin",
                    "Elmah"
                };
            }

            return new string[] { };
        }
    }
}
