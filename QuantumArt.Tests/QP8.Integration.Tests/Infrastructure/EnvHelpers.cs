using System;
using NUnit.Framework;

namespace QP8.Integration.Tests.Infrastructure
{
    internal static class EnvHelpers
    {
        private const string CiDbNameParamPrefix = "qp8_test_ci_";

        private static readonly string CiDbNameParam = $"{CiDbNameParamPrefix}dbname";

        private static readonly string CiDbServerParam = $"{CiDbNameParamPrefix}dbserver";

        private static readonly string CiPgLoginParam = $"{CiDbNameParamPrefix}pg_login";

        private static readonly string CiPgPasswordParam = $"{CiDbNameParamPrefix}pg_password";

        private static readonly string CiLocalDbName = $"{CiDbNameParamPrefix}{Environment.MachineName.ToLowerInvariant()}";

        internal static string DbNameToRunTests => TestContext.Parameters.Get(CiDbNameParam, CiLocalDbName);

        internal static string DbServerToRunTests => TestContext.Parameters.Get(CiDbServerParam);

        internal static string PgDbLoginToRunTests => TestContext.Parameters.Get(CiPgLoginParam);

        internal static string PgDbPasswordToRunTests => TestContext.Parameters.Get(CiPgPasswordParam);
    }
}
