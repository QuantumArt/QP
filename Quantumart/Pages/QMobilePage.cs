using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI.MobileControls;
using Quantumart.QPublishing.Database;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.OnScreen;

namespace Quantumart.QPublishing.Pages
{
    public class QMobilePage : MobilePage, IQPage
    {
        public QMobilePage()
        {
            QPageEssential = new QPageEssential(this);
        }

        public QPageEssential QPageEssential { get; set; }

        public bool UseMultiSiteLogic
        {
            get { return QPageEssential.UseMultiSiteLogic; }
            set { QPageEssential.UseMultiSiteLogic = value; }
        }

        public bool IsLocalAssembling
        {
            get { return QPageEssential.IsLocalAssembling; }
            set { QPageEssential.IsLocalAssembling = value; }
        }

        public string URLToSave
        {
            get { return QPageEssential.UrlToSave; }
            set { QPageEssential.UrlToSave = value; }
        }

        public string PageFolder
        {
            get { return QPageEssential.PageFolder; }
            set { QPageEssential.PageFolder = value; }
        }

        public QScreen QScreen
        {
            get { return QPageEssential.QScreen; }
            set { QPageEssential.QScreen = value; }
        }

        public QpTrace QPTrace
        {
            get { return QPageEssential.QpTrace; }
            set { QPageEssential.QpTrace = value; }
        }

        public string Controls_Folder
        {
            get { return QPageEssential.PageControlsFolder; }
            set { QPageEssential.PageControlsFolder = value; }
        }

        public string ControlsFolderName => QPageEssential.TemplateControlsFolderPrefix;

        public string TemplateNetName
        {
            get { return QPageEssential.TemplateNetName; }
            set { QPageEssential.TemplateNetName = value; }
        }

        public string TemplateName
        {
            get { return QPageEssential.TemplateName; }
            set { QPageEssential.TemplateName = value; }
        }

        public bool IsPreview
        {
            get { return QPageEssential.IsPreview; }
            set { QPageEssential.IsPreview = value; }
        }

        public int Expires
        {
            get { return QPageEssential.Expires; }
            set { QPageEssential.Expires = value; }
        }
        public DateTime LastModified
        {

            get { return QPageEssential.LastModified; }
            set { QPageEssential.LastModified = value; }
        }
        public bool IsLastModifiedDynamic
        {

            get { return QPageEssential.IsLastModifiedDynamic; }
            set { QPageEssential.IsLastModifiedDynamic = value; }
        }

        public bool GenerateTrace
        {
            get { return QPageEssential.GenerateTrace; }
            set { QPageEssential.GenerateTrace = value; }
        }

        public HttpCacheability HttpCacheability
        {
            get { return QPageEssential.HttpCacheability; }
            set { QPageEssential.HttpCacheability = value; }
        }

        public HttpCacheRevalidation HttpCacheRevalidation
        {
            get { return QPageEssential.HttpCacheRevalidation; }
            set { QPageEssential.HttpCacheRevalidation = value; }
        }

        public TimeSpan ProxyMaxAge
        {
            get { return QPageEssential.ProxyMaxAge; }
            set { QPageEssential.ProxyMaxAge = value; }
        }

        public string CharSet
        {
            get { return QPageEssential.CharSet; }
            set { QPageEssential.CharSet = value; }
        }

        public Encoding ContentEncoding
        {
            get { return QPageEssential.ContentEncoding; }
            set { QPageEssential.ContentEncoding = value; }
        }

        public bool IsTest
        {
            get { return QPageEssential.IsTest; }
            set { QPageEssential.IsTest = value; }
        }

        public bool IsStage
        {
            get { return QPageEssential.IsStage; }
            set { QPageEssential.IsStage = value; }
        }

        public bool QP_IsInStageMode
        {
            get { return QPageEssential.IsStage; }
            set { QPageEssential.IsStage = value; }
        }

        public void SetLastModified(DataTable dt)
        {
            QPageEssential.SetLastModified(dt);
        }

        public DataTable GetContentData(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive)
        {
            return QPageEssential.GetContentData(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive);
        }

        public DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel)
        {
            return QPageEssential.GetContentDataWithSecurity(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive, lngUserId, lngGroupId, intStartLevel, intEndLevel);
        }

