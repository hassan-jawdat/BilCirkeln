namespace BilCirkeln.Api.Services;

public enum ServiceError
{
    None,
    NotFound,
    Forbidden,
    BadRequest
}

// A simple way for a service to say "it worked, here is the data"
// or "it failed, and this is why" - without throwing exceptions.
// The controller then turns the error into the right HTTP status code.
public class ServiceResult<T>
{
    public bool Success => Error == ServiceError.None;
    public T? Data { get; private init; }
    public ServiceError Error { get; private init; }
    public string? Message { get; private init; }

    public static ServiceResult<T> Ok(T data) => new() { Data = data };

    public static ServiceResult<T> Fail(ServiceError error, string message) =>
        new() { Error = error, Message = message };
}
