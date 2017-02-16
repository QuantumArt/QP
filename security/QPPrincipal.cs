using System.Security.Principal;

namespace Quantumart.QP8.Security
{
    public class QpPrincipal : GenericPrincipal
    {
        /// <summary>
        /// Конструирует объект типа QPPrincipal
        /// </summary>
        /// <param name="identity">объект типа QPIdentity</param>
        /// <param name="roles">список ролей, в которые входит пользователь</param>
        public QpPrincipal(IIdentity identity, string[] roles)
            : base(identity, roles)
        {
        }
    }
}
