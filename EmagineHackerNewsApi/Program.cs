using EmagineHackerNewsApi.Services;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddLazyCache();

builder.Services.AddHttpClient("HackerNewsClient", client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("HackerNewsApi:BaseUrl");
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("The configuration value for 'HackerNewsApi:BaseUrl' is missing or null.");
    }
    client.BaseAddress = new Uri(baseUrl);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

builder.Services.AddScoped<IStoriesService, StoriesService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