        public DataTable GetContentDataWithSecurity(string siteName, string contentName, string whereExpression, string orderExpression, long startRow, long pageSize, ref long totalRecords, byte useSchedule, string statusName, byte showSplittedArticle, byte includeArchive, long lngUserId, long lngGroupId, int intStartLevel, int intEndLevel, bool blnFilterRecords)
        {
            return QPageEssential.GetContentDataWithSecurity(siteName, contentName, whereExpression, orderExpression, startRow, pageSize, ref totalRecords, useSchedule, statusName, showSplittedArticle,
            includeArchive, lngUserId, lngGroupId, intStartLevel, intEndLevel, blnFilterRecords);
        }

        #region "Functions for loading controls"
        public virtual void ShowObject(string name, object sender, object[] parameters, IQPage qPage)
        {
            QPageEssential.ShowObject(name, sender, parameters, qPage, false);
        }

        public void ShowObject(string name, object sender, object[] parameters)
        {
            ShowObject(name, sender, parameters, this);
        }

        public void ShowObject(string name, object sender)
        {
            ShowObject(name, sender, null, this);
        }

        public void ShowObject(string name)
        {
            ShowObject(name, this, null, this);
        }

        public virtual void ShowObjectSimple(string name, object sender, object[] parameters, IQPage qPage)
        {
            QPageEssential.ShowObject(name, sender, parameters, qPage, true);
        }

        public void ShowObjectSimple(string name, object sender, object[] parameters)
        {
            ShowObjectSimple(name, sender, parameters, this);
        }

        public void ShowObjectSimple(string name, object sender)
        {
            ShowObjectSimple(name, sender, null, this);
        }

        public void ShowObjectSimple(string name)
        {
            ShowObjectSimple(name, this, null, this);
        }

        public void ShowControl(string name, object sender, object[] parameters)
        {
            QPageEssential.ShowControl(name, sender, parameters);
        }

        public void ShowControl(string name, object sender)
        {
            QPageEssential.ShowControl(name, sender, null);
        }

        public void ShowControl(string name)
        {
            QPageEssential.ShowControl(name, this, null);
        }

        public void ShowTemplateControl(string name)
        {
            QPageEssential.ShowTemplateControl(name, this);
        }
        #endregion

        public DBConnector Cnn => QPageEssential.Cnn;

        public int site_id
        {
            get { return QPageEssential.site_id; }
            set { QPageEssential.site_id = value; }
        }

        public int page_id
        {
            get { return QPageEssential.page_id; }
            set { QPageEssential.page_id = value; }
        }

        public int page_template_id
        {
            get { return QPageEssential.page_template_id; }
            set { QPageEssential.page_template_id = value; }
        }

        public Mode PageAssembleMode
        {
            get { return QPageEssential.PageAssembleMode; }
            set { QPageEssential.PageAssembleMode = value; }
        }

        public Hashtable FieldValuesDictionary
        {
            get { return QPageEssential.FieldValuesDictionary; }
            set { QPageEssential.FieldValuesDictionary = value; }
        }

        public Hashtable FieldNamesDictionary
        {
            get { return QPageEssential.FieldNamesDictionary; }
            set { QPageEssential.FieldNamesDictionary = value; }
        }

        public string upload_url
        {
            get { return QPageEssential.UploadUrl; }
            set { QPageEssential.UploadUrl = value; }
        }

        public string site_url
        {
            get { return QPageEssential.SiteUrl; }
            set { QPageEssential.SiteUrl = value; }
        }

        public string absolute_site_url
        {
            get { return QPageEssential.AbsoluteSiteUrl; }
            set { QPageEssential.AbsoluteSiteUrl = value; }
        }

        public bool AbsUploadURL
        {
            get { return QPageEssential.AbsUploadUrl; }
            set { QPageEssential.AbsUploadUrl = value; }
        }

        public string UploadURLPrefix
        {
            get { return QPageEssential.UploadUrlPrefix; }
            set { QPageEssential.UploadUrlPrefix = value; }
        }

        public int published_status_type_id
        {
            get { return QPageEssential.PublishedStatusTypeId; }
            set { QPageEssential.PublishedStatusTypeId = value; }
        }

        public string published_status_name
        {
            get { return QPageEssential.PublishedStatusName; }
            set { QPageEssential.PublishedStatusName = value; }
        }

        public void Initialize()
        {
            QPageEssential.HandleInit(this);
        }

