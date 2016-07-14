using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.EF.CodeFirstV6.Data
{
    public class Region
    {
        public int Id { get; set; }

        public int STATUS_TYPE_ID { get; set; }

        public bool VISIBLE { get; set; }

        public bool ARCHIVE { get; set; }

        public DateTime CREATED { get; set; }

        public DateTime MODIFIED { get; set; }

        public int LAST_MODIFIED_BY { get; set; }

        public string Title { get; set; }

        public string Alias { get; set; }

        public ICollection<Product> Products { get; set; }

        public int? Parent_ID { get; set; }

        public Region Parent { get; set; }

        public ICollection<Region> Children { get; set; }

        public ICollection<Region> AllowedRegions { get; set; }

        public ICollection<Region> DeniedRegions { get; set; }

        public ICollection<Region> AllowedRegionsBackward { get; set; }

        public ICollection<Region> DeniedRegionsBackward { get; set; }

        public ICollection<ProduktyRegionyArticle> ProduktyRegionyArticlesBackward { get; set; }
    }
}
