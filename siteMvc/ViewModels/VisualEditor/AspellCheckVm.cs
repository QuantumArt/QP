using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class AspellCheckVm
    {
        private readonly string _textInputs;

        private readonly StringBuilder _checkerResults = new StringBuilder();

        public MvcHtmlString TextInputsVar => MvcHtmlString.Create(_textInputs);

        public MvcHtmlString CheckerResults => MvcHtmlString.Create(_checkerResults.ToString());

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public AspellCheckVm(string inputs)
        {
            _textInputs = inputs;

            var rgx = new Regex("<[^>]+>");
            var text = rgx.Replace(HttpUtility.UrlDecode(_textInputs), " ");
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.None);

            var requestText = new StringBuilder();
            requestText.AppendLine("%");
            requestText.AppendLine("^A");
            requestText.AppendLine("!");
            foreach (var line in lines)
            {
                requestText.AppendFormat("^{0}\n", line);
            }

            var response = SendRequest(requestText.ToString());
            var responseLines = response.Split('\n');
            var index = 0;
            foreach (var respLine in responseLines.Where(rl => !string.IsNullOrEmpty(rl)))
            {
                var chardesc = respLine.Substring(0, 1);
                if (chardesc == "&" || chardesc == "#")
                {
                    var respLineSplit = respLine.Split(new[] { ' ' }, 5);
                    print_words_elem(respLineSplit[1], index);

                    string[] suggs;
                    if (respLineSplit.Length >= 5 && !string.IsNullOrEmpty(respLineSplit[4]))
                    {
                        suggs = respLineSplit[4].Split(new[] { ", " }, StringSplitOptions.None);
                    }
                    else
                    {
                        suggs = new string[] { };
                    }

                    print_suggs_elem(suggs, index);
                    index++;
                }
            }
        }

        private void print_suggs_elem(IEnumerable<string> suggs, int index)
        {
            _checkerResults.AppendFormat("suggs[0][{0}] = [{1}];\n", index, string.Join(", ", suggs.Select(x => $"'{x}'")));
        }

        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        private static string SendRequest(string text)
        {
            var message = Encoding.UTF8.GetBytes(text);
            var req = WebRequest.Create("http://speller.yandex.net/services/yspell?options=4&lang=ru,en&mode=html");
            req.Method = "POST";
            req.ContentType = @"text/plain;charset=""UTF-8""";
            req.ContentLength = message.Length;

            using (var reqStream = req.GetRequestStream())
            {
                reqStream.Write(message, 0, message.Length);
                reqStream.Close();
            }

            var resp = req.GetResponse();
            var strmReader = new StreamReader(resp.GetResponseStream());
            return strmReader.ReadToEnd().Trim();
        }

        private void print_words_elem(string word, int index)
        {
            _checkerResults.AppendFormat("words[0][{0}] = '{1}';\n", index, EscapeQuote(word));
        }

        private static string EscapeQuote(string input)
        {
            return new Regex("/'/").Replace(input, "\\'");
        }
    }
}
