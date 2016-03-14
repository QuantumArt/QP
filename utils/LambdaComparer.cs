using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public class LambdaComparer<T> : IComparer<T>
	{
		Func<T, T, int> compareTo = null;

		public LambdaComparer(Func<T, T, int> compareTo)
		{
			if (compareTo == null)
				throw new ArgumentNullException("compareTo");
			this.compareTo = compareTo;
		}
		
		#region IComparer<T> Members

		public int Compare(T x, T y)
		{
			return compareTo(x, y);
		}

		#endregion
	}
}
