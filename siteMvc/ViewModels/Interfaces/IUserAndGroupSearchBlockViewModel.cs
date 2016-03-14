using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.ViewModels.Interfaces
{
	public interface IUserAndGroupSearchBlockViewModel
	{
		IEnumerable<ListItem> GetMemberTypes();
		int MemberType { get; }
	}
}
