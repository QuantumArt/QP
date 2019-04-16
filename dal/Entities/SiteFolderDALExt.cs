using System.ComponentModel.DataAnnotations.Schema;

namespace Quantumart.QP8.DAL.Entities
{
    public partial class SiteFolderDAL
    {
        [NotMapped]
        public bool HasChildren { get; set; }
    }
}
