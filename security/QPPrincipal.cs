using System;
using System.Text;
using System.Security;
using System.Security.Principal;

namespace Quantumart.QP8.Security
{
    public class QPPrincipal : GenericPrincipal
    {
        /// <summary>
        /// Конструирует объект типа QPPrincipal
        /// </summary>
        /// <param name="identity">объект типа QPIdentity</param>
        /// <param name="roles">список ролей, в которые входит пользователь</param>
        public QPPrincipal(QPIdentity identity, string[] roles)
            : base(identity, roles)
        {
        }
    }
}
