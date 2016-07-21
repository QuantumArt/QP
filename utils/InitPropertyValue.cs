using System;

namespace Quantumart.QP8.Utils
{
    /// <summary>
    /// Обертка для значения свойства которое должно быть инициализированно при первом вызове геттера
    /// если не разу до этого не вызывался сеттер
    /// </summary>
    public class InitPropertyValue<T>
    {
        private T _value;
        private bool _hasBeenSet;
        private readonly Func<T, T> _customSetter;

        public InitPropertyValue() : this(null) { }

        public InitPropertyValue(Func<T> initializer, Func<T, T> customSetter = null)
        {
            Initializer = initializer;
            _customSetter = customSetter;
        }

        public T Value
        {
            get
            {
                if (!_hasBeenSet && Initializer != null)
                {
                    _value = Initializer();
                    _hasBeenSet = true;
                }

                return _value;
            }
            set
            {
                _hasBeenSet = true;
                _value = _customSetter == null ? value : _customSetter(value);
            }
        }

        public Func<T> Initializer { get; set; }
    }
}
