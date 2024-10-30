using MTOGO.Services.DataAccess;
using MTOGO.Services.FeedbackAPI.Services;
using MTOGO.Services.FeedbackAPI.Services.IServices;
using MTOGO.MessageBus;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register services
builder.Services.AddScoped<IDataAccess, DataAccess>(sp => new DataAccess(connectionString));
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();

// Add caching and session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure CORS
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAllOrigins", builder => {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Add controllers and Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Feedback API", Version = "v1" });
});

var app = builder.Build();

// Use middleware
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Feedback API");
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