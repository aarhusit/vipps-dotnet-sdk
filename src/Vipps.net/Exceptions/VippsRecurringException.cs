using Vipps.net.Models.Recurring;

namespace Vipps.net.Exceptions;

public class VippsRecurringException : VippsBaseException
{
    public ErrorV3 Error { get; }

    internal VippsRecurringException(string message = null, System.Exception innerException = null, ErrorV3 error = null)
        : base(message, innerException)
    {
        Error = error;
    }
}
