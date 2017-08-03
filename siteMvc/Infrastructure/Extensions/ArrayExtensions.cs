using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Extensions
{
    public static class ArrayExtensions
    {
        public static string ConvertToString(this byte[] vals) => BitConverter.ToString(vals).Replace("-", string.Empty);
    }
}
