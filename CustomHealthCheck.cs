using Azure;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using System;
using System.Linq;
using HCMonitoreoAPis.Domain.Models;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using k8s.KubeConfigModels;
using System.Collections.Generic;
using System.Collections;

namespace HCMonitoreoAPis
{

    public class SampleHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result."));
            }

            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus, "An unhealthy result."));
        }
    }

    public class CustomHealtCheckJson: IHealthCheck
    {
        private string url;
        private Object parameters;
        //private string categoria;
        private CustomHealthCheck customHealthCheck;
        private Dictionary<string, int> healthy;
        //private Dictionary<string, int> unhealthy;

      

        public CustomHealtCheckJson(string apiUrl, string parameter, string categoria)
        {
            

            url = apiUrl;

            DbManager dbManager = new DbManager();


            if (healthy is null)
            {
                healthy = new Dictionary<string, int>();
                //unhealthy = new Dictionary<string, int>();

                healthy.Add(apiUrl, 0);
                //unhealthy.Add(apiUrl, 0);
            }
           



            List<EndPoint> feanorEndPoints = dbManager.findByCategoria(categoria);
            EndPoint? endPoint = feanorEndPoints.Find(x => x.name.Contains("Token"));


            customHealthCheck = new CustomHealthCheck(endPoint.url, endPoint.parametros);


            parameters = JsonConvert.DeserializeObject(parameter);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            await customHealthCheck.CheckHealthAsync(context);

            try
            {
                using (var client = new HttpClient())
                {

                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {customHealthCheck.token}");

                    string jsonData = JsonConvert.SerializeObject(parameters);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage result = await client.PostAsync(url, content);

                    if (result.IsSuccessStatusCode)
                    {
                        byte[] reponseInByte = await result.Content.ReadAsByteArrayAsync();
                        var responseContet = await result.Content.ReadAsStringAsync();

                        //AccessTokenResponses token = await result.Content.ReadAsAsync<AccessTokenResponses>(new[] { new JsonMediaTypeFormatter() });
                        //this.token = token.access_token;


                        
                         var totalConsumo = healthy.GetValueOrDefault(url);
                         if (totalConsumo == 0)
                         {
                            healthy[url] = reponseInByte.Length;
                         }
                         else
                         {
                            healthy[url] = healthy[url] + reponseInByte.Length;
                         }
                            
                        


                            return await Task.FromResult(HealthCheckResult.Healthy($"A healthy result: {responseContet}, total consumo en bytes: {healthy.GetValueOrDefault(url)}"));
                    }
                    else
                    {


                       Notificacion notificacion = new Notificacion();
                        notificacion.enviarCorreo();

                        healthy[url] = 0;


                        return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                        $"An unhealthy result, StatusCode : {(int)result.StatusCode}  Reason : {result.ReasonPhrase}"));




                    }
                }
            }
            catch (Exception exp)
            {
                healthy[url] = 0;
                Notificacion notificacion = new Notificacion();
                notificacion.enviarCorreo();

                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                        $"An unhealthy result : {exp.Message}"));
            }


            
        }
    }
    

    public class CustomHealthCheck : IHealthCheck
    {

        private string url;
        public string token = "";
        public Dictionary<string, object> parameter = new Dictionary<string, object>();
        private Dictionary<string, int> healthy;
        //private Dictionary<string, int> unhealthy;



        public CustomHealthCheck(string apiUrl, string parameters)
        {
            url = apiUrl;
            parameter = JsonConvert.DeserializeObject<Dictionary<string, object>>(parameters);

            if (healthy is null)
            {
                healthy = new Dictionary<string, int>();
                //unhealthy = new Dictionary<string, int>();

                healthy.Add(apiUrl, 0);
                //unhealthy.Add(apiUrl, 0);
            }
        }


        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {


                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(url);

                    var formData = new List<KeyValuePair<string, string>>
                    {
                    new KeyValuePair<string, string>("grant_type", parameter["grant_type"].ToString()),
                    new KeyValuePair<string, string>("password", parameter["password"].ToString()),
                    new KeyValuePair<string, string>("username", parameter["username"].ToString()),
                        /*new KeyValuePair<string, string>("grant_type", "password"),
                        new KeyValuePair<string, string>("password", "Ojo123*"),
                        new KeyValuePair<string, string>("username", "Versatec"),*/
                    };

                    HttpContent content = new FormUrlEncodedContent(formData);
                    HttpResponseMessage response = await client.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        byte[] reponseInByte = await response.Content.ReadAsByteArrayAsync();


                        var responseContet = await response.Content.ReadAsStringAsync();
                        AccessTokenResponses token = await response.Content.ReadAsAsync<AccessTokenResponses>(new[] { new JsonMediaTypeFormatter() });

                        this.token = token.access_token.ToString();

                        var totalConsumo = healthy.GetValueOrDefault(url);
                        if (totalConsumo == 0)
                        {
                            healthy[url] = reponseInByte.Length;
                        }
                        else
                        {
                            healthy[url] = healthy[url] + reponseInByte.Length;
                        }


                        return await Task.FromResult(HealthCheckResult.Healthy($"A healthy result:{token.refresh_token} , total consumo en bytes: {healthy.GetValueOrDefault(url)}")); ;
                    }
                    else
                    {

                        Notificacion notificacion = new Notificacion();
                        notificacion.enviarCorreo();

                        healthy[url] = 0;

                        return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                            $"An unhealthy result, StatusCode : {(int)response.StatusCode}  Reason : {response.ReasonPhrase} {context.Registration}"));
                    }


                }


            }
            catch(Exception exp)
            {
                healthy[url] = 0;
                Notificacion notificacion = new Notificacion();
                notificacion.enviarCorreo();

                return await Task.FromResult(new HealthCheckResult(context.Registration.FailureStatus,
                        $"An unhealthy result : {exp.Message}", data: new Dictionary<string, object>() { { "excepcion", "test" } }));
            }
            

        }

    }
}
