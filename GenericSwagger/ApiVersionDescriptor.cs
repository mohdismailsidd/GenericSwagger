using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace GenericSwagger
{
    /// <summary>
    ///     Encapsulates the data describing specific API version.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ApiVersionDescriptor
    {
        /// <summary>
        ///     Gets or sets the version number as the <see cref="string"/> value.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Gets or sets an instance of <see cref="OpenApiInfo"/> class.
        /// </summary>
        public OpenApiInfo ApiInfo { get; set; }

        /// <summary>
        ///     Gets or sets the endpoint for operations specification.
        /// </summary>
        public string Endpoint { get; set; }
    }
}
