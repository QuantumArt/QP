using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditorConfig
{
    public class AspellCheckViewModel
    {        
        public AspellCheckViewModel(string inputs)
        {
            _textInputs = inputs;

            var requestText = new StringBuilder();
            string text = System.Web.HttpUtility.UrlDecode(_textInputs);
            string pattern = "<[^>]+>";
            string replacement = " ";
            Regex rgx = new Regex(pattern);
            text = rgx.Replace(text, replacement);
            var lines = text.Split(new char[] { '\n' }, StringSplitOptions.None);
            
            requestText.AppendLine("%");
		    requestText.AppendLine("^A");
		    requestText.AppendLine("!");
		    foreach(var line in lines) 
            {
			    requestText.AppendFormat("^{0}\n", line);
		    }

            var response = sendRequest(requestText.ToString());

            var responseLines = response.Split(new char[] { '\n' });

            int index = 0;
            foreach (string respLine in responseLines)
            {
                if (!string.IsNullOrEmpty(respLine))
                {
                    string chardesc = respLine.Substring(0, 1);

                    if (chardesc == "&" || chardesc == "#")
                    {
                        var respLineSplit = respLine.Split(new char[] { ' ' }, 5);
                        print_words_elem(respLineSplit[1], index);

                        string[] suggs;

                        if (respLineSplit.Length >= 5 && !string.IsNullOrEmpty(respLineSplit[4]))
                        {
                            suggs = respLineSplit[4].Split(new string[] { ", " }, StringSplitOptions.None);
                        }
                        else
                            suggs = new string[] { };

                        print_suggs_elem(suggs, index);
                        index++;
                    }
                }
            }
        }

        private string escape_quote(string input)
        {
            string pattern = "/'/";
            string replacement = "\\'";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(input, replacement);
        }

        private void print_suggs_elem(string[] suggs, int index)
        {
            _checkerResults.AppendFormat("suggs[0][{0}] = [{1}];\n",
                                        index,
                                        string.Join(", ", suggs.Select(x => string.Format("'{0}'", x)))
                                        );
        }

        private string sendRequest(string text)
        {
            string url = "http://speller.yandex.net/services/yspell?options=4&lang=ru,en&mode=html";

            byte[] message = Encoding.UTF8.GetBytes(text);
            
            var req = HttpWebRequest.Create(url);
            req.Method = "POST";
            req.ContentType = @"text/plain;charset=""UTF-8""";            
            req.ContentLength = message.Length;
            
            Stream dataStream = req.GetRequestStream();            

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(message, 0, message.Length);
                reqStream.Close();
            }

            var resp = req.GetResponse();

            var strmReader = new System.IO.StreamReader(resp.GetResponseStream());
            return strmReader.ReadToEnd().Trim();
        }
        
        private void print_words_elem( string word, int index)
        {
            _checkerResults.AppendFormat("words[0][{0}] = '{1}';\n", index, escape_quote(word));
        }

        private string _textInputs;

        public MvcHtmlString TextInputsVar { get { return MvcHtmlString.Create(_textInputs); } }

        private StringBuilder _checkerResults = new StringBuilder();

        public MvcHtmlString CheckerResults { get { return MvcHtmlString.Create(_checkerResults.ToString()); } }
    }
}