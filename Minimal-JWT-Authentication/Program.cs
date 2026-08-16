using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Minimal_JWT_Authentication.Auth;

namespace Minimal_JWT_Authentication;

internal static class  Program
{
    private static void Main( string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
       
        
        var jwtSettings  = builder.Configuration.GetSection("JwtSettings")
                                                .Get<JwtSettings>() 
                           ?? throw new InvalidOperationException();
        
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key)
        );
        builder.Services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            });
        builder.Services.AddAuthorization();
        
        var app = builder.Build();
        
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.MapGet("/", () => "Hello World!");
        
        app.MapGet("/welcome", () => "Welcome! You are authorized.").RequireAuthorization();

        app.Run();
    }
}