using EMR.Infrastructure;
using EMR.Infrastructure.Persistence.Seeds;
using EMR.Infrastructure.Persistence;
using EMR.Web.Auth;
using EMR.Web.Components;
using EMR.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/login";
        o.LogoutPath = "/auth/logout";
        o.AccessDeniedPath = "/login";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
        o.Cookie.Name = "emr.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization(o =>
{
    o.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddEmrInfrastructure(builder.Configuration);
builder.Services.AddScoped<HoSoService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<BaoCaoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    await DemoDataSeeder.RunAsync(app.Services);
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapWebAuth();

// Serve PDF inline (only authenticated users)
app.MapGet("/hoso/{id:guid}/file/{taiLieuId:guid}", async (
    Guid id, Guid taiLieuId, HoSoService svc, HttpContext http) =>
{
    if (!(http.User.Identity?.IsAuthenticated ?? false)) return Results.Unauthorized();
    var t = await svc.GetTaiLieuAsync(id, taiLieuId);
    if (t is null) return Results.NotFound();
    var stream = await svc.OpenPdfAsync(id, taiLieuId);
    if (stream is null) return Results.NotFound();
    return Results.File(stream, t.MimeType, t.TenFile, enableRangeProcessing: true);
}).AllowAnonymous(); // we check auth manually inside; route is reachable for redirect

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
