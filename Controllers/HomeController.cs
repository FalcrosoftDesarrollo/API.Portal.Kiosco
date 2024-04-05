/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : HomeController.cs                                                        *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase controlador que contiene los métodos para interactuar con las      *
*                páginas de la vista                                                      *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
*   Detalle Cambio: Refactorizacion código -> (Antoine Román - Falcrosoft) 02/01/2024     *
******************************************************************************************/
using APIPortalKiosco.Data;
using APIPortalKiosco.Helpers;
using APIPortalKiosco.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace APIPortalKiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : Controller
    {
        #region CONSTRUCTOR
        /// <summary>
        /// Valor público de parámetro de variables de sesión
        /// </summary>
        public ISession Session => this.HttpContext.Session;

        /// <summary>
        /// Valor privado de parámetro de configuración
        /// </summary>
        private readonly IOptions<MyConfig> config;

        /// <summary>
        /// Constructor del controlador
        /// </summary>
        /// <param name="config">Parámetro de valores de configuración</param>
        public HomeController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        #region GET

        /// <summary>
        /// GET: Index -- Carga de cartelera de péliculas principales desde XML
        /// </summary>
        /// <returns></returns>
       
        [HttpGet]
        [Route("GetCartelera")]
        public ActionResult Home(string Teatro = "0", string Ciudad = "0")
        {
            #region VARIABLES LOCALES
            bool flag;
            var lc_switpel = string.Empty;
            var lc_auxitem = string.Empty;
            var lc_auxipel = string.Empty;
            var lc_swtpel = string.Empty;
            var lc_srvpar = string.Empty;
            var lc_result = string.Empty;

            XmlDocument ob_xmldoc = new XmlDocument();

            Billboard ob_bilmov;
            Cartelera ob_carprg = new Cartelera();
            General ob_fncgrl = new General();
            List<Billboard> ob_lisMov = new List<Billboard>();
            List<hora> ob_horflg = new List<hora>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
            #endregion

            try
            {

                if (Session.GetString("FlagLogin") == "INV")
                {
                    Session.Remove("Usuario");
                    Session.Remove("Nombre");
                    Session.Remove("Passwrd");
                    Session.Remove("Usuario");
                    Session.Remove("Apellido");
                    Session.Remove("Telefono");
                    Session.Remove("Direccion");
                    Session.Remove("Documento");
                    Session.Remove("FlagLogin");
                    ViewBag.ListCarritoR = null;
                    ViewBag.ListCarritoB = null;
                }

                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }



                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                //Validar ciudad y teatro desde web externa
                if (Teatro != "0")
                    Selteatros(Teatro);

                ViewBag.ListaM = null;

                #region SCOCAR
                ob_carprg.Teatro = Session.GetString("Teatro");
                ob_carprg.tercero = config.Value.ValorTercero;
                ob_carprg.IdPelicula = "0";
                ob_carprg.FcPelicula = "19000101";
                ob_carprg.TpPelicula = "Normal";
                ob_carprg.FgPelicula = "1";
                ob_carprg.CfPelicula = Session.GetString("ClienteFrecuente");

                //Generar y encriptar JSON para servicio PRE
                lc_srvpar = ob_fncgrl.JsonConverter(ob_carprg);
                lc_srvpar = lc_srvpar.Replace("teatro", "Teatro");
                lc_srvpar = lc_srvpar.Replace("idPelicula", "IdPelicula");
                lc_srvpar = lc_srvpar.Replace("fcPelicula", "FcPelicula");
                lc_srvpar = lc_srvpar.Replace("tpPelicula", "TpPelicula");
                lc_srvpar = lc_srvpar.Replace("fgPelicula", "FgPelicula");
                lc_srvpar = lc_srvpar.Replace("cfPelicula", "CfPelicula");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocar/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "SalesBol/Detail";
                logSales.Metodo = "SCOCAR";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                    ob_lisMov = (List<Billboard>)JsonConvert.DeserializeObject(ob_diclst["ob_lismov"].ToString(), (typeof(List<Billboard>)));
                }
                else
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
                }
                #endregion

                //Asignar flag maxima hora de funcion a session
                Session.Remove("Finhora");
                if (config.Value.MinDifConf != "0")
                {
                    DateTime FechaHoraTermino = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    hora ob_hora = ob_horflg.ToList().LastOrDefault();

                    //Diferencia de tiempo entre hora funcion y hora del dia 
                    TimeSpan diferencia = ob_hora.fechayhora - FechaHoraTermino;
                    var diferenciaenminutos = diferencia.TotalMinutes;

                    if (diferenciaenminutos > Convert.ToDouble(config.Value.MinDifConf))
                        Session.SetString("Finhora", "S");
                    else
                        Session.SetString("Finhora", "N");
                }
                else
                {
                    Session.SetString("Finhora", "S");
                }

                URLPortal(config);
                ListCarrito();

                //Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                //Devolver a vista
                return View();
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/Home";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }


        /// <summary>
        /// GET: Index -- Carga de preventa de péliculas principales desde XML
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("GetPreventaPeliculas")]
        public ActionResult Home2(string Teatro = "0", string Ciudad = "0")
        {
            var helper = new Helper();
            #region VARIABLES LOCALES
            bool flag;
            string lc_switpel = string.Empty;
            string lc_auxitem = string.Empty;
            string lc_auxipel = string.Empty;
            string lc_swtpel = string.Empty;


            XmlDocument ob_xmldoc = new XmlDocument();

            Billboard ob_bilmov;
            General ob_fncgrl = new General();
            List<Billboard> ob_lisMov = new List<Billboard>();
            #endregion

            try
            {
                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }

                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                URLPortal(config);
                ListCarrito();

                //Validar ciudad y teatro desde web externa
                if (Teatro != "0")
                    Selteatros(Teatro);

                //Obtener información de la web
                ViewBag.ListaM = null;

                ob_xmldoc.Load(config.Value.Variables41T + Session.GetString("Teatro"));
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                //Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {
                    //Validar y obtener id de commet principal
                    if (item.GetAttribute("id").ToString().Length == 8)
                        lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 3);
                    if (item.GetAttribute("id").ToString().Length == 9)
                        lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 4);
                    if (item.GetAttribute("id").ToString().Length == 10)
                        lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 5);

                    //Obtener y validar pelicula en lista
                    flag = ob_lisMov.Any(x => x.Auxids == lc_auxipel);
                    if (!flag)
                    {
                        //Validar si es cartelera
                        if (item.GetAttribute("tipo").ToString() == "Preventa")
                        {
                            //Datos de nodo pelicula
                            ob_bilmov = new Billboard();
                            ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                            ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                            ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();
                            ob_bilmov.Auxids = lc_auxipel;
                            ob_bilmov.Switch = "V";
                            ob_bilmov.TipoSala = helper.TipPelicula(item.GetAttribute("nombre").ToString());

                            //Datos de nodo pelicula/sinopsis
                            ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                            //Datos de nodo pelicula/data
                            XmlNodeList data = item.GetElementsByTagName("data");
                            foreach (XmlElement item2 in data)
                            {
                                ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                ob_bilmov.TituloOriginal = item2.GetAttribute("Titulo").ToString();

                                ob_bilmov.Pais = item2.GetAttribute("pais").ToString();
                                ob_bilmov.Medio = item2.GetAttribute("medio").ToString();
                                ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                ob_bilmov.Idioma = item2.GetAttribute("idioma").ToString();
                                ob_bilmov.Genero = item2.GetAttribute("genero").ToString();
                                ob_bilmov.Reparto = item2.GetAttribute("reparto").ToString();
                                ob_bilmov.Censura = item2.GetAttribute("Censura").ToString();
                                ob_bilmov.Formato = item2.GetAttribute("formato").ToString();
                                ob_bilmov.Version = item2.GetAttribute("versión").ToString();
                                ob_bilmov.Director = item2.GetAttribute("director").ToString();
                                ob_bilmov.Duracion = item2.GetAttribute("duracion").ToString();
                                ob_bilmov.Trailer1 = item2.GetAttribute("trailer1").ToString();
                                ob_bilmov.Trailer2 = item2.GetAttribute("trailer2").ToString();
                                ob_bilmov.FechaEstreno = item2.GetAttribute("fechaEstreno").ToString();
                                ob_bilmov.Distribuidor = item2.GetAttribute("distribuidor").ToString();

                                lc_swtpel = item2.GetAttribute("Habilitado").ToString();

                                //Datos de nodo pelicula/cinemas
                                XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                                foreach (XmlElement item3 in cinemas)
                                {
                                    // Datos de nodo pelicula / cinemas / cinema
                                    XmlNodeList cinema = item3.GetElementsByTagName("cinema");
                                    foreach (XmlElement item4 in cinema)
                                    {
                                        //Validar Teatro
                                        int lc_keytea = Convert.ToInt32(item4.GetAttribute("id"));
                                        int lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));

                                        if (lc_keytea == lc_auxtea && lc_swtpel == "true")
                                        {
                                            //Adiconar a lista para mostrar
                                            ob_lisMov.Add(ob_bilmov);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                //Devolver a vista
                return View();
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/Home2";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }


        /// <summary>
        /// GET: Index -- Carga de cartelera de péliculas desde XML
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("GetCargaCarteleraPeliculas")]
        public ActionResult Index(string pr_fecprg = "", string pr_keypel = "", string Teatro = "0", string Ciudad = "0")
        {
            #region VARIABLES LOCALES
            string Variables41TP = string.Empty;
            string lc_switpel = string.Empty;
            string lc_auxitem = string.Empty;
            string lc_auxipel = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;

            XmlDocument ob_xmldoc = new XmlDocument();

            Billboard ob_bilmov;
            Cartelera ob_carprg = new Cartelera();
            Billboard ob_infrmc = new Billboard();
            General ob_fncgrl = new General();
            List<Billboard> ob_lisMov = new List<Billboard>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
            #endregion

            try
            {
                //Validar inicio de sesión
                //if (Teatro != "0" && Session.GetString("Usuario") == null)
                //    return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "PL" });

                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }

                //Cargar Casback
                Session.Remove("Secuencia");
                Session.Remove("CashBack_Acumulado");

                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                //Validar ciudad y teatro desde web externa
                if (Teatro != "0")
                    Selteatros(Teatro);

                URLPortal(config);
                ListCarrito();

                List<DateCartelera> datePortal = DatePortal(pr_fecprg, "Normal", pr_keypel);
                if (datePortal.Count <= 0)
                    datePortal = DatePortal(pr_fecprg, "Estreno", pr_keypel);

                bool flag = datePortal.Any(x => x.Flags == "S");

                //Validar flag rojo
                if (!flag)
                    datePortal[0].Flags = "S";

                //Validar tag HOY
                string todayString = DateTime.Now.ToString("yyyyMMdd");

                foreach (var fecha in datePortal)
                {
                    if (todayString == fecha.FecSt)
                    {
                        fecha.DiaLt = "HOY";
                    }
                }

                //Validar fecha
                if (pr_fecprg == "")
                    pr_fecprg = datePortal[0].FecSt;

                ViewBag.Cartelera = datePortal;
                ViewBag.Fechaprog = pr_fecprg;
                ViewBag.CommentPPAL = pr_keypel;

                //Obtener información de la web
                ViewBag.ListaM = null;

                #region SCOCAR
                ob_carprg.Teatro = Session.GetString("Teatro");
                ob_carprg.tercero = config.Value.ValorTercero;
                ob_carprg.IdPelicula = pr_keypel;
                ob_carprg.FcPelicula = pr_fecprg;
                ob_carprg.TpPelicula = "Normal";
                ob_carprg.FgPelicula = "3";
                ob_carprg.CfPelicula = Session.GetString("ClienteFrecuente");

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(ob_carprg);
                lc_srvpar = lc_srvpar.Replace("teatro", "Teatro");
                lc_srvpar = lc_srvpar.Replace("idPelicula", "IdPelicula");
                lc_srvpar = lc_srvpar.Replace("fcPelicula", "FcPelicula");
                lc_srvpar = lc_srvpar.Replace("tpPelicula", "TpPelicula");
                lc_srvpar = lc_srvpar.Replace("fgPelicula", "FgPelicula");
                lc_srvpar = lc_srvpar.Replace("cfPelicula", "CfPelicula");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocar/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "SalesBol/Detail";
                logSales.Metodo = "SCOCAR";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                    ob_lisMov = (List<Billboard>)JsonConvert.DeserializeObject(ob_diclst["ob_lisMov"].ToString(), (typeof(List<Billboard>)));
                    ob_infrmc = (Billboard)JsonConvert.DeserializeObject(ob_diclst["ob_infrmc"].ToString(), (typeof(Billboard)));
                }
                else
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
                }
                #endregion



                //Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                //Devolver a vista
                return View(ob_infrmc);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/Index";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }


        /// <summary>
        /// GET: Index -- Carga de cartelera de estrenos desde XML
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("GetCargaCarteleraEstrenos")]
        public ActionResult Index2(string pr_fecprg = "", string pr_keypel = "")
        {
            #region VARIABLES LOCALES
            string Variables41TP = string.Empty;
            string lc_switpel = string.Empty;
            string lc_auxitem = string.Empty;
            string lc_auxipel = string.Empty;

            XmlDocument ob_xmldoc = new XmlDocument();

            Billboard ob_bilmov;
            Billboard ob_infrmc = new Billboard();
            General ob_fncgrl = new General();
            List<Billboard> ob_lisMov = new List<Billboard>();
            #endregion
            var helper = new Helper();
            try
            {
                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }

                //Cargar Casback
                Session.Remove("Secuencia");
                Session.Remove("CashBack_Acumulado");

                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                //Validar fecha
                if (pr_fecprg == "")
                    pr_fecprg = DateTime.Now.ToString("yyyyMMdd");

                URLPortal(config);
                ListCarrito();

                List<DateCartelera> datePortal = DatePortal(pr_fecprg, "Preventa", pr_keypel);
                bool flag = datePortal.Any(x => x.Flags == "S");

                //Validar flago rojo
                if (!flag)
                    datePortal[0].Flags = "S";

                ViewBag.Cartelera = datePortal;
                ViewBag.Fechaprog = ViewBag.Cartelera[0].FecSt;
                ViewBag.CommentPPAL = pr_keypel;

                //Obtener información de la web
                ViewBag.ListaM = null;


                ob_xmldoc.Load(config.Value.Variables41);
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                //Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {
                    //Validar si viene commet ppal
                    if (pr_keypel != "")
                    {
                        //Validar y obtener id de commet principal
                        if (item.GetAttribute("id").ToString().Length == 8)
                            lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 3);
                        if (item.GetAttribute("id").ToString().Length == 9)
                            lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 4);
                        if (item.GetAttribute("id").ToString().Length == 10)
                            lc_auxipel = item.GetAttribute("id").ToString().Substring(0, 5);

                        //Validar comment ppal
                        if (lc_auxipel == pr_keypel)
                        {
                            //Validar si es cartelera
                            if (item.GetAttribute("tipo").ToString() == "Preventa")
                            {
                                //Datos de nodo pelicula
                                ob_bilmov = new Billboard();
                                ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                                ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                                ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();
                                ob_bilmov.Auxids = item.GetAttribute("id").ToString();
                                ob_bilmov.Switch = "V";
                                ob_bilmov.TipoSala = helper.TipPelicula(item.GetAttribute("nombre").ToString());

                                //Datos de nodo pelicula/sinopsis
                                ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                                //Datos de nodo pelicula/data
                                XmlNodeList data = item.GetElementsByTagName("data");
                                foreach (XmlElement item2 in data)
                                {
                                    //Validar flag
                                    if (item2.GetAttribute("Habilitado").ToString() == "true")
                                    {
                                        ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                        ob_bilmov.TituloOriginal = item2.GetAttribute("Titulo").ToString();

                                        ob_bilmov.Pais = item2.GetAttribute("pais").ToString();
                                        ob_bilmov.Medio = item2.GetAttribute("medio").ToString();
                                        ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                        ob_bilmov.Idioma = item2.GetAttribute("idioma").ToString();
                                        ob_bilmov.Genero = item2.GetAttribute("genero").ToString();
                                        ob_bilmov.Reparto = item2.GetAttribute("reparto").ToString();
                                        ob_bilmov.Censura = item2.GetAttribute("Censura").ToString();
                                        ob_bilmov.Formato = item2.GetAttribute("formato").ToString();
                                        ob_bilmov.Version = item2.GetAttribute("versión").ToString();
                                        ob_bilmov.Director = item2.GetAttribute("director").ToString();
                                        ob_bilmov.Duracion = item2.GetAttribute("duracion").ToString();
                                        ob_bilmov.Trailer1 = item2.GetAttribute("trailer1").ToString();
                                        ob_bilmov.Trailer2 = item2.GetAttribute("trailer2").ToString();
                                        ob_bilmov.FechaEstreno = item2.GetAttribute("fechaEstreno").ToString();
                                        ob_bilmov.Distribuidor = item2.GetAttribute("distribuidor").ToString();

                                        //Datos de nodo pelicula/cinemas
                                        XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                                        foreach (XmlElement item3 in cinemas)
                                        {
                                            // Datos de nodo pelicula / cinemas / cinema
                                            XmlNodeList cinema = item3.GetElementsByTagName("cinema");
                                            foreach (XmlElement item4 in cinema)
                                            {
                                                //Validar Teatro
                                                int lc_keytea = Convert.ToInt32(item4.GetAttribute("id"));
                                                int lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));

                                                if (lc_keytea == lc_auxtea)
                                                {
                                                    // Datos de nodo pelicula / salas
                                                    XmlNodeList salas = item4.GetElementsByTagName("salas");
                                                    foreach (XmlElement itemS in salas)
                                                    {
                                                        // Datos de nodo pelicula / salas / sala
                                                        XmlNodeList sala = itemS.GetElementsByTagName("sala");
                                                        foreach (XmlElement itemSS in sala)
                                                        {
                                                            //Datos de nodo pelicula / salas / dia
                                                            XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
                                                            foreach (XmlElement item5 in Fecha)
                                                            {
                                                                lc_auxitem = item5.GetAttribute("univ").ToString();

                                                                // Datos de nodo pelicula / Fecha / hora
                                                                List<Hora> ob_hora = new List<Hora>();
                                                                XmlNodeList hora = item5.GetElementsByTagName("hora");
                                                                foreach (XmlElement item6 in hora)
                                                                    ob_hora.Add(new Hora() { fecunv = lc_auxitem, idFuncion = item6.GetAttribute("idFuncion").ToString(), horario = item6.GetAttribute("horario").ToString() });

                                                                List<Fechas> ob_fecha = new List<Fechas>();
                                                                ob_fecha.Add(new Fechas { fecham = item5.GetAttribute("dia").ToString(), fecunv = item5.GetAttribute("univ").ToString(), horafun = ob_hora });

                                                                //Solo primera fecha
                                                                ob_bilmov.Fechafunc = ob_fecha;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    ob_lisMov.Add(ob_bilmov);
                                                }
                                            }
                                        }
                                    }
                                }

                                ob_infrmc = ob_bilmov;
                            }
                        }
                    }
                    else
                    {
                        //Validar si es cartelera
                        if (item.GetAttribute("tipo").ToString() == "Preventa")
                        {
                            //Datos de nodo pelicula
                            ob_bilmov = new Billboard();
                            ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                            ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                            ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();
                            ob_bilmov.Auxids = item.GetAttribute("id").ToString();
                            ob_bilmov.Switch = "V";
                            ob_bilmov.TipoSala = helper.TipPelicula(item.GetAttribute("nombre").ToString());

                            //Datos de nodo pelicula/sinopsis
                            ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                            //Datos de nodo pelicula/data
                            XmlNodeList data = item.GetElementsByTagName("data");
                            foreach (XmlElement item2 in data)
                            {
                                //Validar flag
                                if (item2.GetAttribute("Habilitado").ToString() == "true")
                                {
                                    ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                    ob_bilmov.TituloOriginal = item2.GetAttribute("Titulo").ToString();

                                    ob_bilmov.Pais = item2.GetAttribute("pais").ToString();
                                    ob_bilmov.Medio = item2.GetAttribute("medio").ToString();
                                    ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                    ob_bilmov.Idioma = item2.GetAttribute("idioma").ToString();
                                    ob_bilmov.Genero = item2.GetAttribute("genero").ToString();
                                    ob_bilmov.Reparto = item2.GetAttribute("reparto").ToString();
                                    ob_bilmov.Censura = item2.GetAttribute("Censura").ToString();
                                    ob_bilmov.Formato = item2.GetAttribute("formato").ToString();
                                    ob_bilmov.Version = item2.GetAttribute("versión").ToString();
                                    ob_bilmov.Director = item2.GetAttribute("director").ToString();
                                    ob_bilmov.Duracion = item2.GetAttribute("duracion").ToString();
                                    ob_bilmov.Trailer1 = item2.GetAttribute("trailer1").ToString();
                                    ob_bilmov.Trailer2 = item2.GetAttribute("trailer2").ToString();
                                    ob_bilmov.FechaEstreno = item2.GetAttribute("fechaEstreno").ToString();
                                    ob_bilmov.Distribuidor = item2.GetAttribute("distribuidor").ToString();

                                    //Datos de nodo pelicula/cinemas
                                    XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                                    foreach (XmlElement item3 in cinemas)
                                    {
                                        // Datos de nodo pelicula / cinemas / cinema
                                        XmlNodeList cinema = item3.GetElementsByTagName("cinema");
                                        foreach (XmlElement item4 in cinema)
                                        {
                                            //Validar Teatro
                                            int lc_keytea = Convert.ToInt32(item4.GetAttribute("id"));
                                            int lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));

                                            if (lc_keytea == lc_auxtea)
                                            {
                                                // Datos de nodo pelicula / salas
                                                XmlNodeList salas = item4.GetElementsByTagName("salas");
                                                foreach (XmlElement itemS in salas)
                                                {
                                                    // Datos de nodo pelicula / salas / sala
                                                    XmlNodeList sala = itemS.GetElementsByTagName("sala");
                                                    foreach (XmlElement itemSS in sala)
                                                    {
                                                        //Datos de nodo pelicula / salas / dia
                                                        XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
                                                        foreach (XmlElement item5 in Fecha)
                                                        {
                                                            lc_auxitem = item5.GetAttribute("univ").ToString();

                                                            // Datos de nodo pelicula / Fecha / hora
                                                            List<Hora> ob_hora = new List<Hora>();
                                                            XmlNodeList hora = item5.GetElementsByTagName("hora");
                                                            foreach (XmlElement item6 in hora)
                                                                ob_hora.Add(new Hora() { fecunv = lc_auxitem, idFuncion = item6.GetAttribute("idFuncion").ToString(), horario = item6.GetAttribute("horario").ToString() });

                                                            List<Fechas> ob_fecha = new List<Fechas>();
                                                            ob_fecha.Add(new Fechas { fecham = item5.GetAttribute("dia").ToString(), fecunv = item5.GetAttribute("univ").ToString(), horafun = ob_hora });

                                                            //Solo primera fecha
                                                            ob_bilmov.Fechafunc = ob_fecha;
                                                            break;
                                                        }
                                                    }
                                                }

                                                ob_lisMov.Add(ob_bilmov);
                                            }
                                        }
                                    }
                                }
                            }
                            ob_infrmc = ob_bilmov;
                        }
                    }
                }

                //Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                //Devolver a vista
                return View(ob_infrmc);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/Index2";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }


        /// <summary>
        /// GET: Index -- Carga de cartelera de proximamente desde XML
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("GetCargaCarteleraProximamente")]
        public ActionResult Index3(string pr_fecprg = "", string Teatro = "0", string Ciudad = "0")
        {
            try
            {
                // Inicializar variables
                var helper = new Helper();
                string lc_auxitem = string.Empty;
                string lc_swtpel = string.Empty;
                XmlDocument ob_xmldoc = new XmlDocument();
                Billboard ob_bilmov;
                List<Billboard> ob_lisMov = new List<Billboard>();

                // Configurar sesiones y cargar datos iniciales
                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }

                // Cargar Casback
                Session.Remove("Secuencia");
                Session.Remove("CashBack_Acumulado");

                // Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                URLPortal(config);
                ListCarrito();

                // Validar ciudad y teatro desde web externa
                if (Teatro != "0")
                    Selteatros(Teatro);

                // Obtener información de la web
                ViewBag.ListaM = null;
                ob_xmldoc.Load(config.Value.Variables41);
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                // Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {
                    // Validar si es cartelera
                    if (item.GetAttribute("tipo").ToString() == "Próximo Estreno")
                    {
                        // Datos de nodo pelicula
                        ob_bilmov = new Billboard();
                        ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                        ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                        ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();
                        ob_bilmov.Auxids = item.GetAttribute("id").ToString();
                        ob_bilmov.Switch = "V";
                        ob_bilmov.TipoSala = helper.TipPelicula(item.GetAttribute("nombre").ToString());

                        // Datos de nodo pelicula/sinopsis
                        ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                        // Datos de nodo pelicula/data
                        XmlNodeList data = item.GetElementsByTagName("data");
                        foreach (XmlElement item2 in data)
                        {
                            if (item2.GetAttribute("Habilitado").ToString() == "true")
                            {
                                ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                ob_bilmov.TituloOriginal = item2.GetAttribute("Titulo").ToString();

                                ob_bilmov.Pais = item2.GetAttribute("pais").ToString();
                                ob_bilmov.Medio = item2.GetAttribute("medio").ToString();
                                ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                ob_bilmov.Idioma = item2.GetAttribute("idioma").ToString();
                                ob_bilmov.Genero = item2.GetAttribute("genero").ToString();
                                ob_bilmov.Reparto = item2.GetAttribute("reparto").ToString();
                                ob_bilmov.Censura = item2.GetAttribute("Censura").ToString();
                                ob_bilmov.Formato = item2.GetAttribute("formato").ToString();
                                ob_bilmov.Version = item2.GetAttribute("versión").ToString();
                                ob_bilmov.Director = item2.GetAttribute("director").ToString();
                                ob_bilmov.Duracion = item2.GetAttribute("duracion").ToString();
                                ob_bilmov.Trailer1 = item2.GetAttribute("trailer1").ToString();
                                ob_bilmov.Trailer2 = item2.GetAttribute("trailer2").ToString();
                                ob_bilmov.FechaEstreno = item2.GetAttribute("fechaEstreno").ToString();
                                ob_bilmov.Distribuidor = item2.GetAttribute("distribuidor").ToString();

                                // Datos de nodo pelicula/cinemas
                                XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                                foreach (XmlElement item3 in cinemas)
                                {
                                    // Datos de nodo pelicula / cinemas / cinema
                                    XmlNodeList cinema = item3.GetElementsByTagName("cinema");
                                    foreach (XmlElement item4 in cinema)
                                    {
                                        // Datos de nodo pelicula / salas
                                        XmlNodeList salas = item4.GetElementsByTagName("salas");
                                        foreach (XmlElement itemS in salas)
                                        {
                                            // Datos de nodo pelicula / salas / sala
                                            XmlNodeList sala = itemS.GetElementsByTagName("sala");
                                            foreach (XmlElement itemSS in sala)
                                            {
                                                // Datos de nodo pelicula / salas / dia
                                                XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
                                                foreach (XmlElement item5 in Fecha)
                                                {
                                                    lc_auxitem = item5.GetAttribute("univ").ToString();

                                                    // Datos de nodo pelicula / Fecha / hora
                                                    List<Hora> ob_hora = new List<Hora>();
                                                    XmlNodeList hora = item5.GetElementsByTagName("hora");
                                                    foreach (XmlElement item6 in hora)
                                                        ob_hora.Add(new Hora() { fecunv = lc_auxitem, idFuncion = item6.GetAttribute("idFuncion").ToString(), horario = item6.GetAttribute("horario").ToString() });

                                                    List<Fechas> ob_fecha = new List<Fechas>();
                                                    ob_fecha.Add(new Fechas { fecham = item5.GetAttribute("dia").ToString(), fecunv = item5.GetAttribute("univ").ToString(), horafun = ob_hora });

                                                    // Solo primera fecha
                                                    ob_bilmov.Fechafunc = ob_fecha;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Adiconar a lista para mostrar
                                ob_lisMov.Add(ob_bilmov);
                            }
                        }
                    }
                }

                // Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                // Devolver a vista
                return View();
            }
            catch (Exception lc_syserr)
            {
                // Generar Log y devolver vista de error
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/Index3";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                // Escribir Log
                logAudit.LogApp(logSales);

                // Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }


        /// <summary>
        /// GET: Index -- Carga de cartelera de proximamente desde XML INFORMACION
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("GetCargaCarteleraProximamenteXMLInformacion")]
        public ActionResult InfIndex3(string pr_keypel = "")
        {
            #region VARIABLES LOCALES
            string lc_auxitem = string.Empty;
            string lc_swtpel = string.Empty;
            string lc_auxipel = string.Empty;
            var helper = new Helper();
            XmlDocument ob_xmldoc = new XmlDocument();

            Billboard ob_bilmov;
            Billboard ob_infrmc = new Billboard();
            General ob_fncgrl = new General();
            List<Billboard> ob_lisMov = new List<Billboard>();
            #endregion

            try
            {
                if (Session.GetString("Usuario") == null)
                {
                    Session.Remove("FlagCompra");
                    Session.SetString("FlagCompra", "N");

                    Session.Remove("ClienteFrecuente");
                    Session.SetString("ClienteFrecuente", "No");
                }

                //Cargar Casback
                Session.Remove("Secuencia");
                Session.Remove("CashBack_Acumulado");

                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                URLPortal(config);
                ListCarrito();

                //Obtener información de la web
                ViewBag.ListaM = null;
                ob_xmldoc.Load(config.Value.Variables41);
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                //Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {

                    //Validar si es cartelera
                    if (item.GetAttribute("tipo").ToString() == "Próximo Estreno")
                    {
                        if (pr_keypel == item.GetAttribute("id").ToString())
                        {
                            //Datos de nodo pelicula
                            ob_bilmov = new Billboard();
                            ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                            ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                            ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();
                            ob_bilmov.Auxids = item.GetAttribute("id").ToString();
                            ob_bilmov.Switch = "V";
                            ob_bilmov.TipoSala = helper.TipPelicula(item.GetAttribute("nombre").ToString());

                            //Datos de nodo pelicula/sinopsis
                            ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                            //Datos de nodo pelicula/data
                            XmlNodeList data = item.GetElementsByTagName("data");
                            foreach (XmlElement item2 in data)
                            {
                                if (item2.GetAttribute("Habilitado").ToString() == "true")
                                {
                                    ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                    ob_bilmov.TituloOriginal = item2.GetAttribute("Titulo").ToString();

                                    ob_bilmov.Pais = item2.GetAttribute("pais").ToString();
                                    ob_bilmov.Medio = item2.GetAttribute("medio").ToString();
                                    ob_bilmov.Imagen = item2.GetAttribute("Imagen").ToString();
                                    ob_bilmov.Idioma = item2.GetAttribute("idioma").ToString();
                                    ob_bilmov.Genero = item2.GetAttribute("genero").ToString();
                                    ob_bilmov.Reparto = item2.GetAttribute("reparto").ToString();
                                    ob_bilmov.Censura = item2.GetAttribute("Censura").ToString();
                                    ob_bilmov.Formato = item2.GetAttribute("formato").ToString();
                                    ob_bilmov.Version = item2.GetAttribute("versión").ToString();
                                    ob_bilmov.Director = item2.GetAttribute("director").ToString();
                                    ob_bilmov.Duracion = item2.GetAttribute("duracion").ToString();
                                    ob_bilmov.Trailer1 = item2.GetAttribute("trailer1").ToString();
                                    ob_bilmov.Trailer2 = item2.GetAttribute("trailer2").ToString();
                                    ob_bilmov.FechaEstreno = item2.GetAttribute("fechaEstreno").ToString();
                                    ob_bilmov.Distribuidor = item2.GetAttribute("distribuidor").ToString();

                                    //Datos de nodo pelicula/cinemas
                                    XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                                    foreach (XmlElement item3 in cinemas)
                                    {
                                        // Datos de nodo pelicula / cinemas / cinema
                                        XmlNodeList cinema = item3.GetElementsByTagName("cinema");
                                        foreach (XmlElement item4 in cinema)
                                        {
                                            // Datos de nodo pelicula / salas
                                            XmlNodeList salas = item4.GetElementsByTagName("salas");
                                            foreach (XmlElement itemS in salas)
                                            {
                                                // Datos de nodo pelicula / salas / sala
                                                XmlNodeList sala = itemS.GetElementsByTagName("sala");
                                                foreach (XmlElement itemSS in sala)
                                                {
                                                    //Datos de nodo pelicula / salas / dia
                                                    XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
                                                    foreach (XmlElement item5 in Fecha)
                                                    {
                                                        lc_auxitem = item5.GetAttribute("univ").ToString();

                                                        // Datos de nodo pelicula / Fecha / hora
                                                        List<Hora> ob_hora = new List<Hora>();
                                                        XmlNodeList hora = item5.GetElementsByTagName("hora");
                                                        foreach (XmlElement item6 in hora)
                                                            ob_hora.Add(new Hora() { fecunv = lc_auxitem, idFuncion = item6.GetAttribute("idFuncion").ToString(), horario = item6.GetAttribute("horario").ToString() });

                                                        List<Fechas> ob_fecha = new List<Fechas>();
                                                        ob_fecha.Add(new Fechas { fecham = item5.GetAttribute("dia").ToString(), fecunv = item5.GetAttribute("univ").ToString(), horafun = ob_hora });

                                                        //Solo primera fecha
                                                        ob_bilmov.Fechafunc = ob_fecha;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            //Adiconar a lista para mostrar
                            ob_lisMov.Add(ob_bilmov);

                            ob_infrmc = ob_bilmov;
                        }
                    }
                }

                //Cargar Viewbag
                if (ob_lisMov != null)
                    ViewBag.ListaM = ob_lisMov;

                //Devolver a vista
                return View(ob_infrmc);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/InfIndex3";
                logSales.Metodo = "GETXML";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        [HttpGet]
        [Route("CerrarSesion")]
        /// <summary>
        /// GET: CloseSession -- Cerrar la sesión del usuario
        /// </summary>
        /// <returns></returns>
        public ActionResult CloseSession()
        {
            //Cerrar Sesión
            Session.Remove("Nombre");
            Session.Remove("Passwrd");
            Session.Remove("Usuario");
            Session.Remove("Apellido");
            Session.Remove("Telefono");
            Session.Remove("Direccion");
            Session.Remove("Documento");
            Session.Remove("ClienteFrecuente");

            //Devolver a vista
            return RedirectToAction("Home", "Home");
        }
        [HttpGet]
        [Route("CargarCiudadesyTeatroxDefecto")]
        public ActionResult Cart(string pr_allpth)
        {
            //Cargar ciudades home y teatro por defecto si aplica
            if (Session.GetString("Teatro") != null)
            {
                Ciuteatros("SEL");
            }
            else
            {
                if (Session.GetString("CiudadTeatro") != null)
                    Ciuteatros(Session.GetString("CiudadTeatro"));
                else
                    Ciuteatros();
            }

            ListCarrito();

            ViewBag.Path = pr_allpth;

            //Devolver Vista parcial
            return View();
        }

        [HttpGet]
        [Route("EliminarItemCarritoCompras")]
        /// <summary>
        /// GET: DeleteItemCart -- Eliminar un item del carrito de compras
        /// </summary>
        /// <param name="pr_iditem"></param>
        /// <param name="pr_flgite"></param>
        /// <returns></returns>
        public ActionResult DeleteItemCart(int pr_iditem, string pr_flgite, string pr_allpth = "")
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            int ListCarritoR = 0;
            int ListCarritoB = 0;
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitm = string.Empty;
            string lc_contrl = string.Empty;
            string lc_clview = string.Empty;

            string[] ls_lstsel = new string[5];

            General ob_fncgrl = new General();
            List<string> ls_lstubi = new List<string>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
            #endregion

            try
            {
                //Obtener objeto BD para eliminar item
                using (var context = new DataDB(config))
                {
                    if (pr_flgite == "B")
                    {
                        //Obtener valores a tabla ReportSales
                        var ReportSales = context.ReportSales.Where(x => x.Id == pr_iditem).ToList();
                        foreach (var report in ReportSales)
                        {
                            //Obtener ubicaciones de vista
                            char[] ar_charst = report.SelUbicaciones.ToCharArray();
                            for (int lc_iditem = 0; lc_iditem < ar_charst.Length; lc_iditem++)
                            {
                                //Concatenar caracteres
                                lc_auxitm += ar_charst[lc_iditem].ToString();

                                //Obtener parámetro
                                if (ar_charst[lc_iditem].ToString() == ";")
                                {
                                    ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
                                    lc_auxitm = string.Empty;
                                }
                            }

                            //Cargar ubicaciones al modelo JSON
                            lc_auxitm = string.Empty;
                            foreach (var item in ls_lstubi)
                            {
                                lc_idearr = 0;
                                char[] ar_chars2 = item.ToCharArray();
                                for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
                                {
                                    //Concatenar caracteres
                                    lc_auxitm += ar_chars2[lc_iditem].ToString();

                                    //Obtener parámetro
                                    if (ar_chars2[lc_iditem].ToString() == "_")
                                    {
                                        ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);

                                        lc_idearr++;
                                        lc_auxitm = string.Empty;
                                    }
                                }

                                #region SCOSIL
                                LiberaSilla ob_libsrv = new LiberaSilla();
                                ob_libsrv.Fila = ls_lstsel[3];
                                ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
                                ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));

                                if (report.HorProg.Length == 4)
                                    ob_libsrv.Funcion = Convert.ToInt32(report.HorProg.Substring(0, 2));
                                else
                                    ob_libsrv.Funcion = Convert.ToInt32(report.HorProg.Substring(0, 1));

                                ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
                                ob_libsrv.Usuario = 777;
                                ob_libsrv.tercero = config.Value.ValorTercero;
                                ob_libsrv.FechaFuncion = report.FecProg;

                                //Generar y encriptar JSON para servicio
                                lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

                                lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
                                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                                lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                                lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                                lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

                                //Encriptar Json LIB
                                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                                //Consumir servicio LIB
                                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);

                                //Generar Log
                                LogSales logSales = new LogSales();
                                LogAudit logAudit = new LogAudit(config);
                                logSales.Id = Guid.NewGuid().ToString();
                                logSales.Fecha = DateTime.Now;
                                logSales.Programa = "Home/DeleteItemCart";
                                logSales.Metodo = "SCOSIL";
                                logSales.ExceptionMessage = lc_srvpar;
                                logSales.InnerExceptionMessage = lc_result;

                                //Escribir Log
                                logAudit.LogApp(logSales);

                                //Devolver vista de error
                                // return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                                #endregion
                            }

                            //Eliminar item y guardar registro a tabla
                            context.ReportSales.Remove(report);
                            context.SaveChanges();
                        }
                    }
                    else
                    {
                        //Agregar valores a tabla ReportSales
                        var retailSales = new RetailSales
                        {
                            Id = pr_iditem,
                            Tipo = "0",
                            Precio = 0,
                            Cantidad = 0,
                            Secuencia = 0,
                            PuntoVenta = 0,
                            KeyProducto = 0,
                            Descripcion = "0",
                            ProCategoria1 = 0,
                            ProCategoria2 = 0,
                            ProCategoria3 = 0,
                            ProCategoria4 = 0,
                            ProCategoria5 = 0,
                            CanCategoria1 = 0,
                            CanCategoria2 = 0,
                            CanCategoria3 = 0,
                            CanCategoria4 = 0,
                            CanCategoria5 = 0,
                            FechaRegistro = DateTime.Now
                        };

                        //Eliminar item y guardar registro a tabla
                        context.RetailSales.Remove(retailSales);
                        context.SaveChanges();
                    }
                }

                //Obtener productos carrito de compra
                decimal lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                using (var context = new DataDB(config))
                {
                    //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                    decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                    decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
                    var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    ListCarritoR = RetailSales.Count;
                }

                //Obtener boletas carrito de compra
                using (var context = new DataDB(config))
                {
                    //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia
                    string PuntoVenta = config.Value.PuntoVenta;
                    string KeyTeatro = Session.GetString("Teatro");
                    var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    ListCarritoB = ReportSales.Count;
                }

                ////Devolver a vista
                //if (ListCarritoR == 0 & ListCarritoB == 0)
                //    return RedirectToAction("Home", "Home");
                //else
                //    return RedirectToAction("Home", "Home");

                if (ListCarritoR == 0 & ListCarritoB == 0)
                {
                    return RedirectToAction("Home", "Home");
                }
                else
                {
                    ListCarrito();

                    //Validar si es compra rapida o medio normal
                    if (pr_allpth.Contains("fastsales"))
                    {
                        //Validar si es Boletas
                        if (pr_allpth.Contains("listcon"))
                            return RedirectToAction("ListCon", "FastSales", new { pr_secpro = Session.GetString("pr_secproFS"), pr_swtven = Session.GetString("pr_swtvenFS"), pr_tiplog = Session.GetString("pr_tiplogFS"), pr_tbview = Session.GetString("pr_tbviewFS"), pr_cenprg = Session.GetString("pr_cenprgFS") });
                    }
                    else
                    {
                        //Validar Si es Boletas
                        if (pr_allpth.Contains("salesbol"))
                        {
                            if (pr_allpth.Contains("preonboarding"))
                                return RedirectToAction("PreOnboarding", "SalesBol", new { pr_keypel = Session.GetString("pr_keypel"), pr_fecprg = Session.GetString("pr_fecprg"), pr_horprg = Session.GetString("pr_horprg"), pr_tarprg = Session.GetString("pr_tarprg"), pr_salprg = Session.GetString("pr_salprg"), pr_nompel = Session.GetString("pr_nompel"), pr_nomfec = Session.GetString("pr_nomfec"), pr_nomhor = Session.GetString("pr_nomhor"), pr_nomtar = Session.GetString("pr_nomtar"), pr_cenprg = Session.GetString("pr_cenprg"), pr_secsec = Session.GetString("pr_secsec"), pr_selubi = Session.GetString("pr_selubi") });

                            if (pr_allpth.Contains("onboarding"))
                                return RedirectToAction("Onboarding", "SalesBol", new { pr_keypel = Session.GetString("pr_keypel"), pr_fecprg = Session.GetString("pr_fecprg"), pr_horprg = Session.GetString("pr_horprg"), pr_tarprg = Session.GetString("pr_tarprg"), pr_salprg = Session.GetString("pr_salprg"), pr_nompel = Session.GetString("pr_nompel"), pr_nomfec = Session.GetString("pr_nomfec"), pr_nomhor = Session.GetString("pr_nomhor"), pr_nomtar = Session.GetString("pr_nomtar"), pr_cenprg = Session.GetString("pr_cenprg"), pr_secsec = Session.GetString("pr_secsec"), pr_selubi = Session.GetString("pr_selubi") });
                        }

                        //Validar Si es Confites
                        if (pr_allpth.Contains("salescon"))
                        {
                            if (pr_allpth.Contains("productlist"))
                                return RedirectToAction("ProductList", "SalesCon", new { pr_secpro = Session.GetString("pr_secpro"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_tbview = Session.GetString("pr_tbview") });

                            if (pr_allpth.Contains("details"))
                                return RedirectToAction("Details", "SalesCon", new { pr_keypro = Session.GetString("pr_keypro"), pr_secpro = Session.GetString("pr_secpro"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog") });
                        }

                        //Validar Si es Pages
                        if (pr_allpth.Contains("pages"))
                        {
                            if (pr_allpth.Contains("payment"))
                                return RedirectToAction("Payment", "Pages", new { pr_secsec = Session.GetString("pr_secsec"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_cenprg = Session.GetString("pr_cenprg"), pr_nomeli = Session.GetString("Nombre"), pr_doceli = Session.GetString("Documento"), pr_coreli = Session.GetString("Usuario"), pr_teleli = Session.GetString("Telefono") });

                            if (pr_allpth.Contains("termconditions"))
                                return RedirectToAction("TermConditions", "Pages", new { pr_secsec = Session.GetString("pr_secsec"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_cenprg = Session.GetString("pr_cenprg") });
                        }

                    }

                    //Devolver a vista
                    return RedirectToAction("Home", "Home");
                }
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/DeleteItemCart";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                ListCarrito();

                //Validar si es compra rapida o medio normal
                if (pr_allpth.Contains("fastsales"))
                {
                    //Validar si es Boletas
                    if (pr_allpth.Contains("listcon"))
                        return RedirectToAction("ListCon", "FastSales", new { pr_secpro = Session.GetString("pr_secproFS"), pr_swtven = Session.GetString("pr_swtvenFS"), pr_tiplog = Session.GetString("pr_tiplogFS"), pr_tbview = Session.GetString("pr_tbviewFS"), pr_cenprg = Session.GetString("pr_cenprgFS") });
                }
                else
                {
                    //Validar Si es Boletas
                    if (pr_allpth.Contains("salesbol"))
                    {
                        if (pr_allpth.Contains("preonboarding"))
                            return RedirectToAction("PreOnboarding", "SalesBol", new { pr_keypel = Session.GetString("pr_keypel"), pr_fecprg = Session.GetString("pr_fecprg"), pr_horprg = Session.GetString("pr_horprg"), pr_tarprg = Session.GetString("pr_tarprg"), pr_salprg = Session.GetString("pr_salprg"), pr_nompel = Session.GetString("pr_nompel"), pr_nomfec = Session.GetString("pr_nomfec"), pr_nomhor = Session.GetString("pr_nomhor"), pr_nomtar = Session.GetString("pr_nomtar"), pr_cenprg = Session.GetString("pr_cenprg"), pr_secsec = Session.GetString("pr_secsec"), pr_selubi = Session.GetString("pr_selubi") });

                        if (pr_allpth.Contains("onboarding"))
                            return RedirectToAction("Onboarding", "SalesBol", new { pr_keypel = Session.GetString("pr_keypel"), pr_fecprg = Session.GetString("pr_fecprg"), pr_horprg = Session.GetString("pr_horprg"), pr_tarprg = Session.GetString("pr_tarprg"), pr_salprg = Session.GetString("pr_salprg"), pr_nompel = Session.GetString("pr_nompel"), pr_nomfec = Session.GetString("pr_nomfec"), pr_nomhor = Session.GetString("pr_nomhor"), pr_nomtar = Session.GetString("pr_nomtar"), pr_cenprg = Session.GetString("pr_cenprg"), pr_secsec = Session.GetString("pr_secsec"), pr_selubi = Session.GetString("pr_selubi") });
                    }

                    //Validar Si es Confites
                    if (pr_allpth.Contains("salescon"))
                    {
                        if (pr_allpth.Contains("productlist"))
                            return RedirectToAction("ProductList", "SalesCon", new { pr_secpro = Session.GetString("pr_secpro"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_tbview = Session.GetString("pr_tbview") });

                        if (pr_allpth.Contains("details"))
                            return RedirectToAction("Details", "SalesCon", new { pr_keypro = Session.GetString("pr_keypro"), pr_secpro = Session.GetString("pr_secpro"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog") });
                    }

                    //Validar Si es Pages
                    if (pr_allpth.Contains("pages"))
                    {
                        if (pr_allpth.Contains("payment"))
                            return RedirectToAction("Payment", "Pages", new { pr_secsec = Session.GetString("pr_secsec"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_cenprg = Session.GetString("pr_cenprg"), pr_nomeli = Session.GetString("Nombre"), pr_doceli = Session.GetString("Documento"), pr_coreli = Session.GetString("Usuario"), pr_teleli = Session.GetString("Telefono") });

                        if (pr_allpth.Contains("termconditions"))
                            return RedirectToAction("TermConditions", "Pages", new { pr_secsec = Session.GetString("pr_secsec"), pr_swtven = Session.GetString("pr_swtven"), pr_tiplog = Session.GetString("pr_tiplog"), pr_cenprg = Session.GetString("pr_cenprg") });
                    }

                }

                //Devolver a vista
                return RedirectToAction("Home", "Home");
            }
        }

        [HttpGet]
        [Route("EliminarCarritoCompras")]
        /// <summary>
        /// GET: DeleteCart -- Eliminar carrito de compras
        /// </summary>
        /// <returns></returns>
        public ActionResult DeleteCart()
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitm = string.Empty;
            string[] ls_lstsel = new string[5];

            General ob_fncgrl = new General();
            List<string> ls_lstubi = new List<string>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
            #endregion

            try
            {
                //Borrar retail
                decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                using (var context = new DataDB(config))
                {
                    //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                    decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                    decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
                    var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec1).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    foreach (var retail in RetailSales)
                    {
                        //Borrar retail detcombo
                        lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                        using (var context1 = new DataDB(config))
                        {
                            //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                            var RetailDet = context.RetailDet.Where(x => x.IdRetailSales == retail.Id).ToList();
                            foreach (var retaildet in RetailDet)
                            {
                                //Eliminar item y guardar registro a tabla
                                context.RetailDet.Remove(retaildet);
                                context.SaveChanges();
                            }
                        }

                        //Eliminar item y guardar registro a tabla
                        context.RetailSales.Remove(retail);
                        context.SaveChanges();
                    }
                }
                //Borrar boletas
                string lc_secsec2 = Session.GetString("Secuencia");
                using (var context = new DataDB(config))
                {
                    //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                    string PuntoVenta = config.Value.PuntoVenta;
                    string KeyTeatro = Session.GetString("Teatro");
                    var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec2).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    foreach (var report in ReportSales)
                    {
                        //Obtener ubicaciones de vista
                        char[] ar_charst = report.SelUbicaciones.ToCharArray();
                        for (int lc_iditem = 0; lc_iditem < ar_charst.Length; lc_iditem++)
                        {
                            //Concatenar caracteres
                            lc_auxitm += ar_charst[lc_iditem].ToString();

                            //Obtener parámetro
                            if (ar_charst[lc_iditem].ToString() == ";")
                            {
                                ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
                                lc_auxitm = string.Empty;
                            }
                        }

                        //Cargar ubicaciones al modelo JSON
                        lc_auxitm = string.Empty;
                        foreach (var item in ls_lstubi)
                        {
                            lc_idearr = 0;
                            char[] ar_chars2 = item.ToCharArray();
                            for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
                            {
                                //Concatenar caracteres
                                lc_auxitm += ar_chars2[lc_iditem].ToString();

                                //Obtener parámetro
                                if (ar_chars2[lc_iditem].ToString() == "_")
                                {
                                    ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);

                                    lc_idearr++;
                                    lc_auxitm = string.Empty;
                                }
                            }

                            #region SCOSIL
                            LiberaSilla ob_libsrv = new LiberaSilla();
                            ob_libsrv.Fila = ls_lstsel[3];
                            ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
                            ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));

                            if (report.HorProg.Length == 4)
                                ob_libsrv.Funcion = Convert.ToInt32(report.HorProg.Substring(0, 2));
                            else
                                ob_libsrv.Funcion = Convert.ToInt32(report.HorProg.Substring(0, 1));

                            ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
                            ob_libsrv.Usuario = 777;
                            ob_libsrv.tercero = config.Value.ValorTercero;
                            ob_libsrv.FechaFuncion = report.FecProg;

                            //Generar y encriptar JSON para servicio
                            lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

                            lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
                            lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                            lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                            lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                            lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                            lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

                            //Encriptar Json LIB
                            lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                            //Consumir servicio LIB
                            lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Home/DeleteCart";
                            logSales.Metodo = "SCOSIL";
                            logSales.ExceptionMessage = lc_srvpar;
                            logSales.InnerExceptionMessage = lc_result;

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                            #endregion
                        }

                        //Eliminar item y guardar registro a tabla
                        context.ReportSales.Remove(report);
                        context.SaveChanges();
                    }
                }

                //Quitar secuencia
                Session.Remove("Secuencia");

                //Devolver a vista
                return RedirectToAction("Home", "Home");
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/DeleteCart";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }
        [HttpGet]
        [Route("SeleccionarCiudadTeatro")]
        /// <summary>
        /// GET: SelCiuteatro -- Seleccionar ciudad y/o teatro
        /// </summary>
        /// <returns></returns>
        public ActionResult SelCiuteatro(string pr_ciudad = "", string pr_teatro = "0", string pr_nomteatro = "", string pr_url = "")
        {
            //Remover sesion
            Session.Remove("Teatro");
            Session.Remove("TeatroNombre");
            Session.Remove("CiudadTeatro");

            //Asignar ciudad
            if (pr_ciudad != "")
                Ciuteatros(pr_ciudad);

            //Asignar teatro
            if (pr_teatro != "0")
                Selteatros(pr_ciudad, pr_teatro, pr_nomteatro, "");

            //Devolver a vista
            if (pr_url.Contains("fastsales"))
                return RedirectToAction("Home", "FastSales");
            else
                return RedirectToAction("Home");

        }

        [HttpGet]
        [Route("ObtenerDatosCompra")]
        /// <summary>
        /// GET: LastSaleIdx -- Obtener datos para consultar compra
        /// </summary>
        /// <returns></returns>
        public ActionResult LastSaleIdx()
        {
            #region VARIABLES LOCALES
            XmlDocument ob_xmldoc = new XmlDocument();
            List<SelectListItem> teatros = new List<SelectListItem>();

            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();
                ViewBag.FlagAdmin = Session.GetString("FlagAdmin");
                Session.Remove("FlagCompra");
                Session.SetString("FlagCompra", "R");

                //Cargar ciudades home y teatro por defecto si aplica
                if (Session.GetString("Teatro") != null)
                {
                    Ciuteatros("SEL");
                }
                else
                {
                    if (Session.GetString("CiudadTeatro") != null)
                        Ciuteatros(Session.GetString("CiudadTeatro"));
                    else
                        Ciuteatros();
                }

                //Obtener información de la web
                ViewBag.Teatro = null;
                //Obtengo fechas del mes
                ViewBag.Fechas = null;
                // Obtener el primer día del mes actual
                DateTime primerDiaDelMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                // Obtener el último día del mes actual
                DateTime ultimoDiaDelMes = primerDiaDelMes.AddMonths(1).AddDays(-1);

                // Crear una lista para almacenar los días del mes como objetos SelectListItem
                List<SelectListItem> diasDelMes = new List<SelectListItem>();

                // Agregar todos los días del mes a la lista como objetos SelectListItem
                for (DateTime fecha = primerDiaDelMes; fecha <= ultimoDiaDelMes; fecha = fecha.AddDays(1))
                {
                    diasDelMes.Add(new SelectListItem
                    {
                        Text = fecha.ToShortDateString(), // Usar la fecha como texto a mostrar
                        Value = fecha.ToShortDateString() // Usar la fecha como valor
                    });
                }

                // Asignar la lista de días del mes al control ListBox1
                ViewBag.Fechas = diasDelMes;
                //Obtener y recorrer teatros de xml para carga en vista
                List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
                foreach (var item in ls_ciuteatros)
                    if (item.Habilitado == "S")
                        teatros.Add(new SelectListItem() { Text = item.nombre, Value = item.id });

                //Devolver a vista
                ViewBag.Teatro = teatros;
                return View();

            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/LastSaleIdx";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        [HttpGet]
        [Route("ConsultarPantallaTransaccion")]
        /// <summary>
        /// GET: LastSaleDtl -- Consultar y mostrar en pantalla transacción
        /// </summary>
        /// <param name="pr_keytea"></param>
        /// <param name="pr_refext"></param>
        /// <returns></returns>
        public ActionResult LastSaleDtl(decimal pr_keytea, string pr_refext)
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            int lc_cntubi = 0;

            string lc_ubilbl = string.Empty;
            string lc_auxitm = string.Empty;
            string lc_ubiprg = string.Empty;

            string[] ls_lstsel = new string[5];
            List<string> ls_lstubi = new List<string>();
            List<OrderItem> ob_ordite = new List<OrderItem>();

            ReportSales ob_bolvta = new ReportSales();
            RetailSales ob_retvta = new RetailSales();
            TransactionSales ob_repsle = new TransactionSales();
            #endregion

            //Inicializar valores
            ViewBag.Id = 0;
            ViewBag.FB = "N";
            ViewBag.FR = "N";

            try
            {
                decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                using (var context = new DataDB(config))
                {
                    //Consultar registro de venta en BD transacciones
                    var ob_repsl1 = context.TransactionSales.Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.Teatro == pr_keytea).Where(x => x.ReferenciaTx == pr_refext);
                    foreach (var TransactionSales in ob_repsl1)
                        ob_repsle = context.TransactionSales.Find(TransactionSales.Id);

                    //Validar transacción
                    if (ob_repsle.Id != 0)
                    {
                        ViewBag.Id = ob_repsle.Id;
                        ViewBag.Secuencia = ob_repsle.Teatro + "-" + ob_repsle.PuntoVenta + "-" + ob_repsle.Secuencia;
                        ViewBag.EmailEli = ob_repsle.EmailEli;
                        ViewBag.NombreEli = ob_repsle.NombreEli;
                        ViewBag.DocumentoEli = ob_repsle.DocumentoEli;
                        ViewBag.TelefonoEli = ob_repsle.TelefonoEli;
                        ViewBag.EstadoTx = ob_repsle.EstadoTx + "-" + ob_repsle.Observaciones;
                        ViewBag.FechaTx = ob_repsle.FechaTx;
                        ViewBag.ValorTx = string.Format("{0:C0}", Convert.ToInt32(ob_repsle.ValorTx));
                        ViewBag.ReferenciaTx = ob_repsle.ReferenciaTx;
                        ViewBag.BancoTx = ob_repsle.BancoTx;

                        //Boletas
                        using (var boletas = new DataDB(config))
                        {
                            //Consultar registro de venta boletas
                            string Secuencia = ob_repsle.Secuencia.ToString();
                            string Teatro = ob_repsle.Teatro.ToString();
                            var ob_repbol = boletas.ReportSales.Where(x => x.Secuencia == Secuencia).Where(x => x.KeyTeatro == Teatro).Where(x => x.KeyPunto == config.Value.PuntoVenta);
                            foreach (var reportSales in ob_repbol)
                                ob_bolvta = boletas.ReportSales.Find(reportSales.Id);

                            //Validar transaccion
                            if (ob_bolvta.Id > 0)
                            {
                                //Obtener ubicaciones de vista
                                char[] ar_charst = ob_bolvta.SelUbicaciones.ToCharArray();
                                for (int lc_iditem = 0; lc_iditem < ar_charst.Length; lc_iditem++)
                                {
                                    //Concatenar caracteres
                                    lc_auxitm += ar_charst[lc_iditem].ToString();

                                    //Obtener parámetro
                                    if (ar_charst[lc_iditem].ToString() == ";")
                                    {
                                        ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
                                        lc_auxitm = string.Empty;
                                    }
                                }

                                //Cargar ubicaciones al modelo JSON
                                lc_auxitm = string.Empty;
                                foreach (var item in ls_lstubi)
                                {
                                    lc_idearr = 0;
                                    char[] ar_chars2 = item.ToCharArray();
                                    for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
                                    {
                                        //Concatenar caracteres
                                        lc_auxitm += ar_chars2[lc_iditem].ToString();

                                        //Obtener parámetro
                                        if (ar_chars2[lc_iditem].ToString() == "_")
                                        {
                                            ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);

                                            lc_idearr++;
                                            lc_auxitm = string.Empty;
                                        }
                                    }

                                    lc_cntubi++;
                                    lc_ubilbl += ls_lstsel[1] + ls_lstsel[2] + " ";
                                }

                                string valor = ob_bolvta.NombreTar.Substring(ob_bolvta.NombreTar.IndexOf(";") + 1);
                                ViewBag.FB = "S";
                                ViewBag.Sala = ob_bolvta.KeySala;
                                ViewBag.SecuenciaBol = ob_bolvta.Secuencia;
                                ViewBag.Ubicaciones = lc_ubilbl;
                                ViewBag.CantidadUbi = lc_cntubi.ToString();
                                ViewBag.Fecha = ob_bolvta.NombreFec;
                                ViewBag.Hora = ob_bolvta.NombreHor;
                                ViewBag.Tarifa = ob_bolvta.NombreTar.Substring(0, ob_bolvta.NombreTar.IndexOf(";")) + " " + string.Format("{0:C0}", Convert.ToDecimal(valor)) + " COP";
                                ViewBag.Imagen = ob_bolvta.Referencia;
                                ViewBag.Valor = string.Format("{0:C0}", Convert.ToInt32(ob_bolvta.Precio));
                            }
                        }

                        //Confites
                        using (var retail = new DataDB(config))
                        {
                            //Consultar registro de venta retail
                            decimal PtoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                            var ob_repcon = retail.RetailSales.Where(x => x.Secuencia == ob_repsle.Secuencia).Where(x => x.KeyTeatro == ob_repsle.Teatro).Where(x => x.PuntoVenta == PtoVenta);
                            foreach (var retailSales in ob_repcon)
                            {
                                //Adicionar a lista
                                ob_ordite.Add(new OrderItem
                                {
                                    Precio = retailSales.Precio * Convert.ToInt32(retailSales.Cantidad),
                                    Cantidad = Convert.ToInt32(retailSales.Cantidad),
                                    Descripcion = retailSales.Descripcion,
                                    KeyProducto = Convert.ToInt32(retailSales.KeyProducto)
                                });
                            }

                            //Validar transacción
                            if (ob_ordite.Count > 0)
                            {
                                ViewBag.FR = "S";
                                ViewBag.ListaP = ob_ordite;
                            }
                        }
                    }
                }
                /*using (var context = new DataDB(config))
                {
                    // Consulta única para obtener los datos de transacción y boletas
                    var query = from ts in context.TransactionSales
                                join rs in context.ReportSales on ts.Secuencia.ToString() equals rs.Secuencia
                                where ts.PuntoVenta == PuntoVenta
                                    && ts.Teatro == pr_keytea
                                    && ts.ReferenciaTx == pr_refext
                                    && Convert.ToDecimal(rs.KeyTeatro) == ts.Teatro
                                    && rs.KeyPunto == config.Value.PuntoVenta
                                select new
                                {
                                    TransactionSales = ts,
                                    ReportSales = rs
                                };

                    var result = query.FirstOrDefault();

                    if (result != null)
                    {
                         ob_repsle = result.TransactionSales;
                         ob_bolvta = result.ReportSales;

                        // Obtener ubicaciones de vista
                        char[] ar_charst = ob_bolvta.SelUbicaciones.ToCharArray();
                        foreach (char c in ar_charst)
                        {
                            if (c == ';')
                            {
                                ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
                                lc_auxitm = string.Empty;
                            }
                            else if (c == '_')
                            {
                                ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);
                                lc_idearr++;
                                lc_auxitm = string.Empty;
                            }
                            else
                            {
                                lc_auxitm += c.ToString();
                            }
                        }

                        lc_cntubi = ls_lstubi.Count;
                        lc_ubilbl = string.Join(" ", ls_lstubi.Select(item => item[1] + item[2]));

                        string valor = ob_bolvta.NombreTar.Substring(ob_bolvta.NombreTar.IndexOf(";") + 1);
                        ViewBag.FB = "S";
                        ViewBag.Sala = ob_bolvta.KeySala;
                        ViewBag.SecuenciaBol = ob_bolvta.Secuencia;
                        ViewBag.Ubicaciones = lc_ubilbl;
                        ViewBag.CantidadUbi = lc_cntubi.ToString();
                        ViewBag.Fecha = ob_bolvta.NombreFec;
                        ViewBag.Hora = ob_bolvta.NombreHor;
                        ViewBag.Tarifa = ob_bolvta.NombreTar.Substring(0, ob_bolvta.NombreTar.IndexOf(";")) + " " + string.Format("{0:C0}", Convert.ToDecimal(valor)) + " COP";
                        ViewBag.Imagen = ob_bolvta.Referencia;
                        ViewBag.Valor = string.Format("{0:C0}", Convert.ToInt32(ob_bolvta.Precio));

                        // Consulta única para obtener los datos de confites
                         ob_ordite = (from retailSales in context.RetailSales
                                         where retailSales.Secuencia == ob_repsle.Secuencia
                                             && retailSales.KeyTeatro == ob_repsle.Teatro
                                             && retailSales.PuntoVenta == PuntoVenta
                                         select new OrderItem
                                         {
                                             Precio = retailSales.Precio * Convert.ToInt32(retailSales.Cantidad),
                                             Cantidad = Convert.ToInt32(retailSales.Cantidad),
                                             Descripcion = retailSales.Descripcion,
                                             KeyProducto = Convert.ToInt32(retailSales.KeyProducto)
                                         }).ToList();

                        if (ob_ordite.Count > 0)
                        {
                            ViewBag.FR = "S";
                            ViewBag.ListaP = ob_ordite;
                        }
                    }
                }*/


                //Devolver a vista
                return View();
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Home/LastSaleDtl";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }
        #endregion

        #region POST
        /// <summary>
        /// POST -- Validar selección de datos y consultar transacción
        /// </summary>
        /// <param name="pr_objint">Objeto con datos para buscar transacción</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("validarDatosyConsultarTransaccion")]
        public ActionResult LastSaleIdx(DateCompraRapida pr_objint, string submitButton)
        {
            #region VARIABLES LOCALES
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            XmlDocument ob_xmldoc = new XmlDocument();
            List<SelectListItem> teatros = new List<SelectListItem>();
            TransactionSales ob_repsle = new TransactionSales();
            ViewBag.Datos = null;
            General ob_fncgrl = new General();
            #endregion
            switch (submitButton)
            {
                case "Consultar compra":

                    try
                    {
                        //Validar seleccion de teatro
                        if (pr_objint.Teatro != null)
                        {
                            //Devolver a vista
                            return RedirectToAction("LastSaleDtl", "Home", new { pr_keytea = Convert.ToDecimal(pr_objint.Teatro), pr_refext = pr_objint.Referencia });
                        }
                        else
                        {
                            URLPortal(config);
                            ListCarrito();

                            Session.Remove("FlagCompra");
                            Session.SetString("FlagCompra", "R");

                            //Cargar ciudades home y teatro por defecto si aplica
                            if (Session.GetString("Teatro") != null)
                            {
                                Ciuteatros("SEL");
                            }
                            else
                            {
                                if (Session.GetString("CiudadTeatro") != null)
                                    Ciuteatros(Session.GetString("CiudadTeatro"));
                                else
                                    Ciuteatros();
                            }

                            //Obtener información de la web
                            ViewBag.Teatro = null;

                            //Obtener y recorrer teatros de xml para carga en vista
                            List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
                            foreach (var item in ls_ciuteatros)
                                if (item.Habilitado == "S")
                                    teatros.Add(new SelectListItem() { Text = item.nombre, Value = item.id });

                            //Devolver a vista
                            ViewBag.Teatro = teatros;

                            //Devolver a vista
                            ModelState.AddModelError("", "Por favor selecciona un teatro");
                            return View();
                        }
                    }
                    catch (Exception lc_syserr)
                    {
                        //Generar Log
                        LogSales logSales = new LogSales();
                        LogAudit logAudit = new LogAudit(config);
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Home/LastSaleIdx";
                        logSales.Metodo = "POST";
                        logSales.ExceptionMessage = lc_syserr.Message;
                        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                        //Escribir Log
                        logAudit.LogApp(logSales);

                        //Devolver vista de error
                        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                    }

                case "General de compras":

                    try
                    {

                        //Validar seleccion de teatro
                        if (pr_objint.FechaRef != null)
                        {
                            using (var context = new DataDB(config))
                            {
                                string fechaSinHora = pr_objint.FechaRef.Split('T')[0];
                                //Consultar registro de venta en BD transacciones
                                var ob_repsl1 = context.ReportSales
                                            .Where(x => x.FechaCreado.ToString().Contains(fechaSinHora))
                                               .Select(x => new
                                               {
                                                   x.Id,
                                                   x.Secuencia,
                                                   x.KeySala,
                                                   x.KeyTeatro,
                                                   x.KeyPelicula,
                                                   x.SelUbicaciones,
                                                   x.Precio,
                                                   x.HorProg,
                                                   x.FecProg,
                                                   x.EmailEli,
                                                   x.NombreEli,
                                                   x.ApellidoEli,
                                                   x.TelefonoEli,
                                                   x.NombrePel,
                                                   x.NombreFec,
                                                   x.NombreHor,
                                                   x.NombreTar,
                                                   x.KeyTarifa,
                                                   x.Transaccion,
                                                   FechaCreado = x.FechaCreado.ToString("yyyy-MM-dd"), // Formatear FechaCreado
                                                   FechaModificado = x.FechaModificado.ToString("yyyy-MM-dd"), // Formatear FechaModificado
                                                   x.KeyPunto,
                                                   x.Referencia
                                               })
                                            .ToList();

                                // Crear un nuevo archivo de Excel

                                var file = "ReporteVentas.xlsx";
                                var filePath = Path.GetFullPath(file);

                                using (var package = new ExcelPackage(filePath))
                                {
                                    var worksheet = package.Workbook.Worksheets.Add("Ventas");
                                    var headerStyle = worksheet.Cells["A1:U1"].Style;
                                    headerStyle.Font.Bold = true;
                                    headerStyle.Font.Size = 18; // Tamaño más grande
                                    headerStyle.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                    headerStyle.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(52, 89, 149)); // Fondo azul claro
                                    headerStyle.Font.Color.SetColor(System.Drawing.Color.White);


                                    // Obtener las propiedades de la clase anónima generada por la consulta LINQ
                                    var properties = ob_repsl1.FirstOrDefault()?.GetType().GetProperties();

                                    // Verificar si hay propiedades y agregar encabezados de columna
                                    if (properties != null)
                                    {
                                        for (int i = 0; i < properties.Length; i++)
                                        {
                                            worksheet.Cells[1, i + 1].Value = properties[i].Name;
                                            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                                            worksheet.Cells[1, i + 1].Style.Font.Size = 18;
                                            worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                                            worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                                            worksheet.Cells[1, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.White);
                                        }

                                        // Agregar datos a las celdas
                                        int row = 2;
                                        foreach (var venta in ob_repsl1)
                                        {
                                            for (int i = 0; i < properties.Length; i++)
                                            {
                                                worksheet.Cells[row, i + 1].Value = properties[i].GetValue(venta);
                                            }
                                            row++;
                                        }
                                    }
                                    package.Save();
                                }

                                var fileContent = System.IO.File.ReadAllBytes(filePath);
                                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                var fileName = "ReporteVentas.xlsx";

                                // Eliminar el archivo después de que se haya enviado al cliente para descargar
                                System.IO.File.Delete(filePath);

                                // Enviar el archivo al cliente para descargar
                                return File(fileContent, contentType, fileName);
                            }
                        }
                        else
                        {
                            URLPortal(config);
                            ListCarrito();

                            Session.Remove("FlagCompra");
                            Session.SetString("FlagCompra", "R");

                            //Cargar ciudades home y teatro por defecto si aplica
                            if (Session.GetString("Teatro") != null)
                            {
                                Ciuteatros("SEL");
                            }
                            else
                            {
                                if (Session.GetString("CiudadTeatro") != null)
                                    Ciuteatros(Session.GetString("CiudadTeatro"));
                                else
                                    Ciuteatros();
                            }

                            //Obtener información de la web
                            ViewBag.Teatro = null;

                            //Obtener y recorrer teatros de xml para carga en vista
                            List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
                            foreach (var item in ls_ciuteatros)
                                if (item.Habilitado == "S")
                                    teatros.Add(new SelectListItem() { Text = item.nombre, Value = item.id });

                            //Devolver a vista
                            ViewBag.Teatro = teatros;

                            //Devolver a vista
                            ModelState.AddModelError("", "Por favor selecciona un teatro");
                            return View();
                        }
                    }
                    catch (Exception lc_syserr)
                    {
                        //Generar Log
                        LogSales logSales = new LogSales();
                        LogAudit logAudit = new LogAudit(config);
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Home/LastSaleIdx";
                        logSales.Metodo = "POST";
                        logSales.ExceptionMessage = lc_syserr.Message;
                        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                        //Escribir Log
                        logAudit.LogApp(logSales);

                        //Devolver vista de error
                        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                    }

                default:
                    // Lógica predeterminada si no se proporciona ningún botón válido
                    return RedirectToAction("Index");
            }

        }
        #endregion

        #region MÉTODOS DE CLASE
        /// <summary>
        /// Método para cargar ciudades y teatros seleccionados forma externa
        /// </summary>
        /// <param name="pr_flag">Parámetro de ciudad</param>
        /// 
        [HttpGet]
        [Route("CargarCiudadesyTeatrosExt")]
        private void Selteatros(string pr_flag = "")
        {
            #region VARIABLES LOCALES
            General ob_fncgrl = new General();
            List<teatro> ls_ciudades = new List<teatro>();
            #endregion

            //Obtener listado de ciudades, teatros y recorrer ciudades
            List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
            foreach (var item in ls_ciuteatros)
            {
                //Validar nombre teatro
                if (item.nombre == pr_flag)
                {
                    Session.Remove("Teatro");
                    Session.Remove("TeatroNombre");
                    Session.Remove("CiudadTeatro");

                    Session.SetString("Teatro", item.id);
                    Session.SetString("TeatroNombre", item.nombre);
                    Session.SetString("CiudadTeatro", item.ciudad);

                    ViewBag.NombreCiudad = item.ciudad.ToString();
                    ViewBag.NombreCiudadTeatro = item.nombre.ToString();

                    break;
                }
            }
        }

        /// <summary>
        /// Método para cargar ciudades y teatros
        /// </summary>
        /// <param name="pr_flag">Parámetro de ciudad</param>
        /// 
   
        private void Ciuteatros(string pr_flag = "")
        {
            #region VARIABLES LOCALES
            General ob_fncgrl = new General();
            List<teatro> ls_ciudades = new List<teatro>();
            #endregion

            //Obtener listado de ciudades, teatros y recorrer ciudades
            List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
            var ls_auxciudad = ls_ciuteatros.Where(x => x.Habilitado == "S").Select(x => x.ciudad).Distinct().ToList();
            foreach (var item in ls_auxciudad)
            {
                //Asignar objeto ciudades y validar ciudad por defecto o selecionada
                teatro ob_auxitem = new teatro();

                //Validar flag
                if (pr_flag == "SEL")
                {
                    ob_auxitem.id = "0";
                    ob_auxitem.ciudad = item.ToString();

                    ViewBag.NombreCiudad = Session.GetString("CiudadTeatro");
                    ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
                }
                else
                {
                    if (item == pr_flag)
                    {
                        ob_auxitem.id = "1";
                        ob_auxitem.ciudad = item.ToString();

                        ViewBag.NombreCiudad = item.ToString();
                        if (Session.GetString("TeatroNombre") != null)
                            ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
                        else
                            ViewBag.NombreCiudadTeatro = "Seleccionar Teatro";

                        Session.Remove("Teatro");
                        Session.Remove("TeatroNombre");
                        Session.Remove("CiudadTeatro");
                        Session.SetString("CiudadTeatro", item.ToString());
                    }
                    else
                    {
                        //Normalizar valores y validar ciudad
                        string auxCiudad = Regex.Replace(item.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
                        if (pr_flag == "" && auxCiudad == config.Value.CiuDefault)
                        {
                            ob_auxitem.id = "1";
                            ob_auxitem.ciudad = item.ToString();

                            ViewBag.NombreCiudad = item.ToString();
                            ViewBag.NombreCiudadTeatro = config.Value.NomDefault;

                            Session.Remove("Teatro");
                            Session.Remove("TeatroNombre");
                            Session.Remove("CiudadTeatro");

                            Session.SetString("Teatro", config.Value.TeaDefault);
                            Session.SetString("TeatroNombre", config.Value.NomDefault);
                            Session.SetString("CiudadTeatro", item.ToString());
                        }
                        else
                        {
                            ob_auxitem.id = "0";
                            ob_auxitem.ciudad = item.ToString();
                        }
                    }
                }

                //Adicionar ciudad a lista
                ls_ciudades.Add(ob_auxitem);
            }

            ViewBag.Ciudades = ls_ciudades;
            ViewBag.TeatrosList = ls_ciuteatros;
        }

      
        /// <summary>
        /// Método para asignar teatro
        /// </summary>
        /// <param name="pr_ciudad">Parámetro de ciudad</param>
        /// <param name="pr_teatro">Parámetro de id teatro</param>
        /// <param name="pr_nomteatro">Parámetro de nombre teatro</param>
        private void Selteatros(string pr_ciudad, string pr_teatro, string pr_nomteatro, string pr_url)
        {
            //Validar si la selecion es en sala
            if (pr_url.Contains("room") || pr_url.Contains("detail"))
            {
                #region VARIABLES LOCALES
                int lc_idearr = 0;
                string lc_result = string.Empty;
                string lc_srvpar = string.Empty;
                string lc_auxitm = string.Empty;
                string[] ls_lstsel = new string[5];

                General ob_fncgrl = new General();
                List<string> ls_lstubi = new List<string>();
                Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
                #endregion

                //Validar secuenicia
                if (Session.GetString("Secuencia") != null)
                {
                    //Borrar retail
                    decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                    using (var context = new DataDB(config))
                    {
                        //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                        decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                        decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
                        var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec1).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                        foreach (var retail in RetailSales)
                        {
                            //Borrar retail detcombo
                            lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                            using (var context1 = new DataDB(config))
                            {
                                //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                                var RetailDet = context.RetailDet.Where(x => x.IdRetailSales == retail.Id).ToList();
                                foreach (var retaildet in RetailDet)
                                {
                                    //Eliminar item y guardar registro a tabla
                                    context.RetailDet.Remove(retaildet);
                                    context.SaveChanges();
                                }
                            }

                            //Eliminar item y guardar registro a tabla
                            context.RetailSales.Remove(retail);
                            context.SaveChanges();
                        }
                    }

                    //Borrar boletas
                    string lc_secsec2 = Session.GetString("Secuencia");
                    using (var context = new DataDB(config))
                    {
                        //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                        string PuntoVenta = config.Value.PuntoVenta;
                        string KeyTeatro = Session.GetString("Teatro");
                        var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec2).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                        foreach (var report in ReportSales)
                        {
                            //Obtener ubicaciones de vista
                            char[] ar_charst = report.SelUbicaciones.ToCharArray();
                            for (int lc_iditem = 0; lc_iditem < ar_charst.Length; lc_iditem++)
                            {
                                //Concatenar caracteres
                                lc_auxitm += ar_charst[lc_iditem].ToString();

                                //Obtener parámetro
                                if (ar_charst[lc_iditem].ToString() == ";")
                                {
                                    ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
                                    lc_auxitm = string.Empty;
                                }
                            }

                            //Cargar ubicaciones al modelo JSON
                            lc_auxitm = string.Empty;
                            foreach (var item in ls_lstubi)
                            {
                                lc_idearr = 0;
                                char[] ar_chars2 = item.ToCharArray();
                                for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
                                {
                                    //Concatenar caracteres
                                    lc_auxitm += ar_chars2[lc_iditem].ToString();

                                    //Obtener parámetro
                                    if (ar_chars2[lc_iditem].ToString() == "_")
                                    {
                                        ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);

                                        lc_idearr++;
                                        lc_auxitm = string.Empty;
                                    }
                                }

                                #region SCOSIL
                                LiberaSilla ob_libsrv = new LiberaSilla();
                                ob_libsrv.Fila = ls_lstsel[3];
                                ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
                                ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));
                                ob_libsrv.Funcion = Convert.ToInt32(report.HorProg);
                                ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
                                ob_libsrv.Usuario = 777;
                                ob_libsrv.tercero = config.Value.ValorTercero;
                                ob_libsrv.FechaFuncion = report.FecProg;

                                //Generar y encriptar JSON para servicio
                                lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

                                lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
                                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                                lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                                lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                                lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

                                //Encriptar Json LIB
                                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                                //Consumir servicio LIB
                                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
                                #endregion
                            }

                            //Eliminar item y guardar registro a tabla
                            context.ReportSales.Remove(report);
                            context.SaveChanges();
                        }
                    }

                    //Quitar secuencia
                    Session.Remove("Secuencia");
                }
            }
            //if (pr_url.Contains("room") || pr_url.Contains("detail"))
            //{

            //    #region VARIABLES LOCALES
            //    int lc_idearr = 0;
            //    string lc_result = string.Empty;
            //    string lc_srvpar = string.Empty;
            //    string lc_auxitm = string.Empty;
            //    string[] ls_lstsel = new string[5];

            //    General ob_fncgrl = new General();
            //    List<string> ls_lstubi = new List<string>();
            //    Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
            //    #endregion


            //    string secuencia = Session.GetString("Secuencia");

            //    if (!string.IsNullOrEmpty(secuencia))
            //    {
            //        decimal lc_secsec1 = Convert.ToDecimal(secuencia);
            //        decimal puntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
            //        decimal keyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));

            //        using (var context = new DataDB(config))
            //        {
            //            var retailSalesList = context.RetailSales
            //                .Where(x => x.Secuencia == lc_secsec1 && x.PuntoVenta == puntoVenta && x.KeyTeatro == keyTeatro)
            //                .ToList();

            //            foreach (var retail in retailSalesList)
            //            {
            //                // Borrar retail detcombo
            //                var retailDetList = context.RetailDet.Where(x => x.IdRetailSales == retail.Id).ToList();

            //                foreach (var retaildet in retailDetList)
            //                {
            //                    // Eliminar item y guardar registro a tabla
            //                    context.RetailDet.Remove(retaildet);
            //                }

            //                // Eliminar item y guardar registro a tabla
            //                context.RetailSales.Remove(retail);
            //            }

            //            context.SaveChanges();
            //        }

            //        using (var context = new DataDB(config))
            //        {
            //            string puntoVentaStr = config.Value.PuntoVenta;
            //            string keyTeatroStr = Session.GetString("Teatro");

            //            var reportSalesList = context.ReportSales
            //                .Where(x => x.Secuencia == secuencia && x.KeyPunto == puntoVentaStr && x.KeyTeatro == keyTeatroStr)
            //                .ToList();

            //            foreach (var report in reportSalesList)
            //            {
            //                // Obtener ubicaciones de vista
            //                char[] ar_charst = report.SelUbicaciones.ToCharArray();

            //                foreach (char c in ar_charst)
            //                {
            //                    // Concatenar caracteres
            //                    lc_auxitm += c;

            //                    // Obtener parámetro
            //                    if (c == ';')
            //                    {
            //                        ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
            //                        lc_auxitm = string.Empty;
            //                    }
            //                }

            //                // Cargar ubicaciones al modelo JSON
            //                lc_auxitm = string.Empty;

            //                foreach (var item in ls_lstubi)
            //                {
            //                    lc_idearr = 0;
            //                    char[] ar_chars2 = item.ToCharArray();

            //                    foreach (char c in ar_chars2)
            //                    {
            //                        // Concatenar caracteres
            //                        lc_auxitm += c;

            //                        // Obtener parámetro
            //                        if (c == '_')
            //                        {
            //                            ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);
            //                            lc_idearr++;
            //                            lc_auxitm = string.Empty;
            //                        }
            //                    }

            //                    #region SCOSIL
            //                    LiberaSilla ob_libsrv = new LiberaSilla();
            //                    ob_libsrv.Fila = ls_lstsel[3];
            //                    ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
            //                    ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));
            //                    ob_libsrv.Funcion = Convert.ToInt32(report.HorProg);
            //                    ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
            //                    ob_libsrv.Usuario = 777;
            //                    ob_libsrv.tercero = config.Value.ValorTercero;
            //                    ob_libsrv.FechaFuncion = report.FecProg;

            //                    // Generar y encriptar JSON para servicio
            //                    lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

            //                    lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
            //                    lc_srvpar = lc_srvpar.Replace("sala", "Sala");
            //                    lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
            //                    lc_srvpar = lc_srvpar.Replace("fila", "Fila");
            //                    lc_srvpar = lc_srvpar.Replace("columna", "Columna");
            //                    lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

            //                    // Encriptar Json LIB
            //                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

            //                    // Consumir servicio LIB
            //                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
            //                    #endregion
            //                }

            //                // Eliminar item y guardar registro a tabla
            //                context.ReportSales.Remove(report);
            //            }

            //            context.SaveChanges();
            //        }

            //        // Quitar secuencia
            //        Session.Remove("Secuencia");
            //    }
            //}

            //Cargar ciudad seleccionada
            Ciuteatros(pr_ciudad);

            //Cargar Teatro
            Session.SetString("Teatro", pr_teatro);
            Session.SetString("TeatroNombre", pr_nomteatro);
            ViewBag.NombreCiudadTeatro = pr_nomteatro;
        }

        [HttpGet]
        [Route("CargarURLPortal")]
        /// <summary>
        /// Método para cargar URL de Header y Footer del portal
        /// </summary>
        /// <returns></returns>
        private void URLPortal(IOptions<MyConfig> config)
        {
            #region VARIABLES LOCALES
            General ob_fncgrl = new General();
            #endregion

            //Inicializar valores
            ViewBag.URLength = HttpContext.Request.Path.ToString().Length;
            ViewBag.URLcartelerawp = config.Value.CarteleraWP;

            if (Session.GetString("Finhora") != null)
                ViewBag.FlagConf = Session.GetString("Finhora");
            else
                ViewBag.FlagConf = "S";

            ViewBag.URLfb = config.Value.URLfb;
            ViewBag.URLig = config.Value.URLig;
            ViewBag.URLtw = config.Value.URLtw;
            ViewBag.URLyb = config.Value.URLyb;
            ViewBag.URLtk = config.Value.URLtk;
            ViewBag.URLfaqs = config.Value.URLfaqs;
            ViewBag.URLblog = config.Value.URLblog;
            ViewBag.URLtarifas = config.Value.URLtarifas;
            ViewBag.URLprocinal = config.Value.URLprocinal;
            ViewBag.URLcontacto = config.Value.URLcontacto;
            ViewBag.URLtermycond = config.Value.URLtermycond;
            ViewBag.URLpoliticas = config.Value.URLpoliticas;
            ViewBag.URLservicios = config.Value.URLservicios;
            ViewBag.URLprotocolos = config.Value.URLprotocolos;
            ViewBag.URLexperiencias = config.Value.URLexperiencias;
            ViewBag.URLsobreprocinal = config.Value.URLsobreprocinal;
            ViewBag.URLpromociones = config.Value.URLpromociones;
            ViewBag.URLeticaytra = config.Value.URLeticaytra;
            ViewBag.URLlaft = config.Value.URLlaft;
            ViewBag.URLresoluccn = config.Value.URLresoluccn;
            ViewBag.URLcinefans = config.Value.URLcinefans;

            ViewBag.FlagLogin = Session.GetString("FlagLogin");

            //Validar inicio de sesión
            ViewBag.NombreUsuario = null;
            if (Session.GetString("Usuario") != null)
            {
                ViewBag.NombreUsuario = "Bienvenido " + Session.GetString("Nombre");

                ViewBag.USUNombre = Session.GetString("Nombre");
                ViewBag.USUApellido = Session.GetString("Apellido");
                ViewBag.USUTelefono = Session.GetString("Telefono");
                ViewBag.USUDireccion = Session.GetString("Direccion");
                ViewBag.USUDocumento = Session.GetString("Documento");
            }
        }

        /// <summary>
        /// Método para obtener lista de carrito de compras
        /// </summary>
        private void ListCarrito()
        {
            #region VARIABLES LOCALES
            decimal lc_secsec = 0;
            #endregion

            //Validar secuencia y asignar valores
            ViewBag.Venta = "V";
            ViewBag.Secuencia = Session.GetString("Secuencia");
            ViewBag.ListCarritoB = null;
            ViewBag.ListCarritoR = null;
            ViewBag.NombreTeatro = Session.GetString("TeatroNombre");
            ViewBag.FlagLogin = Session.GetString("FlagLogin");

            if (Session.GetString("Secuencia") != null)
            {
                //Obtener productos carrito de compra
                lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                using (var context = new DataDB(config))
                {
                    //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
                    decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                    decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
                    var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    ViewBag.ListCarritoR = RetailSales;
                }

                //Obtener boletas carrito de compra
                using (var context = new DataDB(config))
                {
                    //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia
                    string PuntoVenta = config.Value.PuntoVenta;
                    string KeyTeatro = Session.GetString("Teatro");
                    var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                    ViewBag.ListCarritoB = ReportSales;
                }

                if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count == 0)
                    ViewBag.TipoV = "B";
                if (ViewBag.ListCarritoB.Count == 0 && ViewBag.ListCarritoR.Count != 0)
                    ViewBag.TipoV = "P";
                if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count != 0)
                    ViewBag.TipoV = "M";
            }
            /*if (Session.GetString("Secuencia") != null)
            {
                lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                string PuntoVenta = config.Value.PuntoVenta;
                string KeyTeatro = Session.GetString("Teatro");

                using (var context = new DataDB(config))
                {
                    var retailSalesQuery = context.RetailSales
                        .Where(x => x.Secuencia == lc_secsec && x.PuntoVenta == Convert.ToDecimal(PuntoVenta) && x.KeyTeatro == Convert.ToDecimal(KeyTeatro));

                    var reportSalesQuery = context.ReportSales
                        .Where(x => x.Secuencia == lc_secsec.ToString() && x.KeyPunto == PuntoVenta && x.KeyTeatro == KeyTeatro);

                    ViewBag.ListCarritoR = retailSalesQuery.ToList();
                    ViewBag.ListCarritoB = reportSalesQuery.ToList();
                }

                if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count == 0)
                {
                    ViewBag.TipoV = "B";
                }
                else if (ViewBag.ListCarritoB.Count == 0 && ViewBag.ListCarritoR.Count != 0)
                {
                    ViewBag.TipoV = "P";
                }
                else if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count != 0)
                {
                    ViewBag.TipoV = "M";
                }
            }*/
        }
        [HttpGet]
        [Route("ObtenerListadoFechaCartelera")]
        /// <summary>
        /// Método para obtener listado de fechas de cartelera de películas
        /// </summary>
        /// <param name="pr_fecprg">fecha seleccionada</param>
        /// <returns></returns>

        private List<DateCartelera> DatePortal(string pr_fecprg, string pr_tippel, string pr_keypel = "")
        {
            DateTime dt_fechoy = DateTime.Now;
            var helper = new Helper();

            if (string.IsNullOrEmpty(pr_fecprg))
                pr_fecprg = dt_fechoy.ToString("yyyyMMdd");

            // Construir la URL completa con el valor de la sesión del teatro
            string url = config.Value.Variables41T + Session.GetString("Teatro");

            XDocument xdoc = XDocument.Load(url);

            var ob_fechas = (
                from pelicula in xdoc.Descendants("pelicula")
                let idAttr = pelicula.Attribute("id")?.Value
                let lc_auxipel_inner = (idAttr?.Length >= 8 && idAttr?.Length <= 10) ? idAttr.Substring(0, idAttr.Length - 5) : string.Empty
                where pelicula.Attribute("tipo")?.Value == pr_tippel && lc_auxipel_inner == pr_keypel
                from cinema in pelicula.Descendants("cinema")
                where cinema.Attribute("id")?.Value == Session.GetString("Teatro")
                from dia in pelicula.Descendants("DiasDisponiblesTodosCinemas").Descendants("dia")
                let auxFec = dia.Attribute("univ")?.Value
                where !string.IsNullOrEmpty(auxFec)
                let dtAuxFec = DateTime.ParseExact(auxFec, "yyyyMMdd", CultureInfo.InvariantCulture)
                group new { dtAuxFec, auxFec } by dtAuxFec.Date into grouped
                select new DateCartelera
                {
                    DiaLt = helper.DiaMes(grouped.Key.DayOfWeek.ToString(), "D"),
                    Flags = (pr_fecprg == grouped.First().auxFec) ? "S" : "N",
                    FecDt = grouped.Key,
                    FecSt = grouped.First().auxFec,
                    DiaNb = grouped.Key.ToString("dd"),
                    MesLt = helper.DiaMes(grouped.First().auxFec.Substring(4, 2), "M")
                }
            ).OrderBy(o => o.FecDt).ToList();

            ViewBag.Mes = helper.DiaMes(pr_fecprg.Substring(4, 2), "M");
            return ob_fechas;
        }
        #endregion
    }
}
