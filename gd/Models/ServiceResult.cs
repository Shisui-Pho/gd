namespace GD.Models;

internal class ServiceResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public object Result { get; }
    public Exception Exception { get; }
    protected ServiceResult(bool success, string message, object result, Exception exception)
    {
        IsSuccess = success;
        Message = message;
        Result = result;
        Exception = exception;
    }
    public static ServiceResult Success(string message = "Success", object result = null)
    {
        return new ServiceResult(true, message, result, null);
    }
    public static ServiceResult Fail(string message)
    {
        return new ServiceResult(false, message, null, null);
    }
    public static ServiceResult Fail(Exception ex)
    {
        return new ServiceResult(false, ex.Message, null, ex);
    }
}
