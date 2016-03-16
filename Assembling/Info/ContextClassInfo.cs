using System;

namespace QA_Assembling.Info
{
    public class ContextClassInfo
    {
        public string ClassName { get; set; }

        public string NamespaceName { get; set; }

        public string FullClassName
        {
            get
            {
                if (String.IsNullOrEmpty(NamespaceName))
                    return ClassName;
                else
                    return $"{NamespaceName}.{ClassName}";
            }
        }

        public static ContextClassInfo Parse (string input)
        {

            var index = input.LastIndexOf('.');
            string contextClass, contextNamespace;
            if (index == -1)
            {
                contextClass = input;
                contextNamespace = String.Empty;
            }
            else
            {
                contextClass = input.Substring(index + 1);
                contextNamespace = input.Substring(0, index);
            }
            return new ContextClassInfo() { ClassName = contextClass, NamespaceName = contextNamespace };
        }

    }
}