        public void HandlePreRender()
        {
            QPageEssential.HandlePreRender();
        }

        public void HandleRender()
        {
            QPageEssential.HandleRender();
        }

        public void Initialize(int siteId, int pageId, int pageTemplateId, string uploadUrl, string uploadUrlPrefix, string siteUrl, string pageFileName, string pageFolder)
        {


            QPageEssential.Initialize(siteId, pageId, pageTemplateId, uploadUrl, uploadUrlPrefix, siteUrl, pageFileName, pageFolder);
        }

        public void Initialize(int siteId, int pageId, int pageTemplateId, string pageFileName, string pageFolder)
        {
            QPageEssential.Initialize(siteId, pageId, pageTemplateId, pageFileName, pageFolder);
        }

        public void Initialize(int siteId)
        {
            QPageEssential.Initialize(siteId);
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName)
        {


            QPageEssential.Initialize(siteId, uploadUrl, siteUrl, pageFileName, TemplateNetName);
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder, Hashtable pageObjects)
        {


            QPageEssential.Initialize(siteId, uploadUrl, siteUrl, pageFileName, templateNetName, pageFolder);
        }

        public void Initialize(int siteId, string uploadUrl, string siteUrl, string pageFileName, string templateNetName, string pageFolder, Hashtable pageObjects, Hashtable templates)
        {


            QPageEssential.Initialize(siteId, uploadUrl, siteUrl, pageFileName, templateNetName, pageFolder);
        }

        public void AddLastModifiedHeader()
        {
            AddLastModifiedHeader(LastModified);
        }

        public void AddLastModifiedHeader(DateTime dt)
        {
            Response.Cache.SetLastModified(dt);
        }

        public void EnableBackendOnScreen()
        {
            QPageEssential.EnableBackendOnScreen(this, site_id);
        }

        public void FillValues()
        {
            QPageEssential.FillValues();
        }

        public void AddValue(string key, object value)
        {
            QPageEssential.AddValue(key, value);
        }

        public void AddObjectValue(string key, object value)
        {
            QPageEssential.AddObjectValue(key, value);
        }

        public string DirtyValue(string key)
        {
            return QPageEssential.DirtyValue(key);
        }

        public string Value(string key)
        {
            return QPageEssential.Value(key);
        }

        public string Value(string key, string defaultValue)
        {
            return QPageEssential.Value(key, defaultValue);
        }

        public long NumValue(string key)
        {
            return QPageEssential.NumValue(key);
        }

        public string StrValue(string key)
        {
            return QPageEssential.StrValue(key);
        }

        public string InternalStrValue(string valueName)
        {
            return QPageEssential.InternalStrValue(valueName);
        }

        public Hashtable Values => QPageEssential.Values;

        public void CallStackOverflow()
        {
            QPageEssential.CallStackOverflow();
        }

        public bool IsOrderSqlValid(string orderSql)
        {
            return QPageEssential.IsOrderSqlValid(orderSql);
        }

        public string CleanSQL(string text)
        {
            return QPageEssential.CleanSql(text);
        }

        public string Field(string key)
        {
            return QPageEssential.Field(key);
        }

        public void AddHeader(string key, string value)
        {
            QPageEssential.AddHeader(key, value);
        }

        public void SaveURL(string siteId)
        {
            QPageEssential.SaveUrl(siteId);
        }

        public string GetSiteDNS(string siteId)
        {
            return QPageEssential.GetSiteDns(siteId);
        }

        public string GetInternalCall(string userCall)
        {
            return QPageEssential.GetInternalCall(userCall);
        }

        public virtual void BeforeFirstCallInitialize()
        {

        }

        public string GetControlUrl(string controlFileName)
        {
            return QPageEssential.GetControlUrl(controlFileName);
        }

        #region "form.inc"
        public void RemoveContentItem(int contentItemId)
        {
            QPageEssential.RemoveContentItem(contentItemId);
        }

        public void DeleteContentItem()
        {
            QPageEssential.DeleteContentItem();
        }

        public int GetContentID(string contentName)
        {
            return QPageEssential.GetContentId(contentName);
        }

        public int GetContentVirtualType(int contentId)
        {
            return QPageEssential.GetContentVirtualType(contentId);
        }

        public string FieldName(string contentName, string fieldName)
        {
            return QPageEssential.FieldName(contentName, fieldName);
        }

