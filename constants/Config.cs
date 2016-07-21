using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.Constants
{
    public class Config
    {
        /// <summary>
        /// Ключи в конфигурационном файле QP
        /// </summary>
        public static readonly string TempKey = "TempDirectory";
		public static readonly string MailHostKey = "MailHost";
		public static readonly string MailLoginKey = "MailLogin";
		public static readonly string MailPasswordKey = "MailPassword";
		public static readonly string MailFromNameKey = "MailFromName";
		public static readonly string MailAssembleKey = "MailAssemble";
        public static readonly string SqlMetalKey = "SqlMetalPath";
		public static readonly string UseScheduleService = "UseScheduleService";
		public static readonly string ApplicationTitle = "ApplicationTitle";
		public static readonly string AllowSelectCustomerCode = "AllowSelectCustomerCode";
        public static readonly string ADsConnectionStringKey = "ADsConnectionString";
        public static readonly string ADsPathKey = "ADsPath";
        public static readonly string ADsFieldNameKey = "ADsFieldName";
        public static readonly string ADsConnectionUsernameKey = "ADsConnectionUsername";
        public static readonly string ADsConnectionPasswordKey = "ADsConnectionPassword";

    }
}
