namespace AlMosafer.Application.Common;

/// <summary>
/// Represents a standardized enterprise operation result model across application use cases.
/// </summary>
/// <typeparam name="T">Payload data type.</typeparam>
public class OperationResult<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    private OperationResult(bool isSuccess, T? data, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    public static OperationResult<T> Success(T data, int statusCode = 200)
    {
        return new OperationResult<T>(true, data, null, statusCode);
    }

    public static OperationResult<T> Failure(string errorMessage, int statusCode = 400)
    {
        return new OperationResult<T>(false, default, errorMessage, statusCode);
    }

    public static OperationResult<T> NotFound(string errorMessage = "المورد المطلوب غير موجود")
    {
        return Failure(errorMessage, 404);
    }

    public static OperationResult<T> Unauthorized(string errorMessage = "غير مصرح بالوصول")
    {
        return Failure(errorMessage, 401);
    }

    public static OperationResult<T> Forbidden(string errorMessage = "لا تملك الصلاحية الكافية للوصول لهذا المورد")
    {
        return Failure(errorMessage, 403);
    }

    public static OperationResult<T> Conflict(string errorMessage = "تعارض في تنفيذ العملية البرمجية")
    {
        return Failure(errorMessage, 409);
    }

    public static OperationResult<T> Unprocessable(string errorMessage = "بيانات المدخلات غير صالحة لمعالجة الطلب")
    {
        return Failure(errorMessage, 422);
    }
}
