using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobBoard.Core.DTOs.Common
{

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public ApiError? Error { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string code, string message, List<FieldError>? details = null) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message, Details = details } };
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public ApiError? Error { get; set; }

        public static ApiResponse Ok(string message) =>
            new() { Success = true, Message = message };

        public static ApiResponse Fail(string code, string message) =>
            new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }

    public class ApiError
    {
        public string Code { get; set; } = null!;
        public string Message { get; set; } = null!;
        public List<FieldError>? Details { get; set; }
    }

    public class FieldError
    {
        public string Field { get; set; } = null!;
        public string Message { get; set; } = null!;
    }
}
