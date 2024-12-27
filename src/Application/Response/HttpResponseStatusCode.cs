namespace Application.Response;

public enum HttpResponseStatusCode
{
    OK = 200,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 404,
    InternalServerError = 500
}