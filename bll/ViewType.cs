using System;
using System.Collections.Generic;
using Quantumart.QP8;
using Quantumart.QP8.BLL;

namespace Quantumart.QP8.BLL
{
	public class ViewType
	{
		/// <summary>
		/// идентификатор типа представления
		/// </summary>
		public int Id
		{
			get;
			set;
		}

		/// <summary>
		/// код типа представления
		/// </summary>
		public string Code
		{
			get;
			set;
		}

		/// <summary>
		/// название типа представления
		/// </summary>
		public string Name
		{
			get;
			set;
		}

		/// <summary>
		/// название файла пиктограммы
		/// </summary>
		public string Icon
		{
			get;
			set;
		}
	}
}
