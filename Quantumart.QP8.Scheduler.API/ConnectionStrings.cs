using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.Scheduler.API
{
    public class ConnectionStrings : IConnectionStrings
    {
        private const string ExceptCustomerCodesKey = "ExceptCustomerCodes";
        private readonly ServiceDescriptor _descriptor;

        public ConnectionStrings(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IEnumerator<string> GetEnumerator()
        {
            var exceptCodes = GetexceptCustomerCodes();
            return QPConfiguration.ConfigConnectionStrings(_descriptor.Name, exceptCodes).GetEnumerator();
        }

        private static IEnumerable<string> GetexceptCustomerCodes()
        {
            var codes = ConfigurationManager.AppSettings[ExceptCustomerCodesKey];
            return codes?.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
