using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    [System.Configuration.ConfigurationCollectionAttribute(typeof(IpRangeElement))]
    public class IpRangeCollection : System.Configuration.ConfigurationElementCollection
    {
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new IpRangeElement();
        }

        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            return ((IpRangeElement)(element)).Name;
        }

        public void Add(IpRangeElement element)
        {
            this.BaseAdd(element);
        }

        public void Remove(string key)
        {
            this.BaseRemove(key);
        }

        public void Clear()
        {
            this.BaseClear();
        }
    }
}
