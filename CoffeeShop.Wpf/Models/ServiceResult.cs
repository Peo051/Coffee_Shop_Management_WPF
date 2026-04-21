namespace CoffeeShop.Wpf.Models;

public class ServiceResult
{
    protected ServiceResult(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }

    public bool IsSuccess { get; }

    public string Message { get; }

    public static ServiceResult Success(string message = "Thành công.")
    {
        return new ServiceResult(true, message);
    }

    public static ServiceResult Fail(string message)
    {
        return new ServiceResult(false, message);
    }
}

public sealed class ServiceResult<T> : ServiceResult
{
    private ServiceResult(bool isSuccess, string message, T? data)
        : base(isSuccess, message)
    {
        Data = data;
    }

    public T? Data { get; }

    public static ServiceResult<T> Success(T data, string message = "Thành công.")
    {
        return new ServiceResult<T>(true, message, data);
    }

    public new static ServiceResult<T> Fail(string message)
    {
        return new ServiceResult<T>(false, message, default);
    }
}
