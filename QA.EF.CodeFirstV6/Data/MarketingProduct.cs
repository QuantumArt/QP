namespace QA.EF.CodeFirstV6.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class MarketingProduct
    {     
        public int Id { get; set; }
    
        public int STATUS_TYPE_ID { get; set; }
 
        public bool VISIBLE { get; set; }

        public bool ARCHIVE { get; set; }
        public DateTime CREATED { get; set; }

        public DateTime MODIFIED { get; set; }
     
        public int LAST_MODIFIED_BY { get; set; }

        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Alias { get; set; }

        public int? ProductType { get; set; }

        public string Benefit { get; set; }

        public string ShortBenefit { get; set; }

        public string Legal { get; set; }

        public string Description { get; set; }

        public string ShortDescription { get; set; }

        public string Purpose { get; set; }

        public int? Categories { get; set; }

        public int? Family { get; set; }

        [StringLength(255)]
        public string TitleForFamily { get; set; }

        public string CommentForFamily { get; set; }

        public int? PreOpenParamGroups { get; set; }

        public int? Tabs { get; set; }

        public long? MarketingSign { get; set; }

        public int? Modifiers { get; set; }

        public int? OldSiteId { get; set; }

        public int? Images { get; set; }

        public int? ProductUsages { get; set; }

        public virtual ICollection<Product> Products { get; set; }

        public int? ExtServices { get; set; }
    }
}
