using System.Data;

namespace Quantumart.QP8.DAL
{
    public class RelationSecurityPathItem
    {
        public RelationSecurityPathItem()
        {
        }

        public RelationSecurityPathItem(DataRow row)
        {
            AttributeName = row.Field<string>("attribute_name");
            AggAttributeName = row.Field<string>("agg_attribute_name");
            AttributeId = (int)row.Field<decimal>("attribute_id");
            ContentId = (int)row.Field<decimal>("content_id");
            RelContentId = (int?)row.Field<decimal?>("rel_content_id") ?? 0;
            LinkId = (int?)row.Field<decimal?>("link_id");
            IsClassifier = row.Field<bool>("is_classifier");
        }

        public int ContentId { get; set; }

        public int RelContentId { get; set; }

        public string AttributeName { get; set; }

        public string AggAttributeName { get; set; }

        public int AttributeId { get; set; }

        public int? LinkId { get; set; }

        public int Order { get; set; }

        public int JoinOrder { get; set; }

        public bool IsClassifier { get; set; }

        public RelationSecurityPathItem[] Extensions { get; set; }

        public RelationSecurityPathItem[] Secondary { get; set; }
    }
}
