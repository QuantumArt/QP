using System.Security.Cryptography;
using System.Text;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal class HashHelpers
    {
        internal static string CalculateMd5Hash(string input)
        {
            Ensure.Argument.NotNullOrEmpty(input);

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
