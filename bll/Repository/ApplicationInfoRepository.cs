using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.DAL;

namespace Quantumart.QP8.BLL.Repository
{	
	internal static class ApplicationInfoRepository
	{		
		public static string GetCurrentDBVersion()
		{
			using (var scope = new QPConnectionScope())
			{
				return Common.GetCurrentDBVersion(scope.DbConnection);
			}
		}
	
		public static bool RecordActions()
		{
			using (var scope = new QPConnectionScope())
			{
				return DbRepository.Get().RecordActions;
			}
		}
	}
}
