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
