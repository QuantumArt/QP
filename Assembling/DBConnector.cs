using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Win32;

namespace Assembling
{

    public class DbConnector
    {
        public DbConnector(string connectionParameter, bool isCustomerCode)
        {
            if (isCustomerCode)
            {
                CustomerCode = connectionParameter;
            }
            else
            {
                _mConnectionString = RemoveProvider(connectionParameter);
            }
        }

        private static string RemoveProvider(string cnnString)
        {
            return Regex.Replace(cnnString, @"provider[\s]?=[\s]?[^;]+", "", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public string CustomerCode { get; }

        private string _mConnectionString;
        public string ConnectionString
        {
            get
            {
                if (String.IsNullOrEmpty(_mConnectionString))
                {
                    _mConnectionString = GetConnectionString();
                }
                return _mConnectionString;
            }
        }

        private string GetConnectionString()
        {

            var qKey = Registry.LocalMachine.OpenSubKey(@"Software\Quantum Art\Q-Publishing") ??
                               Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Quantum Art\Q-Publishing");

            if (qKey == null)
                throw new InvalidOperationException("QP7 is not installed");

            var regValue = qKey.GetValue("Configuration File");
            if (regValue == null)
                throw new InvalidOperationException("QP7 records in the registry are inconsistent or damaged");

            var doc = new XmlDocument();
            doc.Load(regValue.ToString());
            var node = doc.SelectSingleNode("configuration/customers/customer[@customer_name='" + CustomerCode + "']/db/text()");
            if (node == null)
                throw new InvalidOperationException("Cannot load connection string for ASP.NET in QP7 configuration file");

            return node.Value.Replace("Provider=SQLOLEDB;", "");
        }


        public SqlConnection CreateConnection()
        {
            var cnn = new SqlConnection(ConnectionString);
            cnn.Open();
            return cnn;
        }

        /*private int m_loggedUserId; // = 0;

        private int LoggedUserId
        {
          get { return m_loggedUserId; }
          set { m_loggedUserId = value; }
        }
          
        public bool IsUserAuthenticated {
            get {
                return (m_loggedUserId != 0);
            }
        }
        public void AuthenticateUser(string login, string password) {
            SqlConnection cnn = CreateConnection();
            SqlCommand cmd = new SqlCommand("select user_id from users where login = '" + login + "' and password = '" + password + "'", cnn);
            LoggedUserId = Convert.ToInt32((decimal)cmd.ExecuteScalar());
            cnn.Close();
            cmd.Dispose();
        }
         */

        public void ExecuteCmd(SqlCommand cmd)
        {
            using (var cnn = CreateConnection())
            {
                cmd.Connection = cnn;
                cmd.ExecuteNonQuery();
                cnn.Close();
            }

        }
        public void ExecuteSql(string sqlQuery)
        {
            var cmd = new SqlCommand(sqlQuery);
            ExecuteCmd(cmd);
        }
        public DataSet GetData(string sqlQuery)
        {
            var ds = new DataSet {Locale = CultureInfo.InvariantCulture};
            var cnn = CreateConnection();
            var cmd = new SqlCommand(sqlQuery, cnn);
            var da = new SqlDataAdapter {SelectCommand = cmd};
            da.Fill(ds);
            cnn.Close();
            cmd.Dispose();
            return ds;
        }

        public void GetData(string sqlQuery, DataTable dt)
        {
            var cnn = CreateConnection();
            var cmd = new SqlCommand(sqlQuery, cnn);
            var da = new SqlDataAdapter {SelectCommand = cmd};
            da.Fill(dt);
            cnn.Close();
            cmd.Dispose();
        }

        public DataTable GetDataTable(string sqlQuery)
        {
            DataTable result = null;
            var ds = GetData(sqlQuery);
            if (ds.Tables.Count > 0)
            {
                result = ds.Tables[0];
            }
            return result;
        }


        public DataRow GetOneRow(string sqlQuery)
        {
            DataRow result = null;
            var dt = GetDataTable(sqlQuery);
            if (dt != null && dt.Rows.Count > 0)
            {
                result = dt.Rows[0];
            }
            return result;
        }

        public static string GetValue(DataRow row, string columnName, string defaultValue)
        {
            var obj = row[columnName];
            return obj == null || obj.ToString() == "" ? defaultValue : obj.ToString();
        }

    }
}
