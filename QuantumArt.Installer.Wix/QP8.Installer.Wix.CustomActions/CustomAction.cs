using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Web.Administration;
using Microsoft.Win32;

namespace QP8.Installer.Wix.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CheckQp7Installed(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            var lKey = IsKey(@"SOFTWARE\Quantum Art\QP7.Framework", RegistryView.Registry32);

            if (!lKey)
            {
                lKey = IsKey(@"SOFTWARE\Quantum Art\QP7.Framework", RegistryView.Registry64);
            }

            xiSession["QP_7_INSTALLED"] = !lKey ? "0" : "1";

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckQp8Installed(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            var lKey = IsKey(@"SOFTWARE\Quantum Art\QP8.Framework", RegistryView.Registry32);

            if (!lKey)
            {
                lKey = IsKey(@"SOFTWARE\Quantum Art\QP8.Framework", RegistryView.Registry64);
            }

            xiSession["QP_8_INSTALLED"] = !lKey ? "0" : "1";

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult GetQp7ConfigurationPath(Session xiSession)
        {
            string val = GetValue(@"SOFTWARE\Quantum Art\Q-Publishing", "Configuration File");

            xiSession["QP_CONFIGURATION_FILE"] = val;

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult TestConnection(Session xiSession)
        {
            string connectionString = "Initial Catalog=master;Data Source={0};persist security info=True; Integrated Security=SSPI;";
            string fillConnectionString =
                string.Format(connectionString,
                    xiSession["DB_SERVER_NAME"]);

            try
            {
                using (var connection = new SqlConnection(fillConnectionString))
                {
                    connection.Open();

                    xiSession["DB_CONNECTION_SUCCESS"] = connection.State == ConnectionState.Open ? "1" : "0";

                    if (connection.State == ConnectionState.Open)
                    {
                        MessageBox.Show("Соединение успешно установлено.");
                    }
                }
            }
            catch (Exception ex)
            {
                xiSession["DB_CONNECTION_SUCCESS"] = "0";
                MessageBox.Show(
                    string.Format("Ошибка подключения к БД ({0}), ошибка: {1}.",
                        fillConnectionString, ex.Message));
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckDbDialog(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            xiSession["DB_DIALOG_SUCCESS"] = "1";

            string connectionString = "Initial Catalog=master;Data Source={0};persist security info=True; Integrated Security=SSPI;";
            string fillConnectionString =
                string.Format(connectionString,
                    xiSession["DB_SERVER_NAME"]);

            try
            {
                using (var connection = new SqlConnection(fillConnectionString))
                {
                    connection.Open();

                    string cmdText = @"select CAST(LEFT(CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(MAX)),
                        CHARINDEX('.',CAST(SERVERPROPERTY('ProductVersion') AS NVARCHAR(MAX))) - 1) AS INT)";
                    
                    using (SqlCommand sqlCmd = new SqlCommand(cmdText, connection))
                    {
                        object nRet = sqlCmd.ExecuteScalar();

                        if (nRet != null)
                        {
                            int majorVersion = 0;
                            int.TryParse(nRet.ToString(), out majorVersion);

                            if (majorVersion < 10)
                            {
                                MessageBox.Show(string.Format("Неподдерживаемая версия SQL Server ({0}).",
                                    majorVersion));
                                xiSession["DB_DIALOG_SUCCESS"] = "0";
                                return ActionResult.Success;
                            }
                        }
                    }

                    cmdText = "select 1 from master.dbo.sysdatabases where name=\'" + xiSession["DB_NAME"] + "\'";

                    using (SqlCommand sqlCmd = new SqlCommand(cmdText, connection))
                    {
                        object nRet = sqlCmd.ExecuteScalar();

                        if (nRet != null)
                        {
                            MessageBox.Show(string.Format("БД '{0}' уже существует.",
                                xiSession["DB_NAME"]));
                            xiSession["DB_DIALOG_SUCCESS"] = "0";
                            return ActionResult.Success;
                        }
                    }

                    cmdText = "select name from master.sys.sql_logins where name = @loginName";

                    using (SqlCommand sqlCmd = new SqlCommand(cmdText, connection))
                    {
                        SqlParameter param = new SqlParameter();
                        param.ParameterName = "@loginName";
                        param.Value = xiSession["DB_LOGIN"];
                        sqlCmd.Parameters.Add(param);

                        object nRet = sqlCmd.ExecuteScalar();

                        if (nRet != null)
                        {
                            MessageBox.Show(string.Format("Пользователь '{0}' уже существует.",
                                xiSession["DB_LOGIN"]));
                            xiSession["DB_DIALOG_SUCCESS"] = "0";
                            return ActionResult.Success;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                xiSession["DB_DIALOG_SUCCESS"] = "0";
                xiSession["DB_CONNECTION_SUCCESS"] = "0";
                MessageBox.Show(
                    string.Format("Ошибка подключения к БД ({0}), ошибка: {1}.",
                        fillConnectionString, ex.Message));
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckIisSiteDialog(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            xiSession["IIS_DIALOG_SUCCESS"] = "1";

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    if (serverManager.Sites
                        .Any(w => w.Name.ToLower() == xiSession["QP8_SITE_WEB_APP_NAME"].ToLower()))
                    {
                        MessageBox.Show(string.Format("Сайт '{0}' уже существует.",
                                xiSession["QP8_SITE_WEB_APP_NAME"]));
                        xiSession["IIS_DIALOG_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }

                    if (serverManager.ApplicationPools
                        .Any(w => w.Name.ToLower() == xiSession["QP8_SITE_APP_POOL_NAME"].ToLower()))
                    {
                        MessageBox.Show(string.Format("Пул приложений '{0}' уже существует.",
                                xiSession["QP8_SITE_APP_POOL_NAME"]));
                        xiSession["IIS_DIALOG_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                xiSession["DB_DIALOG_SUCCESS"] = "0";
                MessageBox.Show(
                    string.Format("Ошибка подключения к IIS. Ошибка: {0}.", ex.Message));
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult CheckIisDemoSiteDialog(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            xiSession["IIS_DIALOG_SUCCESS"] = "1";

            try
            {
                using (ServerManager serverManager = new ServerManager())
                {
                    if (serverManager.Sites
                        .Any(w => w.Name.ToLower() == xiSession["QP8_DEMO_SITE_WEB_APP_NAME"].ToLower()))
                    {
                        MessageBox.Show(string.Format("Сайт '{0}' уже существует.",
                                xiSession["QP8_DEMO_SITE_WEB_APP_NAME"]));
                        xiSession["IIS_DIALOG_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }

                    if (serverManager.ApplicationPools
                        .Any(w => w.Name.ToLower() == xiSession["QP8_DEMO_SITE_APP_POOL_NAME"].ToLower()))
                    {
                        MessageBox.Show(string.Format("Пул приложений '{0}' уже существует.",
                                xiSession["QP8_DEMO_SITE_APP_POOL_NAME"]));
                        xiSession["IIS_DIALOG_SUCCESS"] = "0";
                        return ActionResult.Success;
                    }
                }
            }
            catch (Exception ex)
            {
                xiSession["DB_DIALOG_SUCCESS"] = "0";
                MessageBox.Show(
                    string.Format("Ошибка подключения к IIS. Ошибка: {0}.", ex.Message));
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult FillSqlServersComboBox(Session xiSession)
        {
            //System.Diagnostics.Debugger.Launch();

            string instances = GetFullValue(@"SOFTWARE\Microsoft\Microsoft SQL Server", "InstalledInstances");

            if (!string.IsNullOrEmpty(instances))
            {
                var items = instances.Split(new string[] {","}, StringSplitOptions.RemoveEmptyEntries);

                if (items.Length == 0)
                {
                    if (MessageBox.Show("SQL Server не установлен на данном компьютере.", "Ошибка", MessageBoxButtons.OK) == DialogResult.OK)
                    {
                        return ActionResult.Failure;
                    }
                }

                foreach (var item in items)
                {
                    string value = item;

                    if (value == "MSSQLSERVER")
                    {
                        value = xiSession["ComputerName"];
                    }
                    else
                    {
                        value = xiSession["ComputerName"] + "\\" + item;
                    }

                    int order = MaxOrder(xiSession, "ComboBox", "SQL_SERVERS");
                    order++;

                    int rowCount = xiSession.Database.CountRows("ComboBox", "(`Property`='SQL_SERVERS' AND `Value`='" + value + "')");

                    if (rowCount == 0)
                    {
                        InsertRecord(xiSession, "ComboBox",
                            new Object[] {
                            "SQL_SERVERS",
                            order,
                            value,
                            value});
                    }
                }
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UpgradeQPDatabases(Session xiSession)
        {
            System.Diagnostics.Debugger.Launch();

            return ActionResult.Success;
        }

        private static bool IsKey(string name, RegistryView viewType)
        {
            var registryKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, viewType);

            var lKey = registryKey.OpenSubKey(name);

            return lKey != null;
        }

        private static RegistryKey GetKey(string name, RegistryView viewType)
        {
            var registryKey = RegistryKey.OpenBaseKey(
                RegistryHive.LocalMachine, viewType);

            var lKey = registryKey.OpenSubKey(name);

            return lKey;
        }

        private static string GetValue(string path, string name)
        {
            var lKey = IsKey(path, RegistryView.Registry32);

            RegistryKey key = null;
            if (!lKey)
            {
                lKey = IsKey(path, RegistryView.Registry64);

                if (!lKey)
                {
                    key = GetKey(path, RegistryView.Registry64);
                }
            }
            else
            {
                key = GetKey(path, RegistryView.Registry32);
            }

            if (key != null)
            {
                return key.GetValue(name, string.Empty, RegistryValueOptions.None).ToString();
            }

            return string.Empty;
        }

        private static string GetFullValue(string path, string name)
        {
            string result = string.Empty;

            var lKey = IsKey(path, RegistryView.Registry32);

            if (lKey)
            {
                var key = GetKey(path, RegistryView.Registry32);
                if (key != null)
                {
                    var value = key.GetValue(name, string.Empty, RegistryValueOptions.None);

                    if (value is String)
                    {
                        result = key.GetValue(name, string.Empty, RegistryValueOptions.None).ToString();
                    }
                    else if (value is string[])
                    {
                        string[] array = key.GetValue(name, string.Empty, RegistryValueOptions.None) as string[];

                        result = string.Join(",", array);
                    }
                    else
                    {
                        result = key.GetValue(name, string.Empty, RegistryValueOptions.None).ToString();
                    }
                }
            }

            lKey = IsKey(path, RegistryView.Registry64);

            if (lKey)
            {
                var key = GetKey(path, RegistryView.Registry64);
                if (key != null)
                {
                    var value = key.GetValue(name, string.Empty, RegistryValueOptions.None);

                    if (value is String)
                    {
                        result += "," + key.GetValue(name, string.Empty, RegistryValueOptions.None).ToString();
                    }
                    else if (value is string[])
                    {
                        string[] array = key.GetValue(name, string.Empty, RegistryValueOptions.None) as string[];

                        result += "," + string.Join(",", array);
                    }
                    else
                    {
                        result += "," + key.GetValue(name, string.Empty, RegistryValueOptions.None).ToString();
                    }
                }
            }

            return result;
        }

        private static void InsertRecord(Session session, string tableName, Object[] objects)
        {
            var db = session.Database;
            string sqlInsertString = db.Tables[tableName].SqlInsertString + " TEMPORARY";

            //            session.Message(InstallMessage.Info, new Record { FormatString = "InsertRecord does sql: " + sqlInsertString });

            var view = db.OpenView(sqlInsertString);
            view.Execute(new Record(objects));

            view.Close();
        }

        private static int MaxOrder(Session session, string tableName, string property)
        {
            string query = session.Database.Tables[tableName].SqlSelectString;

            var view = session.Database.OpenView(query + " WHERE (`Property`='" + property + "')");
            view.Execute();
            var items = view.GetEnumerator();
            int value = 0;
            while (items.MoveNext())
            {
                int order = int.Parse(items.Current["Order"].ToString());
                if (order > value)
                {
                    value = order;
                }
            }

            return value;
        }
    }
}
