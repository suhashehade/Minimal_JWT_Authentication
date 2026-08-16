using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Minimal_JWT_Authentication.Auth;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Minimal_JWT_Authentication;

public class JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
{
    private readonly JwtSettings _settings = jwtSettings.Value;
    
    public  string GenerateToken(string username, string password)
    {
        // if (username != "testUser" || password != "P@ssw0rd!")
        //     return Unauthorized();
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
        
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = credentials
        };
        var handler = new JsonWebTokenHandler();
        var token = handler.CreateToken(descriptor);
        return token;
    }

    public async Task<bool> ValidateToken(string token)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key)
        );

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,

            ValidateAudience = true,
            ValidAudience = _settings.Audience,

            ValidateLifetime = true,

            ClockSkew = TimeSpan.Zero
        };

        var handler = new JsonWebTokenHandler();
        var result = await handler.ValidateTokenAsync(token, validationParameters);
        return result.IsValid;
    }
}