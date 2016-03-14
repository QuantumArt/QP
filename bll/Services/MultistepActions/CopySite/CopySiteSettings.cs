using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteSettings : IMultistepActionParams
    {
        public int SourceSiteId { get; set; }
        public int DestinationSiteId { get; set; }
        public DateTime DateTimeOnStart { get; set; }
        public string PathForFileWithLinks
        {
            get
            {
                return String.Format("{0}\\copy_site_{1}_links_{2}.xml", QPConfiguration.TempDirectory, DestinationSiteId, GetDateTimeForFileName());

            }
        }
        public string PathForFileWithFilesToCopy
        {
            get
            {
                return String.Format("{0}\\copy_site_{1}_files_buffer_{2}.txt", QPConfiguration.TempDirectory, DestinationSiteId, GetDateTimeForFileName());
            }
        }
        public int? DoNotCopyArticles { get; set; }
        public bool DoNotCopyTemplates { get; set; }
        public bool DoNotCopyFiles { get; set; }
        public string ContentsToCopy
        {
            get
            {
                if (DoNotCopyArticles != null)
                {
                    return ContentRepository.GetContentIdsToCopy(DoNotCopyArticles.Value, this.SourceSiteId);
                }
                else
                {
                    return ContentRepository.GetContentIdsBySiteId(this.SourceSiteId);
                }
            }
        }
        public CopySiteSettings(int destinationSiteId, int sourceSiteId, DateTime dateTimeOnStart, int? doNotCopyArticles, bool doNotCopyTemplates, bool doNotCopyFiles)
        {
            this.DestinationSiteId = destinationSiteId;
            this.SourceSiteId = sourceSiteId;
            this.DateTimeOnStart = dateTimeOnStart;
            this.DoNotCopyArticles = doNotCopyArticles;
            this.DoNotCopyTemplates = doNotCopyTemplates;
            this.DoNotCopyFiles = doNotCopyFiles;
        }

        private string GetDateTimeForFileName()
        {
            return String.Format("{0}_{1}_{2}_{3}_{4}_{5}",
                                              DateTimeOnStart.Year,
                                              DateTimeOnStart.Month,
                                              DateTimeOnStart.Day,
                                              DateTimeOnStart.Hour,
                                              DateTimeOnStart.Minute,
                                              DateTimeOnStart.Second);
        }
    }
}
