using System;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteSettings : IMultistepActionParams
    {
        public int SourceSiteId { get; set; }

        public int DestinationSiteId { get; set; }

        public DateTime DateTimeOnStart { get; set; }

        public string PathForFileWithLinks => $"{QPConfiguration.TempDirectory}\\copy_site_{DestinationSiteId}_links_{GetDateTimeForFileName()}.xml";

        public string PathForFileWithFilesToCopy => $"{QPConfiguration.TempDirectory}\\copy_site_{DestinationSiteId}_files_buffer_{GetDateTimeForFileName()}.txt";

        public int? DoNotCopyArticles { get; set; }

        public bool DoNotCopyTemplates { get; set; }

        public bool DoNotCopyFiles { get; set; }

        public string ContentsToCopy => DoNotCopyArticles != null ? ContentRepository.GetContentIdsToCopy(DoNotCopyArticles.Value, SourceSiteId) : ContentRepository.GetContentIdsBySiteId(SourceSiteId);

        public CopySiteSettings(int destinationSiteId, int sourceSiteId, DateTime dateTimeOnStart, int? doNotCopyArticles, bool doNotCopyTemplates, bool doNotCopyFiles)
        {
            DestinationSiteId = destinationSiteId;
            SourceSiteId = sourceSiteId;
            DateTimeOnStart = dateTimeOnStart;
            DoNotCopyArticles = doNotCopyArticles;
            DoNotCopyTemplates = doNotCopyTemplates;
            DoNotCopyFiles = doNotCopyFiles;
        }

        private string GetDateTimeForFileName() => $"{DateTimeOnStart.Year}_{DateTimeOnStart.Month}_{DateTimeOnStart.Day}_{DateTimeOnStart.Hour}_{DateTimeOnStart.Minute}_{DateTimeOnStart.Second}";
    }
}
