using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;

namespace LDN.Framework.GenericSwagger
{
    /// <summary>
    ///     Implements <see cref="IOperationFilter"/> to apply authorization for Swagger.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SwaggerAuthorizationOperationFilter : IOperationFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Security == null)
            {
                operation.Security = new List<OpenApiSecurityRequirement>();
            }

            var contextRequireAuthorize = context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (contextRequireAuthorize)
            {
                operation.Responses.Add("401", new OpenApiResponse() { Description = "Unauthorized" });
                operation.Responses.Add("403", new OpenApiResponse() { Description = "Forbidden" });

                var scheme = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "bearer"
                    }
                };

                operation.Security.Add(new OpenApiSecurityRequirement()
                {
                    [scheme] = new List<string>()
                });
            }
        }
    }
}
