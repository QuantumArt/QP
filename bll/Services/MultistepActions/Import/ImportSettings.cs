using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL.Services.MultistepActions.Import
{
    public class ImportSettings : IMultistepActionParams
    {
        private int siteId { get; set; }
        private int contentId { get; set; }

        public ImportSettings(int siteId, int contentId)
        {
            this.siteId = siteId;
            this.contentId = contentId;
			Id = Guid.NewGuid();
			UpdatedArticleIds = new List<int>();
			InsertedArticleIds = new List<int>();
        }

		public Guid Id { get; private set; }
        public string UploadFilePath
        {
            get
            {
                return GetUploadFilePath();
            }
        }
        public string TempFileUploadPath
        {
            get
            {
                FileInfo fileInfo = new FileInfo(HttpUtility.UrlDecode(this.UploadFilePath));
                return String.Format("{0}\\{1}", QPConfiguration.TempDirectory, fileInfo.Name);
            }
        }
        public string TempFileForRelFields
        {
            get
            {
                string fileName = Path.GetFileNameWithoutExtension(this.UploadFilePath) + "_rel";
                string fileNameExtension = fileName + ".dat";
                return String.Format("{0}\\{1}", QPConfiguration.TempDirectory, fileNameExtension);
            }
        }
        public bool ContainsO2MRelationOrM2MRelationFields
        {
            get
            {
                List<Field> fields = FieldRepository.GetFullList(contentId);
                return fields.Where(a => a.ExactType == FieldExactTypes.O2MRelation || a.ExactType == FieldExactTypes.M2MRelation).Count() > 0;
            }
        }

        #region Properties

        public int ImportAction { get; set; }
        public string FileName { get; set; }
        public char Delimiter { get; set; }
        public bool NoHeaders { get; set; }
        public string Culture { get; set; }
        public string Encoding { get; set; }
        public string LineSeparator { get; set; }
        public bool UpdateAndInsert { get; set; }
		public Field UniqueContentField { get; set; }

        private string uniqueFieldToUpdate = String.Empty;
        public string UniqueFieldToUpdate
        {
            get
            {
				return uniqueFieldToUpdate;
			}
			set
			{
				uniqueFieldToUpdate = value;
			}
		}
		public Dictionary<int, string> UniqueAggregatedFieldsToUpdate { get; set; }
		public List<KeyValuePair<string, BLL.Field>> FieldsList { get; set; }

		public List<int> UpdatedArticleIds { get; set; }
		public List<int> InsertedArticleIds { get; set; }
		#endregion

        private string GetUploadFilePath()
        {
            Site currentSite = SiteRepository.GetById(this.siteId);

			if (!Directory.Exists(currentSite.UploadDir))
                throw new DirectoryNotFoundException();

            string result = String.Format("{0}\\contents\\{1}\\{2}", currentSite.UploadDir, this.contentId, this.FileName);
            return HttpUtility.UrlDecode(result);
        }
    }
    public enum ImportActions
    {
        InsertAll = 0,
        InsertNew = 1,
        InsertAndUpdate = 2,
        Update = 3,
        UpdateIfChanged = 4
    }
}
