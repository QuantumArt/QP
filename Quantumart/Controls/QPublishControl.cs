using System.Data;

namespace Quantumart.QPublishing.Controls
{

    public abstract class QPublishControl : QUserControl, IQPublishControl
    {
        protected QPublishControl()
        {
            QPublishControlEssential = new QPublishControlEssential(this);
        }

        public QPublishControlEssential QPublishControlEssential { get; set; }


        public DataTable Data
        {
            get { return QPublishControlEssential.Data; }
            set { QPublishControlEssential.Data = value; }
        }

        public long TotalRecords
        {
            get { return QPublishControlEssential.TotalRecords; }
            set { QPublishControlEssential.TotalRecords = value; }
        }
        
        public long AbsoluteTotalRecords
        {
            get { return QPublishControlEssential.AbsoluteTotalRecords; }
            set { QPublishControlEssential.AbsoluteTotalRecords = value; }
        }

        public int Duration
        {
            get { return QPublishControlEssential.Duration; }
            set { QPublishControlEssential.Duration = value; }
        }

        public long RecordsPerPage
        {
            get { return QPublishControlEssential.RecordsPerPage; }
            set { QPublishControlEssential.RecordsPerPage = value; }
        }

        public bool EnableCacheInvalidation
        {
            get { return QPublishControlEssential.EnableCacheInvalidation; }
            set { QPublishControlEssential.EnableCacheInvalidation = value; }
        }
        public bool ForceUnited
        {
            get { return QPublishControlEssential.ForceUnited; }
            set { QPublishControlEssential.ForceUnited = value; }
        }
        public bool UseSchedule
        {
            get { return QPublishControlEssential.UseSchedule; }
            set { QPublishControlEssential.UseSchedule = value; }
        }
        public bool ShowArchive
        {
            get { return QPublishControlEssential.ShowArchive; }
            set { QPublishControlEssential.ShowArchive = value; }
        }
        public string Statuses
        {
            get { return QPublishControlEssential.Statuses; }
            set { QPublishControlEssential.Statuses = value; }
        }
        public string CustomFilter
        {
            get { return QPublishControlEssential.CustomFilter; }
            set { QPublishControlEssential.CustomFilter = value; }
        }
        public string StaticOrder
        {
            get { return QPublishControlEssential.StaticOrder; }
            set { QPublishControlEssential.StaticOrder = value; }
        }
        public string DynamicOrder
        {
            get { return QPublishControlEssential.DynamicOrder; }
            set { QPublishControlEssential.DynamicOrder = value; }
        }
        public string StartRow
        {
            get { return QPublishControlEssential.StartRow; }
            set { QPublishControlEssential.StartRow = value; }
        }
        public string PageSize
        {
            get { return QPublishControlEssential.PageSize; }
            set { QPublishControlEssential.PageSize = value; }
        }
        public bool UseSecurity
        {
            get { return QPublishControlEssential.UseSecurity; }
            set { QPublishControlEssential.UseSecurity = value; }
        }
        public bool UseLevelFiltration
        {
            get { return QPublishControlEssential.UseLevelFiltration; }
            set { QPublishControlEssential.UseLevelFiltration = value; }
        }
        public string StartLevel
        {
            get { return QPublishControlEssential.StartLevel; }
            set { QPublishControlEssential.StartLevel = value; }
        }
        public string EndLevel
        {
            get { return QPublishControlEssential.EndLevel; }
            set { QPublishControlEssential.EndLevel = value; }
        }

        public bool RotateContent
        {
            get { return QPublishControlEssential.RotateContent; }
            set { QPublishControlEssential.RotateContent = value; }
        }

        public bool IsRoot
        {
            get { return QPublishControlEssential.IsRoot; }
            set { QPublishControlEssential.IsRoot = value; }
        }

        public string DynamicVariable
        {
            get { return QPublishControlEssential.DynamicVariable; }
            set { QPublishControlEssential.DynamicVariable = value; }
        }


        public int ContentID
        {
            get { return QPublishControlEssential.ContentId; }
            set { QPublishControlEssential.ContentId = value; }
        }

        public string ContentName
        {
            get { return QPublishControlEssential.ContentName; }
            set { QPublishControlEssential.ContentName = value; }
        }

        public string ContentUploadURL
        {
            get { return QPublishControlEssential.ContentUploadUrl; }
            set { QPublishControlEssential.ContentUploadUrl = value; }
        }

        public string CacheKey
        {
            get { return QPublishControlEssential.CacheKey; }
            set { QPublishControlEssential.CacheKey = value; }
        }

        public string GetBackendUrlForNotification(string defaultBackendUrl)
        {
            return QPublishControlEssential.GetBackendUrlForNotification(defaultBackendUrl);
        }

        public string GetFieldUploadUrl(string fieldName)
        {
            return QPublishControlEssential.GetFieldUploadUrl(fieldName);
        }

        public void FillData()
        {
            QPublishControlEssential.FillData();
        }


    }
}