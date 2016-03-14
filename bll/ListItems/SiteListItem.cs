using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quantumart.QP8.BLL.ListItems
{
    public class SiteListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int IsLive { get; set; }

        public string IsLiveIcon { get; set; }

        public int LockedBy { get; set; }

        public string LockedByFullName { get; set; }

        public string LockedByIcon { get; set; }

        public string LockedByToolTip { get; set; }

        public string Dns { get; set; }

        public string UploadUrl { get; set; }

        public string Created { get; set; }

        public string Modified { get; set; }

        public string LastModifiedByUser { get; set; }
    }
}
