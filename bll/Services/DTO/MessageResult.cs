using System;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.DTO
{
    [Serializable]
    public class MessageResult
    {
        public string Type { get; set; }

        public string Text { get; set; }

        public string Url { get; set; }

        public int[] FailedIds { get; set; }

        protected MessageResult(string actionMessageType, string message, int[] failedIds)
        {
            Type = actionMessageType;
            Text = message;
            FailedIds = failedIds;
        }

        public static MessageResult Error(string message, int[] failedIds = null) => new MessageResult(ActionMessageType.Error, message, failedIds);

        public static MessageResult Info(string message, int[] failedIds = null) => new MessageResult(ActionMessageType.Info, message, failedIds);

        public static MessageResult Confirm(string message, int[] failedIds = null) => new MessageResult(ActionMessageType.Confirm, message, failedIds);

        public static MessageResult Warning(string message, int[] failedIds = null) => new MessageResult(ActionMessageType.Warning, message, failedIds);

        public static MessageResult Download(string url) => new MessageResult(ActionMessageType.Download, "", null) { Url = url };
    }
}
