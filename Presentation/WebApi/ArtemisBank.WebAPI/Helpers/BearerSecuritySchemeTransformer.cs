using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ArtemisBank.WebAPI.Helpers
{
    internal sealed class BearerSecuritySchemeTransformer(IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

            if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Ingrese su token JWT directamente."
                };

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                };

                if (document.Paths != null)
                {
                    foreach (var pathItem in document.Paths.Values)
                    {
                        if (pathItem.Operations != null)
                        {
                            foreach (var operation in pathItem.Operations.Values)
                            {
                                operation.Security ??= new List<OpenApiSecurityRequirement>();
                                operation.Security.Add(securityRequirement);
                            }
                        }
                    }
                }
            }
        }
    }
}
