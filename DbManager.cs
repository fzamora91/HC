using HCMonitoreoAPis.Infraestructura;
using HCMonitoreoAPis.Infraestructura.Repositories;
using HCMonitoreoAPis.Domain.Models;
using System;
using IdentityModel.Client;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using static IdentityModel.OidcConstants;

namespace HCMonitoreoAPis
{
    public class DbManager
    {

        public void CreateInitialDatabase()
        {

            using (var context = new MonitoringContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                List<EndPoint> endPoints = new List<EndPoint>();


                /*new KeyValuePair<string, string>("grant_type", "password"),
                       new KeyValuePair<string, string>("password", "Ojo123*"),
                       new KeyValuePair<string, string>("username", "Versatec"),*/



                endPoints.Add(new EndPoint
                {
                   name = "Feanor Api Token",
                    url = "http://localhost:32818/api/token",
                    tipoParametros = "json",
                    etiqueta = "Api Feanor",
                    parametros = JsonConvert.SerializeObject(new  { grant_type = "password", password= "Ojo123*", username= "Versatec" }) 
                });

                endPoints.Add(new EndPoint
                {
                    name = "Feanor Api Base",
                    url = "http://localhost:32818/",
                    tipoParametros = "json",
                    etiqueta = "Api Feanor",
                    parametros = JsonConvert.SerializeObject(new Object { })
                });

                endPoints.Add(new EndPoint
                                    {   name = "VtcGestionarKYC",
                                        url = "http://localhost:32818/api/EmisorCta/VtcGestionarKYC",
                                        tipoParametros = "json",
                                        etiqueta = "Api Feanor",
                                        parametros = JsonConvert.SerializeObject(new
                                        {
                                            TipoTarjeta = 2,
                                            NombreCompleto = "Hector Antonio Saravia",
                                            TipoIdentificacion = 1,
                                            Telefono = "82040575",
                                            Correo = "test@gmail.com",
                                            Usuario = "tester"
                                        })
                                    });


                endPoints.Add(new EndPoint
                {   name = "Feanor InfoTarjeta",
                    url = "http://localhost:32818/api/EmisorCta/VtcInfoTarjeta",
                    tipoParametros = "json",
                    etiqueta = "Api Feanor",
                    parametros = JsonConvert.SerializeObject(new { Tarjeta = "0000000002" })
                });


                var endpointRepository = new EndpointsRepository(context);
                endpointRepository.AddRange(endPoints);







                endpointRepository.SaveChanges();







            }
        }


        public List<EndPoint> findByCategoria(string categoria)
        {

            using (var context = new MonitoringContext())
            {

                IRepository<EndPoint> EndpointsRepository = new EndpointsRepository(context);

                List<EndPoint> endPoints = EndpointsRepository.Find(x => x.etiqueta.Equals(categoria)).ToList();


                return endPoints;

            }
               



        }


    }
}
