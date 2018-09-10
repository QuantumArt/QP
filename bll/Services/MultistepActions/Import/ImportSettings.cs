using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportSettings : IMultistepActionParams
    {
        private int SiteId { get; }

        private int ContentId { get; }

        public ImportSettings(int siteId, int contentId)
        {
            SiteId = siteId;
            ContentId = contentId;
            Id = Guid.NewGuid();
            UpdatedArticleIds = new List<int>();
            InsertedArticleIds = new List<int>();
        }

        public Guid Id { get; }

        public string UploadFilePath => GetUploadFilePath();

        public string TempFileUploadPath
        {
            get
            {
                var url = HttpUtility.UrlDecode(UploadFilePath) ?? string.Empty;
                var fileInfo = new FileInfo(url);
                return $"{QPConfiguration.TempDirectory}\\{fileInfo.Name}";
            }
        }

        public string TempFileForRelFields
        {
            get
            {
                var fileName = Path.GetFileNameWithoutExtension(UploadFilePath) + "_rel";
                var fileNameExtension = fileName + ".dat";
                return $"{QPConfiguration.TempDirectory}\\{fileNameExtension}";
            }
        }

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

        public Field UniqueContentField { get; set; }

        public string UniqueFieldToUpdate { get; set; } = string.Empty;

        public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }

        public List<KeyValuePair<string, Field>> FieldsList { get; set; }

        public List<int> UpdatedArticleIds { get; set; }

        public List<int> InsertedArticleIds { get; set; }

        private string GetUploadFilePath()
        {
            var currentSite = SiteRepository.GetById(SiteId);
            if (!Directory.Exists(currentSite.UploadDir))
            {
                throw new DirectoryNotFoundException();
            }

            return HttpUtility.UrlDecode($"{currentSite.UploadDir}\\contents\\{ContentId}\\_temp\\{FileName}");
        }

        public bool IsWorkflowAssigned { get; set; }
    }
}
