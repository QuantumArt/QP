using System;
using System.Collections;

namespace Quantumart.QP8.EntityFramework6.Data
{
    public interface IQPArticle
    {
        int Id { get; set; }
        int StatusTypeId { get; set; }
        bool Visible { get; set; }
        bool Archive { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
        int LastModifiedBy { get; set; }
        StatusType StatusType { get; set; }

        void OnMaterialized(IQPLibraryService context);

        Hashtable Pack(IQPFormService context, params string[] propertyNames);
    }  
}
