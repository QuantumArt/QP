using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportSettings : IMultistepActionParams
    {
        public int SiteId { get; set; }

        public int ContentId { get; set; }

        public ImportSettings(int siteId, int contentId)
        {
            SiteId = siteId;
            ContentId = contentId;
            Id = Guid.NewGuid();
            UpdatedArticleIds = new List<int>();
            InsertedArticleIds = new List<int>();
            CreateVersions = true;
        }

        public Guid Id { get; set; }

        public string UploadFilePath => GetUploadFilePath();

        public string TempFileUploadPath => UploadFilePath + FileName;

        public string TempFileForRelFields => UploadFilePath + Path.GetFileNameWithoutExtension(FileName) + "_rel" + ".dat";

        public bool ContainsO2MRelationOrM2MRelationFields
        {
            get { return FieldRepository.GetFullList(ContentId).Any(a => a.ExactType == FieldExactTypes.O2MRelation || a.ExactType == FieldExactTypes.M2MRelation); }
        }

        public int ImportAction { get; set; }

        public string FileName { get; set; }

        public char Delimiter { get; set; }

        public bool NoHeaders { get; set; }

        public string Culture { get; set; }

        public string Encoding { get; set; }

        public string LineSeparator { get; set; }

        public bool UpdateAndInsert { get; set; }

        public int UniqueContentFieldId { get; set; }

        public string UniqueFieldToUpdate { get; set; } = string.Empty;

        public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }

        public List<KeyValuePair<string, int>> FieldsList { get; set; }

        public List<int> UpdatedArticleIds { get; set; }

        public List<int> InsertedArticleIds { get; set; }

        private string GetUploadFilePath()
        {
            var currentSite = SiteRepository.GetById(SiteId);
            if (!Directory.Exists(currentSite.UploadDir))
            {
                throw new DirectoryNotFoundException();
            }

            return WebUtility.UrlDecode(ImportArticlesParams.GetUploadPath(currentSite, ContentId));
        }

        public bool IsWorkflowAssigned { get; set; }

        public bool CreateVersions { get; set; }
    }
}
