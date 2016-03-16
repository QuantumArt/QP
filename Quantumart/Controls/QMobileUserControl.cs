using System;
using System.Collections;
using System.Data;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Pages;

namespace Quantumart.QPublishing.Controls
{
    
    public class QMobileUserControl : QMobileUserControlBase, IQUserControl
    {
        public QMobileUserControl()
        {
            QUserControlEssential = new QUserControlEssential(this);
        }
        
        public QUserControlEssential QUserControlEssential { get; set; }

        public bool DisableDataBind {
            get { return QUserControlEssential.DisableDataBind; }
            set { QUserControlEssential.DisableDataBind = value; }
        }
        
        public bool UseSimpleInitOrder {
            get { return QUserControlEssential.UseSimpleInitOrder; }
            set { QUserControlEssential.UseSimpleInitOrder = value; }
        }
        
        public DateTime TraceStartTime {
            get { return QUserControlEssential.TraceStartTime; }
            set { QUserControlEssential.TraceStartTime = value; }
        }
        
        public string TraceObjectString {
            get { return QUserControlEssential.TraceObjectString; }
            set { QUserControlEssential.TraceObjectString = value; }
        }
        
        public string UndefTraceString {
            get { return QUserControlEssential.UndefTraceString; }
            set { QUserControlEssential.UndefTraceString = value; }
        }
        
        public Hashtable QpDefValues {
            get { return QUserControlEssential.QpDefValues; }
            set { QUserControlEssential.QpDefValues = value; }
        }
        
        public void AddQpDefValue(string key, object value)
        {
            QUserControlEssential.AddQpDefValue(key, value);
        }
        
        public string QpDefValue(string key)
        {
            return QUserControlEssential.QpDefValue(key);
        }
        
        public int FormatId {
            get { return QUserControlEssential.FormatId; }
            set { QUserControlEssential.FormatId = value; }
        }
        
        public bool OnInitFired {
            get { return QUserControlEssential.OnInitFired; }
            set { QUserControlEssential.OnInitFired = value; }
        }
        
        public QpTrace QPTrace {
            get { return QPage.QPTrace; }
            set { QPage.QPTrace = value; }
        }
        
        public virtual void OnLoadControl(object sender, ref string objectCallName)
        {
            QUserControlEssential.OnLoadControl(sender, ref objectCallName);
        }
        
        public bool IsTest => QPage.IsTest;

        public bool IsStage => QPage.IsStage;

        // ReSharper disable once InconsistentNaming
        public bool QP_IsInStageMode => IsStage;

        public IQPage QPage {
            get { return (QMobilePage)Page; }
            set { Page = (QMobilePage)value; }
        }
        
        public void ShowObject(string name)
        {
            QPage.ShowObject(name);
        }
        public void ShowObject(string name, object sender)
        {
            QPage.ShowObject(name, sender);
        }
        public void ShowObject(string name, object sender, object[] parameters)
        {
            QPage.ShowObject(name, sender, parameters);
        }
        
        public void ShowObjectSimple(string name)
        {
            QPage.ShowObjectSimple(name);
        }
        public void ShowObjectSimple(string name, object sender)
        {
            QPage.ShowObjectSimple(name, sender);
        }
        public void ShowObjectSimple(string name, object sender, object[] parameters)
        {
            QPage.ShowObjectSimple(name, sender, parameters);
        }
        
        public void ShowObjectNS(string name)
        {
            ShowObject(name);
        }
        public void ShowObjectNS(string name, object sender)
        {
            ShowObject(name, sender);
        }
        public void ShowObjectNS(string name, object sender, object[] parameters)
        {
            ShowObject(name, sender, parameters);
        }
        public virtual void ShowControl(string name)
        {
            QPage.ShowControl(name);
        }
        public virtual void ShowControl(string name, object sender)
        {
            QPage.ShowControl(name, sender);
        }
        public virtual void ShowControl(string name, object sender, object[] parameters)
        {
            QPage.ShowControl(name, sender, parameters);
        }
        public void ShowControlNS(string name)
        {
            ShowControl(name);
        }
        public void ShowControlNS(string name, object sender)
        {
            ShowControl(name, sender);
        }
        public void ShowControlNS(string name, object sender, object[] parameters)
        {
            ShowControl(name, sender, parameters);
        }
        
