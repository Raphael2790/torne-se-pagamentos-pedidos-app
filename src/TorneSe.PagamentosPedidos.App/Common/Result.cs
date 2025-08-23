namespace TorneSe.PagamentosPedidos.App.Common;

public class Result<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public static Result<T> Success(T data)
    {
        return new Result<T> { IsSuccess = true, Data = data };
    }

    public static Result<T> Error(string message)
    {
        return new Result<T> { IsSuccess = false, Message = message };
    }
}