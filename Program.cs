using Azure.Core;
using HCMonitoreoAPis;
using HCMonitoreoAPis.Infraestructura;
using HCMonitoreoAPis.Infraestructura.Repositories;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using HCMonitoreoAPis.Domain.Models;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using YamlDotNet.Core.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<MonitoringContext>();
builder.Services.AddTransient<IRepository<EndPoint>, EndpointsRepository>();


DbManager dbManager = new DbManager();
dbManager.CreateInitialDatabase();

var registro = builder.Services
                      .AddHealthChecks()
                      .Add(new HealthCheckRegistration(name: "SampleHealthCheck1", instance: new SampleHealthCheck(), failureStatus: null, tags: null, timeout: default)
{ Delay = TimeSpan.FromSeconds(40), Period = TimeSpan.FromSeconds(30) });


registro.AddSqlServer(builder.Configuration.GetConnectionString("Connection2"), "select 1;", name: "VmailerBD", failureStatus: HealthStatus.Unhealthy, tags: new[] { "database" });

List<EndPoint> feanorEndPoints = dbManager.findByCategoria("Api Feanor");
foreach(EndPoint endPoint in feanorEndPoints)
{
    //object? obj = JsonConvert.DeserializeObject(endPoint.parametros);

    if (endPoint.name.Contains("Api Base")) {

        registro.AddUrlGroup(setup => { setup.AddUri(new Uri(endPoint.url), x => { x.UsePost(); }); }, endPoint.name, HealthStatus.Unhealthy, tags: new string[] { endPoint.etiqueta });

    }
    else if(endPoint.name.Contains("Api Token")) {
        
        registro.AddCheck(name: endPoint.name, new CustomHealthCheck(endPoint.url,endPoint.parametros), tags: new string[] { endPoint.etiqueta });
    
    } else {

        registro.AddCheck(name: endPoint.name,
                     new CustomHealtCheckJson(endPoint.url, parameter: endPoint.parametros, categoria: endPoint.etiqueta),
                     tags: new string[] { endPoint.etiqueta });

    }


    
}







/*var r = builder.Services.AddHealthChecks()
  .Add(new HealthCheckRegistration(name: "SampleHealthCheck1",instance: new SampleHealthCheck(),failureStatus: null, tags: null, timeout: default)
                                                                           { Delay = TimeSpan.FromSeconds(40), Period = TimeSpan.FromSeconds(30)})
  .AddSqlServer(builder.Configuration.GetConnectionString("Connection2"), "select 1;",name:"VmailerBD", failureStatus: HealthStatus.Unhealthy, tags: new[] { "database" })
  .AddUrlGroup(setup =>{ setup.AddUri(new Uri($"http://localhost:32818"), x => { x.UsePost();});},"Feanor Api Base", HealthStatus.Unhealthy, tags: new string[] { "api", "Api Feanor" })
  .AddCheck(name: "Feanor Api Token", new CustomHealthCheck($"http://localhost:32818/api/token"), tags: new string[]{"api", "Api Feanor"})
  .AddCheck(name: "Feanor Kyc", new CustomHealtCheckJson($"http://localhost:32818/api/EmisorCta/VtcGestionarKYC", 
                                  new {TipoTarjeta = 2, NombreCompleto = "Hector Antonio Saravia",TipoIdentificacion = 1,Telefono = "82040575",Correo = "test@gmail.com",Usuario = "tester"}),
                                  tags: new string[] { "api", "Api Feanor" })
  .AddCheck(name: "Feanor InfoTarjeta", new CustomHealtCheckJson($"http://localhost:32818/api/EmisorCta/VtcInfoTarjeta", new { TarjetaEq = "0000000002"}), tags: new string[] {"api", "Api Feanor" })
  .AddCheck(name: "BiBanK Api Token", new CustomHealthCheck($"http://localhost:32818/api/token"), tags: new string[] { "api", "Api BiBank" })
  .AddCheck(name: "ViaCarte Api Token", new CustomHealthCheck($"http://localhost:32818/api/token"), tags: new string[] { "api", "Api ViaCarte" });*/



builder.Services.AddHealthChecksUI(setupSettings: x=>{

   /* x.SetEvaluationTimeInSeconds(10);
    x.SetApiMaxActiveRequests(60);
    x.MaximumHistoryEntriesPerEndpoint(5);*/
   


}).AddInMemoryStorage();

// Add services to the container.
builder.Services.AddControllersWithViews();

/*builder.Services.AddAuthorization(options => { 
    
    options.AddPolicy("HealthCheckPolicy", policy => policy.RequireClaim("client_policy", "healthChecks"));

});*/

/*builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", Options =>
    {

        Options.Authority = "http://localhost:32818";
        Options.RequireHttpsMetadata = false;
        Options.Audience = "Monitoreo Api";
        


    });*/





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();




/* endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },
        ResponseWriter = WriteHealthCheckResponse,
        AllowCachingResponses = false
    });

    endpoints.MapHealthChecks("/healthcheckui", new
    HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    });*/


app.UseEndpoints(endpoints => 
{

    

    endpoints.MapHealthChecks("/healthcheckui-db", new
    HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = r => r.Tags.Contains("database"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    

    endpoints.MapHealthChecks("/healthcheckui-api", new
    HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = r=>r.Tags.Contains("Api Feanor"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    });


    endpoints.MapHealthChecks("/healthcheckui-apibibank", new
    HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = r => r.Tags.Contains("Api BiBank"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    });


    endpoints.MapHealthChecks("/healthcheckui-apiviacarte", new
   HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = r => r.Tags.Contains("Api ViaCarte"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    });


   /**endpoints.MapHealthChecks("/", new
    HealthCheckOptions()
    {

        ResultStatusCodes =
            {
                 [HealthStatus.Healthy]=StatusCodes.Status200OK,
                 [HealthStatus.Degraded]=StatusCodes.Status401Unauthorized,
                 [HealthStatus.Degraded]=StatusCodes.Status500InternalServerError,
                 [HealthStatus.Unhealthy]=StatusCodes.Status503ServiceUnavailable,
            },

        Predicate = r => r.Tags.Contains("Api Feanor"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

    });**/

});

 async Task<Task> WriteHealthCheckResponse(HttpContext httpcontext, HealthReport result)
{
    httpcontext.Response.ContentType = "application/json";
    var json = new JObject(
                     new JProperty("OverallStatus",result.Status.ToString()),
                     new JProperty("TotalCheckDuration", result.TotalDuration.TotalSeconds.ToString("0:0")),
                     new JProperty("DependencyHealthChecks",new JObject(
                            result.Entries.Select(dict=>new JProperty(dict.Key, new JObject(
                                          new JProperty("status",dict.Value.Status.ToString()),
                                          new JProperty("Duration", dict.Value.Duration.TotalSeconds.ToString("0:0"))
                                       )))
                         ))
        );

    return httpcontext.Response.WriteAsync(json.ToString(Newtonsoft.Json.Formatting.Indented));

}



app.UseStaticFiles();
app.UseCookiePolicy();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseHealthChecksUI(delegate (Options options) { options.UIPath = "/healthui-ui"; options.AddCustomStylesheet("Custom.css") ; options.PageTitle = "Healtchecks";});

//app.UseHealthChecksUI(delegate (Options options) { options.UIPath = ""; options.AddCustomStylesheet("Custom.css"); options.PageTitle = "Healtchecks"; });



app.Run();
