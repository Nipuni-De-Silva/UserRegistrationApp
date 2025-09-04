using UserRegistrationApp.Components;
using Microsoft.EntityFrameworkCore;
using UserRegistrationApp.Data;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using UserRegistrationApp.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using UserRegistrationApp;
using Microsoft.AspNetCore.Identity.UI.Services;
using UserRegistrationApp.Data.Models;

var builder = WebApplication.CreateBuilder(args);

// Add logging services
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

// Add controllers
builder.Services.AddControllers();

// Other Services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRazorComponents().AddInteractiveServerComponents(
    options =>
    {
        options.DetailedErrors = true;
        // options.DisableFormMapping = true; // Removed: property does not exist on CircuitOptions
    }
);

// Add authentication state provider for Blazor
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider,
    Microsoft.AspNetCore.Components.Server.ServerAuthenticationStateProvider>();
builder.Services.AddCascadingAuthenticationState();

// Configure Email Settings
builder.Services.Configure<UserRegistrationApp.Data.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<UserRegistrationApp.Data.IEmailService, UserRegistrationApp.Data.EmailService>();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure basic HttpClient (keeping for any remaining HTTP needs)
builder.Services.AddHttpClient();

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5171/")
});

// Add CORS for testing (remove or restrict in production)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Enable email confirmation
    options.SignIn.RequireConfirmedEmail = true;   // Require confirmed email
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Remember me duration
    options.SlidingExpiration = true;
    options.Cookie.Name = "MyNoteSpaceAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Log environment for debugging
app.Logger.LogInformation("Environment: {EnvironmentName}", app.Environment.EnvironmentName);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.Logger.LogInformation("Applying Development middleware.");
    app.UseDeveloperExceptionPage();
}
else
{
    app.Logger.LogInformation("Running in non-Development environment.");
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

// app.UseHttpsRedirection();
app.UseCors("AllowAll"); // Before UseRouting (optional, for testing)
app.UseRouting();

// Authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}

app.Logger.LogInformation("Application starting in {Environment} environment.", app.Environment.EnvironmentName);

app.Run();
