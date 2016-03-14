using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.Communication
{
	public interface ISyncService<T>
	{
		void Sync(T data);
		void Sync();
	}
}
