using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.OnScreen;

// ReSharper disable InconsistentNaming
namespace Quantumart.QPublishing.Pages
{
    public interface IQPage
    {
        QPageEssential QPageEssential { get; set; }

        bool UseMultiSiteLogic { get; set; }

        bool IsLocalAssembling { get; set; }

        QScreen QScreen { get; set; }

        QpTrace QPTrace { get; set; }

        string Controls_Folder { get; set; }

        string ControlsFolderName { get; }

        string TemplateNetName { get; set; }

        string TemplateName { get; set; }

        bool IsPreview { get; set; }

        int Expires { get; set; }

        DateTime LastModified { get; set; }

        bool IsLastModifiedDynamic { get; set; }

        bool GenerateTrace { get; set; }

        HttpCacheability HttpCacheability { get; set; }

        HttpCacheRevalidation HttpCacheRevalidation { get; set; }

        TimeSpan ProxyMaxAge { get; set; }

        string CharSet { get; set; }

        Encoding ContentEncoding { get; set; }

        bool IsTest { get; set; }

        bool IsStage { get; set; }

        bool QP_IsInStageMode { get; set; }

        string URLToSave { get; set; }

        string PageFolder { get; set; }

        Mode PageAssembleMode { get; set; }

        void SetLastModified(DataTable dt);

        DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive);

        DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel);

        DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel, bool blnFilterRecords);

        DBConnector Cnn { get; }

        int site_id { get; set; }

        int page_id { get; set; }

        int page_template_id { get; set; }

        Hashtable FieldValuesDictionary { get; set; }

        Hashtable FieldNamesDictionary { get; set; }

        string upload_url { get; set; }

        string site_url { get; set; }

        string absolute_site_url { get; set; }

        bool AbsUploadURL { get; set; }

        string UploadURLPrefix { get; set; }

        int published_status_type_id { get; set; }

        string published_status_name { get; set; }

        void Initialize();

        void HandlePreRender();

        void HandleRender();

        void Initialize(int siteId, int pageId, int pageTemplateId, string uploadUrl, string uploadUrlPrefix, string siteUrl, string pageFileName, string pageFolder);

        void Initialize(int siteId, int pageId, int pageTemplateId, string pageFileName, string pageFolder);

        void Initialize(int siteId);

        void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName);

        void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder, Hashtable pageObjects);

        void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder, Hashtable pageObjects, Hashtable templates);

        void EnableBackendOnScreen();

        void AddLastModifiedHeader();

        void AddLastModifiedHeader(DateTime dt);

        void FillValues();

        void ShowObject(string name);

        void ShowObject(string name, object sender);

        void ShowObject(string name, object sender, object[] parameters);

        void ShowObject(string name, object sender, object[] parameters, IQPage qPage);

        void ShowObjectSimple(string name);

        void ShowObjectSimple(string name, object sender);

        void ShowObjectSimple(string name, object sender, object[] parameters);

        void ShowObjectSimple(string name, object sender, object[] parameters, IQPage qPage);

        void ShowControl(string name);

        void ShowControl(string name, object sender);

        void ShowControl(string name, object sender, object[] parameters);

        void ShowTemplateControl(string name);

        void AddValue(string key, object value);

        void AddObjectValue(string key, object value);

        string DirtyValue(string key);

        string Value(string key);

        string Value(string key, string defaultValue);

        long NumValue(string key);

        string StrValue(string key);

        string InternalStrValue(string valueName);

        Hashtable Values { get; }

        void CallStackOverflow();

        bool IsOrderSqlValid(string orderSql);

        string CleanSQL(string text);

        string Field(string key);

        void AddHeader(string key, string value);

        void SaveURL(string siteId);

        string GetSiteDNS(string siteId);

        void RemoveContentItem(int contentItemId);

        void DeleteContentItem();

        int GetContentID(string contentName);

        int GetContentVirtualType(int contentId);

        string FieldName(string contentName, string fieldName);

        int FieldID(string contentName, string fieldName);

        string InputName(string contentName, string fieldName);

        bool CheckMaxLength(string str, int maxlength);

        string ReplaceHTML(string str);

        void SendNotification(string notificationOn, int contentItemId, string notificationEmail);

        string GetSiteUrl();

        string GetActualSiteUrl();

        string GetContentItemLinkIDs(string linkFieldName, long itemId);

        string GetContentItemLinkIDs(string linkFieldName, string itemId);

        string GetContentItemLinkQuery(string linkFieldName, long itemId);

        string GetContentItemLinkQuery(string linkFieldName, string itemId);

        string GetLinkIDs(string linkFieldName);

        int GetLinkIDForItem(string linkFieldName, int itemId);

        int AddFormToContentWithoutNotification(string contentName, string statusName, int contentItemId);

        int AddFormToContentWithoutNotification(string contentName, string statusName);

        int AddFormToContent(string contentName, string statusName, int contentItemId);

        int AddFormToContent(string contentName, string statusName);

        string GetContentFieldValue(int itemId, string fieldName);

        void UpdateContentItemField(string contentName, string fieldName, int contentItemId);

        void UpdateContentItemField(string contentName, string fieldName, int contentItemId, bool withNotification);

        void UpdateContentItem();

        void UpdateContentItemWithoutNotification();

        void UpdateContentItem(bool updateEmpty, string statusName);

        void UpdateContentItemWithoutNotification(bool updateEmpty, string statusName);

        string GetContentUploadUrl(string contentName);

        string GetContentUploadUrlByID(int contentId);

        string GetContentName(int content_id);

        string GetFieldUploadUrl(string fieldName, int contentId);

        DataTable GetUsersByItemID_And_Permission(int itemId, int permissionLevel);

        string GetInternalCall(string userCall);

        string GetControlUrl(string controlFileName);

        void BeforeFirstCallInitialize();
    }
}
