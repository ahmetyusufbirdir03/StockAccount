using System.Text.Json.Serialization;

namespace StockAccountContracts.Dtos;

public class ResponseDto<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("errors")]
    public List<string>? Errors { get; set; }

    [JsonIgnore]
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    // Success - With Data
    public static ResponseDto<T> Success(T? data, int statusCode = 200, string? message = null)
    {
        return new ResponseDto<T>
        {
            Data = data,
            StatusCode = statusCode,
            Message = message ?? "İşlem başarılı"
        };
    }

    // Success - Without Data
    public static ResponseDto<T> Success(int statusCode = 200, string? message = null)
    {
        return new ResponseDto<T>
        {
            StatusCode = statusCode,
            Message = message ?? "İşlem başarılı"
        };
    }

    // Fail - Multiple Errors
    public static ResponseDto<T> Fail(List<string> errors, int statusCode = 400, string? message = null)
    {
        return new ResponseDto<T>
        {
            Errors = errors,
            StatusCode = statusCode,
            Message = message ?? "İşlem başarısız"
        };
    }

    // Fail - Single Error
    public static ResponseDto<T> Fail(string error, int statusCode = 400, string? message = null)
    {
        return new ResponseDto<T>
        {
            Errors = new List<string> { error },
            StatusCode = statusCode,
            Message = message ?? "İşlem başarısız"
        };
    }
}

