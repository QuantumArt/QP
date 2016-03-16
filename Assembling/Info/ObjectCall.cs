using System;

namespace QA_Assembling.Info
{
    public class ObjectCall
    {
        public ObjectCall(string call, AssembleInfo info)
        {
            TemplateName = info.TemplateName;
            FormatName = "";

            char[] splitParams = { '.' };
            var callParams = call.Split(splitParams);
            if (callParams.Length > 3)
            {
                throw new ArgumentException("Invalid object call: " + call);
            }
            else if (callParams.Length == 3)
            {
                TemplateName = callParams[0];
                ObjectName = callParams[1];
                FormatName = callParams[2];
                TypeCode = "TOF";
            }
            else if (callParams.Length == 2)
            {
                if (info.IsTemplateName(callParams[0]))
                {
                    TypeCode = "TO";
                    TemplateName = callParams[0];
                    ObjectName = callParams[1];

                }
                else
                {
                    TypeCode = "OF";
                    ObjectName = callParams[0];
                    FormatName = callParams[1];
                }
            }
            else if (callParams.Length == 1)
            {
                TypeCode = "O";
                ObjectName = callParams[0];
            }

            TemplateName = TemplateName.ToLowerInvariant();
            ObjectName = ObjectName.ToLowerInvariant();
            FormatName = FormatName.ToLowerInvariant();
        }


        public string TypeCode { get; }

        public string TemplateName { get; }

        public string ObjectName { get; }

        public string FormatName { get; }
    }
}
