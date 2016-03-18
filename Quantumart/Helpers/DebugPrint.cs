using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;
using Microsoft.VisualBasic;

namespace Quantumart.QPublishing.Helpers
{

    public class DebugPrint
    {

        public string GetSessionString()
        {
            var result = new StringBuilder();
            foreach (string key in HttpContext.Current.Session.Contents) {
                result.Append(GetElementString(key, HttpContext.Current.Session[key]));
            }
            return result.ToString();
        }

        public string GetElementString(string key, object value)
        {
            var name = "";
            var typeIndex = (int)Information.VarType(value);
            var result = key + "=";
            //If typeIndex <> VariantType.Object Then
            if (typeIndex >= 0 && typeIndex <= 8 || typeIndex == 11) {
                result = result + value;
            }
            else {
                try {
                    name = Information.TypeName(value);
                }
                catch (Exception ex) {
                    var errorMessage =
                        $"{"DebugPrint.cs, GetElementString(string key, object value)"}, MESSAGE: {ex.Message} STACK TRACE: {ex.StackTrace}";
                    System.Diagnostics.EventLog.WriteEntry("Application", errorMessage);
                }

                if (string.IsNullOrEmpty(name)) name = "Object"; 
                result = result + "{" + name + "}";
            }
            result = result + "; ";
            return result;
        }

        public string GetCookiesString()
        {
            var result = new StringBuilder();

            foreach (string key in HttpContext.Current.Request.Cookies) {
                result.Append(key + ": ");
                var httpCookie = HttpContext.Current.Request.Cookies[key];
                if (httpCookie != null && httpCookie.HasKeys) {
                    var cookie = HttpContext.Current.Request.Cookies[key];
                    if (cookie != null)
                    {
                        var subCookieValues = new NameValueCollection(cookie.Values);
                        foreach (string subkey in subCookieValues) {
                            result.Append(subkey + "=" + cookie[subkey] + "; ");
                        }
                    }
                    result.Append("<br>");
                }
                else
                {
                    var cookie = HttpContext.Current.Request.Cookies[key];
                    if (cookie != null)
                        result.Append(key + "=" + cookie.Value + ";<br>");
                }
            }
            return result.ToString();
        }

        public string GetSimpleDictionaryString(ref Hashtable values)
        {
            var result = new StringBuilder();
            foreach (string key in values.Keys) {
                result.Append(GetElementString(key, values[key]));
            }
            result.Append("<br>");
            return result.ToString();
        }


    }
}
