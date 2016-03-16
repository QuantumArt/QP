using System;
using System.Collections;
using System.Data;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Pages;
// ReSharper disable InconsistentNaming

namespace Quantumart.QPublishing.Controls
{
    
    public interface IQUserControl
    {
        
        IQPage QPage {
            get;
            set;
        }
        bool DisableDataBind {
            get;
            set;
        }
        bool UseSimpleInitOrder {
            get;
            set;
        }
        DateTime TraceStartTime {
            get;
            set;
        }
        string TraceObjectString {
            get;
            set;
        }
        string TraceString {
            get;
            set;
        }
        string UndefTraceString {
            get;
            set;
        }
        string TraceStartText {
            get;
            set;
        }
        Hashtable QpDefValues {
            get;
            set;
        }
        void AddQpDefValue(string key, object value);
        string QpDefValue(string key);
        int FormatId {
            get;
            set;
        }
        bool OnInitFired {
            get;
            set;
        }
        QpTrace QPTrace {
            get;
            set;
        }
        void OnLoadControl(object sender, ref string objectCallName);
        bool IsStage {
            get;
        }
        bool IsTest {
            get;
        }
        
        void ShowObject(string name);
        void ShowObject(string name, object sender);
        void ShowObject(string name, object sender, object[] parameters);
        
        void ShowObjectSimple(string name);
        void ShowObjectSimple(string name, object sender);
        void ShowObjectSimple(string name, object sender, object[] parameters);
        
        
        void ShowObjectNS(string name);
        void ShowObjectNS(string name, object sender);
        void ShowObjectNS(string name, object sender, object[] parameters);
        void ShowControl(string name);
        void ShowControl(string name, object sender);
        void ShowControl(string name, object sender, object[] parameters);
        void ShowControlNS(string name);
        void ShowControlNS(string name, object sender);
        void ShowControlNS(string name, object sender, object[] parameters);
        DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, 
        byte includeArchive);
        DBConnector Cnn {
            get;
        }
        DataTable GetData(string queryString);
        int site_id {
            get;
        }
        string site_url {
            get;
        }
        string absolute_site_url {
            get;
        }
        int page_id {
            get;
        }
        string upload_url {
            get;
        }
        bool AbsUploadURL {
            get;
        }
        string UploadURLPrefix {
            get;
        }
        void AddValue(string key, object value);
        void AddObjectValue(string key, object value);
        string Value(string key);
        string Value(string key, string defaultValue);
        long NumValue(string key);
        string StrValue(string key);
        string InternalStrValue(string valueName);
        Hashtable Values {
            get;
        }
        string DirtyValue(string key);
        void RemoveContentItem(int contentItemId);
        void DeleteContentItem();
        int GetContentID(string contentName);
        string FieldName(string contentName, string fieldName);
        int FieldID(string contentName, string fieldName);
        bool CheckMaxLength(string str, int maxlength);
        string ReplaceHTML(string str);
        void SendNotification(string notificationOn, int contentItemId, string notificationEmail);
        string GetSiteUrl();
        string GetActualSiteUrl();
        string GetContentItemLinkIDs(string linkFieldName, long itemID);
        string GetContentItemLinkIDs(string linkFieldName, string itemID);
        string GetContentItemLinkQuery(string linkFieldName, long itemID);
        string GetContentItemLinkQuery(string linkFieldName, string itemID);
        string GetLinkIDs(string LinkFieldName);
        int GetLinkIDForItem(string LinkFieldName, int ItemID);
        string GetContentFieldValue(int ItemId, string fieldName);
        int AddFormToContentWithoutNotification(string content_name, string statusName, int contentItemId);
        int AddFormToContentWithoutNotification(string content_name, string statusName);
        int AddFormToContent(string content_name, string statusName, int contentItemId);
        int AddFormToContent(string content_name, string statusName);
        void UpdateContentItemField(string content_name, string fieldName, int contentItemId);
        void UpdateContentItemField(string content_name, string fieldName, int contentItemId, bool withNotification);
        void UpdateContentItem();
        void UpdateContentItemWithoutNotification();
        void UpdateContentItem(bool updateEmpty, string statusName);
        void UpdateContentItemWithoutNotification(bool updateEmpty, string statusName);
        string GetContentUploadUrl(string content_name);
        string GetContentUploadUrlByID(int contentId);
        string GetContentName(int content_id);
        Hashtable FieldValuesDictionary {
            get;
            set;
        }
        Hashtable FieldNamesDictionary {
            get;
            set;
        }
        int published_status_type_id {
            get;
        }
        string published_status_name {
            get;
        }
        void LoadControlData(object sender, EventArgs e);
        void InitUserHandlers(EventArgs e);
        string GetObjectFullName(string templateNetName, string objectNetName, string formatNetName);
        
        ///Fields methods'''
        string Field(string key);
        string Field(DataRowView pDataItem, string key);
        string Field(DataRowView pDataItem, string key, string defaultValue);
        string Field(DataRow pDataItem, string key);
        string Field(DataRow pDataItem, string key, string defaultValue);
        
        string FormatField(string field);
        
        string FieldNS(string key);
        string FieldNS(DataRowView pDataItem, string key);
        string FieldNS(DataRowView pDataItem, string key, string defaultValue);
        string FieldNS(DataRow pDataItem, string key);
        string FieldNS(DataRow pDataItem, string key, string defaultValue);
        
        ///Methods for OnScreen'''
        
        string OnFly(DataRowView pDataItem, string key);
        string OnFly(DataRowView pDataItem, string key, string defaultValue);
        string OnFly(DataRow pDataItem, string key);
        string OnFly(DataRow pDataItem, string key, string defaultValue);
        string OnFlyExec(DataRow pDataItem, string key, string defaultValue);
        string OnFlyExec(DataRowView pDataItem, string key, string defaultValue);
        
        string OnStage(string value, string ItemID);
        string OnStage(string value, int ItemID);
        string OnScreen(string value, int ItemID);
        string OnScreen(string value, string ItemID);
        string OnScreenFlyEdit(string value, int ItemID, string fieldName);
        string OnScreenFlyEdit(string value, string ItemID, string fieldName);
        string OnStageFlyEdit(string value, string ItemID, string fieldName);
        string OnStageFlyEdit(string value, int ItemID, string fieldName);
        string OnStageDiv(string value, string fieldName, int ItemID, int contentId, string isBorderStatic, bool editable, string attrType, int attrRequired);
        
        string GetReturnStageURL();
        void SimulateOnInit(EventArgs e);
        string GetFieldUploadUrl(string contentName, string fieldName);
        
    }
}