        public DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, 
        byte includeArchive)
        {
            
            return QPage.GetContentData(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle, 
                
            includeArchive);
        }
        
        public DBConnector Cnn => QPage.Cnn;

        public DataTable GetData(string queryString)
        {
            return QPage.Cnn.GetData(queryString);
        }
        
        public int site_id => QPage.site_id;

        public string site_url => QPage.site_url;

        public string absolute_site_url => QPage.absolute_site_url;

        public int page_id => QPage.page_id;

        public string upload_url => QPage.upload_url;

        public bool AbsUploadURL => QPage.AbsUploadURL;

        public string UploadURLPrefix => QPage.UploadURLPrefix;

        public void AddValue(string key, object value)
        {
            QPage.AddValue(key, value);
        }
        
        public void AddObjectValue(string key, object value)
        {
            QPage.AddObjectValue(key, value);
        }
        
        public string Value(string key)
        {
            return QPage.Value(key);
        }
        
        public string Value(string key, string defaultValue)
        {
            return QPage.Value(key, defaultValue);
        }
        
        public long NumValue(string key)
        {
            return QPage.NumValue(key);
        }
        
        public string StrValue(string key)
        {
            return QPage.StrValue(key);
        }
        
        public string InternalStrValue(string valueName)
        {
            return QPage.InternalStrValue(valueName);
        }
        
        public Hashtable Values => QPage.Values;

        public string DirtyValue(string key)
        {
            return QPage.DirtyValue(key);
        }
        
        public void RemoveContentItem(int contentItemId)
        {
            QPage.RemoveContentItem(contentItemId);
        }
        
        public void DeleteContentItem()
        {
            QPage.DeleteContentItem();
        }
        
        public int GetContentID(string contentName)
        {
            return QPage.GetContentID(contentName);
        }
        
        public string FieldName(string contentName, string fieldName)
        {
            return QPage.FieldName(contentName, fieldName);
        }
        
        public int FieldID(string contentName, string fieldName)
        {
            return QPage.FieldID(contentName, fieldName);
        }
        
        public bool CheckMaxLength(string str, int maxlength)
        {
            return QPage.CheckMaxLength(str, maxlength);
        }
        
        public string ReplaceHTML(string str)
        {
            return QPage.ReplaceHTML(str);
        }
        
        public void SendNotification(string notificationOn, int contentItemId, string notificationEmail)
        {
            QPage.SendNotification(notificationOn, contentItemId, notificationEmail);
        }
        
        public string GetSiteUrl()
        {
            return QPage.GetSiteUrl();
        }
        
        public string GetActualSiteUrl()
        {
            return QPage.GetActualSiteUrl();
        }
        
        public string GetContentItemLinkIDs(string linkFieldName, long itemId)
        {
            return QPage.GetContentItemLinkIDs(linkFieldName, itemId);
        }
        public string GetContentItemLinkIDs(string linkFieldName, string itemId)
        {
            return QPage.GetContentItemLinkIDs(linkFieldName, itemId);
        }
        
        public string GetContentItemLinkQuery(string linkFieldName, long itemId)
        {
            return QPage.GetContentItemLinkQuery(linkFieldName, itemId);
        }
        public string GetContentItemLinkQuery(string linkFieldName, string itemId)
        {
            return QPage.GetContentItemLinkQuery(linkFieldName, itemId);
        }
        
        
        public string GetLinkIDs(string linkFieldName)
        {
            return QPage.GetLinkIDs(linkFieldName);
        }
        
        public int GetLinkIDForItem(string linkFieldName, int itemId)
        {
            return QPage.GetLinkIDForItem(linkFieldName, itemId);
        }
        
        public string GetContentFieldValue(int itemId, string fieldName)
        {
            return QPage.GetContentFieldValue(itemId, fieldName);
        }
        public int AddFormToContentWithoutNotification(string contentName, string statusName, int contentItemId)
        {
            return QPage.AddFormToContentWithoutNotification(contentName, statusName, contentItemId);
        }
        
