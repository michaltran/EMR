using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EMR.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EMR.Api.Auth;

public class JwtTokenService(IOptions<JwtSettings> opt)
{
    private readonly JwtSettings _s = opt.Value;

    public (string token, DateTime expiresAt) Issue(NguoiDung user, IEnumerable<string> roleCodes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.TenDangNhap),
            new("hoTen", user.HoTen),
        };
        if (!string.IsNullOrEmpty(user.CCCD)) claims.Add(new Claim("cccd", user.CCCD));
        if (user.KhoaId.HasValue) claims.Add(new Claim("khoaId", user.KhoaId.Value.ToString()));
        foreach (var r in roleCodes) claims.Add(new Claim(ClaimTypes.Role, r));

        var expires = DateTime.UtcNow.AddMinutes(_s.ExpiryMinutes);
        var token = new JwtSecurityToken(
            issuer: _s.Issuer,
            audience: _s.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
