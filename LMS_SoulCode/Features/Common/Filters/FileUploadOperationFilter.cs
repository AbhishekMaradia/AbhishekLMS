using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;

namespace LMS_SoulCode.Features.Common
{ 
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // 1. STRICT CHECK: Does this endpoint actually have a file upload intention?
            var parameterDescriptions = context.ApiDescription.ParameterDescriptions;
            var hasFile = parameterDescriptions.Any(p => 
                p.ModelMetadata?.ModelType == typeof(IFormFile) || 
                p.ModelMetadata?.ModelType == typeof(IFormFile[]));

            // If no file property is detected in any parameter, exit immediately.
            // This ensures NO global side effects on normal JSON APIs.
            if (!hasFile) return;

            // 2. Identify the target parameter that uses [FromForm]
            var formParam = context.MethodInfo.GetParameters()
                .FirstOrDefault(p => p.GetCustomAttributes(typeof(FromFormAttribute), true).Any());

            if (formParam == null) return;

            // 3. Ensure RequestBody exists and forced to multipart/form-data ONLY for this endpoint
            operation.RequestBody ??= new OpenApiRequestBody();

            // Clear any default application/json mapping if present (to avoid the 415 error in Swagger)
            if (operation.RequestBody.Content.ContainsKey("application/json"))
                operation.RequestBody.Content.Remove("application/json");

            // Define the multipart schema based on the actual model type
            operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
            {
                Schema = context.SchemaGenerator.GenerateSchema(formParam.ParameterType, context.SchemaRepository)
            };
        }
    }
}