        public int FieldID(string contentName, string fieldName)
        {
            return QPageEssential.FieldId(contentName, fieldName);
        }

        public string InputName(string contentName, string fieldName)
        {
            return QPageEssential.InputName(contentName, fieldName);
        }

        public bool CheckMaxLength(string str, int maxlength)
        {
            return QPageEssential.CheckMaxLength(str, maxlength);
        }

        public string ReplaceHTML(string str)
        {
            return QPageEssential.ReplaceHtml(str);
        }

        public void SendNotification(string notificationOn, int contentItemId, string notificationEmail)
        {
            QPageEssential.SendNotification(notificationOn, contentItemId, notificationEmail);
        }

        public string GetSiteUrl()
        {
            return QPageEssential.GetSiteUrl();
        }

        public string GetActualSiteUrl()
        {
            return QPageEssential.GetActualSiteUrl();
        }

        public string GetContentItemLinkIDs(string linkFieldName, long itemId)
        {
            return QPageEssential.GetContentItemLinkIDs(linkFieldName, itemId);
        }

        public string GetContentItemLinkIDs(string linkFieldName, string itemId)
        {
            return QPageEssential.GetContentItemLinkIDs(linkFieldName, itemId);
        }

        public string GetContentItemLinkQuery(string linkFieldName, long itemId)
        {
            return QPageEssential.GetContentItemLinkQuery(linkFieldName, itemId);
        }

        public string GetContentItemLinkQuery(string linkFieldName, string itemId)
        {
            return QPageEssential.GetContentItemLinkQuery(linkFieldName, itemId);
        }

        public string GetLinkIDs(string linkFieldName)
        {
            return QPageEssential.GetLinkIDs(linkFieldName);
        }

        public int GetLinkIDForItem(string linkFieldName, int itemId)
        {
            return QPageEssential.GetLinkIdForItem(linkFieldName, itemId);
        }

        public string GetContentFieldValue(int itemId, string fieldName)
        {
            return QPageEssential.GetContentFieldValue(itemId, fieldName);
        }

        public int AddFormToContentWithoutNotification(string contentName, string statusName, int contentItemId)
        {
            return QPageEssential.AddFormToContentWithoutNotification(contentName, statusName, contentItemId);
        }

        public int AddFormToContent(string contentName, string statusName, int contentItemId)
        {
            return QPageEssential.AddFormToContent(contentName, statusName, contentItemId);
        }

        public int AddFormToContentWithoutNotification(string contentName, string statusName)
        {
            return AddFormToContentWithoutNotification(contentName, statusName, 0);
        }

        public int AddFormToContent(string contentName, string statusName)
        {
            return AddFormToContent(contentName, statusName, 0);
        }

        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId)
        {
            QPageEssential.UpdateContentItemField(contentName, fieldName, contentItemId);
        }

        public void UpdateContentItemField(string contentName, string fieldName, int contentItemId, bool withNotification)
        {
            QPageEssential.UpdateContentItemField(contentName, fieldName, contentItemId, withNotification);
        }

        public void UpdateContentItem()
        {
            QPageEssential.UpdateContentItem(true, "");
        }

        public void UpdateContentItemWithoutNotification()
        {
            QPageEssential.UpdateContentItemWithoutNotification(false, "");
        }

        public void UpdateContentItem(bool updateEmpty, string statusName)
        {
            QPageEssential.UpdateContentItem(updateEmpty, statusName);
        }

        public void UpdateContentItemWithoutNotification(bool updateEmpty, string statusName)
        {
            QPageEssential.UpdateContentItemWithoutNotification(updateEmpty, statusName);
        }

        public string GetContentUploadUrl(string contentName)
        {
            return QPageEssential.GetContentUploadUrl(contentName);
        }

        public string GetContentUploadUrlByID(int contentId)
        {
            return QPageEssential.GetContentUploadUrlById(contentId);
        }

        public string GetContentName(int contentId)
        {
            return QPageEssential.GetContentName(contentId);
        }

        public string GetFieldUploadUrl(string fieldName, int contentId)
        {
            return QPageEssential.GetFieldUploadUrl(fieldName, contentId);
        }
        #endregion

        public DataTable GetUsersByItemID_And_Permission(int itemId, int permissionLevel)
        {
            return QPageEssential.GetUsersByItemID_And_Permission(itemId, permissionLevel);
        }
    }
}
