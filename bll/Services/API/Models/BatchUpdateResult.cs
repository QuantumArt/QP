using System;
using Quantumart.QP8.BLL.Services.DTO;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL.Services.API.Models;

public class BatchUpdateResult : MessageResult
{
    public InsertData[] InsertData = Array.Empty<InsertData>();

    public BatchUpdateResult() : this(ActionMessageType.Info, String.Empty, null)
    {
    }

    public static BatchUpdateResult CreateError(string message, int[] failedIds = null) => new BatchUpdateResult(ActionMessageType.Error, message, failedIds);

    public BatchUpdateResult(string actionMessageType, string message, int[] failedIds)
        : base(actionMessageType, message, failedIds)
    {
    }
}
