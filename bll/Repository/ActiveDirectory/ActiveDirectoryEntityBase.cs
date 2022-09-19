using System;
using Novell.Directory.Ldap;

namespace Quantumart.QP8.BLL.Repository.ActiveDirectory
{
    public abstract class ActiveDirectoryEntityBase
    {
        public string ReferencedPath { get; }

        public string Name { get; }

        public string[] MemberOf { get; }

        protected ActiveDirectoryEntityBase(LdapEntry entry)
        {
            ReferencedPath = entry.Dn;
            LdapAttributeSet attributes = entry.GetAttributeSet();
            Name = GetAttrbibuteValue<string>(attributes, "cn", true);
            MemberOf = GetAttrbibuteValue<string[]>(attributes, "memberOf", false);
        }

        protected T GetAttrbibuteValue<T>(LdapAttributeSet attributes, string attributeName, bool throwIfNull)
        {
            bool result = attributes.TryGetValue(attributeName, out LdapAttribute attribute);

            if (!result && throwIfNull)
                throw new ArgumentException($"Can't find attribute in AD entry.", attributeName);

            switch (typeof(T))
            {
                case Type stringType when stringType == typeof(string):
                    return attribute is null ? ConvertType<T>(string.Empty) : ConvertType<T>(attribute.StringValue);
                case Type stringArray when stringArray == typeof(string[]):
                    return attribute is null ? ConvertType<T>(Array.Empty<string>()) : ConvertType<T>(attribute.StringValueArray);
                default:
                    throw new ArgumentException("Can't match attribute to expected type.", attributeName);
            }
        }

        private T ConvertType<T>(object value)
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
