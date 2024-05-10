using GenericSwagger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ATI.EAPI.Framework.Swagger
{
    /// <summary>
    ///     Provides extension methods for <see cref="IServiceCollection"/> to
    ///     add Swagger services to the application pipeline.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class SwaggerServiceCollectionExtensions
    {
        /// <summary>
        ///     Adds Swagger services to the <see cref="IServiceCollection"/> instance.
        /// </summary>
        /// <param name="services">
        ///     Instance of <see cref="IServiceCollection"/>.
        /// </param>
        /// <param name="optionsBuilder">
        ///     Action to configure <see cref="SwaggerOptions"/>.
        /// </param>
        /// <returns>
        ///     <see cref="IServiceCollection"/>
        /// </returns>
        public static IServiceCollection AddSwagger(this IServiceCollection services,
            Action<IServiceProvider, SwaggerOptions> optionsBuilder)
        {
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            ILogger logger = serviceProvider.GetService<ILogger<SwaggerOptions>>();

            logger.LogInformation("Adding Swagger Gen services");

            var options = new SwaggerOptions();
            optionsBuilder?.Invoke(serviceProvider, options);

            services
                .AddSwaggerGen(swaggerGenOptions =>
                {
                    try
                    {
                        foreach (ApiVersionDescriptor apiVersion in options.ApiVersions)
                        {
                            swaggerGenOptions.SwaggerDoc(apiVersion.Version, apiVersion.ApiInfo);
                            logger.LogInformation("Added definition for version " + apiVersion.Version + ".");
                        }

                        swaggerGenOptions.DocInclusionPredicate(delegate (string version, ApiDescription apiDescription)
                        {
                            if (!apiDescription.TryGetMethodInfo(out MethodInfo methodInfo))
                            {
                                return false;
                            }
                            var versions = methodInfo.DeclaringType.GetConstructors()
                                .SelectMany(constructorInfo => constructorInfo.DeclaringType.CustomAttributes
                                    .Where(attributeData => attributeData.AttributeType == typeof(ApiVersionAttribute))
                                    .SelectMany(attributeData => attributeData.ConstructorArguments
                                        .Select(attributeTypedArgument => attributeTypedArgument.Value)));
                            var result = versions.Any(v => string.Equals($"v{v}", version));
                            return result;
                        });

                        swaggerGenOptions.CustomSchemaIds(type => type.FullName);

                        swaggerGenOptions.AddSecurityDefinition("bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                        {
                            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                            BearerFormat = "JWT",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                            Scheme = "bearer"
                        });
                        swaggerGenOptions.OperationFilter<SwaggerAuthorizationOperationFilter>();

                        string xmlDocPath = PlatformServices.Default.Application.ApplicationBasePath;
                        logger.LogInformation("Including xmldoc path: '{0}'", xmlDocPath);

                        var directoryInfo = new System.IO.DirectoryInfo(xmlDocPath);

                        if (directoryInfo.Exists)
                        {
                            IEnumerable<FileInfo> xmlFiles = directoryInfo.EnumerateFiles("*.xml");

                            foreach (var xmlFile in xmlFiles)
                            {
                                var xmlFullName = xmlFile.FullName;
                                swaggerGenOptions.IncludeXmlComments(xmlFullName);
                                logger.LogInformation("Added the document file to Swagger: '{0}'", xmlFullName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred while adding options for Swagger.");
                        throw;
                    }
                })
                .AddSwaggerGenNewtonsoftSupport();

            services.AddSingleton(options);

            return services;
        }
    }
}
