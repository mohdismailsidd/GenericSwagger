using ATI.EAPI.Framework.Swagger;
using GenericSwagger;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ATI.EAPI.Framework.Swagger
{
    /// <summary>
    ///     Encapsulates the options for Swagger configuration.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class SwaggerOptions
    {
        /// <summary>
        ///     Get the list of <see cref="ApiVersionDescriptor"/> instances.
        /// </summary>
        public List<ApiVersionDescriptor> ApiVersions { get; }

        /// <summary>
        ///     Default <see cref="SwaggerOptions"/> constructor.
        /// </summary>
        public SwaggerOptions()
        {
            ApiVersions = new List<ApiVersionDescriptor>();
        }
    }
}
