using System;

namespace Quantumart.QP8.BLL.Exceptions;

public class ExternalWorkflowPublishException : Exception
{
    public ExternalWorkflowPublishException()
    {

    }

    public ExternalWorkflowPublishException(string message) : base(message)
    {

    }

    public ExternalWorkflowPublishException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
