using Newtonsoft.Json;
using Quantumart.QP8.BLL.Converters;
using Quantumart.QP8.Resources;
using System;
using System.ComponentModel.DataAnnotations;

namespace Quantumart.QP8.BLL
{
    public class SessionsLog
    {
        public SessionsLog()
        {
            IsQP7 = false;
        }

        public int SessionId { get; set; }

        [Display(Name = "UserLogin", ResourceType = typeof(AuditStrings))]
        public string Login { get; set; }

        public int? UserId { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "SessionStartTime", ResourceType = typeof(AuditStrings))]
        public DateTime StartTime { get; set; }

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "SessionEndTime", ResourceType = typeof(AuditStrings))]
        public DateTime? EndTime { get; set; }

        [Display(Name = "IP", ResourceType = typeof(AuditStrings))]
        public string IP { get; set; }

        [Display(Name = "Browser", ResourceType = typeof(AuditStrings))]
        public string Browser { get; set; }

        [Display(Name = "ServerName", ResourceType = typeof(AuditStrings))]
        public string ServerName { get; set; }

        [Display(Name = "AutoLogged", ResourceType = typeof(AuditStrings))]
        public int AutoLogged { get; set; }

        public string Sid { get; set; }

        [Display(Name = "IsQP7", ResourceType = typeof(AuditStrings))]
        public bool IsQP7 { get; set; }

        #region Caulculated

        [JsonConverter(typeof(DateTimeConverter))]
        [Display(Name = "FailedTime", ResourceType = typeof(AuditStrings))]
        public DateTime FailedTime => StartTime;

        [Display(Name = "Duration", ResourceType = typeof(AuditStrings))]
        public string Duration
        {
            get
            {
                if (EndTime.HasValue)
                {
                    return (EndTime.Value - StartTime).Duration().ToString(@"hh\:mm\:ss");
                }

                return null;
            }
        }

        public string AutoLoggedChecked => AutoLogged > 0 ? "checked=\"checked\"" : null;

        public string IsQP7Checked => IsQP7 ? "checked=\"checked\"" : null;

        #endregion
    }
}
