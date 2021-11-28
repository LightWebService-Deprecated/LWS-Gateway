using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LWS_Gateway.Swagger
{
    [ExcludeFromCodeCoverage]
    public class SwaggerHeaderOptions : IOperationFilter
    {
        private readonly Dictionary<string, string> _apiBlackList;

        public SwaggerHeaderOptions()
        {
            _apiBlackList = new Dictionary<string, string>
            {
                ["api/user"] = "POST",
                ["api/user/login"] = "POST",
                ["api/manage/node"] = "POST"
            };
        }

        private bool IsRelativePathBlackList(string relativePath, string method)
        {
            if (!_apiBlackList.ContainsKey(relativePath)) return false;

            if (_apiBlackList[relativePath] == method) return true;

            return false;
        }
            
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Create Parameter Options
            operation.Parameters ??= new List<OpenApiParameter>();

            if (!IsRelativePathBlackList(context.ApiDescription.RelativePath, context.ApiDescription.HttpMethod))
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