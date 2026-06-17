namespace AllChecksOut.FunctionsApi.Domain;

public sealed class DomainException(string message, int statusCode = 400) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public static DomainException Forbidden(string message = "You do not have access to this resource.") =>
        new(message, 403);

    public static DomainException NotFound(string message) =>
        new(message, 404);
}
