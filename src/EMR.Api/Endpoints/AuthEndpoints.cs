using EMR.Api.Auth;
using EMR.Domain.Entities;
using EMR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMR.Api.Endpoints;

public static class AuthEndpoints
{
    public record LoginRequest(string TenDangNhap, string MatKhau);
    public record LoginResponse(string Token, DateTime ExpiresAt, string HoTen, string[] VaiTro, Guid? KhoaId);

    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Auth");

        g.MapPost("/login", async (
            [FromBody] LoginRequest req,
            EmrDbContext db,
            JwtTokenService jwt,
            HttpContext http,
            CancellationToken ct) =>
        {
            var user = await db.NguoiDungs
                .Include(u => u.VaiTros).ThenInclude(v => v.VaiTro)
                .FirstOrDefaultAsync(u => u.TenDangNhap == req.TenDangNhap && u.TrangThai == 1, ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(req.MatKhau, user.MatKhauHash))
            {
                db.AuditLogs.Add(new AuditLog
                {
                    HanhDong = "LOGIN_FAIL",
                    ActorTen = req.TenDangNhap,
                    IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = http.Request.Headers.UserAgent.ToString()
                });
                await db.SaveChangesAsync(ct);
                return Results.Unauthorized();
            }

            var roles = user.VaiTros.Select(x => x.VaiTro.Ma).ToArray();
            var (token, exp) = jwt.Issue(user, roles);

            db.AuditLogs.Add(new AuditLog
            {
                HanhDong = "LOGIN",
                ActorId = user.Id,
                ActorTen = user.HoTen,
                IpAddress = http.Connection.RemoteIpAddress?.ToString(),
                UserAgent = http.Request.Headers.UserAgent.ToString()
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(new LoginResponse(token, exp, user.HoTen, roles, user.KhoaId));
        });

        return app;
    }
}
