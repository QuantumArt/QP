using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.WebMvc.ViewModels.Interfaces
{
	public interface IActionAccessChecker
	{
		bool IsActionAccessible(string actionCode);
	}
}
