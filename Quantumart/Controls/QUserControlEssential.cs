using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.OnScreen;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Controls
{
    
    public class QUserControlEssential
    {
        //shows if OnInit was raised

        ///relation to the parent control
        private readonly IQUserControl _currentControl;
        
        public QUserControlEssential(IQUserControl control)
        {
            _currentControl = control;
        }
        
        public bool DisableDataBind { get; set; } = false;

        public bool UseSimpleInitOrder { get; set; } = false;

        public DateTime TraceStartTime { get; set; }

        public string TraceObjectString { get; set; } = "";

        public string UndefTraceString { get; set; } = "";

        public Hashtable QpDefValues { get; set; } = new Hashtable();

        public void AddQpDefValue(string key, object value)
        {
            if (key != null) {
                key = key.ToLowerInvariant();
                if (QpDefValues.ContainsKey(key)) QpDefValues.Remove(key); 
                QpDefValues.Add(key, value);
            }
        }
        
        public string QpDefValue(string key)
        {
            key = key.ToLowerInvariant();
            var functionReturnValue = QpDefValues.ContainsKey(key) ? QpDefValues[key].ToString().Replace("'", "") : "";
            return functionReturnValue;
        }
        
        public int FormatId { get; set; } = 0;

        public bool IsNs { get; set; } = false;

        public bool OnInitFired { get; set; } = false;

        public QScreen QScreenInst => _currentControl.QPage.QScreen;

        public void TraceObject(IQPage qPage)
        {
            if (qPage.QPTrace != null) {
                var traceBuilder = new StringBuilder();
                traceBuilder.Append(FormatId + "Def:");
                var emptyValues = new Hashtable();
                foreach (string qpKey in QpDefValues.Keys) {
                    if (string.IsNullOrEmpty(QpDefValues[qpKey].ToString())) {
                        emptyValues[qpKey] = 1;
                    }
                }
                foreach (string qpKey in emptyValues.Keys) {
                    AddQpDefValue(qpKey, qPage.Value(qpKey));
                }
                foreach (string qpKey in QpDefValues.Keys) {
                    traceBuilder.Append("Value(" + qpKey + ")=" + QpDefValues[qpKey] + ";");
                }
                traceBuilder.Append("Undef:");
                traceBuilder.Append(UndefTraceString);
                traceBuilder.Append(qPage.QPTrace.TraceStartText + "<br>");
                TraceObjectString += traceBuilder.ToString();
            }
        }
        
        public virtual void OnLoadControl(Object sender, ref string objectCallName)
        {
        }
        
        public virtual void LoadControlData(object sender, EventArgs e)
        {
            
        }
        
        public virtual void InitUserHandlers(EventArgs e)
        {
            
        }
        
        public string GetObjectFullName(string templateNetName, string objectNetName, string formatNetName)
        {
            var str = string.Empty;
            
            if (templateNetName != string.Empty) {
                str = templateNetName + ",";
            }
            
            str = str + objectNetName;
            
            if (formatNetName != string.Empty) {
                str = str + "_" + formatNetName;
            }
            
            return str + ".ascx";
        }
        
        
        ///Fields methods
        
        public virtual string FieldNs(DataRow pDataItem, string key, string defaultValue)
        {
            return Field(pDataItem, key, defaultValue);
        }
        
        public virtual string Field(bool isStage, DataRow pDataItem, string key, string defaultValue)
        {
            if (isStage) {
                return OnFly(pDataItem, key, defaultValue);
            }
            else {
                return Field(pDataItem, key, defaultValue);
            }
        }
        
        public string OnFly(DataRow pDataItem, string key)
        {
            return OnFly(pDataItem, key, "");
        }
        
        public string OnFly(DataRowView pDataItem, string key, string defaultValue)
        {
            return OnFly(pDataItem.Row, key, defaultValue);
        }
        
        public string OnFly(DataRowView pDataItem, string key)
        {
            return OnFly(pDataItem.Row, key);
        }
        
        public string OnFly(DataRow pDataItem, string key, string defaultValue)
        {
            var fieldValue = Field(pDataItem, key, defaultValue);
            var itemId = DBConnector.GetNumInt(pDataItem["content_item_id"]);
            return OnScreenFlyEditCommon(fieldValue, itemId, key);
        }
        
        public virtual string Field(DataRow pDataItem, string key, string defaultValue)
        {
            var result = pDataItem != null ? FormatField(pDataItem[key].ToString()) : "";
            if (string.IsNullOrEmpty(result)) {
                result = defaultValue;
            }
            return result;
        }
        
        internal string FormatField(string field)
        {
            var result = field;
            var uploadUrl = _currentControl.upload_url;
            result = result.Replace("<%=upload_url%>", uploadUrl);
            result = result.Replace("<%#upload_url%>", uploadUrl);
            var siteUrl = DBConnector.AppSettings["UseAbsoluteSiteUrl"] == "1" ? _currentControl.absolute_site_url : _currentControl.site_url;
            result = result.Replace("<%=site_url%>", siteUrl);
            result = result.Replace("<%#site_url%>", siteUrl);
            return result;
        }
        
        protected bool CheckAllowOnScreen()
        {
            return 
                _currentControl.IsStage
                && QScreen.UserAuthenticated()
                && !QScreen.IsBrowseServerMode()
                && _currentControl.QPage.Cnn.GetEnableOnScreen(_currentControl.QPage.site_id)
                && _currentControl.QPage.QScreen.FieldBorderMode != 0
                && HttpContext.Current.Session["allow_stage_edit_field"].ToString() != "0"
            ;
        }
        
        public string OnScreenFlyEdit(string value, string itemId, string fieldName)
        {
            int id;
            var functionReturnValue = Int32.TryParse(itemId, out id) ? OnScreenFlyEdit(value, id, fieldName) : FormatField(value);
            return functionReturnValue;
        }
        
        public string OnStageFlyEdit(string value, string itemId, string fieldName)
        {
            return OnScreenFlyEdit(value, itemId, fieldName);
        }
        
        public string OnScreenFlyEdit(string value, Int32 itemId, string fieldName)
        {
            return OnScreenFlyEditCommon(FormatField(value), itemId, fieldName);
        }
        
        public string OnStageFlyEdit(string value, Int32 itemId, string fieldName)
        {
            return OnScreenFlyEdit(value, itemId, fieldName);
        }
        
        protected string OnScreenFlyEditCommon(string value, Int32 itemId, string fieldName)
        {
            if (!CheckAllowOnScreen()) {
                return value;
            }
            else {

                var isBorderStatic = "";

                var sql = " SELECT LOWER(t.database_type) AS dbt, LOWER(t.type_name) AS at, i.content_id AS content_id," + " a.required AS required," + " a.allow_stage_edit AS allow_stage_edit" + " FROM content_item AS i" + " LEFT OUTER JOIN content_attribute AS a ON a.content_id = i.content_id" + " LEFT OUTER JOIN attribute_type AS t ON t.attribute_type_id = a.attribute_type_id" + " WHERE i.content_item_id=" + itemId + " AND a.attribute_name=N'" + fieldName + "'";
                
                var conn = new DBConnector();
                var dt = conn.GetCachedData(sql);
                
                if (dt.Rows.Count == 0) {
                    return value;
                }
                
                var attrDbType = dt.Rows[0]["dbt"].ToString();
                var attrType = dt.Rows[0]["at"].ToString();
                var attrRequired = DBConnector.GetNumInt(dt.Rows[0]["required"]);
                var contentId = DBConnector.GetNumInt(dt.Rows[0]["content_id"]);
                var allowStageEdit = DBConnector.GetNumInt(dt.Rows[0]["allow_stage_edit"]);
                
                if (allowStageEdit != 1) {
                    return value;
                }
                
                switch (_currentControl.QPage.QScreen.FieldBorderMode) {
                    case 2:
                        isBorderStatic = "true";
                        break;
                    case 1:
                        isBorderStatic = "false";
                        break;
                    case 0:
                        isBorderStatic = "none";
                        break;
                }
                
                var editable = attrDbType == "ntext";
                _currentControl.QPage.QScreen.OnFlyObjCount = _currentControl.QPage.QScreen.OnFlyObjCount + 1;
                
                return OnStageDiv(value, fieldName, itemId, contentId, isBorderStatic, editable, attrType, attrRequired);
            }
        }
        
        public string OnStage(string value, string itemId)
        {
            return OnScreen(value, itemId);
        }
        
        public string OnStage(string value, Int32 itemId)
        {
            return OnScreen(value, itemId);
        }
        
        public string OnScreen(string value, string itemId)
        {
            int id;
            var functionReturnValue = Int32.TryParse(itemId, out id) ? OnScreen(value, id) : FormatField(value);
            return functionReturnValue;
        }
        
        
        public string OnScreen(string value, Int32 itemId)
        {
            if (!CheckAllowOnScreen()) {
                return FormatField(value);
            }
            else {
                var conn = new DBConnector();
                var contentId = conn.GetContentIdForItem(itemId);
                
                _currentControl.QPage.QScreen.OnFlyObjCount = _currentControl.QPage.QScreen.OnFlyObjCount + 1;
                
                return OnStageDiv(value, "", itemId, contentId, "", false, "", 0);
            }
        }
        
        
        public string OnStageDiv(string value, string fieldName, Int32 itemId, Int32 contentId, string isBorderStatic, bool editable, string attrType, Int32 attrRequired)
        {
            if (HttpContext.Current.Session["UID"] == null) {
                return value;
            }

            var access = QScreen.DbGetUserAccess("content_item", itemId, (int)HttpContext.Current.Session["UID"]);
            if (access < 3) {
                return value;
            }
            
            var id = _currentControl.QPage.QScreen.OnFlyObjCount.ToString();
            var btnHtml = @"<td class=""qp_button_td"" on_fly_type=""onfly_toolbar_td"" width=""28"" id=""onfly_{2}_btn_{0}"" {3} style=""cursor:pointer;""><img style=""display:none;"" src=""/rs/images/onfly/onfly_{2}_over.jpg"" width=""28"" height=""26"" ><img src=""/rs/images/onfly/onfly_{2}.jpg"" picture_name=""onfly_{2}"" on_fly_type=""onfly_toolbar_btn"" title=""{1}"" width=""28"" height=""26"" id=""onfly_{2}_btn_img_{0}""></td>";
            
            var toolbarClass = editable ? "qp_toolbar_editable" : "qp_toolbar";
            var containerInitStyle = "position:relative;display:inline;left:0;top:0;";
            var onFlyInitStyle = editable ? "position:relative;display:;left:0;top:0;" : "position:relative;display:inline;left:0;top:0;";

            var sb = new StringBuilder();
            sb.Append(String.Format(@"<div on_fly_type=""onfly_container"" style=""{0}""><div id=""onfly_toolbar_{1}"" onfly_no=""{1}"" on_fly_type=""onfly_toolbar"" class=""{2}"" style=""display:none;""><table cellpadding=0 cellspacing=0 width=1 class=""qp_toolbar_table""><tr>", containerInitStyle, id, toolbarClass));

            if (editable) 
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Cancel"), "cancel", "");
 
            if ((access >= 3) && editable)
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Save"), "save", "");

            sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Edit in Form View"), "go_info", @"style=""display:none""");
            sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Add New"), "go_new", @"style=""display:none""");
            sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Create Like"), "create_like", @"style=""display:none""");

            if (attrType == "visualedit")
            {
                sb.Append(@"<td width=""14""><img src=""/rs/images/onfly/onfly_div.jpg"" width=""14"" height=""26"" border=""0""></td>");
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Bold"), "bold", "");
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Italic"), "italic", "");
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Underline"), "underline", "");
                sb.Append(@"<td width=""14""><img src=""/rs/images/onfly/onfly_div.jpg"" width=""14"" height=""26"" border=""0""></td>");
                sb.AppendFormat(btnHtml,  id, TranslateManager.Translate("Align Left"), "left", "" );
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Align Center"), "center", "");
                sb.AppendFormat(btnHtml,  id, TranslateManager.Translate("Align Right"), "right", "");
                sb.AppendFormat(btnHtml, id, TranslateManager.Translate("Align Justify"), "justify", "");
            }
            
            sb.AppendFormat(@"</tr></table></div><div id=""onfly_div_{0}"" on_fly_type=""onfly_div"" onfly_no=""{0}"" style=""{1}"" hicolor=""#aaaaff"" item_id=""{2}"" is_dotnet=""1"" content_id=""{3}"" site_id=""{4}"" attr_required=""{5}"" attribute_name=""{6}"" static_border = ""{7}"" contenteditable=""{8}"" >", id, onFlyInitStyle, itemId, contentId, _currentControl.site_id, attrRequired, fieldName, isBorderStatic, editable ? "true" : "false");
            
            if (!editable) 
                sb.Append(@"<img width=""15"" height=""16"" src='/rs/images/onfly/onfly_edit_indicator.gif' border='0' align='texttop' style='position:relative;top:0;left:0'>");
            
            sb.Append(value);
            
            sb.Append("</div></div>");
            if (editable) {
                sb.AppendFormat(@"<iframe src=""about:blank""  name=id=""onfly_iframe_{0}"" id=""onfly_iframe_{0}"" on_fly_type=""onfly_iframe"" style=""display: none;"" class=""qp_onfly_iframe"" ></iframe>", id);
            }
            
            return sb.ToString();
        }
        
        public string GetReturnStageUrl()
        {
            var functionReturnValue = QScreenInst != null ? QScreenInst.GetReturnStageUrl() : "";

            return functionReturnValue;
        }
    }
}