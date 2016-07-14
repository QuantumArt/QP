using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QA.EF.CodeFirstV6.Data
{
    public class ProduktyRegionyArticle
    {
        public int Product_ID { get; set; }
        public int Region_ID { get; set; }
        public Product Product { get; set; }
        public Region Region { get; set; }

    }
}
