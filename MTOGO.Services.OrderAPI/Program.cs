using MTOGO.MessageBus;
using MTOGO.Services.DataAccess;
using MTOGO.Services.OrderAPI.Services;
using MTOGO.Services.OrderAPI.Services.IServices;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<IDataAccess, DataAccess>(sp =>
    new DataAccess(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IOrderService, OrderService>();  // Register IOrderService
builder.Services.AddSingleton<IMessageBus, MessageBus>();   // Register IMessageBus as Singleton

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
});

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
