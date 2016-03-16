using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Xsl;
using System.Text.RegularExpressions;
using System.Linq;
using QA_Assembling.Info;


namespace QA_Assembling
{
    public class AssembleContentsController : AssembleControllerBase
    {
        public int SiteId { get; set; }

        public string SqlMetalPath { get; set; }

        public string NameSpace { get; set; }

        private DataTable _contentsTable;
        private DataTable _contentGroupTable;
        private DataTable _fieldsTable;
        private DataTable _fieldsInfoTable;
        private DataTable _linkTable;
        private DataTable _userQueryTable;
        private DataTable _statusTable;
        private DataRow _siteRow;
        private DataTable _contentToContentTable;


        public void ClearTables()
        {
            _contentsTable = null;
            _contentGroupTable = null;
            _fieldsTable = null;
            _linkTable = null;
            _userQueryTable = null;
            _statusTable = null;
            _siteRow = null;
            _contentToContentTable = null;
        }
        
        public DataRow SiteRow => _siteRow ?? (_siteRow = Cnn.GetDataTable("select * from site where site_id = " + SiteId).Rows[0]);

        public DataTable StatusTable => _statusTable ?? (_statusTable = Cnn.GetDataTable("select * from status_type"));

        public DataTable AdditionalContextClassNameTable => _contentGroupTable ??
                                                            (_contentGroupTable =
                                                                Cnn.GetDataTable(
                                                                    "select distinct add_context_class_name from content where add_context_class_name is not null and site_id = " +
                                                                    SiteId));

        public DataTable UserQueryTable {
            get
            {
                if (null == _userQueryTable)
                {
                    var qb = new StringBuilder();
                    qb.Append(" select uq.*, c.site_id as real_site_id, c2.site_id as virtual_site_id from user_query_contents uq ");
                    qb.Append(" inner join content c on uq.real_content_id = c.CONTENT_ID");
                    qb.Append(" inner join content c2 on uq.virtual_content_id = c2.CONTENT_ID");
                    _userQueryTable = Cnn.GetDataTable(qb.ToString());
                }
                return _userQueryTable;
            }
        }

        public DataTable ContentsTable {
            get
            {
                if (null == _contentsTable)
                {
                    var qb = new StringBuilder();
                    qb.Append("select * from content ");
                    qb.AppendFormat(" where site_id = {0}", SiteId);
                    _contentsTable = Cnn.GetDataTable(qb.ToString());
                }
                return _contentsTable;
            }
        }

        public DataTable FieldsTable
        {
            get
            {
                if (null == _fieldsTable)
                {
                    var qb = new StringBuilder();
                    qb.Append("select * from (");
                    qb.Append(" select row_number() over(PARTITION BY ca.attribute_id order by ca.attribute_id asc) as count, ");
                    qb.Append(" c.virtual_type, ca.*, at.type_name, ua.union_attr_id, ca2.attribute_id as uq_attr_id, ca3.attribute_id as related_m2o_id from content_attribute ca ");
                    qb.Append(" inner join attribute_type at on ca.attribute_type_id = at.attribute_type_id");
                    qb.Append(" inner join content c on ca.content_id = c.content_id ");
                    qb.Append(" left join union_attrs ua on ua.virtual_attr_id = ca.attribute_id ");
                    qb.Append(" left join user_query_contents uqa on ca.content_id = uqa.virtual_content_id ");
                    qb.Append(" left join content_attribute ca2 on ca2.content_id = uqa.real_content_id and ca.attribute_name = ca2.attribute_name");
                    qb.Append(" left join content_attribute ca3 on ca.attribute_id = ca3.back_related_attribute_id");

                    qb.AppendFormat(" where c.site_id = {0} ", SiteId);
                    qb.Append(" ) cc where cc.COUNT = 1"); 
                    _fieldsTable = Cnn.GetDataTable(qb.ToString());
                }
                return _fieldsTable;
            }
        }

        public DataTable FieldsInfoTable => _fieldsInfoTable ??
                                            (_fieldsInfoTable =
                                                Cnn.GetDataTable("select COLUMN_NAME, TABLE_NAME, DATA_TYPE from INFORMATION_SCHEMA.COLUMNS"));

        public DataTable LinkTable => _linkTable ?? (_linkTable = Cnn.GetDataTable("select * from content_link"));

        public DataTable ContentToContentTable => _contentToContentTable ??
                                                  (_contentToContentTable =
                                                      Cnn.GetDataTable(
                                                          string.Format(
                                                              "select cc.* from content_to_content cc inner join CONTENT c on l_content_id = c.CONTENT_ID INNER JOIN CONTENT c2 on r_content_id = c2.CONTENT_ID WHERE c.SITE_ID = {0} and c2.SITE_ID = {0} and cc.link_id in (select link_id from content_attribute ca)",
                                                              SiteId)));

        #region constructors and initializers
        public AssembleContentsController(int siteId, string sqlMetalPath, string customerCode) : base(customerCode)
        {
            FillController(siteId, sqlMetalPath);
        }

        public AssembleContentsController(int siteId, string sqlMetalPath, DbConnector cnn)
            : base(cnn)
        {
            FillController(siteId, sqlMetalPath);
        }

        public void FillController(int siteId, string sqlMetalPath) {
            CurrentAssembleMode = AssembleMode.Contents;
            SiteId = siteId;
            if (string.IsNullOrEmpty(sqlMetalPath)) {
                throw new ArgumentException("Path to SqlMetal utility cannot be null or empty");
            }
            SqlMetalPath = sqlMetalPath;
        }
        #endregion

        #region paths

        private string _siteRoot;
        private string SiteRoot
        {
            get
            {
                if (null == _siteRoot) {
                    var resultColumn = SiteRow["is_live"].ToString() == "1" ? "assembly_path" : "stage_assembly_path";
                    _siteRoot = SiteRow[resultColumn].ToString().Replace(@"\bin", "");
                }
                return _siteRoot;
            }
        }

