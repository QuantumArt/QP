using System.IO;
using System.Text;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Csv
{
    public class CustomStreamReader : StreamReader
    {
        private readonly string _lineDelimiter;
        private readonly char _fieldDelimiter;

        public CustomStreamReader(Stream stream, Encoding encoding, string lineDelimiter, char fieldDelimiter)
            : base(stream, encoding)
        {
            _lineDelimiter = lineDelimiter;
            _fieldDelimiter = fieldDelimiter;
        }

        private bool IsEmpty(string line)
        {
            string res = line.Trim(_fieldDelimiter, '"', '\r', '\n');
            return string.IsNullOrEmpty(res) || string.IsNullOrWhiteSpace(line);
        }

        public override string ReadLine()
        {
            int c = Read();
            if (c == -1)
            {
                return null;
            }

            StringBuilder sb = new();
            char lastCh = char.MinValue;
            bool quoteOpen = false;
            do
            {
                if ((char)c == '"' && !quoteOpen)
                {
                    quoteOpen = true;
                }
                else if ((char)c == '"' && quoteOpen)
                {
                    quoteOpen = false;
                }

                char[] lineSepArr = _lineDelimiter.ToCharArray();
                char sep = lineSepArr[0];
                if (lineSepArr.Length == 2)
                {
                    sep = lineSepArr[1];
                }

                char ch = (char)c;
                if (ch == sep && !quoteOpen && (lineSepArr.Length == 1 || (lineSepArr.Length == 2 && lastCh == lineSepArr[0])))
                {
                    if (!IsEmpty(sb.ToString()))
                    {
                        return sb.ToString();
                    }

                    _ = sb.Remove(0, sb.Length);
                }
                else
                {
                    _ = sb.Append(ch);
                }

                lastCh = (char)c;
            } while ((c = Read()) != -1);

            return sb.ToString();
        }
    }
}
