using MTOGO.Services.DataAccess;
using MTOGO.Services.RestaurantAPI.Services.IServices;
using MTOGO.Services.RestaurantAPI.Services;
using MTOGO.MessageBus;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<IDataAccess, DataAccess>(sp =>
    new DataAccess(connectionString));

builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IMessageBus, MessageBus>();

builder.Services.AddDistributedMemoryCache();  

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
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
