using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.CodeGeneration.Services;
using System.Data.Common;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class DatabaseSchemaProvider : ISchemaProvider
    {
        #region Queries
        private const string ContentQuery = @"
            select
	            c.CONTENT_ID,
	            c.NET_CONTENT_NAME,
	            c.USE_DEFAULT_FILTRATION
            from
	            CONTENT c
            where
	            c.SITE_ID = @siteId";

        private const string AttributeQuery = @"
            select
	            a.ATTRIBUTE_ID,
	            a.CONTENT_ID,
	            c.NET_CONTENT_NAME,
                a.ATTRIBUTE_NAME,
	            a.NET_ATTRIBUTE_NAME,
	            a.link_id,
                t.[TYPE_NAME]
            from
	            CONTENT_ATTRIBUTE a
	            join CONTENT c on a.CONTENT_ID = c.CONTENT_ID
                join ATTRIBUTE_TYPE t on a.ATTRIBUTE_TYPE_ID = t.ATTRIBUTE_TYPE_ID
            where
	            c.SITE_ID = @siteId";
        #endregion

        private readonly string _siteName;
        private readonly DbConnection _connection;

        public DatabaseSchemaProvider(string siteName, DbConnection connection)
        {
            _siteName = siteName;
            _connection = connection;
        }

        #region ISchemaProvider implementation
        public ModelReader GetSchema()
        {
            var connector = new DBConnector(_connection);
            int siteId = connector.GetSiteId(_siteName);
            bool replaceUrls;

            using (var cmd = new SqlCommand("SELECT TOP 1 REPLACE_URLS FROM SITE WHERE SITE_ID = @siteId"))
            {
                cmd.Parameters.AddWithValue("@siteId", siteId);
                replaceUrls = (bool)connector.GetRealScalarData(cmd);
            }

            var attributes = GetAttributes(connector, siteId);
            var contents = GetContents(connector, siteId, attributes);

            var model = new ModelReader();

            model.Schema.ReplaceUrls = replaceUrls;
            model.Schema.SiteName = _siteName;
            model.Attributes.AddRange(attributes);
            model.Contents.AddRange(contents);

            return model;
        }

        public object GetCacheKey()
        {
            return new { _siteName, _connection.ConnectionString };
        }
        #endregion

        #region Private methods
        private ContentInfo[] GetContents(DBConnector connector, int siteId, AttributeInfo[] attributes)
        {
            var command = new SqlCommand(ContentQuery);
            command.Parameters.AddWithValue("@siteId", siteId);

            var attributesLookup = attributes.ToLookup(a => a.ContentId, a => a);

            var contents = connector
                .GetRealData(command)
                .AsEnumerable()
                .Select(row => {
                    var contentId = (int)row.Field<decimal>("CONTENT_ID");
                    var mappedName = row.Field<string>("NET_CONTENT_NAME");
                    var useDefaultFiltration = row.Field<bool>("USE_DEFAULT_FILTRATION");

                    var content = new ContentInfo
                    {
                        Id = contentId,
                        MappedName = mappedName,
                        UseDefaultFiltration = useDefaultFiltration,
                        Attributes = new List<AttributeInfo>(attributesLookup[contentId])
                    };

                    content.Attributes.ForEach(a => a.Content = content);

                    return content;
                })
                .ToArray();

            return contents;
        }

        private AttributeInfo[] GetAttributes(DBConnector connector, int siteId)
        {
            var command = new SqlCommand(AttributeQuery);
            command.Parameters.AddWithValue("@siteId", siteId);

            var attributes = connector
                .GetRealData(command)
                .AsEnumerable()
                .Select(row => new AttributeInfo
                {
                    Id = (int)row.Field<decimal>("ATTRIBUTE_ID"),
                    ContentId = (int)row.Field<decimal>("CONTENT_ID"),
                    Name = row.Field<string>("ATTRIBUTE_NAME"),
                    MappedName = row.Field<string>("NET_ATTRIBUTE_NAME"),
                    LinkId = (int)(row.Field<decimal?>("LINK_ID")?? 0),
                    Type = row.Field<string>("TYPE_NAME")
                })
                .ToArray();

            foreach (var a in attributes)
            {
                a.Type = GetType(a);
            }

            return attributes;
        }
        
        private string GetType(AttributeInfo attribute)
        {
            if (attribute.Type == "Relation")
            {
                if (attribute.LinkId == 0)
                {
                    return "O2M";
                }
                else
                {
                    return "M2M";
                }
            }
            else if (attribute.Type == "Relation Many-to-One")
            {
                return "M2O";
            }

            return attribute.Type;
        }
                    
        #endregion
    }
}
