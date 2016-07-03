using System;
using System.Security.Cryptography;
using System.Text;

namespace qp8dbupdate.Versioning
{
    public class DBUpdateLogEntry
    {
        public DateTime Applied { get; set; }
        public string FileName { get; set; }
        public int UserId { get; set; }
        public string Body { get; set; }
        public string Version { get; set; }

        public string GetHash()
        {
            if (string.IsNullOrEmpty(Body))
                throw new ArgumentNullException("Body");

            return CalculateMD5Hash(Body);
        }

        static string CalculateMD5Hash(string input)
        {

            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);


            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
