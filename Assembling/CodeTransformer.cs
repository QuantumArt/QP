using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using QA_Assembling.Info;

namespace QA_Assembling
{
    public class CodeTransformer
    {
        public AssembleInfo Info { get; }

        public CodeTransformer(AssembleInfo info)
        {
            Info = info;
        }


        public static Hashtable SystemNamespaces { get; } = GetSystemNamespaces();

        internal static RegexOptions StandardRegexOptions => RegexOptions.Compiled | RegexOptions.IgnoreCase;

        private static Hashtable GetSystemNamespaces()
        {
            var hash = new Hashtable
            {
                {"System", ""},
                {"System.Data", ""},
                {"System.Data.SqlClient", ""},
                {"System.Configuration", ""},
                {"System.Collections", ""},
                {"System.Text", ""},
                {"System.Text.RegularExpressions", ""},
                {"System.Web.UI", ""},
                {"System.Web.UI.WebControls", ""},
                {"System.Web.Caching", ""},
                {"Quantumart.QPublishing", ""}
            };
            return hash;

        }


        private static string ReplaceContainerFields(ControlInfo control, string code)
        {
            var pattern = "((?:#|\\s|\\()(?:Field|FieldNS))[ \\t]*\\(\\s*\"([^\",\\)\\r\\n]+)\"\\s*\\)";
            var expressionToAppend = control.IsCSharp ? "((DataRowView)(Container.DataItem))" : "CType(Container.DataItem, DataRowView)";
            var m = Regex.Match(code, pattern, StandardRegexOptions);
            while (m.Success)
            {

                var sb = new StringBuilder();
                sb.Append(m.Groups[1].Value);
                sb.Append("(");
                sb.Append(expressionToAppend);
                sb.Append(", \"");
                sb.Append(m.Groups[2].Value);
                sb.Append("\")");

                code = code.Replace(m.Value, sb.ToString());

                m = m.NextMatch();
            }
            return code;
        }

        private static string AppendOnScreenHead(string head, string result)
        {
            return Regex.Replace(result, "</head>", String.Format(CultureInfo.InvariantCulture, "{0}$0", head), StandardRegexOptions);
        }

        public static string GetProcessedPresentation(ControlInfo control)
        {
            var code = control.Presentation;

            if (control.CurrentType == ControlType.PublishingContainer)
            {
                code = ReplaceContainerFields(control, code);
            }

            if (control.Info.GenerateOnScreen)
            {
                code = AppendOnScreenHead(AssembleControllerBase.GetOnScreenHeadHtml(control), code);
            }
            return code;
        }

        public static string GetProcessedCodeBehind(ControlInfo control)
        {
            return control.CodeBehind;
        }

        public static string EliminateIndent(string code)
        {
            return code.Replace("\n\t\t", "\n");
        }

        public static string AppendIndent(string code)
        {
            return code.Replace("\n", "\n\t\t");
        }
        
        public static string GetInitialCodeBehind(string code)
        {
            var sb = new StringBuilder();
            var result = code;
            bool isCSharp;
            var userNameSpaces = CutNamespaceDefinitionsFromCode(ref result, out isCSharp);
            var pattern = !isCSharp ? @"[^\w]class[^\w][^\n]+\n" : @"[^\w]class[^\w][^{]+{[^\n]*\n";
            var m = Regex.Match(result, pattern, StandardRegexOptions);
            if (m.Success)
            {
                result = result.Substring(m.Index + m.Value.Length);
                var newPattern = !isCSharp ? @"end class.+$" : @"}[^}]+}[^}]*$";
                result = Regex.Replace(result, newPattern, "");
            }
            sb.Append(AssembleControllerBase.GetUsingNamespaces(isCSharp, userNameSpaces));
            sb.Append(result);
            return EliminateIndent(sb.ToString());

        }

        public static string GetInitialPresentation(string code)
        {
            var pattern = "<%@ Register TagPrefix=\"qp\" [^\\n]+\\n";
            var m = Regex.Match(code, pattern, StandardRegexOptions);
            var result = code.Substring(m.Index + m.Value.Length);
            return result;
        }

        public static string Preprocess(string code)
        {
            code = Regex.Replace(code, "<%[ \\s]*\\=upload_url[ \\s]*%>", "<%# upload_url %>", StandardRegexOptions);
            code = code.Replace("\n", "\r\n");
            code = code.Replace("\r\r\n", "\r\n");
            return code;
        }
        internal static Hashtable CutNamespaceDefinitionsFromCode(ref string text)
        {
            bool isCSharp;
            return CutNamespaceDefinitionsFromCode(ref text, out isCSharp);
        }
        internal static Hashtable CutNamespaceDefinitionsFromCode(ref string text, out bool isCSharp)
        {
            var userNamespaces = new Hashtable();
            var testPattern = "\\s*using\\s+";
            isCSharp = Regex.IsMatch(text, testPattern);
            var pattern = isCSharp ? "\\s*using\\s+([^\\s;]+)\\s*;" : "\\s*Imports\\s+(\\S+)\\s?";
            var m = Regex.Match(text, pattern);
            while (m.Success)
            {
                var code = m.Groups[1].Value;
                if (!SystemNamespaces.Contains(code))
                {
                    if (!userNamespaces.Contains(code))
                    {
                        userNamespaces.Add(code, "");
                    }
                }
                m = m.NextMatch();
            }

            text = Regex.Replace(text, pattern, "");
            return userNamespaces;
        }
    }
}
