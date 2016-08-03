using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.CodeGeneration.Services;
using System.Data.Common;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;

namespace Quantumart.QP8.EntityFramework6.DevData
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
	            a.link_id
            from
	            CONTENT_ATTRIBUTE a
	            join CONTENT c on a.CONTENT_ID = c.CONTENT_ID
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

            var attributes = GetAttributes(connector, siteId);
            var contents = GetContents(connector, siteId, attributes);

            var model = new ModelReader();
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
                    LinkId = (int)row.Field<decimal>("ATTRIBUTE_ID")
                })
                .ToArray();

            return attributes;
        }        
        #endregion
    }
}
