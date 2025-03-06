using System;

namespace Quantumart.QP8.BLL.Exceptions;

public class ExternalWorkflowNotFoundInDbException : Exception
{
    public ExternalWorkflowNotFoundInDbException()
    {

    }

    public ExternalWorkflowNotFoundInDbException(string message) : base(message)
    {

    }

    public ExternalWorkflowNotFoundInDbException(string message, Exception inner) : base(message, inner)
    {

    }
}
