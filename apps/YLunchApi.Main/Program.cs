using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using YLunchApi.Application.UserAggregate;
using YLunchApi.Domain.UserAggregate;
using YLunchApi.Infrastructure.Database;
using YLunchApi.Infrastructure.Database.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For Entity Framework
var database = builder.Configuration["DbName"];
var user = builder.Configuration["DbUser"];
var password = builder.Configuration["DbPassword"];
var connectionString = $"Server=127.0.0.1;Port=3309;Database={database};User={user};Password={password};";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        b => b.MigrationsAssembly("YLunchApi.Main")
    );
});

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsPolicyBuilder =>
    {
        corsPolicyBuilder.AllowAnyOrigin();
        corsPolicyBuilder.AllowAnyHeader();
        corsPolicyBuilder.WithMethods(
            HttpMethods.Post,
            HttpMethods.Get,
            HttpMethods.Patch,
            HttpMethods.Put,
            HttpMethods.Delete
        );
    });
});

// ------------------------ MIDDLEWARES ------------------------

var app = builder.Build();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger-original.json", "Simple Inventory API Original"));
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.Urls.Add("http://localhost:5254");
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("Something went wrong, try again later");
        });
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
