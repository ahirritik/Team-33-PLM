using Microsoft.AspNetCore.Identity;
using MudBlazor.Services;
using PLM.Application.Mappings;
using FluentValidation;
using PLM.Domain.Entities;
using PLM.Infrastructure;
using PLM.Infrastructure.Seed;
using PLM.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();
builder.Services.AddAutoMapper(cfg => 
{
    cfg.AddProfile<MappingProfile>();
});
builder.Services.AddValidatorsFromAssemblyContaining<PLM.Application.Validators.ECOCreateValidator>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<PLM.Infrastructure.Data.PlmDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapPost("/Account/PerformLogin", async ([Microsoft.AspNetCore.Mvc.FromForm] string? returnUrl, [Microsoft.AspNetCore.Mvc.FromForm] string? email, [Microsoft.AspNetCore.Mvc.FromForm] string? password, SignInManager<ApplicationUser> signInManager) =>
{
    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    {
        return Results.Redirect($"/Account/Login?error=Missing credentials. Email received: '{email}'");
    }
    var result = await signInManager.PasswordSignInAsync(email, password, false, false);
    if (result.Succeeded)
    {
        return Results.LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
    }
    return Results.Redirect("/Account/Login?error=Invalid login attempt");
});

app.MapPost("/Account/Logout", async ([Microsoft.AspNetCore.Mvc.FromForm] string? returnUrl, SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.LocalRedirect(string.IsNullOrEmpty(returnUrl) ? "/" : returnUrl);
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
