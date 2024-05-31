using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using MyDotNetAPP.Services;
using System.Data;
using MyDotNetAPP;
using MyDotNetAPP.Middlewares;
using MyDotNetAPP.Utils;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<ApplicationEnvironment>(builder.Configuration.GetSection("ApplicationSettings"));
var appSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationEnvironment>();
// Add services to the container.
builder.Services.AddControllers();

// Configure Npgsql connection
builder.Services.AddTransient<IDbConnection>((sp) => new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IDbProvider, PostgreSQLProvider>();
builder.Services.AddChatServices();
builder.Services.AddScoped<RequestState>();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.OnAppendCookie = cookieContext =>
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
    options.OnDeleteCookie = cookieContext =>
        cookieContext.CookieOptions.SameSite = SameSiteMode.None;
    options.Secure = CookieSecurePolicy.Always; // required for chromium-based browsers

});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlerMiddleware>();
app.UseMiddleware<JwtMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRouting();

app.MapControllers();

app.Run();
