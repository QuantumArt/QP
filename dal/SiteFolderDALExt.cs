using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Quantumart.QP8.DAL
{
    public partial class SiteFolderDAL
    {
        [NotMapped]
        public bool HasChildren { get; set; }
    }
}
