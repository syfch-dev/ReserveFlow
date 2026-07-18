using ReserveFlow.Api.Options;
using ReserveFlow.Application;
using ReserveFlow.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<HttpsOptions>()
    .Bind(builder.Configuration.GetSection(HttpsOptions.SectionName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var httpsOptions = builder.Configuration
    .GetSection(HttpsOptions.SectionName)
    .Get<HttpsOptions>() ?? new HttpsOptions();

if (httpsOptions.Port is int httpsPort)
{
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = httpsPort);
}

var app = builder.Build();

app.UseMiddleware<ReserveFlow.Api.Middleware.ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "ReserveFlow API v1");
    });
}

if (httpsOptions.Port.HasValue)
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();
