using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using System;

namespace Quantumart.QP8.BLL.ListItems
{
    public class QpPluginVersionListItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Created { get; set; }

        public string Modified { get; set; }

        public string LastModifiedByLogin { get; set; }
    }
}
