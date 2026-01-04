using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class JwtTokenValidator
{
    private readonly TokenValidationParameters _parameters;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtTokenValidator(string domain, string audience)
    {
        _parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = domain,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = GetSigningKeys(domain),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    }

    public ClaimsPrincipal Validate(string token)
    {
        return _handler.ValidateToken(token, _parameters, out _);
    }

    private static IEnumerable<SecurityKey> GetSigningKeys(string domain)
    {
        var configManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<
            Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
            $"{domain}.well-known/openid-configuration",
            new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever()
        );

        var config = configManager.GetConfigurationAsync().Result;
        return config.SigningKeys;
    }
}
