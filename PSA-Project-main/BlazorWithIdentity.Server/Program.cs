using Blazored.SessionStorage;
using Microsoft.EntityFrameworkCore;
using Project.Backend.Server.AuthenticationService;
using Project.Backend.Server.Database;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<DatabaseContext>(options =>
             options.UseSqlite("Filename=SqlLiteDatabase.db"));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "Project", // Replace with your JWT issuer
        ValidAudience = "Project-Audience", // Replace with your JWT audience
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("my-32-character-ultra-secure-and-ultra-long-secret")) // Replace with your secret key
    };
});

builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IHashingService, HashingService>();
builder.Services.AddSingleton<ITokenManager, TokenManager>();


builder.Services.AddBlazoredSessionStorage();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
    {
        //Note: Microsoft recommends to NOT migrate your database at Startup. 
        //You should consider your migration strategy according to the guidelines.
        // serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.Migrate();
    }

    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
