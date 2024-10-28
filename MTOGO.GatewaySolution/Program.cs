using MMLib.SwaggerForOcelot.DependencyInjection;
using MTOGO.GatewaySolution.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.AddAppAuthetication();

string ocelotConfigFile = builder.Environment.IsProduction() ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true);


builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddSwaggerForOcelot(builder.Configuration);

var app = builder.Build();

app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html", true);
    }
    else
    {
        await next();
    }
});

await app.UseOcelot();

app.Run();
