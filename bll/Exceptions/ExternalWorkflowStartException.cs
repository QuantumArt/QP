using System;

namespace Quantumart.QP8.BLL.Exceptions;

public class ExternalWorkflowStartException : Exception
{
    public ExternalWorkflowStartException()
    {

    }

    public ExternalWorkflowStartException(string message) : base(message)
    {

    }

    public ExternalWorkflowStartException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
