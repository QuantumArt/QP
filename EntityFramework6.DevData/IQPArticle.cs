using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework6.DevData
{
    ///<summary>
    /// An interface for all contents
    ///</summary>
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

        ///<summary>
        /// Method used for initialization entities after them to be loaded from database
        ///</summary>
        void OnMaterialized(IQPLibraryService context);

        Hashtable Pack(IQPFormService context, params string[] propertyNames);
    }  
}
