using System.Data;

namespace QA_Assembling.Info
{
    public class SchemaInfo
    {
        private SchemaInfo()
        {

        }
        
        public string ConnectionStringName { get; set; }

        public string ClassName { get; set; }

        public string NamespaceName { get; set; }

        public bool UseLongUrls { get; set; }

        public bool ReplaceUrls { get; set; }

		public bool SendNotifications { get; set; }

        public bool DbIndependent { get; set; }

        public string SiteName { get; set; }

        public bool IsPartial { get; set; }

        public static SchemaInfo Create(DataRow row)
        {
            var info = new SchemaInfo
            {
                ConnectionStringName = DbConnector.GetValue(row, "CONNECTION_STRING_NAME", "qp_database"),
                ClassName = DbConnector.GetValue(row, "CONTEXT_CLASS_NAME", "QPDataContext"),
                NamespaceName = DbConnector.GetValue(row, "namespace", ""),
                UseLongUrls = (bool) row["USE_LONG_URLS"],
                ReplaceUrls = (bool) row["REPLACE_URLS"],
                DbIndependent = (bool) row["PROCEED_DB_INDEPENDENT_GENERATION"],
                SendNotifications = (bool) row["SEND_NOTIFICATIONS"],
                SiteName = row["SITE_NAME"].ToString(),
                IsPartial = false
            };
            return info;
        }
    }
}
