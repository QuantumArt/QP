using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Utils
{
	/// <summary>
	/// Обертка для значения свойства которое должно быть инициализированно при первом вызове геттера
	/// если не разу до этого не вызывался сеттер
	/// </summary>
	public class InitPropertyValue<T>
	{
		private T value = default(T);
		private bool hasBeenSet = false;

		private Func<T> initializer = null;
		private Func<T, T> customSetter = null;

		public InitPropertyValue(Func<T> initializer, Func<T,T> customSetter = null)
		{
			this.initializer = initializer;
			this.customSetter = customSetter;
		}

		public InitPropertyValue() : this(null, null)
		{

		}

		public T Value
		{
			get
			{
				if (!hasBeenSet && initializer != null)
				{
					value = initializer();
					hasBeenSet = true;
				}
				return value;
			}
			set
			{
				hasBeenSet = true;
				if (customSetter == null)
					this.value = value;
				else
					this.value = customSetter(value);
			}
		}

		public Func<T> Initializer 
		{
			get { return initializer; }
			set { initializer = value; } 
		}

	}
}
