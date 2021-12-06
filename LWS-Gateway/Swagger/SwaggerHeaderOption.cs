using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LWS_Gateway.Swagger
{
    [ExcludeFromCodeCoverage]
    public class SwaggerHeaderOptions : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Create Parameter Options
            operation.Parameters ??= new List<OpenApiParameter>();

            if (context.ApiDescription.RelativePath.Contains("api/user") &&
                context.ApiDescription.HttpMethod == "DELETE")
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-API-AUTH",
                    Description = "Token for authentication",
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = "string" },
                    Required = true
                });
                return;
            }
            
            // Add them
            if (!context.ApiDescription.RelativePath.Contains("api/user"))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "X-API-AUTH",
                    Description = "Token for authentication",
                    In = ParameterLocation.Header,
                    Schema = new OpenApiSchema { Type = "string" },
                    Required = true
                });
            }
        }
    }
}