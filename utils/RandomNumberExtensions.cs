using System;
using System.Security.Cryptography;

namespace Quantumart.QP8.Utils
{
    public static class RandomNumberExtensions
    {
        private const int Integer32Size = 4;

        public static int Next(this RandomNumberGenerator generator)
        {          
            var randomUnsignedInteger32Bytes = new byte[Integer32Size];
            generator.GetBytes(randomUnsignedInteger32Bytes);
            return BitConverter.ToInt32(randomUnsignedInteger32Bytes, 0);
        }

        public static int Next(this RandomNumberGenerator generator, int maxValue)
        {
           double sample = generator.Next() * (1.0 / int.MaxValue);
           return (int)(sample * maxValue);
        }
    }
}
