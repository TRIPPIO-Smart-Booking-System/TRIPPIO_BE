using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Trippio.Api.Filters
{
    /// <summary>
    /// Swagger filter to properly display IFormFile as a file upload input
    /// Instead of showing all properties, it shows a single "Choose File" button
    /// </summary>
    public class FormFileSwaggerFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParams = context.MethodInfo
                .GetParameters()
                .Where(p => p.ParameterType == typeof(IFormFile) ||
                           p.ParameterType == typeof(IFormFileCollection) ||
                           (p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericArguments()[0] == typeof(IFormFile)))
                .ToList();

            if (!formFileParams.Any())
                return;

            foreach (var param in formFileParams)
            {
                // Remove any existing parameters for this form file
                var existingParam = operation.Parameters
                    .FirstOrDefault(p => p.Name.Equals(param.Name, StringComparison.OrdinalIgnoreCase));

                if (existingParam != null)
                {
                    operation.Parameters.Remove(existingParam);
                }

                // Add correct file parameter
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = param.Name,
                    In = ParameterLocation.Query,
                    Description = "Select image file (JPG, PNG, GIF, WebP)",
                    Required = true,
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                });
            }

            // Update request body to use form data
            if (operation.RequestBody?.Content.ContainsKey("multipart/form-data") == true)
            {
                var multipartContent = operation.RequestBody.Content["multipart/form-data"];
                
                if (multipartContent.Schema.Properties.Any())
                {
                    // Clear properties
                    multipartContent.Schema.Properties.Clear();

                    // Add only file parameter
                    foreach (var param in formFileParams)
                    {
                        multipartContent.Schema.Properties[param.Name] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary",
                            Description = $"Image file ({param.Name})"
                        };
                    }

                    multipartContent.Schema.Required = formFileParams
                        .Select(p => p.Name)
                        .ToHashSet();
                }
            }
        }
    }
}
