using System.Collections.Generic;

namespace Quantumart.QP8.BLL
{
    public interface IContextStorage
    {
        T GetValue<T>(string key);

        void SetValue<T>(T value, string key);

        void ResetValue(string key);

        IEnumerable<string> Keys { get; }
    }
}
