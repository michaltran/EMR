using System.Security.Claims;
using EMR.Domain.Entities;
using EMR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMR.Web.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapWebAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/login", async (
            [FromForm] string tenDangNhap,
            [FromForm] string matKhau,
            [FromForm] string? returnUrl,
            EmrDbContext db,
            HttpContext http) =>
        {
            var user = await db.NguoiDungs
                .Include(u => u.VaiTros).ThenInclude(v => v.VaiTro)
                .Include(u => u.Khoa)
                .FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap && u.TrangThai == 1);

            if (user is null || !BCrypt.Net.BCrypt.Verify(matKhau, user.MatKhauHash))
            {
                db.AuditLogs.Add(new AuditLog { HanhDong = "LOGIN_FAIL", ActorTen = tenDangNhap, IpAddress = http.Connection.RemoteIpAddress?.ToString() });
                await db.SaveChangesAsync();
                return Results.Redirect("/login?err=1");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.TenDangNhap),
                new("hoTen", user.HoTen),
                new("khoaTen", user.Khoa?.Ten ?? ""),
            };
            if (user.KhoaId.HasValue) claims.Add(new Claim("khoaId", user.KhoaId.Value.ToString()));
            if (!string.IsNullOrEmpty(user.CCCD)) claims.Add(new Claim("cccd", user.CCCD));
            foreach (var r in user.VaiTros) claims.Add(new Claim(ClaimTypes.Role, r.VaiTro.Ma));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            db.AuditLogs.Add(new AuditLog { HanhDong = "LOGIN", ActorId = user.Id, ActorTen = user.HoTen, IpAddress = http.Connection.RemoteIpAddress?.ToString() });
            await db.SaveChangesAsync();

            return Results.Redirect(string.IsNullOrEmpty(returnUrl) ? "/hoso" : returnUrl);
        }).AllowAnonymous().DisableAntiforgery();

        app.MapPost("/auth/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        }).DisableAntiforgery();

        return app;
    }
}
