using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.CodeGeneration.Services;


namespace Quantumart.QP8.EntityFramework6.DevData
{
    public class StaticSchemaProvider : ISchemaProvider
    {
       public StaticSchemaProvider()
       {
       }

        #region ISchemaProvider implementation
        public ModelReader GetSchema()
        {
            var schema = new ModelReader();

            schema.Schema.SiteName = "Product Catalog";
            schema.Schema.ReplaceUrls = true;

            schema.Attributes = new List<AttributeInfo>
            {
                new AttributeInfo
                {
                    Id = 1110,
                    ContentId = 287,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1112,
                    ContentId = 287,
                    Name = "Alias",
                    MappedName = "Alias",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1134,
                    ContentId = 287,
                    Name = "ProductType",
                    MappedName = "ProductType",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1124,
                    ContentId = 287,
                    Name = "Benefit",
                    MappedName = "Benefit",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1302,
                    ContentId = 287,
                    Name = "ShortBenefit",
                    MappedName = "ShortBenefit",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1125,
                    ContentId = 287,
                    Name = "Legal",
                    MappedName = "Legal",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1126,
                    ContentId = 287,
                    Name = "Description",
                    MappedName = "Description",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1303,
                    ContentId = 287,
                    Name = "ShortDescription",
                    MappedName = "ShortDescription",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1127,
                    ContentId = 287,
                    Name = "Purpose",
                    MappedName = "Purpose",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1201,
                    ContentId = 287,
                    Name = "Family",
                    MappedName = "Family_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1287,
                    ContentId = 287,
                    Name = "TitleForFamily",
                    MappedName = "TitleForFamily",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1288,
                    ContentId = 287,
                    Name = "CommentForFamily",
                    MappedName = "CommentForFamily",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 1297,
                    ContentId = 287,
                    Name = "MarketingSign",
                    MappedName = "MarketingSign_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1328,
                    ContentId = 287,
                    Name = "OldSiteId",
                    MappedName = "OldSiteId",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1604,
                    ContentId = 287,
                    Name = "Products",
                    MappedName = "Products",
                    LinkId = 0,
                    Type = "M2O"
                },
                new AttributeInfo
                {
                    Id = 1115,
                    ContentId = 288,
                    Name = "MarketingProduct",
                    MappedName = "MarketingProduct",
                    LinkId = 0,
                    Type = "O2M"
                },
                new AttributeInfo
                {
                    Id = 1228,
                    ContentId = 288,
                    Name = "Regions",
                    MappedName = "Regions",
                    LinkId = 21,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1193,
                    ContentId = 288,
                    Name = "Parameters",
                    MappedName = "Parameters",
                    LinkId = 0,
                    Type = "M2O"
                },
                new AttributeInfo
                {
                    Id = 1233,
                    ContentId = 288,
                    Name = "Type",
                    MappedName = "Type",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1229,
                    ContentId = 288,
                    Name = "PDF",
                    MappedName = "PDF",
                    LinkId = 0,
                    Type = "File"
                },
                new AttributeInfo
                {
                    Id = 1230,
                    ContentId = 288,
                    Name = "Legal",
                    MappedName = "Legal",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 1231,
                    ContentId = 288,
                    Name = "Benefit",
                    MappedName = "Benefit",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 1232,
                    ContentId = 288,
                    Name = "SortOrder",
                    MappedName = "SortOrder",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1296,
                    ContentId = 288,
                    Name = "MarketingSign",
                    MappedName = "MarketingSign_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1358,
                    ContentId = 288,
                    Name = "StartDate",
                    MappedName = "StartDate",
                    LinkId = 0,
                    Type = "Date"
                },
                new AttributeInfo
                {
                    Id = 1359,
                    ContentId = 288,
                    Name = "EndDate",
                    MappedName = "EndDate",
                    LinkId = 0,
                    Type = "Date"
                },
                new AttributeInfo
                {
                    Id = 1160,
                    ContentId = 288,
                    Name = "ArchiveTitle",
                    MappedName = "ArchiveTitle",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1314,
                    ContentId = 288,
                    Name = "ArchiveNotes",
                    MappedName = "ArchiveNotes",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1327,
                    ContentId = 288,
                    Name = "OldSiteId",
                    MappedName = "OldSiteId",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1119,
                    ContentId = 291,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 1172,
                    ContentId = 291,
                    Name = "Product",
                    MappedName = "Product",
                    LinkId = 0,
                    Type = "O2M"
                },
                new AttributeInfo
                {
                    Id = 1170,
                    ContentId = 291,
                    Name = "Group",
                    MappedName = "GroupMapped_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1169,
                    ContentId = 291,
                    Name = "BaseParameter",
                    MappedName = "BaseParameter_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1396,
                    ContentId = 291,
                    Name = "Zone",
                    MappedName = "Zone_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1397,
                    ContentId = 291,
                    Name = "Direction",
                    MappedName = "Direction_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1171,
                    ContentId = 291,
                    Name = "SortOrder",
                    MappedName = "SortOrder",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1175,
                    ContentId = 291,
                    Name = "NumValue",
                    MappedName = "NumValue",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1173,
                    ContentId = 291,
                    Name = "Value",
                    MappedName = "Value",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1178,
                    ContentId = 291,
                    Name = "Unit",
                    MappedName = "Unit_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1174,
                    ContentId = 291,
                    Name = "Legal",
                    MappedName = "Legal",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1300,
                    ContentId = 291,
                    Name = "ShortTitle",
                    MappedName = "ShortTitle",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 1301,
                    ContentId = 291,
                    Name = "ShortValue",
                    MappedName = "ShortValue",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 1373,
                    ContentId = 291,
                    Name = "MatrixParameter",
                    MappedName = "MatrixParameter_ID",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1362,
                    ContentId = 291,
                    Name = "OldSiteId",
                    MappedName = "OldSiteId",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1137,
                    ContentId = 294,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1138,
                    ContentId = 294,
                    Name = "Parent",
                    MappedName = "Parent",
                    LinkId = 0,
                    Type = "O2M"
                },
                new AttributeInfo
                {
                    Id = 1139,
                    ContentId = 294,
                    Name = "Alias",
                    MappedName = "Alias",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1337,
                    ContentId = 294,
                    Name = "OldSiteId",
                    MappedName = "OldSiteId",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1659,
                    ContentId = 294,
                    Name = "AllowedRegions",
                    MappedName = "AllowedRegions",
                    LinkId = 71,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1660,
                    ContentId = 294,
                    Name = "DeniedRegions",
                    MappedName = "DeniedRegions",
                    LinkId = 72,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1192,
                    ContentId = 305,
                    Name = "Product",
                    MappedName = "Product",
                    LinkId = 0,
                    Type = "O2M"
                },
                new AttributeInfo
                {
                    Id = 1441,
                    ContentId = 305,
                    Name = "SplitInternetDeviceCount",
                    MappedName = "SplitInternetDeviceCount",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1598,
                    ContentId = 349,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1599,
                    ContentId = 349,
                    Name = "Value",
                    MappedName = "ValueMapped",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 1657,
                    ContentId = 349,
                    Name = "RelatedSettings",
                    MappedName = "RelatedSettings",
                    LinkId = 69,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1668,
                    ContentId = 349,
                    Name = "DecimalValue",
                    MappedName = "DecimalValue",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 1669,
                    ContentId = 294,
                    Name = "Children",
                    MappedName = "Children",
                    LinkId = 0,
                    Type = "M2O"
                },
                new AttributeInfo
                {
                    Id = 1670,
                    ContentId = 288,
                    Name = "MobileTariffs",
                    MappedName = "MobileTariffs",
                    LinkId = 0,
                    Type = "M2O"
                },
                new AttributeInfo
                {
                    Id = 1671,
                    ContentId = 294,
                    Name = "BackwardForRegions",
                    MappedName = "BackwardForRegions",
                    LinkId = 21,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1672,
                    ContentId = 349,
                    Name = "BackwardForRelatedSettings",
                    MappedName = "BackwardForRelatedSettings",
                    LinkId = 69,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1673,
                    ContentId = 294,
                    Name = "BackwardForAllowedRegions",
                    MappedName = "BackwardForAllowedRegions",
                    LinkId = 71,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 1674,
                    ContentId = 294,
                    Name = "BackwardForDeniedRegions",
                    MappedName = "BackwardForDeniedRegions",
                    LinkId = 72,
                    Type = "M2M"
                },
            };

            var attributesLookup = schema.Attributes.ToLookup(a => a.ContentId, a => a);

            schema.Contents = new List<ContentInfo>
            {
                new ContentInfo
                {
                   Id = 287,
                   MappedName = "MarketingProduct",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[287])
                },
                new ContentInfo
                {
                   Id = 288,
                   MappedName = "Product",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[288])
                },
                new ContentInfo
                {
                   Id = 291,
                   MappedName = "ProductParameter",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[291])
                },
                new ContentInfo
                {
                   Id = 294,
                   MappedName = "Region",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[294])
                },
                new ContentInfo
                {
                   Id = 305,
                   MappedName = "MobileTariff",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[305])
                },
                new ContentInfo
                {
                   Id = 349,
                   MappedName = "Setting",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[349])
                },
            };

            schema.Contents.ForEach(c => c.Attributes.ForEach(a => a.Content = c));

            return schema;
        }

        public object GetCacheKey()
        {
            return null;
        }
        #endregion
    }
}
