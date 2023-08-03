using Ars.Commom.Host.Extension;
using Ars.Commom.Tool.Certificates;
using Ars.Common.Consul.Extension;
using Ars.Common.Consul.IApplicationBuilderExtension;
using Ars.Common.Core.AspNetCore.Extensions;
using Ars.Common.Host.Extension;
using Ars.Common.IdentityServer4.Extension;
using Ars.Common.SkyWalking.Extensions;
using GrpcService.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using WebApiGrpcServices.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddTransient<ArsResourcePasswordValidator>();
builder.Services
    .AddArserviceCore(builder, config =>
    {
        config.AddArsIdentityClient();
        config.AddArsConsulRegisterServer();
        config.AddArsSkyApm();
    });
builder.Services.AddGrpc();

//builder.WebHost.UseUrls("https://127.0.0.1:5134");
builder.WebHost.UseArsKestrel(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
//}

app.UsArsExceptionMiddleware();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

app.UseRouting();
app.UseArsCore();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb();
    endpoints.MapGrpcService<HealthCheckService>().EnableGrpcWeb();
    endpoints.MapGet("healthCheck", context =>
    {
        return context.Response.WriteAsync("ok");
    });
});


app.Run();
