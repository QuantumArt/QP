using System.Collections.Generic;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Interfaces
{
    public interface IUserAndGroupSearchBlockViewModel
    {
        IEnumerable<ListItem> GetMemberTypes();

        int MemberType { get; }
    }
}
