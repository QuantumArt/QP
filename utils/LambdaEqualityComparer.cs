using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	public class LambdaEqualityComparer<T> : IEqualityComparer<T>
	{
		public LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode = null) 
        { 
            this.equals = equals;
			if (getHashCode == null)
				this.getHashCode = obj => obj.GetHashCode();
			else
				this.getHashCode = getHashCode; 
        } 
 
        readonly Func<T, T, bool> equals; 
        public bool Equals(T x, T y) 
        { 
            return equals(x, y); 
        } 
 
        readonly Func<T, int> getHashCode; 
        public int GetHashCode(T obj) 
        { 
            return getHashCode(obj); 
        } 		
	}
}
