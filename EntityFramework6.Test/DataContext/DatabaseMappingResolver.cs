using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quantumart.QP8.CodeGeneration.Services;
using Quantumart.QPublishing.Database;
using System.Data.SqlClient;
using System.Data.Common;

namespace EntityFramework6.Test.DataContext
{
    public class DatabaseMappingResolver : IMappingResolver
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
	            a.NET_ATTRIBUTE_NAME,
	            a.link_id
            from
	            CONTENT_ATTRIBUTE a
	            join CONTENT c on a.CONTENT_ID = c.CONTENT_ID
            where
	            c.SITE_ID = @siteId";
        #endregion

        protected string SiteName { get; private set; }
        protected ContentInfo[] Contents { get; private set; }
        private string ConnectionString { get; set; }

        public DatabaseMappingResolver(string siteName)
        {
            SiteName = siteName;
            Contents = new ContentInfo[0];
        }

        #region IMappingResolver implementation
        public object GetCacheKey()
        {
            return new { SiteName, ConnectionString };
        }
        public void Initialize(DbConnection connection)
        {
            ConnectionString = connection.ConnectionString;
            var connector = new DBConnector(connection);
            int siteId = connector.GetSiteId(SiteName);
            Contents = GetContents(connector, siteId);
        }

        public AttributeInfo GetAttribute(string contentMappedName, string fieldMappedName)
        {
            var attributes = from c in Contents
                             from a in c.Attributes
                             where
                                 c.MappedName == contentMappedName &&
                                 a.MappedName == fieldMappedName
                             select a;

            return attributes.Single();
        }

        public ContentInfo GetContent(string mappedName)
        {
            return Contents.Single(c => c.MappedName == mappedName);
        }
        #endregion

        #region Private methods
        private ContentInfo[] GetContents(DBConnector connector, int siteId)
        {
            var command = new SqlCommand(ContentQuery);
            command.Parameters.AddWithValue("@siteId", siteId);

            var attributesLookup = GetAttributes(connector, siteId).ToLookup(a => a.ContentId, a => a);

            var contents = connector
                .GetRealData(command)
                .AsEnumerable()
                .Select(row => new ContentInfo
                {
                    Id = (int)row.Field<decimal>("CONTENT_ID"),
                    MappedName = row.Field<string>("NET_CONTENT_NAME"),
                    UseDefaultFiltration = row.Field<bool>("USE_DEFAULT_FILTRATION"),
                    Attributes = new List<AttributeInfo>(attributesLookup[(int)row.Field<decimal>("CONTENT_ID")])
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
                    MappedName = row.Field<string>("NET_ATTRIBUTE_NAME"),
                    LinkId = (int)row.Field<decimal>("ATTRIBUTE_ID")
                })
                .ToArray();

            return attributes;
        }
        #endregion
    }
}
