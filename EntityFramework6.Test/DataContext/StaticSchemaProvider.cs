using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.CodeGeneration.Services;


namespace EntityFramework6.Test.DataContext
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

            schema.Schema.SiteName = "original_site";
            schema.Schema.ReplaceUrls = true;

            schema.Attributes = new List<AttributeInfo>
            {
                new AttributeInfo
                {
                    Id = 38004,
                    ContentId = 618,
                    Name = "StringField",
                    MappedName = "String",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38005,
                    ContentId = 618,
                    Name = "IntegerField",
                    MappedName = "Integer",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 38006,
                    ContentId = 618,
                    Name = "DecimalField",
                    MappedName = "Decimal",
                    LinkId = 0,
                    Type = "Numeric"
                },
                new AttributeInfo
                {
                    Id = 38007,
                    ContentId = 618,
                    Name = "BooleanFiled",
                    MappedName = "Boolean",
                    LinkId = 0,
                    Type = "Boolean"
                },
                new AttributeInfo
                {
                    Id = 38008,
                    ContentId = 618,
                    Name = "DateField",
                    MappedName = "Date",
                    LinkId = 0,
                    Type = "Date"
                },
                new AttributeInfo
                {
                    Id = 38009,
                    ContentId = 618,
                    Name = "TimeField",
                    MappedName = "Time",
                    LinkId = 0,
                    Type = "Time"
                },
                new AttributeInfo
                {
                    Id = 38010,
                    ContentId = 618,
                    Name = "DateTimeField",
                    MappedName = "DateTime",
                    LinkId = 0,
                    Type = "DateTime"
                },
                new AttributeInfo
                {
                    Id = 38011,
                    ContentId = 618,
                    Name = "FileField",
                    MappedName = "File",
                    LinkId = 0,
                    Type = "File"
                },
                new AttributeInfo
                {
                    Id = 38012,
                    ContentId = 618,
                    Name = "ImageField",
                    MappedName = "Image",
                    LinkId = 0,
                    Type = "Image"
                },
                new AttributeInfo
                {
                    Id = 38013,
                    ContentId = 618,
                    Name = "TextBoxField",
                    MappedName = "TextBox",
                    LinkId = 0,
                    Type = "Textbox"
                },
                new AttributeInfo
                {
                    Id = 38014,
                    ContentId = 618,
                    Name = "VisualEditField",
                    MappedName = "VisualEdit",
                    LinkId = 0,
                    Type = "VisualEdit"
                },
                new AttributeInfo
                {
                    Id = 38015,
                    ContentId = 618,
                    Name = "DynamicImageField",
                    MappedName = "DynamicImage",
                    LinkId = 0,
                    Type = "Dynamic Image"
                },
                new AttributeInfo
                {
                    Id = 38016,
                    ContentId = 618,
                    Name = "EnumField",
                    MappedName = "Enum",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38031,
                    ContentId = 620,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38032,
                    ContentId = 621,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38033,
                    ContentId = 622,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38034,
                    ContentId = 623,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38035,
                    ContentId = 624,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38036,
                    ContentId = 625,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38037,
                    ContentId = 626,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38038,
                    ContentId = 626,
                    Name = "Alias",
                    MappedName = "Alias",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38039,
                    ContentId = 627,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 38182,
                    ContentId = 628,
                    Name = "FileItem",
                    MappedName = "FileItem",
                    LinkId = 0,
                    Type = "File"
                },
                new AttributeInfo
                {
                    Id = 38259,
                    ContentId = 694,
                    Name = "SymmetricRelation",
                    MappedName = "SymmetricRelation",
                    LinkId = 100,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 38260,
                    ContentId = 695,
                    Name = "ToSymmetricRelation",
                    MappedName = "ToSymmetricRelation",
                    LinkId = 101,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 39282,
                    ContentId = 993,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 39284,
                    ContentId = 993,
                    Name = "Reference",
                    MappedName = "Reference",
                    LinkId = 149,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 39283,
                    ContentId = 994,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 39290,
                    ContentId = 998,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 39293,
                    ContentId = 998,
                    Name = "Reference",
                    MappedName = "Reference",
                    LinkId = 0,
                    Type = "O2M"
                },
                new AttributeInfo
                {
                    Id = 39291,
                    ContentId = 999,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0,
                    Type = "String"
                },
                new AttributeInfo
                {
                    Id = 39298,
                    ContentId = 1002,
                    Name = "DateValueField",
                    MappedName = "DateValueField",
                    LinkId = 0,
                    Type = "Date"
                },
                new AttributeInfo
                {
                    Id = 39299,
                    ContentId = 1003,
                    Name = "TimeValueField",
                    MappedName = "TimeValueField",
                    LinkId = 0,
                    Type = "Time"
                },
                new AttributeInfo
                {
                    Id = 39300,
                    ContentId = 1004,
                    Name = "DateTimeValueField",
                    MappedName = "DateTimeValueField",
                    LinkId = 0,
                    Type = "DateTime"
                },
                new AttributeInfo
                {
                    Id = 39308,
                    ContentId = 1008,
                    Name = "FileValueField",
                    MappedName = "FileValueField",
                    LinkId = 0,
                    Type = "File"
                },
                new AttributeInfo
                {
                    Id = 39309,
                    ContentId = 1009,
                    Name = "ImageValueField",
                    MappedName = "ImageValueField",
                    LinkId = 0,
                    Type = "Image"
                },
                new AttributeInfo
                {
                    Id = 39310,
                    ContentId = 999,
                    Name = "BackReference",
                    MappedName = "BackReference",
                    LinkId = 0,
                    Type = "M2O"
                },
                new AttributeInfo
                {
                    Id = 39311,
                    ContentId = 695,
                    Name = "BackwardForSymmetricRelation",
                    MappedName = "BackwardForSymmetricRelation",
                    LinkId = 100,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 39312,
                    ContentId = 694,
                    Name = "BackwardForToSymmetricRelation",
                    MappedName = "BackwardForToSymmetricRelation",
                    LinkId = 101,
                    Type = "M2M"
                },
                new AttributeInfo
                {
                    Id = 39313,
                    ContentId = 994,
                    Name = "BackwardForReference",
                    MappedName = "BackwardForReference",
                    LinkId = 149,
                    Type = "M2M"
                },
            };

            var attributesLookup = schema.Attributes.ToLookup(a => a.ContentId, a => a);

            schema.Contents = new List<ContentInfo>
            {
                new ContentInfo
                {
                   Id = 618,
                   MappedName = "AfiellFieldsItem",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[618])
                },
                new ContentInfo
                {
                   Id = 620,
                   MappedName = "Schema",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[620])
                },
                new ContentInfo
                {
                   Id = 621,
                   MappedName = "StringItem",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[621])
                },
                new ContentInfo
                {
                   Id = 622,
                   MappedName = "StringItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[622])
                },
                new ContentInfo
                {
                   Id = 623,
                   MappedName = "StringItemForUnsert",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[623])
                },
                new ContentInfo
                {
                   Id = 624,
                   MappedName = "ItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[624])
                },
                new ContentInfo
                {
                   Id = 625,
                   MappedName = "ItemForInsert",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[625])
                },
                new ContentInfo
                {
                   Id = 626,
                   MappedName = "PublishedNotPublishedItem",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[626])
                },
                new ContentInfo
                {
                   Id = 627,
                   MappedName = "ReplacingPlaceholdersItem",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[627])
                },
                new ContentInfo
                {
                   Id = 628,
                   MappedName = "FileFieldsItem",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[628])
                },
                new ContentInfo
                {
                   Id = 694,
                   MappedName = "SymmetricRelationArticle",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[694])
                },
                new ContentInfo
                {
                   Id = 695,
                   MappedName = "ToSymmetricRelationAtricle",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[695])
                },
                new ContentInfo
                {
                   Id = 993,
                   MappedName = "MtMItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[993])
                },
                new ContentInfo
                {
                   Id = 994,
                   MappedName = "MtMDictionaryForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[994])
                },
                new ContentInfo
                {
                   Id = 998,
                   MappedName = "OtMItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[998])
                },
                new ContentInfo
                {
                   Id = 999,
                   MappedName = "OtMDictionaryForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[999])
                },
                new ContentInfo
                {
                   Id = 1002,
                   MappedName = "DateItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[1002])
                },
                new ContentInfo
                {
                   Id = 1003,
                   MappedName = "TimeItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[1003])
                },
                new ContentInfo
                {
                   Id = 1004,
                   MappedName = "DateTimeItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[1004])
                },
                new ContentInfo
                {
                   Id = 1008,
                   MappedName = "FileItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[1008])
                },
                new ContentInfo
                {
                   Id = 1009,
                   MappedName = "ImageItemForUpdate",
                   UseDefaultFiltration = true,
                   Attributes = new List<AttributeInfo>(attributesLookup[1009])
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