        public int AddFormToContentWithoutNotification(string contentName, string statusName)
        {
            return AddFormToContentWithoutNotification(contentName, statusName, 0);
        }
        
        public int AddFormToContent(string contentName, string statusName, int contentItemId)
        {
            return QPage.AddFormToContent(contentName, statusName, contentItemId);
        }
        
        public int AddFormToContent(string contentName, string statusName)
        {
            return AddFormToContent(contentName, statusName, 0);
        }
        
        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId)
        {
            QPage.UpdateContentItemField(contentName, fieldName, contentItemId);
        }
        
        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId, bool withNotification)
        {
            QPage.UpdateContentItemField(contentName, fieldName, contentItemId, withNotification);
        }
        
        
        public void UpdateContentItem()
        {
            QPage.UpdateContentItem(true, "");
        }
        
        public void UpdateContentItemWithoutNotification()
        {
            QPage.UpdateContentItemWithoutNotification(false, "");
        }
        
        public void UpdateContentItem(bool updateEmpty, string statusName)
        {
            QPage.UpdateContentItem(updateEmpty, statusName);
        }
        
        public void UpdateContentItemWithoutNotification(bool updateEmpty, string statusName)
        {
            QPage.UpdateContentItemWithoutNotification(updateEmpty, statusName);
        }
        
        public string GetContentUploadUrl(string contentName)
        {
            return QPage.GetContentUploadUrl(contentName);
        }
        
        public string GetContentUploadUrlByID(int contentId)
        {
            return QPage.GetContentUploadUrlByID(contentId);
        }
        
        public string GetContentName(int contentId)
        {
            return QPage.GetContentName(contentId);
        }
        
        public Hashtable FieldValuesDictionary {
            get { return QPage.FieldValuesDictionary; }
            set { QPage.FieldValuesDictionary = value; }
        }
        
        public Hashtable FieldNamesDictionary {
            get { return QPage.FieldNamesDictionary; }
            set { QPage.FieldNamesDictionary = value; }
        }

        
        public int published_status_type_id => QPage.published_status_type_id;

        public string published_status_name => QPage.published_status_name;

        
        public virtual void LoadControlData(object sender, EventArgs e)
        {
            QUserControlEssential.LoadControlData(sender, e);
        }
        public virtual void InitUserHandlers(EventArgs e)
        {
            QUserControlEssential.InitUserHandlers(e);
        }
        
        public void SimulateOnInit(EventArgs e)
        {
            OnInit(e);
        }
        
        protected override void OnInit(EventArgs e)
        {
            if (UseSimpleInitOrder) {
                base.OnInit(e);
                LoadControlData(this, new EventArgs());
                InitUserHandlers(new EventArgs());
            }
            else {
                
                if (!OnInitFired) {
                    base.OnInit(e);
                    
                    //if traceString doesn't end with "-", that means it's PartialCachingControl
                    var isCached = !TraceObjectString.EndsWith("-");
                    
                    LoadControlData(this, new EventArgs());
                    
                    //saving trace for object if it's not PartialCachingControl
                    if ((QPTrace != null) && FormatId != 0 && !isCached) {
                        QUserControlEssential.TraceObject(QPage);
                        QPTrace.TraceString = QPTrace.TraceString + TraceObjectString;
                    }
                    
                    InitUserHandlers(new EventArgs());
                    if (!DisableDataBind) DataBind(); 
                    OnInitFired = true;
                }
            }
        }
        
        public override void DataBind()
        {
            if (UseSimpleInitOrder) {
                base.DataBind();
            }
            else {
                if (!ChildControlsCreated) {
                    base.DataBind();
                    ChildControlsCreated = true;
                }
            }
        }
        
        public string GetObjectFullName(string templateNetName, string objectNetName, string formatNetName)
        {
            return QUserControlEssential.GetObjectFullName(templateNetName, objectNetName, formatNetName);
        }
        
        public string TraceString {
            get { return QPage.QPTrace.TraceString; }
            set { QPage.QPTrace.TraceString = value; }
        }
        
        
        public string TraceStartText {
            get { return QPage.QPTrace.TraceStartText; }
            set { QPage.QPTrace.TraceStartText = value; }
        }
        
        ///Fields methods
        
        public virtual string Field(string key)
        {
            return QPage.Field(key);
        }
        
        public virtual string Field(DataRowView pDataItem, string key)
        {
            return QUserControlEssential.Field(QPage.IsStage, pDataItem.Row, key, "");
        }
        
        public virtual string Field(DataRowView pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.Field(QPage.IsStage, pDataItem.Row, key, defaultValue);
        }
        
        public virtual string Field(DataRow pDataItem, string key)
        {
            return QUserControlEssential.Field(QPage.IsStage, pDataItem, key, "");
        }
        
        public virtual string Field(DataRow pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.Field(QPage.IsStage, pDataItem, key, defaultValue);
        }
        
        public string FormatField(string field)
        {
            return QUserControlEssential.FormatField(field);
        }
        
        public virtual string FieldNS(string key)
        {
            return QPage.Field(key);
        }
        
        public virtual string FieldNS(DataRowView pDataItem, string key)
        {
            return QUserControlEssential.FieldNs(pDataItem.Row, key, "");
        }
        
        public virtual string FieldNS(DataRowView pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.FieldNs(pDataItem.Row, key, defaultValue);
        }
        
        public virtual string FieldNS(DataRow pDataItem, string key)
        {
            return QUserControlEssential.FieldNs(pDataItem, key, "");
        }
        
        public virtual string FieldNS(DataRow pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.FieldNs(pDataItem, key, defaultValue);
        }
        
        ///'''''''''''''''''''''''''''''''''
        ///''''Methods for field's on-fly'''
        ///'''''''''''''''''''''''''''''''''
        
        public string OnFly(DataRowView pDataItem, string key)
        {
            return QUserControlEssential.OnFly(pDataItem.Row, key, "");
        }
        
        public string OnFly(DataRowView pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.OnFly(pDataItem.Row, key, defaultValue);
        }
        
        public string OnFly(DataRow pDataItem, string key)
        {
            return QUserControlEssential.OnFly(pDataItem, key, "");
        }
        
        public string OnFly(DataRow pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.OnFly(pDataItem, key, defaultValue);
        }
        
        public string OnFlyExec(DataRow pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.OnFly(pDataItem, key, defaultValue);
        }
        
        public string OnFlyExec(DataRowView pDataItem, string key, string defaultValue)
        {
            return QUserControlEssential.OnFly(pDataItem.Row, key, defaultValue);
        }
        
        
        public string OnStage(string value, string itemId)
        {
            return QUserControlEssential.OnStage(value, itemId);
        }
        
        public string OnStage(string value, int itemId)
        {
            return QUserControlEssential.OnStage(value, itemId);
        }
        
        public string OnScreen(string value, int itemId)
        {
            return QUserControlEssential.OnScreen(value, itemId);
        }
        
        public string OnScreen(string value, string itemId)
        {
            return QUserControlEssential.OnScreen(value, itemId);
        }
        
        public string OnScreenFlyEdit(string value, int itemId, string fieldName)
        {
            return QUserControlEssential.OnScreenFlyEdit(value, itemId, fieldName);
        }
        
        public string OnScreenFlyEdit(string value, string itemId, string fieldName)
        {
            return QUserControlEssential.OnScreenFlyEdit(value, itemId, fieldName);
        }
        
        public string OnStageFlyEdit(string value, string itemId, string fieldName)
        {
            return QUserControlEssential.OnStageFlyEdit(value, itemId, fieldName);
        }
        
        public string OnStageFlyEdit(string value, int itemId, string fieldName)
        {
            return QUserControlEssential.OnStageFlyEdit(value, itemId, fieldName);
        }
        
        public string OnStageDiv(string value, string fieldName, int itemId, int contentId, string isBorderStatic, bool editable, string attrType, int attrRequired)
        {
            return QUserControlEssential.OnStageDiv(value, fieldName, itemId, contentId, isBorderStatic, editable, attrType, attrRequired);
        }
        
        public string GetReturnStageURL()
        {
            return QUserControlEssential.GetReturnStageUrl();
        }
        
        public string GetFieldUploadUrl(string contentName, string fieldName)
        {
            return QPage.GetFieldUploadUrl(fieldName, GetContentID(contentName));
        }
        
    }
}