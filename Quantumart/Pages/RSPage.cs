using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using Microsoft.VisualBasic;

namespace Quantumart.QPublishing.Pages
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteScriptingMethodAttribute : Attribute
    {
        public string Description { get; set; } = string.Empty;
    }

    // ReSharper disable once InconsistentNaming
    public abstract class RSPage : Page
    {
        private string _debugFilepath = "c:\\rs.html";
        private readonly string[] _months = { "", "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        protected string RsVersion = "1.0.8044";
        private int _uniqueIdentifier;
        private bool _writeDebugFile;

        private string _GetFooterHtml()
        {
            return "<br/><p class=\"heading3\"><a href=\"http://www.thycotic.com/dotnet_remotescripting.html\">Thycotic.Web.RemoteScripting " + Version + "</a><br/>Developed by Jonathan Cogley</p><br/><br/><p>Example code to create a Remote Scripting Method:</p> <div><pre>[RemoteScriptingMethod(Description=\"Converts text to uppercase.\")]" + Constants.vbLf + "public virtual string ToUpperCase(string s) " + Constants.vbLf + "{" + Constants.vbLf + Constants.vbTab + "return s.ToUpperInvariant();" + Constants.vbLf + "}" + Constants.vbLf + "</pre></div></body></html>";
        }

        private string _GetHeaderHtml()
        {
            var str = _GetPageName();
            return "<html><head><style type=\"text/css\">BODY { color: #000000; background-color: white; font-family: Verdana; margin-left: 0px; margin-top: 0px; }#content { margin-left: 30px; font-size: .70em; padding-bottom: 2em; }A:link { color: #336699; font-weight: bold; text-decoration: underline; }A:visited { color: #6699cc; font-weight: bold; text-decoration: underline; }A:active { color: #336699; font-weight: bold; text-decoration: underline; }A:hover { color: cc3300; font-weight: bold; text-decoration: underline; }P { color: #000000; margin-top: 0px; margin-bottom: 12px; font-family: Verdana; }pre { width: 500px; background-color: #e5e5cc; padding: 5px; font-family: Courier New; font-size: x-small; margin-top: -5px; border: 1px #f0f0e0 solid; }td { color: #000000; font-family: Verdana; font-size: .7em; }h2 { font-size: 1.5em; font-weight: bold; margin-top: 25px; margin-bottom: 10px; border-top: 1px solid #003366; margin-left: -15px; color: #003366; }h3 { font-size: 1.1em; color: #000000; margin-left: -15px; margin-top: 10px; margin-bottom: 10px; }ul, ol { margin-top: 10px; margin-left: 20px; }li { margin-top: 10px; color: #000000; }font.value { color: darkblue; font: bold; }font.key { color: darkgreen; font: bold; }.heading1 { color: #ffffff; font-family: Tahoma; font-size: 26px; font-weight: normal; background-color: #003366; margin-top: 0px; margin-bottom: 0px; margin-left: -30px; padding-top: 10px; padding-bottom: 3px; padding-left: 15px; width: 105%; }.heading2 { color: #ffffff; font-family: Tahoma; font-size: 16px; font-weight: bolder; background-color: #003366; margin-top: 0px; margin-bottom: 0px; margin-left: -30px; padding-top: 10px; padding-bottom: 3px; padding-left: 15px; width: 105%; }.heading3 { color: #000000; font-family: Tahoma; font-size: 12px; font-weight: bolder; background-color: #c0c0c0; margin-top: 0px; margin-bottom: 0px; margin-left: -30px; padding-top: 10px; padding-bottom: 10px; padding-left: 15px; width: 105%; }.button { background-color: #dcdcdc; font-family: Verdana; font-size: 1em; border-top: #cccccc 1px solid; border-bottom: #666666 1px solid; border-left: #cccccc 1px solid; border-right: #666666 1px solid; }.frmheader { color: #000000; background: #dcdcdc; font-family: Verdana; font-size: .7em; font-weight: normal; border-bottom: 1px solid #dcdcdc; padding-top: 2px; padding-bottom: 2px; }.frmtext { font-family: Verdana; font-size: .7em; margin-top: 8px; margin-bottom: 0px; margin-left: 32px; }.frmInput { font-family: Verdana; font-size: 1em; }.intro { margin-left: -15px; }</style><title>" + str + " Remote Scripting</title></head><body><div id=\"content\"><p class=\"heading1\">" + str + "</p><br>";
        }

        private string _GetNextVariableName()
        {
            return "_o" + Math.Max(Interlocked.Increment(ref _uniqueIdentifier), _uniqueIdentifier - 1);
        }

        private string _GetOperationsHtml()
        {
            var builder = new StringBuilder();
            builder.Append("<span><p class=\"intro\">The following operations are supported.</p>");
            var hashtable = _GetRSOperations();
            builder.Append("<ul>");
            foreach (string str in hashtable.Keys)
            {
                builder.Append("<li><a href=\"?op=" + str + "\">" + str + "</a>");
                var str2 = hashtable[str].ToString();
                if (!str2.Equals(""))
                {
                    builder.Append("<br><font color=\"#898989\">");
                    builder.Append(str2);
                    builder.Append("</font>");
                }
                builder.Append("</li>");
            }

            builder.Append("</ul></span>");
            builder.Append("<span><p class=\"intro\">Test obtaining a client-side proxy object to the server-side methods:</p>");
            builder.Append("<ul><li><a href=\"?op=GetServerProxy\">GetServerProxy</a></li></ul>");
            return builder.ToString();
        }

        private string _GetPageName()
        {
            var name = GetType().Name;
            if (name.EndsWith("_aspx"))
            {
                name = name.Substring(0, name.Length - 5);
            }

            return name;
        }

        private string _GetRequestInfoHtml()
        {
            var builder = new StringBuilder();
            builder.Append("<html><body TOPMARGIN=\"0\" LEFTMARGIN=\"0\">");
            builder.Append("<style>" + Constants.vbLf);
            builder.Append("BODY { background-color: #aa0000; color: #ffffff; font-family: verdana; font-size: 10pt; } " + Constants.vbLf);
            builder.Append(".xheading { font-family: verdana; font-size: 10pt; color: #FFFFFF; background-color: #DB1D28; font-weight: bolder; padding-left: 10px; padding-right: 5px; padding-top: 5px; padding-bottom: 5px; } " + Constants.vbLf);
            builder.Append(".xtable { padding-left: 2px; padding-right: 2px; padding-top: 2px; padding-bottom: 2px; border: 0.5pt solid #999999; background-color: #EFEFEF; font-family: verdana; font-size: 10pt; color: #000000; width: 100%; }" + Constants.vbLf);
            builder.Append(".xdiv { padding-left: 20px; } " + Constants.vbLf);
            builder.Append(".xrowodd { background-color: #E2E2E2; } " + Constants.vbLf);
            builder.Append(".xroweven { background-color: #F2F2F2; } " + Constants.vbLf);
            builder.Append(".xkey { padding-left: 10px; font-weight: bolder; } " + Constants.vbLf);
            builder.Append(".xvalue { padding-left: 10px; font-size: 8pt; } " + Constants.vbLf);
            builder.Append("</style>" + Constants.vbLf);
            builder.Append("<table WIDTH=\"100%\" CELLPADDING=\"0\" CELLSPACING=\"0\"><tr><td>");
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td CLASS=\"xheading\"><a NAME=\"Params\">RemoteScriptingPage</td></tr>");
            builder.Append("<tr CLASS=\"xroweven\" VALIGN=\"top\"><td CLASS=\"xvalue\">");
            builder.Append("<pre>");

            try
            {
                builder.Append(ToString());
            }
            catch (Exception exception)
            {
                builder.Append("Failed to retrieve exception information: " + exception.Message);
            }

            builder.Append("<pre>");
            builder.Append("</td></tr>");
            builder.Append("</table>");

            var request = HttpContext.Current.Request;
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td COLSPAN=\"2\" CLASS=\"xheading\"><a NAME=\"Params\">HttpRequest.Params</td></tr>");

            var params1 = request.Params;
            var allKeys = params1.AllKeys;
            var length = allKeys.Length;
            for (var i = 0; i <= length - 1; i++)
            {
                builder.Append("<tr ");
                builder.Append(Math.Round(new decimal(i / 2), 0) * 2m == i ? "CLASS=\"xroweven\"" : "CLASS=\"xrowodd\"");
                builder.Append(" VALIGN=\"top\"><td CLASS=\"xkey\" WIDTH=\"30%\">");
                builder.Append(allKeys[i]);
                builder.Append("</td><td CLASS=\"xvalue\" WIDTH=\"70%\">");
                builder.Append(params1.Get(allKeys[i]));
                builder.Append("</td></tr>");
            }

            if (length == 0)
            {
                builder.Append("<tr CLASS=\"xrowodd\"><td CLASS=\"xkey\">Empty</td></tr>");
            }

            builder.Append("</table>");
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td COLSPAN=\"2\" CLASS=\"xheading\"><a NAME=\"QueryString\">HttpRequest.QueryString</td></tr>");
            params1 = request.QueryString;
            allKeys = params1.AllKeys;
            length = allKeys.Length;
            for (var j = 0; j <= length - 1; j++)
            {
                builder.Append("<tr ");
                builder.Append(Math.Round(new decimal(j / 2), 0) * 2m == j ? "CLASS=\"xroweven\"" : "CLASS=\"xrowodd\"");
                builder.Append(" VALIGN=\"top\"><td CLASS=\"xkey\" WIDTH=\"30%\">");
                builder.Append(allKeys[j]);
                builder.Append("</td><td CLASS=\"xvalue\" WIDTH=\"70%\">");
                builder.Append(params1.Get(allKeys[j]));
                builder.Append("</td></tr>");
            }

            if (length == 0)
            {
                builder.Append("<tr CLASS=\"xrowodd\"><td CLASS=\"xkey\">Empty</td></tr>");
            }

            builder.Append("</table>");
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td COLSPAN=\"2\" CLASS=\"xheading\"><a NAME=\"Form\">HttpRequest.Form</td></tr>");
            params1 = request.Form;
            allKeys = params1.AllKeys;
            length = allKeys.Length;
            for (var k = 0; k <= length - 1; k++)
            {
                builder.Append("<tr ");
                builder.Append(Math.Round(new decimal(k / 2), 0) * 2m == k ? "CLASS=\"xroweven\"" : "CLASS=\"xrowodd\"");
                builder.Append(" VALIGN=\"top\"><td CLASS=\"xkey\" WIDTH=\"30%\">");
                builder.Append(allKeys[k]);
                builder.Append("</td><td CLASS=\"xvalue\" WIDTH=\"70%\">");
                builder.Append(params1.Get(allKeys[k]));
                builder.Append("</td></tr>");
            }

            if (length == 0)
            {
                builder.Append("<tr CLASS=\"xrowodd\"><td CLASS=\"xkey\">Empty</td></tr>");
            }

            builder.Append("</table>");
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td COLSPAN=\"2\" CLASS=\"xheading\"><a NAME=\"Cookies\">HttpRequest.Cookies</td></tr>");

            var cookies = request.Cookies;
            allKeys = cookies.AllKeys;
            length = allKeys.Length;
            for (var m = 0; m <= length - 1; m++)
            {
                builder.Append("<tr ");
                builder.Append(Math.Round(new decimal(m / 2), 0) * 2m == m ? "CLASS=\"xroweven\"" : "CLASS=\"xrowodd\"");
                builder.Append(" VALIGN=\"top\"><td CLASS=\"xkey\" WIDTH=\"30%\">");
                builder.Append(allKeys[m]);
                builder.Append("</td><td CLASS=\"xvalue\" WIDTH=\"70%\">");
                var cookie = cookies.Get(allKeys[m]);
                if (cookie?.HasKeys ?? false)
                {
                    var values = cookie.Values;
                    foreach (string str in values)
                    {
                        builder.Append(str);
                        builder.Append("=");
                        builder.Append(values.Get(str));
                        builder.Append("<br>");
                    }
                }
                else
                {
                    if (cookie != null)
                    {
                        builder.Append(cookie.Value);
                    }
                }

                builder.Append("</td></tr>");
            }

            if (length == 0)
            {
                builder.Append("<tr CLASS=\"xrowodd\"><td CLASS=\"xkey\">Empty</td></tr>");
            }

            builder.Append("</table>");
            builder.Append("<table CLASS=\"xtable\" CELLSPACING=\"0\">");
            builder.Append("<tr><td COLSPAN=\"2\" CLASS=\"xheading\"><a NAME=\"ServerVariables\">HttpRequest.ServerVariables</td></tr>");
            params1 = request.ServerVariables;
            allKeys = params1.AllKeys;
            length = allKeys.Length;
            for (var n = 0; n <= length - 1; n++)
            {
                builder.Append("<tr ");
                builder.Append(Math.Round(new decimal(n / 2), 0) * 2m == n ? "CLASS=\"xroweven\"" : "CLASS=\"xrowodd\"");
                builder.Append(" VALIGN=\"top\"><td CLASS=\"xkey\" WIDTH=\"30%\">");
                builder.Append(allKeys[n]);
                builder.Append("</td><td CLASS=\"xvalue\" WIDTH=\"70%\">");
                builder.Append(params1.Get(allKeys[n]));
                builder.Append("</td></tr>");
            }

            if (length == 0)
            {
                builder.Append("<tr CLASS=\"xrowodd\"><td CLASS=\"xkey\">Empty</td></tr>");
            }

            builder.Append("</table>");
            builder.Append("</td></tr></table>");
            builder.Append("</body></html>");
            return builder.ToString();
        }

        private Hashtable _GetRSOperations()
        {
            var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
            var hashtable = new Hashtable(methods.Length);
            var keys = new string[methods.Length];
            for (var i = 0; i <= methods.Length - 1; i++)
            {
                keys[i] = methods[i].Name;
            }

            Array.Sort(keys, methods);
            StringBuilder builder = null;
            for (var j = 0; j <= methods.Length - 1; j++)
            {
                var customAttributes = methods[j].GetCustomAttributes(false);
                if (customAttributes.Length > 0)
                {
                    var flag = false;
                    var description = "";
                    for (var k = 0; k <= customAttributes.Length - 1; k++)
                    {
                        var attribute = customAttributes[k] as RemoteScriptingMethodAttribute;
                        if (attribute != null)
                        {
                            description = attribute.Description;
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        var parameters = methods[j].GetParameters();
                        for (var m = 0; m <= parameters.Length - 1; m++)
                        {
                            if (!parameters[m].ParameterType.Name.Equals("String"))
                            {
                                if (builder == null)
                                {
                                    builder = new StringBuilder();
                                }
                                builder.Append("Parameter \"" + parameters[m].Name + "\" in method \"" + methods[j].Name + "\" must be of type string. ");
                            }
                        }

                        hashtable[methods[j].Name] = description;
                    }
                }
            }

            if (builder != null && builder.Length > 0)
            {
                throw new Exception("Remote Scripting methods can only accept parameters of type string as input. " + builder);
            }

            return hashtable;
        }

        private void _GetServerProxy()
        {
            try
            {
                var builder = new StringBuilder();
                builder.Append("var undefined; ");

                var str = _GetNextVariableName();
                var hashtable = _GetRSOperations();
                var hashtable2 = new Hashtable();
                foreach (string str2 in hashtable.Keys)
                {
                    var str3 = _GetNextVariableName();
                    builder.Append("function ");
                    builder.Append(str3);
                    builder.Append("() { return 0; }");
                    hashtable2[str2] = str3;
                }

                builder.Append("var ");
                builder.Append(str);
                builder.Append(" = new Object;");
                foreach (string str4 in hashtable2.Keys)
                {
                    builder.Append(str);
                    builder.Append("[\"");
                    builder.Append(str4);
                    builder.Append("\"]");
                    builder.Append(" = ");
                    builder.Append(hashtable2[str4]);
                    builder.Append(";");
                }

                builder.Append(str);
                Response.Write("<METHOD VERSION=\"" + RsVersion + "\"><RETURN_VALUE TYPE=EVAL_OBJECT>" + builder + "</RETURN_VALUE></METHOD>");
            }
            catch (Exception exception)
            {
                _HandleException("Could not create the object proxy. ", exception);
            }
        }

        private void _HandleException(string message, Exception e)
        {
            Response.Write("<METHOD VERSION=\"" + RsVersion + "\"><RETURN_VALUE TYPE=ERROR>" + message + Constants.vbLf);
            if (e != null)
            {
                Response.Write(e.ToString());
            }

            Response.Write("</RETURN_VALUE></METHOD>");
        }

        private void _HandleTestException(string message, Exception e = null)
        {
            Response.Write("<p style=\"font-weight: bolder; font-family: verdana; font-size: 10pt; color: #ff0000\">");
            Response.Write(message);
            if (e != null)
            {
                Response.Write("<br><pre>");
                Response.Write(e.ToString());
                Response.Write("</pre>");
            }

            Response.Write("</p>");
        }

        private void _InvokeMethod(string methodName, object[] parameters)
        {
            try
            {
                if (!_GetRSOperations().Contains(methodName))
                {
                    throw new Exception("There is no method matching \"" + methodName + "\" to run. Methods must be marked with the [RemoteScriptingMethod] attribute to be accessible.");
                }

                var o = GetType().InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, this, parameters);
                if (o is string)
                {
                    Response.Write(string.Concat("<METHOD VERSION=\"", RsVersion, "\"><RETURN_VALUE TYPE=SIMPLE>", o, "</RETURN_VALUE></METHOD>"));
                }
                else
                {
                    Response.Write("<METHOD VERSION=\"" + RsVersion + "\"><RETURN_VALUE TYPE=EVAL_OBJECT>" + _UnEval(o) + "</RETURN_VALUE></METHOD>");
                }
            }
            catch (Exception exception)
            {
                _HandleException(string.Concat("Could not run the specified method \"", methodName, "\" with ", parameters.Length, " parameter(s)."), exception);
            }
        }

        private void _RunRemoteScriptingEngine()
        {
            var request = HttpContext.Current.Request;
            if (request["pcount"] == null)
            {
                var flag = false;
                if (request["test"] != null)
                {
                    _ShowOperationResultPage();
                    flag = true;
                }

                if (!flag && request["op"] != null)
                {
                    if (request["op"].Equals("GetServerProxy"))
                    {
                        Response.Write(_GetHeaderHtml());
                        _TestInvokeOperation("_GetServerProxy", new object[-1 + 1]);
                        Response.Write(_GetFooterHtml());
                        flag = true;
                    }
                    else
                    {
                        _ShowOperationOptionsPage();
                        flag = true;
                    }
                }

                if (!flag)
                {
                    _ShowOperationsPage();
                }
            }
            else
            {
                if (_writeDebugFile)
                {
                    try
                    {
                        var writer = File.CreateText(_debugFilepath);
                        writer.Write(_GetRequestInfoHtml());
                        writer.Close();
                    }
                    catch (Exception exception)
                    {
                        _HandleException("Failed to write the Remote Scripting debug file.", exception);
                        return;
                    }
                }

                var methodName = string.Empty;
                var parameters = new object[-1 + 1];
                try
                {
                    methodName = request["_method"];
                    var s = request["pcount"];
                    parameters = new object[int.Parse(s)];
                    for (var i = 0; i <= parameters.Length - 1; i++)
                    {
                        parameters[i] = request["p" + i];
                    }
                }
                catch (Exception exception2)
                {
                    _HandleException("Failed to correctly parse the Remote Scripting parameters.", exception2);
                }

                if (methodName.Equals("GetServerProxy"))
                {
                    _GetServerProxy();
                }
                else
                {
                    _InvokeMethod(methodName, parameters);
                }
            }
        }

        private void _ShowOperationOptionsPage()
        {
            var request = HttpContext.Current.Request;
            Response.Write(_GetHeaderHtml());

            var name = request["op"];
            var method = GetType().GetMethod(name);
            if (method == null)
            {
                _HandleTestException("Could not find the operation \"" + name + "\".");
            }
            else
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    _TestInvokeOperation(name, new object[-1 + 1]);
                }
                else
                {
                    Response.Write("Click <a href=\"" + request.ServerVariables["SCRIPT_NAME"] + "\">here</a> for a complete list of operations.<br><br>");
                    Response.Write("Please enter the following parameters to test operation \"<b>" + name + "</b>\"<br>");
                    Response.Write("<form METHOD=\"GET\" ACTION=\"?op=" + name + "&test=true\">");
                    Response.Write("<input TYPE=\"HIDDEN\" NAME=\"op\" VALUE=\"" + name + "\">");
                    Response.Write("<input TYPE=\"HIDDEN\" NAME=\"test\" VALUE=\"true\">");
                    Response.Write("<input TYPE=\"HIDDEN\" NAME=\"tcount\" VALUE=\"" + parameters.Length + "\">");
                    Response.Write("<table>");
                    for (var i = 0; i <= parameters.Length - 1; i++)
                    {
                        Response.Write(string.Concat("<tr><td><b>", parameters[i].Name, "</b></td><td><input TYPE=\"TEXT\" NAME=\"t", i, "\" VALUE=\"\"></td></tr>"));
                    }
                    Response.Write("</table><br>");
                    Response.Write("<input TYPE=\"SUBMIT\" VALUE=\"Test the operation\">");
                    Response.Write("<br><br><a href=\"" + request.ServerVariables["SCRIPT_NAME"] + "\">Back</a><br>");
                }

                Response.Write(_GetFooterHtml());
            }
        }

        private void _ShowOperationResultPage()
        {
            var request = HttpContext.Current.Request;
            Response.Write(_GetHeaderHtml());

            var name = request["op]"];
            var method = GetType().GetMethod(name);
            if (method == null)
            {
                _HandleTestException("Could not find the operation \"" + name + "\".");
            }
            else
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    _TestInvokeOperation(name, new object[-1 + 1]);
                }
                else
                {
                    var strArray = new object[-1 + 1];
                    try
                    {
                        var s = request["tcount"];
                        strArray = new object[int.Parse(s)];
                        for (var i = 0; i <= parameters.Length - 1; i++)
                        {
                            strArray[i] = request["t" + i];
                        }
                    }
                    catch (Exception exception)
                    {
                        _HandleException("Failed to correctly parse the Remote Scripting parameters.", exception);
                    }

                    _TestInvokeOperation(name, strArray);
                }

                Response.Write(_GetFooterHtml());
            }
        }

        private void _ShowOperationsPage()
        {
            Response.Write(_GetHeaderHtml());
            Response.Write(_GetOperationsHtml());
            Response.Write(_GetFooterHtml());
        }

        private void _TestInvokeOperation(string methodName, object[] parameters)
        {
            if (methodName.Equals("_GetServerProxy"))
            {
                Response.Write("Test generating client-side proxy object<br><br>");
            }
            else
            {
                Response.Write("Testing operation \"<b>" + methodName + "</b>\"<br><br>");
            }

            Response.Write("<textarea rows=\"12\" cols=\"70\">");
            if (methodName.Equals("_GetServerProxy"))
            {
                _GetServerProxy();
            }
            else
            {
                _InvokeMethod(methodName, parameters);
            }

            Response.Write("</textarea>");
            var request = HttpContext.Current.Request;
            Response.Write("<br><br><a href=\"");
            Response.Write(parameters.Length > 0 ? "javascript:window.history.back()" : request.ServerVariables["SCRIPT_NAME"]);
            Response.Write("\">Back</a><br>");
        }

        private static string _ToTwoDigits(int i)
        {
            if (i < 10)
            {
                return "0" + i;
            }

            return i.ToString();
        }

        private string _UnEval(object o)
        {
            if (o is bool)
            {
                return o.ToString().ToLowerInvariant();
            }

            if (o is int || o is long || o is double || o is float)
            {
                return o.ToString();
            }

            var array1 = o as Array;
            if (array1 != null)
            {
                var array = array1;
                var builder = new StringBuilder();
                builder.Append("new Array(");
                var length = array.Length;
                for (var i = 0; i <= length - 1; i++)
                {
                    builder.Append(_UnEval(array.GetValue(i)));
                    if (i < length - 1)
                    {
                        builder.Append(",");
                    }
                }

                builder.Append(")");
                return builder.ToString();
            }

            if (o is DateTime)
            {
                var builder2 = new StringBuilder();
                builder2.Append("new Date(Date.parse(\"");
                var time = (DateTime)o;
                builder2.Append(GetShortenedDayOfWeek(time.DayOfWeek));
                builder2.Append(' ');
                builder2.Append(_months[time.Month]);
                builder2.Append(' ');
                builder2.Append(time.Day);
                builder2.Append(' ');
                builder2.Append(time.Hour);
                builder2.Append(':');
                builder2.Append(_ToTwoDigits(time.Minute));
                builder2.Append(':');
                builder2.Append(_ToTwoDigits(time.Second));
                builder2.Append(' ');
                builder2.Append(GetAbbreviatedTimeZone(TimeZone.CurrentTimeZone.StandardName));
                builder2.Append(' ');
                builder2.Append(time.Year);
                builder2.Append("\"));");
                return builder2.ToString();
            }

            return "\"" + o + "\"";
        }

        private static string GetAbbreviatedTimeZone(string s)
        {
            var matchs = new Regex("\\s*([A-Z]{1})\\w+").Matches(s);
            var builder = new StringBuilder(matchs.Count);
            for (var i = 0; i <= matchs.Count - 1; i++)
            {
                if (matchs[i].Groups.Count == 2)
                {
                    builder.Append(matchs[i].Groups[1].Value);
                }
            }

            return builder.ToString();
        }

        private static string GetShortenedDayOfWeek(DayOfWeek dayOfWeek1)
        {
            switch (dayOfWeek1)
            {
                case DayOfWeek.Sunday:

                    return "Sun";
                case DayOfWeek.Monday:

                    return "Mon";
                case DayOfWeek.Tuesday:

                    return "Tues";
                case DayOfWeek.Wednesday:

                    return "Weds";
                case DayOfWeek.Thursday:

                    return "Thurs";
                case DayOfWeek.Friday:

                    return "Fri";
                case DayOfWeek.Saturday:
                    return "Sat";
            }

            return "Mon";
        }

        private void InitializeComponent()
        {
            Load += Page_Load;
        }

        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        private void Page_Load(object sender, EventArgs e)
        {
            _RunRemoteScriptingEngine();
        }

        public virtual string DebugFilePath
        {
            get { return _debugFilepath; }
            set { _debugFilepath = value; }
        }

        protected virtual string RemoteScriptingVersion
        {
            get { return RsVersion; }
            set { RsVersion = value; }
        }

        public double Version { get; } = 0.9;

        public virtual bool WriteDebugFile
        {
            get { return _writeDebugFile; }
            set { _writeDebugFile = value; }
        }
    }
}
