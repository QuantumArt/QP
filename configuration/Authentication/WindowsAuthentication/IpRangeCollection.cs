using System.Configuration;

namespace Quantumart.QP8.Configuration.Authentication.WindowsAuthentication
{
    [ConfigurationCollection(typeof(IpRangeElement))]
    public class IpRangeCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement() => new IpRangeElement();

        protected override object GetElementKey(ConfigurationElement element) => ((IpRangeElement)element).Name;

        public void Add(IpRangeElement element)
        {
            BaseAdd(element);
        }

        public void Remove(string key)
        {
            BaseRemove(key);
        }

        public void Clear()
        {
            BaseClear();
        }
    }
}
