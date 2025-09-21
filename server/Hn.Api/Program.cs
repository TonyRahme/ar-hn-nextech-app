using Hn.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
const string CorsPolicy = "AngularDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod());
});

builder.Services.AddMemoryCache();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Typed HttpClient for Hacker News
builder.Services.AddHttpClient<IHackerNewsService, HackerNewsService>(client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/");
    client.Timeout = TimeSpan.FromSeconds(10);
}).AddStandardResilienceHandler();

var app = builder.Build();
var pathBase = app.Configuration["ASPNETCORE_PATHBASE"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    // Make the app behave as if hosted under /api
    app.UsePathBase(pathBase);
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // relative path works with or without PathBase
    c.SwaggerEndpoint("swagger/v1/swagger.json", "HN API v1");
    c.RoutePrefix = "swagger";
});
app.MapControllers();
app.Run();

public partial class Program { }

