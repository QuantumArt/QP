using System;
using System.Text;

namespace Quantumart.QPublishing.Pages
{
    public class ObjectCall
    {
        public ObjectCall(string call, QPageEssential page)
        {
            if (page == null)
            {
                throw new ArgumentException("Argument cannnot be null");
            }

            TemplateName = page.TemplateName;
            FormatName = string.Empty;

            char[] splitParams = { '.' };
            var callParams = call.Split(splitParams);
            if (callParams.Length > 3)
            {
                throw new ArgumentException("Invalid object call: " + call);
            }

            switch (callParams.Length)
            {
                case 3:
                    TemplateName = callParams[0];
                    ObjectName = callParams[1];
                    FormatName = callParams[2];
                    TypeCode = "TOF";
                    break;
                case 2:
                    if (page.IsTemplateName(callParams[0], page.site_id))
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
                    break;
                case 1:
                    TypeCode = "O";
                    ObjectName = callParams[0];
                    break;
            }

            TemplateName = TemplateName.ToLowerInvariant();
            TemplateId = page.GetTemplateId(TemplateName, page.site_id);
            ObjectName = ObjectName.ToLowerInvariant();
            FormatName = FormatName.ToLowerInvariant();
        }

        public string TypeCode { get; }

        public string TemplateName { get; }

        public int TemplateId { get; }

        public string ObjectName { get; }

        public string FormatName { get; }

        public bool WithoutTemplate => TypeCode == "OF" || TypeCode == "O";

        public bool WithoutFormat => TypeCode == "TO" || TypeCode == "O";

        public string GetCallFilter()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[template_name] = '{0}' and [object_name] = '{1}' ", TemplateName, ObjectName);
            switch (TypeCode)
            {
                case "TOF":
                case "OF":
                    sb.AppendFormat(" and [format_name] = '{0}' ", FormatName);
                    break;
                case "TO":
                case "O":
                    sb.Append(" and [default_format_id] = [current_format_id] ");
                    break;
                default:
                    throw new Exception("Unknown call type - " + TypeCode);
            }

            return sb.ToString();
        }

        public string GetCallFilter(int siteId)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[siteId] = {0} and ", siteId);
            sb.Append(GetCallFilter());
            return sb.ToString();
        }
    }
}
