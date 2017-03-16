using System.Security.Cryptography;
using System.Text;
using QP8.Infrastucture;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    internal static class HashHelpers
    {
        internal static string CalculateMd5Hash(string input)
        {
            Ensure.Argument.NotNullOrWhiteSpace(input);

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
