using System.Data;
// ReSharper disable InconsistentNaming
namespace Quantumart.QPublishing.Controls
{

    public interface IQPublishControl
    {

        DataTable Data
        {
            get;
            set;
        }
        long TotalRecords
        {
            get;
            set;
        }
        long AbsoluteTotalRecords
        {
            get;
            set;
        }
        long RecordsPerPage
        {
            get;
            set;
        }
        int ContentID
        {
            get;
            set;
        }
        string ContentName
        {
            get;
            set;
        }

        string ContentUploadURL
        {
            get;
            set;
        }

        string CacheKey
        {
            get;
            set;
        }
        int Duration
        {
            get;
            set;
        }

        bool EnableCacheInvalidation
        {
            get;
            set;
        }
        bool ForceUnited
        {
            get;
            set;
        }
        bool UseSchedule
        {
            get;
            set;
        }
        bool ShowArchive
        {
            get;
            set;
        }
        string Statuses
        {
            get;
            set;
        }
        string CustomFilter
        {
            get;
            set;
        }
        string StaticOrder
        {
            get;
            set;
        }
        string DynamicOrder
        {
            get;
            set;
        }
        string StartRow
        {
            get;
            set;
        }
        string PageSize
        {
            get;
            set;
        }
        bool UseSecurity
        {
            get;
            set;
        }
        bool UseLevelFiltration
        {
            get;
            set;
        }
        string StartLevel
        {
            get;
            set;
        }
        string EndLevel
        {
            get;
            set;
        }
        bool RotateContent
        {
            get;
            set;
        }
        bool IsRoot
        {
            get;
            set;
        }
        string DynamicVariable
        {
            get;
            set;
        }

        string GetBackendUrlForNotification(string defaultBackendUrl);
        string GetFieldUploadUrl(string fieldName);
        void FillData();

    }
}