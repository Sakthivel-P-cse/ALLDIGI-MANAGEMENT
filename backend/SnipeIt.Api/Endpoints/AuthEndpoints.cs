using Microsoft.EntityFrameworkCore;
using SnipeIt.Api.Models.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using BCrypt.Net;

namespace SnipeIt.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", async (LoginRequest request, SnipeitContext dbContext, IConfiguration config) =>
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Results.Unauthorized();
            }

            // Verify password using BCrypt
            try
            {
                var storedHash = user.Password ?? string.Empty;
                if (!BCrypt.Net.BCrypt.Verify(request.Password ?? string.Empty, storedHash))
                {
                    return Results.Unauthorized();
                }
            }
            catch
            {
                return Results.Unauthorized();
            }

            // Build JWT token
            var jwtKey = config["Jwt:Key"] ?? "ThisIsASuperSecretKeyForSnipeItApi123!";
            var issuer = config["Jwt:Issuer"] ?? "SnipeItApi";
            var audience = config["Jwt:Audience"] ?? "SnipeItUi";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey);
            var isAdmin = !string.IsNullOrEmpty(user.Permissions) && user.Permissions.Contains("\"superuser\":");
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username ?? string.Empty),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, isAdmin ? "admin" : "user")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);

            return Results.Ok(new { Token = token, User = new { user.Id, user.Username, user.Permissions } });
        })
        .WithName("Login")
        .WithOpenApi();
    }
}

public record LoginRequest(string Username, string Password);
