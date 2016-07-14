namespace QA.EF.CodeFirstV6.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Product
    {
        public int Id { get; set; }

        public int STATUS_TYPE_ID { get; set; }

        public bool VISIBLE { get; set; }
        public bool ARCHIVE { get; set; }

        public DateTime CREATED { get; set; }

        public DateTime MODIFIED { get; set; }

        public int LAST_MODIFIED_BY { get; set; }

        public int? MarketingProduct_ID { get; set; }

        public MarketingProduct MarketingProduct { get; set; }

        public virtual ICollection<Region> Regions { get; set; }

        public int? Parameters { get; set; }

        public int? Type { get; set; }

        public string PDF { get; set; }

        public string Legal { get; set; }

        public string Benefit { get; set; }

        public int? SortOrder { get; set; }

        public int? Modifiers { get; set; }

        public int? MarketingSign { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string ArchiveTitle { get; set; }

        public int? ArchiveTabs { get; set; }

        public string ArchiveNotes { get; set; }

        public int? OldSiteId { get; set; }

        public int? ExtServices { get; set; }


        public ICollection<ProduktyRegionyArticle> ProduktyRegionyArticles { get; set; }
    }
}
