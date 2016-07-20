using System;
using System.Security.Cryptography;
using System.Text;

namespace qp8dbupdate.Infrastructure.Versioning
{
    public class DbUpdateLogEntry
    {
        public DateTime Applied { get; set; }

        public string FileName { get; set; }

        public int UserId { get; set; }

        public string Body { get; set; }

        public string Version { get; set; }

        public string GetHash()
        {
            if (string.IsNullOrEmpty(Body))
            {
                throw new NullReferenceException("Body");
            }

            return CalculateMd5Hash(Body);
        }

        private static string CalculateMd5Hash(string input)
        {
            var md5 = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
