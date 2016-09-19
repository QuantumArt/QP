using System.Linq;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static int[] ToIntArray(this string input)
        {
            return string.IsNullOrEmpty(input) ? null : input.Split(",".ToCharArray()).Select(int.Parse).ToArray();
        }
    }
}
