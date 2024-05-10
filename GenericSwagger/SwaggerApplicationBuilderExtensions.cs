using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace ATI.EAPI.Framework.Swagger
{
    /// <summary>
    ///     Provides extension methods for <see cref="IApplicationBuilder"/> to
    ///     configure and add Swagger to HTTP pipeline of E-API applications.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SwaggerApplicationBuilderExtensions
    {
        /// <summary>
        ///     Adds Swagger and Swagger UI to HTTP pipeline.
        /// </summary>
        /// <param name="applicationBuilder">
        ///     Instance of <see cref="IApplicationBuilder"/>.
        /// </param>
        /// <returns>
        ///     <see cref="IApplicationBuilder"/>
        /// </returns>
        public static IApplicationBuilder UseSwagger(this IApplicationBuilder applicationBuilder)
        {
            var options = applicationBuilder.ApplicationServices.GetRequiredService<SwaggerOptions>();
            var logger = applicationBuilder.ApplicationServices.GetRequiredService<ILogger<SwaggerOptions>>();
            var configuration = applicationBuilder.ApplicationServices.GetRequiredService<IConfiguration>();

            string applicationBasePath = configuration["ApplicationBasePath"];
            string swaggerRouteTemplate = @"swagger/{documentName}/swagger.json";

            applicationBuilder.UseSwagger(options =>
            {
                options.RouteTemplate = swaggerRouteTemplate;
                options.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Servers = new System.Collections.Generic.List<OpenApiServer>
                {
                    new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}/{applicationBasePath}" }
                });
            });

            logger.LogInformation("The Swagger JSON doc generation template is added: {0}", swaggerRouteTemplate);

            applicationBuilder.UseSwaggerUI(swaggerUiOptions =>
            {
                string swaggerUiRoute = @"swagger/ui/index";
                swaggerUiOptions.RoutePrefix = swaggerUiRoute;

                foreach (var descriptor in options.ApiVersions)
                {
                    var endpoint = string.IsNullOrWhiteSpace(applicationBasePath) ? descriptor.Endpoint : $"/{applicationBasePath}{descriptor.Endpoint}";

                    logger.LogInformation("The Swagger UI will use route '{0}' and file '{1}'", swaggerUiRoute, endpoint);

                    swaggerUiOptions.SwaggerEndpoint(endpoint, descriptor.Version);

                    logger.LogInformation("The Swagger UI has been added for route '{0}' and file '{1}'", swaggerUiRoute, endpoint);
                }
            });

            return applicationBuilder;
        }
    }
}
