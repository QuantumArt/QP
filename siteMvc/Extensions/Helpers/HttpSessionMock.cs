using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public class HttpSessionMock : HttpSessionStateBase
    {
        private readonly NameValueCollection _keyCollection = new NameValueCollection();
        private readonly Dictionary<string, object> _objects = new Dictionary<string, object>();

        public override object this[string name]
        {
            get
            {
                object result = null;
                if (_objects.ContainsKey(name))
                {
                    result = _objects[name];
                }

                return result;

            }
            set
            {
                _objects[name] = value;
                _keyCollection[name] = null;
            }
        }

        public override NameObjectCollectionBase.KeysCollection Keys => _keyCollection.Keys;
    }
}
