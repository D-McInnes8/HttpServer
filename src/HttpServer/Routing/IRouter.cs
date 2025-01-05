using HttpServer.Pipeline;

namespace HttpServer.Routing;

/// <summary>
/// Used to determine the route based on the request.
/// </summary>
public interface IRouter
{
    /// <summary>
    /// Determines the route based on the request.
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns>Returns a <see cref="RouterResult"/> based on the request.</returns>
    Task<RouterResult> RouteAsync(RequestPipelineContext ctx);
}

/// <summary>
/// A result indicating the outcome of a router.
/// </summary>
public enum RouterResult
{
    /// <summary>
    /// The router was successful.
    /// </summary>
    Success,
    
    /// <summary>
    /// The router was not successful.
    /// </summary>
    NotFound,
}

public readonly record struct Option<TValue>
{
    /// <summary>
    /// 
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// 
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Option(TValue value)
    {
        HasValue = true;
        Value = value;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Option<TValue>(TValue value) => new(value);
}

public static class Result
{
    public static Result<TValue, TError> Success<TValue, TError>(TValue value) => new(value);
    public static Result<TValue, TError> Error<TValue, TError>(TError error) => new(error);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <typeparam name="TError"></typeparam>
public readonly record struct Result<TValue, TError>
{
    private readonly bool _success;
    private readonly TValue _value;
    private readonly TError _error;
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsSuccess => _success;
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsError => !_success;
    
    /// <summary>
    /// 
    /// </summary>
    public TValue Value => _value;
    
    /// <summary>
    /// 
    /// </summary>
    public TError Error => _error;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    public Result(TValue value)
    {
        _success = true;
        _value = value;
        _error = default!;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    public Result(TError error)
    {
        _success = false;
        _value = default!;
        _error = error;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static implicit operator Result<TValue, TError>(TError error) => new(error);
}