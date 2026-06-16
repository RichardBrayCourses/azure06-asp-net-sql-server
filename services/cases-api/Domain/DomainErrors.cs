namespace AllChecksOut.Cases.Api.Domain;

public sealed class DomainException(string message, int statusCode = StatusCodes.Status400BadRequest) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    public static DomainException Forbidden(string message = "You do not have access to this resource.") =>
        new(message, StatusCodes.Status403Forbidden);

    public static DomainException NotFound(string message) =>
        new(message, StatusCodes.Status404NotFound);
}
