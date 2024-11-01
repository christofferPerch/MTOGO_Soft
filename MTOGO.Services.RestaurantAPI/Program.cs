using MTOGO.Services.DataAccess;
using MTOGO.Services.RestaurantAPI.Services;
using MTOGO.Services.RestaurantAPI.Services.IServices;
using MTOGO.MessageBus;
using Microsoft.OpenApi.Models;
using MTOGO.Services.RestaurantAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Register services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
}

// Register the IDataAccess service
builder.Services.AddScoped<IDataAccess, DataAccess>(sp => new DataAccess(connectionString));
builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();

// Add caching and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Restaurant API", Version = "v1" });
});

builder.AddAppAuthetication();

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Restaurant API");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAllOrigins");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
