using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using SiteBuilder.Data;
using SiteBuilder.Middleware;
using SiteBuilder.Models;
using SiteBuilder.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var corsOrigins = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
var explicitOrigin = builder.Configuration["ContactApi:AllowedOrigin"];
if (!string.IsNullOrWhiteSpace(explicitOrigin))
{
    corsOrigins.Add(explicitOrigin.Trim().TrimEnd('/'));
}

var pagesBaseUrl = builder.Configuration["GitHubPublish:PagesBaseUrl"];
if (Uri.TryCreate(pagesBaseUrl, UriKind.Absolute, out var pagesUri))
{
    corsOrigins.Add($"{pagesUri.Scheme}://{pagesUri.Host}");
}

if (builder.Environment.IsDevelopment())
{
    corsOrigins.Add("https://localhost:5001");
    corsOrigins.Add("http://localhost:5000");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("ContactApi", policy =>
    {
        if (corsOrigins.Count == 0)
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            return;
        }

        policy.WithOrigins(corsOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var googleClientId = builder.Configuration["GOOGLE_CLIENT_ID"];
var googleClientSecret = builder.Configuration["GOOGLE_CLIENT_SECRET"];
if (string.IsNullOrWhiteSpace(googleClientId) || string.IsNullOrWhiteSpace(googleClientSecret))
{
    throw new InvalidOperationException("Set GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET environment variables.");
}

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
    })
    .AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SaveTokens = false;
        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var email = context.Principal?.FindFirstValue(ClaimTypes.Email)
                            ?? context.Principal?.FindFirstValue("email");
                var googleId = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(googleId))
                {
                    throw new InvalidOperationException("Google profile did not return required claims.");
                }

                var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                var user = await db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        Email = email,
                        GoogleId = googleId,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                }
                else if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    await db.SaveChangesAsync();
                }

                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                if (!identity.HasClaim(c => c.Type == "app_user_id"))
                {
                    identity.AddClaim(new Claim("app_user_id", user.Id.ToString()));
                }
            }
        };
    });

builder.Services.AddSingleton<HtmlTemplateGenerator>();

builder.Services.AddHostedService<TrialCheckService>();

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("ContactApi");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TrialMiddleware>();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();



