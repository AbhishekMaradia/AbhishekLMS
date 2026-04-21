using System.Net;
using System.Text.Json;
using Serilog;
using Microsoft.Data.SqlClient;

namespace LMS_SoulCode.Features.Common
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public GlobalExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        //private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        //{
        //    context.Response.ContentType = "application/json";
        //    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        //    var response = ApiResponse<string>.Fail("Internal Server Error. Please try again later.", context.Response.StatusCode);

        //    // Optional: return stack trace in dev mode only, but for now we keep it generic as per plan

        //    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        //    return context.Response.WriteAsync(json);
        //}


        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            ApiResponse<string> response;
            int code;

            switch (exception)
            {
                // EF Core database errors
                case DbUpdateException:
                    code = StatusCodes.ServerError;
                    response = ApiResponse<string>.Fail("Database operation failed.", code);
                    break;

                // SQL Server connection / command errors
                case SqlException:
                    code = StatusCodes.ServerError;
                    response = ApiResponse<string>.Fail("Database connection error.", code);
                    break;

                // Validation / bad input
                case ArgumentException argEx:
                    code = StatusCodes.BadRequest;
                    response = ApiResponse<string>.Fail(argEx.Message, code);
                    break;

                // Unauthorized
                case UnauthorizedAccessException:
                    code = StatusCodes.Unauthorized;
                    response = ApiResponse<string>.Fail("Unauthorized access.", code);
                    break;

                // Everything else
                default:
                    code = StatusCodes.ServerError;
                    response = ApiResponse<string>.Fail("Internal Server Error. Please try again later.", code);
                    break;
            }

            context.Response.StatusCode = code;

#if DEBUG
            // DEV: append real exception to same message (no new ApiResponse)
            response.Message +=
                exception.InnerException != null
                    ? $" | {exception.InnerException.Message}"
                    : $" | {exception.Message}";
#endif

            var json = JsonSerializer.Serialize(response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return context.Response.WriteAsync(json);
        }
    }
}