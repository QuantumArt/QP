using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Database;

namespace Quantumart.QPublishing.Helpers
{
    public class QpTrace
    {
        public string TraceString { get; set; }

        public int TraceId { get; set; }

        public string TraceStartText { get; set; }

        public DateTime TraceStartTime { get; set; }

        public int InitTrace(int pageId)
        {
            int functionReturnValue;
            var query = HttpContext.Current.Request.ServerVariables["QUERY_STRING"].Replace("'", "''");
            var traceSql = "select * from page_trace where query_string = '" + query + "' and page_id = " + pageId;
            var conn = new DBConnector();
            var dt = conn.GetRealData(traceSql);

            if (dt.Rows.Count == 0)
            {
                traceSql = "insert into page_trace(query_string, page_id, traced) values ('" + query + "', " + pageId + ", '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                functionReturnValue = conn.InsertDataWithIdentity(traceSql);
            }
            else
            {
                traceSql = "update page_trace set query_string = '" + query + "', page_id = " + pageId + ", traced = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' where page_id = " + pageId;
                conn.ProcessData(traceSql);

                traceSql = "select trace_id from page_trace where query_string = '" + query + "' and page_id = " + pageId;
                dt = conn.GetRealData(traceSql);
                functionReturnValue = dt.Rows.Count != 0 ? DBConnector.GetNumInt(dt.Rows[0]["trace_id"]) : 0;
            }

            return functionReturnValue;
        }

        public void DoneTrace(TimeSpan duration, bool allowUserSessions, Hashtable values)
        {
            var dp = new DebugPrint();
            var traceSession = allowUserSessions ? "" : dp.GetSessionString();

            var traceCookies = dp.GetCookiesString();
            var traceValues = dp.GetSimpleDictionaryString(ref values);

            var traceSql = "update page_trace set SESSION = '" + traceSession.Replace("'", "''") + "', COOKIES = '" + traceCookies.Replace("'", "''") + "', [VALUES] = '" + traceValues.Replace("'", "''") + "', DURATION = " + Math.Round(duration.TotalMilliseconds).ToString(CultureInfo.InvariantCulture) + " where TRACE_ID = " + TraceId;
            var conn = new DBConnector();

            conn.ProcessData(traceSql);
        }

        public void SaveTraceToDb(string trace, int traceId)
        {
            var conn = new DBConnector();
            conn.ProcessData("delete from page_trace_format where trace_id = " + TraceId);
        }

        public void ExtractFirstLine(ref string traceString, ref string trace)
        {
            const string divider = "<br>";
            if (trace.IndexOf(divider, StringComparison.Ordinal) > 0)
            {
                var substrLength = trace.IndexOf(divider, StringComparison.Ordinal) + divider.Length;
                traceString = Strings.Mid(trace, 1, substrLength);
                trace = Strings.Mid(trace, substrLength + 1, Strings.Len(trace) - substrLength);
            }
            else
            {
                throw new Exception("Cannot complete saving trace. Remainder: " + trace);
            }
        }

        public bool MatchLine(string line, string pattrn, ref Match firstMatch)
        {
            var regEx = new Regex(pattrn, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regEx.Matches(line);
            var functionReturnValue = matches.Count > 0;
            foreach (Match match in matches)
            {
                firstMatch = match;
            }

            return functionReturnValue;
        }

        public bool MatchesLine(string line, string pattrn, out MatchCollection firstMatch)
        {
            var regEx = new Regex(pattrn, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = regEx.Matches(line);
            var functionReturnValue = matches.Count > 0;
            firstMatch = matches;

            return functionReturnValue;
        }

        public int SaveLine(int traceId, int formatId, int parentId, int order, int traced, string defValuesString, string undefValuesString)
        {
            var conn = new DBConnector();
            var parent = parentId == 0 ? "NULL" : parentId.ToString();
            var traceSql = "insert into page_trace_format(parent_trace_format_id, format_id, number, duration, trace_id) values (" + parent + ", " + formatId + ", " + order + ", " + traced + ", " + traceId + ")";
            var id = conn.InsertDataWithIdentity(traceSql);
            var functionReturnValue = id;

            SaveDefValues(defValuesString, id);
            SaveUndefValues(undefValuesString, id);

            return functionReturnValue;
        }

        public void SaveTraceLines(string trace, int traceId, int parent)
        {
            var traceString = string.Empty;
            Match firstMatch = null;

            var order = 1;
            while (!string.IsNullOrEmpty(trace))
            {
                var found = false;
                var childLines = "";
                ExtractFirstLine(ref traceString, ref trace);

                if (MatchLine(traceString, "(?<depth>[\\d])-(?<fid>[\\d]+)Def:(?<def>.*?)Undef:(?<undef>.*?)[\\d\\w]+<br>", ref firstMatch))
                {
                    var currentLevel = firstMatch.Groups["depth"].ToString();
                    var formatId = firstMatch.Groups["fid"].ToString();
                    var defValuesString = firstMatch.Groups["def"].ToString();
                    var undefValuesString = firstMatch.Groups["undef"].ToString();
                    string traced;

                    if (MatchLine(traceString, "started<br>", ref firstMatch))
                    {
                        while (!found)
                        {
                            ExtractFirstLine(ref traceString, ref trace);
                            if (MatchLine(traceString, currentLevel + "-" + formatId + "-(?<dur>[\\d]+)", ref firstMatch))
                            {
                                traced = firstMatch.Groups["dur"].ToString();
                                found = true;
                                var traceFormatId = SaveLine(traceId, int.Parse(formatId), parent, order, int.Parse(traced), defValuesString, undefValuesString);
                                SaveTraceLines(childLines, traceId, traceFormatId);
                            }
                            else
                            {
                                childLines = childLines + traceString;
                            }
                        }
                    }
                    else
                    {
                        MatchLine(traceString, "(?<dur>[\\d]+)ms", ref firstMatch);
                        traced = firstMatch.Groups["dur"].ToString();
                        SaveLine(traceId, int.Parse(formatId), parent, order, int.Parse(traced), defValuesString, undefValuesString);
                    }
                }

                order = order + 1;
            }
        }

        public void SaveDefValues(string defValuesString, int traceFormatId)
        {
            MatchCollection matches;
            MatchesLine(defValuesString, "Value\\((?<key>.*?)\\)=(?<value>.*?);", out matches);
            var conn = new DBConnector();
            foreach (Match match in matches)
            {
                var key = match.Groups["key"].ToString().Replace("'", "''");
                var value = match.Groups["value"].ToString().Replace("'", "''");
                if (key.Length <= 50 && value.Length <= 255)
                {
                    var strSql = " INSERT INTO PAGE_TRACE_FORMAT_VALUES(trace_format_id, name, value, defined) ";
                    strSql = strSql + " VALUES(" + traceFormatId + ",'" + key + "', '" + value + "', " + 1 + ") ";
                    conn.ProcessData(strSql);
                }
            }
        }

        public void SaveUndefValues(string undefValuesString, int traceFormatId)
        {
            MatchCollection matches;
            MatchesLine(undefValuesString, "Value\\((?<key>.*?)\\);", out matches);
            var conn = new DBConnector();
            foreach (Match match in matches)
            {
                var key = match.Groups["key"].ToString().Replace("'", "''");
                if (key.Length <= 50)
                {
                    var strSql = " INSERT INTO PAGE_TRACE_FORMAT_VALUES(trace_format_id, name, value, defined) ";
                    strSql = strSql + " VALUES(" + traceFormatId + ", '" + key + "', NULL , " + 0 + ") ";
                    conn.ProcessData(strSql);
                }
            }
        }
    }
}
