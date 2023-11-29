using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using coffee_shop_backend.Business.Abstracts;
using Microsoft.IdentityModel.Tokens;

namespace coffee_shop_backend.Business.Concreates;

public class JwtManager: IJwtServices
{
    private IConfiguration _configuration { get; }

    public JwtManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJwtToken(long Id, string email)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
                new Claim(ClaimTypes.Email, email)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public long GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
        var jwtToken = validatedToken as JwtSecurityToken;
        if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
            StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.Parse(userId);
    }

    public bool IsTokenValid(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
            var jwtToken = validatedToken as JwtSecurityToken;

            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

}