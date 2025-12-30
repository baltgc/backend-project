namespace NetChallenge.Infrastructure.External;

public sealed class ExternalServiceException : Exception
{
    public ExternalServiceException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}