        public new bool IsLive => Convert.ToBoolean(int.Parse(SiteRow["is_live"].ToString()));

        public bool ProceedMappingWithDb => GetFlag("proceed_mapping_with_db", false);

        public bool ImportMappingToDb => GetFlag("import_mapping_to_db", false);

        public bool GenerateMapFileOnly => GetFlag("generate_map_file_only", false);

        public bool ProceedDbIndependentGeneration => GetFlag("proceed_db_independent_generation", false);

        public bool GetFlag(string key, bool defaultValue)
        {
            return !SiteRow.Table.Columns.Contains(key) ? defaultValue : (bool)SiteRow[key];     
        }

        public string DataContextClass
        {
            get
            {
                var result = Convert.ToString(SiteRow["context_class_name"]);
                return string.IsNullOrEmpty(result) ? "QPDataContext" : result;
            }
        }

        #endregion

        public override void Assemble()
        {
            var helper = new FileNameHelper() { SiteRoot = SiteRoot, DataContextClass = DataContextClass, ProceedMappingWithDb = ProceedMappingWithDb };
            var xmlProcessor = new XmlPreprocessor(this);
            if (ImportMappingToDb)
            {
                xmlProcessor.ImportMapping(helper);
                ClearTables();
            }
            xmlProcessor.GenerateMainMapping(helper);
            GenerateClasses(helper);
            if (ProceedDbIndependentGeneration)
            {
                foreach (DataRow row in AdditionalContextClassNameTable.Rows)
                {
                    var info = ContextClassInfo.Parse(Convert.ToString(row["add_context_class_name"]));
                    helper.DataContextClass = info.ClassName;
                    xmlProcessor.GeneratePartialMapping(helper, info);
                    GenerateClasses(helper);
                }

            }
        }

        private void GenerateClasses(FileNameHelper helper)
        {
            GenerateDbmlFile(helper);
            ProcessDbmlFile(helper);
            if (!GenerateMapFileOnly)
            {
                GenerateManyToMany(helper);
                GenerateModifications(helper);
                GenerateExtensions(helper);
            }
        }

        private void GenerateDbmlFile(FileNameHelper helper)
        {
            var xslTran = new XslCompiledTransform();
            xslTran.Load(helper.MappingXsltFileName);
            xslTran.Transform(helper.MappingResultXmlFileName, helper.DbmlFileName);
        }

        private void GenerateManyToMany(FileNameHelper helper)
        {
            var xslTran = new XslCompiledTransform();
            xslTran.Load(helper.ManyXsltFileName);
            xslTran.Transform(helper.MappingResultXmlFileName, helper.ExtendCodeFileName);
        }

        private void GenerateModifications(FileNameHelper helper)
        {
            var xslTran = new XslCompiledTransform();
            xslTran.Load(helper.ModificationXsltFileName);
            xslTran.Transform(helper.MappingResultXmlFileName, helper.ModificationCodeFileName);
        }

        private void GenerateExtensions(FileNameHelper helper)
        {
            if (File.Exists(helper.OldExtensionsCodeFileName))
                File.Delete(helper.OldExtensionsCodeFileName);
            var xslTran = new XslCompiledTransform();
            xslTran.Load(helper.ExtensionsXsltFileName);
            xslTran.Transform(helper.MappingResultXmlFileName, helper.ExtensionsCodeFileName);
        }

        private readonly StringBuilder _output = new StringBuilder();

        private string Output => _output.ToString();

        private Encoding OutputEncoding => Encoding.GetEncoding("cp866");


        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!string.IsNullOrEmpty(outLine.Data)) {
                _output.AppendLine(OutputEncoding.GetString(Encoding.Default.GetBytes(outLine.Data)));
            }
        }
        
        private void ProcessDbmlFile(FileNameHelper helper)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = SqlMetalPath,
                    Arguments = GenerateCommandLineParams(helper),
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                }
            };
            process.OutputDataReceived += OutputHandler;
            process.StartInfo.CreateNoWindow = true;

            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.WaitForExit();
                File.WriteAllText(helper.SqlMetalLogFileName, Output);
                if (process.ExitCode != 0)
                {
                    var message = string.Join("\r\n", 
                        Regex.Split(Output, "\r\n")
                        .Where(n => n.Contains("Error "))
                        .Select(n =>
                            {
                                var line = Regex.Match(n, @".dbml\(([\d]+)\)").Groups[1].Value;
                                line = string.IsNullOrEmpty(line) ? "" : $"Line {line}: ";
                                return line + n.Substring(n.IndexOf("Error ", StringComparison.Ordinal));
                            }
                        )
                        .ToArray()
                    );
                    throw new ApplicationException(
                        $"Some errors has been found while processing the file {helper.DbmlFileName}:\r\n{message}");
                }
            }

            if (GenerateMapFileOnly && File.Exists(helper.FakeCodeFileName))
                File.Delete(helper.FakeCodeFileName);
        }

        private string GenerateCommandLineParams(FileNameHelper helper)
        {
            var cmdBuilder = new StringBuilder();
            cmdBuilder.AppendFormat("{0} ", helper.DbmlFileName);
            cmdBuilder.AppendFormat("/code:{0} ", GenerateMapFileOnly ? helper.FakeCodeFileName : helper.MainCodeFileName);
            if (ProceedDbIndependentGeneration)
                cmdBuilder.AppendFormat("/map:{0} ", helper.MapFileName);
            if (!string.IsNullOrEmpty(NameSpace))
                cmdBuilder.AppendFormat("/namespace:{0} ", NameSpace);
            return cmdBuilder.ToString();
        }




    }

}
