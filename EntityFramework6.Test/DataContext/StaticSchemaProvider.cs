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

			schema.Attributes = new List<AttributeInfo>
            {
                new AttributeInfo
                {
                    Id = 38004,
                    ContentId = 618,
                    Name = "StringField",
                    MappedName = "String",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38005,
                    ContentId = 618,
                    Name = "IntegerField",
                    MappedName = "Integer",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38006,
                    ContentId = 618,
                    Name = "DecimalField",
                    MappedName = "Decimal",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38007,
                    ContentId = 618,
                    Name = "BooleanFiled",
                    MappedName = "Boolean",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38008,
                    ContentId = 618,
                    Name = "DateField",
                    MappedName = "Date",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38009,
                    ContentId = 618,
                    Name = "TimeField",
                    MappedName = "Time",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38010,
                    ContentId = 618,
                    Name = "DateTimeField",
                    MappedName = "DateTime",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38011,
                    ContentId = 618,
                    Name = "FileField",
                    MappedName = "File",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38012,
                    ContentId = 618,
                    Name = "ImageField",
                    MappedName = "Image",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38013,
                    ContentId = 618,
                    Name = "TextBoxField",
                    MappedName = "TextBox",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38014,
                    ContentId = 618,
                    Name = "VisualEditField",
                    MappedName = "VisualEdit",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38015,
                    ContentId = 618,
                    Name = "DynamicImageField",
                    MappedName = "DynamicImage",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38016,
                    ContentId = 618,
                    Name = "EnumField",
                    MappedName = "Enum",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38031,
                    ContentId = 620,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38032,
                    ContentId = 621,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38033,
                    ContentId = 622,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38034,
                    ContentId = 623,
                    Name = "StringValueField",
                    MappedName = "StringValue",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38037,
                    ContentId = 626,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38038,
                    ContentId = 626,
                    Name = "Alias",
                    MappedName = "Alias",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38039,
                    ContentId = 627,
                    Name = "Title",
                    MappedName = "Title",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38182,
                    ContentId = 628,
                    Name = "FileItem",
                    MappedName = "FileItem",
                    LinkId = 0
                },
                new AttributeInfo
                {
                    Id = 38259,
                    ContentId = 694,
                    Name = "SymmetricRelation",
                    MappedName = "SymmetricRelation",
                    LinkId = 100
                },
                new AttributeInfo
                {
                    Id = 38260,
                    ContentId = 695,
                    Name = "ToSymmetricRelation",
                    MappedName = "ToSymmetricRelation",
                    LinkId = 101
                },
                new AttributeInfo
                {
                    Id = 38261,
                    ContentId = 695,
                    Name = "BackwardForSymmetricRelation",
                    MappedName = "BackwardForSymmetricRelation",
                    LinkId = 100
                },
                new AttributeInfo
                {
                    Id = 38262,
                    ContentId = 694,
                    Name = "BackwardForToSymmetricRelation",
                    MappedName = "BackwardForToSymmetricRelation",
                    LinkId = 101
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
