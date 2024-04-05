using APIPortalKiosco.Data;
using APIPortalKiosco.Entities;
using APIPortalKiosco.Helpers; 
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace APIPortalKiosco.Controllers
{
    public class FastSalesController : Controller
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
        public FastSalesController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        private List<SelectListItem> FechasPortal(string pr_fecprg, string pr_tippel, string pr_keypel = "")
        {
            DateTime dt_fechoy = DateTime.Now;
            var helper = new Helper();

            if (string.IsNullOrEmpty(pr_fecprg))
                pr_fecprg = dt_fechoy.ToString("yyyyMMdd");
             
            string url = config.Value.Variables41T + Session.GetString("Teatro");

            XDocument xdoc = XDocument.Load(url);

            var ob_fechas = (
                from pelicula in xdoc.Descendants("pelicula")
                let idAttr = pelicula.Attribute("id")?.Value
                let lc_auxipel_inner = (idAttr?.Length >= 8 && idAttr?.Length <= 10) ? idAttr.Substring(0, idAttr.Length - 5) : string.Empty
                where pelicula.Attribute("tipo")?.Value == pr_tippel && lc_auxipel_inner == (pr_keypel.Length >= 5 ? pr_keypel.Substring(0, pr_keypel.Length - 5) : pr_keypel)
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

            var fechasList = ob_fechas.Select(f => new SelectListItem
            {
                Text = $"{f.DiaLt}, {f.DiaNb} {f.MesLt}",
                Value = f.FecSt,
                Selected = f.Flags == "S"
            }).ToList();

            return fechasList;

        }

        #region GET
        /// <summary>
        /// GET: Index -- Carga de cartelera de péliculas desde XML
        /// </summary>
        /// <returns></returns>
        /// 
        //[HttpGet]
        //[Route("CargarCarteleraXML")]
        //public ActionResult Home(string Teatro = "0", string Ciudad = "0", string Pelicula = "0", string Fecha = "0", string tipo = "")
        //{
        //    #region VARIABLES LOCALES
        //    bool flag;
        //    string fr_auxfec = string.Empty;
        //    string lc_swtpel = string.Empty;

        //    XmlDocument ob_xmldoc = new XmlDocument();

        //    DataCompraRapida dateCompraRapida = new DataCompraRapida();
        //    General ob_fncgrl = new General();

        //    List<hora> ob_horflg = new List<hora>();
        //    #endregion

        //    try
        //    {
        //        Session.Remove("FlagCompra");
        //        Session.SetString("FlagCompra", "R");

        //        //Validar inicio de sesión
        //        if (Teatro != "0" && Session.GetString("Usuario") == null)
        //            return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "PR-" + Teatro + ";" + Ciudad });

        //        //Cargar ciudades home y teatro por defecto si aplica
        //        if (Session.GetString("Teatro") != null)
        //        {
        //            Ciuteatros("SEL");
        //        }
        //        else
        //        {
        //            if (Session.GetString("CiudadTeatro") != null)
        //                Ciuteatros(Session.GetString("CiudadTeatro"));
        //            else
        //                Ciuteatros();
        //        }

        //        //Validar ciudad y teatro desde web externa
        //        if (Teatro != "0")
        //            Selteatros(Teatro);

        //        //Validar seleccion
        //        if (Pelicula == "Elegir Película")
        //        {
        //            dateCompraRapida.Pelicula = "0";
        //            dateCompraRapida.Fecha = "0";
        //            return View(dateCompraRapida);
        //        }

        //        //Validar seleccion
        //        if (Fecha == "Elegir Fecha")
        //        {
        //            dateCompraRapida.Pelicula = "0";
        //            dateCompraRapida.Fecha = "0";
        //            return View(dateCompraRapida);
        //        }

        //        //Obtener información de la web
        //        ViewBag.Pelicula = null;
        //        ViewBag.Fecha = null;

        //        dateCompraRapida.Fecha = "0";
        //        dateCompraRapida.Pelicula = "0";

        //        //Recorrer xml y obtener datos
        //        var fechas = new List<SelectListItem>();
        //        var peliculas = new List<SelectListItem>();


        //        peliculas = DataPeliculas(Pelicula, ob_xmldoc, ob_horflg);
        //        if (Pelicula != "0")
        //        {
        //            dateCompraRapida.Pelicula = Pelicula;
        //            Pelicula = dateCompraRapida.Pelicula;
        //        }



        //        if (Pelicula != "0")
        //        {
        //            var datePortal = FechasPortal("", "Normal", Pelicula);
        //            if (datePortal.Count <= 0)
        //            {
        //                datePortal = FechasPortal("", "Estreno", Pelicula);
        //            }

        //            if (datePortal.Count <= 0)
        //            {
        //                datePortal = FechasPortal("", "Preventa", Pelicula);
        //            }

        //            fechas = datePortal;
        //        }

        //        if (Fecha != "0")
        //        {
        //            dateCompraRapida.Fecha = Fecha;
        //            return RedirectToAction("DetailsBol", "FastSales", new { pr_keypel = dateCompraRapida.Pelicula, pr_fecprg = dateCompraRapida.Fecha });
        //        }

        //        //Asignar flag maxima hora de funcion a session
        //        Session.Remove("Finhora");
        //        if (config.Value.MinDifConf != "0")
        //        {
        //            DateTime FechaHoraTermino = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
        //            hora ob_hora = ob_horflg.ToList().LastOrDefault();

        //            //Diferencia de tiempo entre hora funcion y hora del dia 
        //            TimeSpan diferencia = ob_hora.fechayhora - FechaHoraTermino;
        //            var diferenciaenminutos = diferencia.TotalMinutes;

        //            if (diferenciaenminutos > Convert.ToDouble(config.Value.MinDifConf))
        //                Session.SetString("Finhora", "S");
        //            else
        //                Session.SetString("Finhora", "N");
        //        }
        //        else
        //        {
        //            Session.SetString("Finhora", "S");
        //        }

        //        URLPortal(config);
        //        ListCarrito();

        //        //Devolver a vista
        //        ViewBag.Pelicula = peliculas;
        //        ViewBag.Fecha = fechas;
        //        return View(dateCompraRapida);
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        //Generar Log
        //        LogSales logSales = new LogSales();
        //        LogAudit logAudit = new LogAudit(config);
        //        logSales.Id = Guid.NewGuid().ToString();
        //        logSales.Fecha = DateTime.Now;
        //        logSales.Programa = "FastSales/Home";
        //        logSales.Metodo = "GETXML";
        //        logSales.ExceptionMessage = lc_syserr.Message;
        //        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

        //        //Escribir Log
        //        logAudit.LogApp(logSales);

        //        //Devolver vista de error
        //        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
        //    }
        //}

        /// <summary>
        /// GET: Detail -- Cargar vista con el detalle de la película ciudad-teatro-fecha-hora-tarifa
        /// </summary>
        /// <param name="pr_keypel">Párametro ID de película para obtener información</param>
        /// <param name="pr_fecprg">Párametro fecha de película para obtener información</param>
        /// <returns></returns>
        [HttpGet]
        [Route("CargarDetallePelicula")]
        public ActionResult DetailsBol(string pr_keypel, string pr_fecprg)
        {
            #region VARIABLES LOCALES
            int lc_keypel = 0;
            int lc_auxpel = 0;
            int lc_keytea = 0;
            int lc_auxtea = 0;
            int lc_swtflg = 0;
            string lc_auxitem = string.Empty;
            string lc_fecitem = string.Empty;

            DateTime dt_fecpro;

            List<DateCartelera> ob_fechas = new List<DateCartelera>();

            XmlDocument ob_xmldoc = new XmlDocument();
            Billboard ob_bilmov = new Billboard();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);

                //Validar inicio de sesión
                if (Session.GetString("Usuario") == null)
                    return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "PR" });

                //Validar seleccion de teatro
                if (Session.GetString("Teatro") == null)
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
                }
                else
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
                }

                ListCarrito();

                ViewBag.Fechaprog = pr_fecprg;
                ViewBag.PelCodigo = pr_keypel;

                //Obtener información de la web
                //ob_fechas = null;
                ob_xmldoc.Load(config.Value.Variables41);
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                //Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {
                    //Validar película
                    lc_keypel = Convert.ToInt32(item.GetAttribute("id"));
                    lc_auxpel = Convert.ToInt32(pr_keypel);
                    if (lc_keypel == lc_auxpel)
                    {
                        //Datos de nodo pelicula
                        ob_bilmov.Id = Convert.ToInt32(item.GetAttribute("id"));
                        ob_bilmov.Tipo = item.GetAttribute("tipo").ToString();
                        ob_bilmov.Nombre = item.GetAttribute("nombre").ToString();

                        ob_bilmov.Auxids = pr_keypel;
                        ob_bilmov.Switch = "V";
                        ob_bilmov.TipoSala = TipPelicula(item.GetAttribute("nombre").ToString());

                        //Datos de nodo pelicula/sinopsis
                        ob_bilmov.Sinopsis = item.SelectSingleNode("sinopsis").InnerText;

                        //Datos de nodo pelicula/data
                        XmlNodeList data = item.GetElementsByTagName("data");
                        foreach (XmlElement item2 in data)
                        {
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
                                    lc_keytea = Convert.ToInt32(item4.GetAttribute("id"));
                                    lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));
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
                                                List<Fechas> ob_fecha = new List<Fechas>();
                                                XmlNodeList Fecha = itemSS.GetElementsByTagName("Fecha");
                                                foreach (XmlElement item5 in Fecha)
                                                {
                                                    lc_auxitem = item5.GetAttribute("univ").ToString();

                                                    // Datos de nodo pelicula / Fecha / hora
                                                    List<Hora> ob_hora = new List<Hora>();
                                                    XmlNodeList hora = item5.GetElementsByTagName("hora");
                                                    foreach (XmlElement item6 in hora)
                                                    {
                                                        if (item6.GetAttribute("webserviceVentas") == "Si")
                                                        {
                                                            ob_hora.Add(new Hora() { fecunv = lc_auxitem, idFuncion = item6.GetAttribute("idFuncion").ToString(), horario = item6.GetAttribute("horario").ToString() });
                                                        }
                                                    }

                                                    ob_fecha.Add(new Fechas { fecham = item5.GetAttribute("dia").ToString(), fecunv = item5.GetAttribute("univ").ToString(), horafun = ob_hora });
                                                }

                                                //Solo primera fecha
                                                ob_bilmov.Fechafunc = ob_fecha;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Cortar el ciclo
                        break;
                    }
                }

                //Obtner Horas y tarifas
                ViewBag.Zonas = null;
                ViewBag.Hora = GetHora(pr_keypel, pr_fecprg);
                lc_fecitem = ViewBag.Fecha3;

                //Devolver a vista
                return View(ob_bilmov);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/DetailsBol";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }

        /// <summary>
        /// GET: RoomProg -- Proceso de ejecución SCOGRU para dejar ubicaciones en preventa
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ProcesoSCOGRU")]
        public ActionResult RoomBol(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_imgpel)
        {
            #region VARIABLES LOCALES
            int lc_maxcol = 0;
            int lc_maxfil = 0;
            int lc_idxrow = 0;
            string lc_auxval = string.Empty;
            string lc_auxtar = string.Empty;
            string lc_auxhor = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;

            DataTable ob_datubi = new DataTable();

            XmlDocument ob_xmldoc = new XmlDocument();

            Dictionary<string, object> ob_estsil = new Dictionary<string, object>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
            Dictionary<string, object>[] ob_diclst2;
            List<BolVenta> ob_lisprg = new List<BolVenta>();

            MapaSala ob_datsal = new MapaSala();
            BolVenta ob_datprg = new BolVenta();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

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

                //Validar inicio de sesión
                if (Session.GetString("Usuario") == null)
                    return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "P" });

                //Validar seleccion de teatro
                if (Session.GetString("Teatro") == null)
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
                }
                else
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
                }

                //Asignar valores url
                ob_datprg.HorProg = pr_horprg;
                ob_datprg.FechaPrg = pr_fecprg;
                ob_datprg.SwtVenta = "V";
                ob_datprg.KeyTarifa = pr_tarprg;
                ob_datprg.NombrePel = pr_nompel;
                ob_datprg.NombreFec = pr_nomfec;
                ob_datprg.NombreHor = pr_nomhor;
                ob_datprg.NombreTar = pr_nomtar;
                ob_datprg.KeyPelicula = pr_keypel;

                ob_datprg.KeySala = pr_salprg;
                ob_datprg.TipoSilla = ob_datprg.KeySala.Substring(ob_datprg.KeySala.IndexOf(";") + 1);
                ob_datprg.KeySala = ob_datprg.KeySala.Substring(0, ob_datprg.KeySala.IndexOf(";"));

                lc_auxtar = pr_nomtar.Substring(0, pr_nomtar.IndexOf(";"));
                lc_auxval = pr_nomtar.Substring(pr_nomtar.IndexOf(";") + 1);
                lc_auxval = lc_auxval.Substring(0, lc_auxval.Length - 3);

                //Asignar valores
                ViewBag.Sala = ob_datprg.TipoSilla;
                ViewBag.Hora = pr_nomhor;
                ViewBag.Fecha = pr_nomfec;
                ViewBag.Imagen = pr_imgpel;
                ViewBag.Teatro = Session.GetString("TeatroNombre");
                ViewBag.Tarifa = lc_auxtar;
                ViewBag.NumValor = lc_auxval;
                ViewBag.Tarvalor = String.Format("{0:C0}", Convert.ToInt32(lc_auxval));
                ViewBag.Censura = pr_cenprg;
                ViewBag.TipoSilla = ob_datprg.TipoSilla;
                ViewBag.NombreUsuario = Session.GetString("Nombre") + " " + Session.GetString("Apellido");
                ViewBag.CantSillasBol = config.Value.CantSillasBol;

                #region SERVICIO SCOMAP
                //Asignar valores MAP
                ob_datsal.Sala = Convert.ToInt32(ob_datprg.KeySala);
                ob_datsal.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                ob_datsal.Tercero = config.Value.ValorTercero;
                ob_datsal.Correo = "";
                ob_datsal.FechaFuncion = "";

                //Generar y encriptar JSON para servicio MAP
                lc_srvpar = ob_fncgrl.JsonConverter(ob_datsal);
                lc_srvpar = lc_srvpar.Replace("sala", "Sala");

                //Encriptar Json MAP
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio MAP
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scomap/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/RoomBol";
                logSales.Metodo = "SCOMAP";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");

                    //Deserializar Json y validar respuesta MAP
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);

                    //Devolver a vista
                    return View(ob_datprg);
                }
                #endregion

                #region SERVICIO SCOEST
                //Asignar valores EST
                ob_datsal.Sala = Convert.ToInt32(ob_datprg.KeySala);
                ob_datsal.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                ob_datsal.Tercero = config.Value.ValorTercero;

                ob_datsal.Correo = Session.GetString("Usuario");
                ob_datsal.FechaFuncion = ob_datprg.FecProg;

                lc_auxhor = ob_datprg.HorProg;
                ob_datsal.Funcion = Convert.ToInt32(lc_auxhor.Substring(0, 2));

                //Generar y encriptar JSON para servicio EST
                lc_srvpar = ob_fncgrl.JsonConverter(ob_datsal);
                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                lc_srvpar = lc_srvpar.Replace("correo", "Correo");
                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");

                //Encriptar Json EST
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio EST
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoest/"), lc_srvpar);

                //Generar Log
                var logSales = new LogSales();
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/RoomBol";
                logSales.Metodo = "SCOEST";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");

                    //Deserializar Json y validar respuesta EST
                    ob_diclst2 = (Dictionary<string, object>[])JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>[])));
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);

                    //Devolver a vista
                    return View(ob_datprg);
                }
                #endregion

                #region MATRIZ SALA
                //Obtener maximo de filas, columnas y estado de sillas para matriz de sala
                foreach (var item in ob_diclst2)
                {
                    lc_maxcol = Convert.ToInt32(item["maxCol"]);
                    lc_maxfil = ob_diclst2.Length;
                    ob_estsil.Add(item["filRel"].ToString(), item["DescripcionSilla"]);
                }

                //Obtener arreglos de filas y columnas de la matriz SCOMAP
                double[] ColumnaTotal = (double[])JsonConvert.DeserializeObject(ob_diclst["ColumnaTotal"].ToString(), (typeof(double[])));
                double[] ColumnaRelativa = (double[])JsonConvert.DeserializeObject(ob_diclst["ColumnaRelativa"].ToString(), (typeof(double[])));
                string[] FilaTotal = (string[])JsonConvert.DeserializeObject(ob_diclst["FilaTotal"].ToString(), (typeof(string[])));
                string[] FilaRelativa = (string[])JsonConvert.DeserializeObject(ob_diclst["FilaRelativa"].ToString(), (typeof(string[])));
                string[] TipoSilla = (string[])JsonConvert.DeserializeObject(ob_diclst["TipoSilla"].ToString(), (typeof(string[])));
                string[] TipoZona = (string[])JsonConvert.DeserializeObject(ob_diclst["TipoZona"].ToString(), (typeof(string[])));

                //Recorrer y cargar matriz de sala (filas)
                Ubicaciones[,] mt_datsal = new Ubicaciones[lc_maxfil, lc_maxcol];
                for (int lc_idxiii = 0; lc_idxiii < lc_maxfil; lc_idxiii++)
                {
                    //Recorrer y cargar matriz de sala (columnas)
                    for (int lc_idxjjj = 0; lc_idxjjj < lc_maxcol; lc_idxjjj++)
                    {
                        //Inicializar objeto de ubicaciones 
                        Ubicaciones ob_ubisal = new Ubicaciones();

                        //Cargar valores numericos de los arreglos al objeto
                        ob_ubisal.Columna = Convert.ToInt32(ColumnaTotal[lc_idxrow]);
                        ob_ubisal.ColRelativa = Convert.ToInt32(ColumnaRelativa[lc_idxrow]);

                        //Cargar valores string de los arreglos al objeto
                        ob_ubisal.Fila = FilaTotal[lc_idxrow];
                        ob_ubisal.FilRelativa = FilaRelativa[lc_idxrow];
                        ob_ubisal.TipoSilla = TipoSilla[lc_idxrow];
                        ob_ubisal.TipoZona = TipoZona[lc_idxrow];

                        //Recorrer y buscar fila en ciclo de matriz
                        List<EstadoDeSilla> ls_estsil = new List<EstadoDeSilla>((List<EstadoDeSilla>)JsonConvert.DeserializeObject(ob_estsil[FilaRelativa[lc_idxrow]].ToString(), (typeof(List<EstadoDeSilla>))));
                        foreach (var item in ls_estsil)
                        {
                            //Validar columna en ciclo de matriz
                            if (Convert.ToInt32(item.Columna) == ColumnaRelativa[lc_idxrow])
                            {
                                //Asignar valor y romper ciclo
                                ob_ubisal.EstadoSilla = item.EstadoSilla;
                                break;
                            }
                        }

                        //Cargar objeto ubicaciones a la matriz
                        mt_datsal[lc_idxiii, lc_idxjjj] = ob_ubisal;
                        lc_idxrow++;
                    }
                }

                //Asignar Sala a Objeto
                ob_datprg.FilSala = lc_maxfil;
                ob_datprg.ColSala = lc_maxcol;
                ob_datprg.MapaSala = mt_datsal;
                #endregion

                //Devolver a vista
                return View(ob_datprg);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/RoomBol";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }

        /// <summary>
        /// GET: RoomReverse -- Proceso de liberar sillas
        /// </summary>
        /// <param name="pr_keypel"></param>
        /// <param name="pr_fecprg"></param>
        /// <param name="pr_horprg"></param>
        /// <param name="pr_tarprg"></param>
        /// <param name="pr_salprg"></param>
        /// <param name="pr_nompel"></param>
        /// <param name="pr_nomfec"></param>
        /// <param name="pr_nomhor"></param>
        /// <param name="pr_nomtar"></param>
        /// <param name="pr_cenprg"></param>
        /// <param name="pr_secsec"></param>
        /// <param name="pr_selubi"></param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ProcesoLiberarSillas")]
        public ActionResult RoomReverse(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_secsec, string pr_selubi)
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
                //Obtener ubicaciones de vista
                char[] ar_charst = pr_selubi.ToCharArray();
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
                    ob_libsrv.Sala = Convert.ToInt32(pr_salprg.Substring(0, pr_salprg.IndexOf(";")));
                    ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));
                    ob_libsrv.Funcion = Convert.ToInt32(pr_horprg.Length == 4 ? pr_horprg.Substring(0, 2) : pr_horprg.Substring(0, 1));
                    ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
                    ob_libsrv.Usuario = 777;
                    ob_libsrv.tercero = config.Value.ValorTercero;
                    ob_libsrv.FechaFuncion = pr_fecprg;

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
                    logSales.Programa = "FastSales/RoomReverse";
                    logSales.Metodo = "SCOSIL";
                    logSales.ExceptionMessage = lc_srvpar;
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    logAudit.LogApp(logSales);

                    //Validar secuencia
                    if (lc_result.Substring(0, 1) == "0")
                    {
                        //Quitar switch
                        lc_result = lc_result.Replace("0-", "");
                        lc_result = lc_result.Replace("[", "");
                        lc_result = lc_result.Replace("]", "");

                        //Deserializar Json y validar respuesta
                        ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                        //Validar respuesta llave 1
                        if (ob_diclst.ContainsKey("Validación"))
                        {
                            return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString(), pr_flag = "PR" });
                        }
                        else
                        {
                            //Validar respuesta llave 2
                            if (ob_diclst.ContainsKey("Respuesta"))
                            {
                                if (ob_diclst["Respuesta"].ToString() == "Proceso exitoso")
                                    continue;
                                else
                                {
                                    return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Respuesta"].ToString(), pr_flag = "ER" });
                                }
                            }
                        }
                    }
                    else
                    {
                        return RedirectToAction("Error", "Pages", new { pr_message = "Error al liberar silla SCOLIB", pr_flag = "ER" });
                    }
                    #endregion
                }

                //Validar acción
                return RedirectToAction("RoomBol", "FastSales", new { pr_keypel = pr_keypel, pr_fecprg = pr_fecprg, pr_horprg = pr_horprg, pr_tarprg = pr_tarprg, pr_salprg = pr_salprg, pr_nompel = pr_nompel, pr_nomfec = pr_nomfec, pr_nomhor = pr_nomhor, pr_nomtar = pr_nomtar, pr_cenprg = pr_cenprg });
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/RoomReverse";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }

        /// <summary>
        /// GET: ProductList -- Listado de productos para ventas por el portal web
        /// </summary>
        /// <param name="pr_secpro">Secuencia tran</param>
        /// <param name="pr_swtven">Switch Venta</param>
        /// <param name="pr_tiplog">Tipo Compra</param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ListarProductos")]
        public ActionResult ListCon(string pr_secpro, string pr_swtven, string pr_tiplog, string pr_tbview = "", string pr_cenprg = "")
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitem = string.Empty;

            List<Producto> ob_return = new List<Producto>();
            List<Producto> ob_result = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            Secuencia ob_scopre = new Secuencia();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //Inicializar valores de entrada
                ViewBag.pr_secpro = pr_secpro;
                ViewBag.pr_swtven = pr_swtven;
                ViewBag.pr_tiplog = pr_tiplog;
                ViewBag.pr_tbview = pr_tbview;

                //Session carrito de compras
                Session.Remove("pr_tbviewFS");
                Session.SetString("pr_tbviewFS", pr_tbview);
                Session.Remove("pr_secproFS");
                Session.SetString("pr_secproFS", pr_secpro);
                Session.Remove("pr_swtvenFS");
                Session.SetString("pr_swtvenFS", pr_swtven);
                Session.Remove("pr_tiplogFS");
                Session.SetString("pr_tiplogFS", pr_tiplog);
                Session.Remove("pr_cenprgFS");
                Session.SetString("pr_cenprgFS", pr_cenprg);

                URLPortal(config);
                ListCarrito();

                //Validar seleccion de teatro
                if (Session.GetString("Teatro") == null)
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
                }
                else
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
                }

                //Inicializar variables
                ViewBag.ListaM = null;
                ViewBag.alertS = false;
                ViewBag.CantidadProductos = config.Value.CantProductos;
                ViewBag.UrlRetailImg = config.Value.UrlRetailImg;
                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente");

                ViewBag.Tipo = pr_tiplog;
                ViewBag.Teatro = Session.GetString("TeatroNombre");
                ViewBag.Correo = Session.GetString("Usuario");
                ViewBag.Nombre = Session.GetString("Nombre");
                ViewBag.Apellido = Session.GetString("Apellido");
                ViewBag.Telefono = Session.GetString("Telefoho");
                ViewBag.KeyTeatro = Session.GetString("Teatro");
                ViewBag.Secuencia = pr_secpro;
                ViewBag.SwitchVenta = pr_swtven;
                ViewBag.Censura = pr_cenprg;

                #region SERVICIO SCOPRE
                //Asignar valores PRE
                ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                ob_scopre.Teatro = Session.GetString("Teatro") != "0" ? Convert.ToInt32(Session.GetString("Teatro")) : Convert.ToInt32(Session.GetString("Teatro"));
                ob_scopre.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio PRE
                lc_srvpar = ob_fncgrl.JsonConverter(ob_scopre);
                lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                //Encriptar Json PRE
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio PRE
                if (ViewBag.ClientFrecnt == "No")
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);
                else
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopcf/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/ListCon";
                logSales.Metodo = "SCOPRE";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                    ob_return = (List<Producto>)JsonConvert.DeserializeObject(ob_diclst["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString(), pr_flag = "PR" });
                    }
                    else
                    {
                        //Recorrido por objeto para obtener descripcion de receta combos
                        foreach (Producto item in ob_return)
                        {
                            if (item.Tipo == "C")
                            {
                                string fr_axurec = string.Empty;
                                foreach (var receta in item.Receta)
                                    fr_axurec += string.Concat(receta.Cantidad.ToString().Substring(0, receta.Cantidad.ToString().IndexOf(",")), " ", receta.Descripcion, ", ");

                                item.RecetaResumen = fr_axurec.Substring(0, fr_axurec.Length - 2);
                                ob_result.Add(item);
                            }
                            else
                            {
                                ob_result.Add(item);
                            }
                        }

                        //Recorrido por objeto para obtener el orden de pantallas y mostrar en vista
                        List<Producto> CombosWeb = new List<Producto>();
                        List<Producto> AlimentosWeb = new List<Producto>();
                        List<Producto> BebidasWeb = new List<Producto>();
                        List<Producto> SnacksWeb = new List<Producto>();
                        foreach (Producto item in ob_return)
                        {
                            //Recorrido por pantallas
                            foreach (var pantallas in item.Pantallas)
                            {
                                switch (pantallas.Descripcion)
                                {
                                    case "COMBOS WEB":
                                        int lc_cntcom = CombosWeb.Count();

                                        CombosWeb.Add(item);
                                        CombosWeb[lc_cntcom].OrdenView = pantallas.Orden;
                                        CombosWeb[lc_cntcom].Descripcion_Web = pantallas.Descripcion_Web;
                                        CombosWeb[lc_cntcom].Flag = pantallas.Flag;
                                        break;

                                    case "ALIMENTOS WEB":
                                        int lc_cntali = AlimentosWeb.Count();

                                        AlimentosWeb.Add(item);
                                        AlimentosWeb[lc_cntali].OrdenView = pantallas.Orden;
                                        AlimentosWeb[lc_cntali].Descripcion_Web = pantallas.Descripcion_Web;
                                        AlimentosWeb[lc_cntali].Flag = pantallas.Flag;
                                        break;

                                    case "BEBIDAS WEB":
                                        int lc_cntbeb = BebidasWeb.Count();

                                        BebidasWeb.Add(item);
                                        BebidasWeb[lc_cntbeb].OrdenView = pantallas.Orden;
                                        BebidasWeb[lc_cntbeb].Descripcion_Web = pantallas.Descripcion_Web;
                                        BebidasWeb[lc_cntbeb].Flag = pantallas.Flag;
                                        break;

                                    case "SNACKS WEB":
                                        int lc_cntsnk = SnacksWeb.Count();

                                        SnacksWeb.Add(item);
                                        SnacksWeb[lc_cntsnk].OrdenView = pantallas.Orden;
                                        SnacksWeb[lc_cntsnk].Descripcion_Web = pantallas.Descripcion_Web;
                                        SnacksWeb[lc_cntsnk].Flag = pantallas.Flag;
                                        break;
                                }
                            }
                        }

                        //Validar productos a mostrar combos
                        if (pr_tbview == "" || pr_tbview == "tab-combos")
                            ViewBag.ListaM = CombosWeb.OrderBy(o => o.OrdenView).ToList();

                        //Validar productos a mostrar alimentos
                        if (pr_tbview == "tab-alimentos")
                            ViewBag.ListaM = AlimentosWeb.OrderBy(o => o.OrdenView).ToList();

                        //Validar productos a mostrar bebidas
                        if (pr_tbview == "tab-bebidas")
                            ViewBag.ListaM = BebidasWeb.OrderBy(o => o.OrdenView).ToList();

                        //Validar productos a mostrar snacks
                        if (pr_tbview == "tab-snacks")
                            ViewBag.ListaM = SnacksWeb.OrderBy(o => o.OrdenView).ToList();
                    }
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);
                }
                #endregion

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
                logSales.Programa = "FastSales/ListCon";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }

        /// <summary>
        /// GET: Details -- Cargar vista de detalle producto seleccionado
        /// </summary>
        /// <param name="pr_keypro">Id Producto</param>
        /// <param name="pr_secpro">Secuencia tran</param>
        /// <param name="pr_swtven">Switch Venta</param>
        /// <param name="pr_tiplog">Tipo Compra</param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("CargarListaProductos")]
        public ActionResult DetailsCon(string pr_swtadd, string pr_keypro, string pr_secpro, string pr_swtven, string pr_tiplog, string pr_cenprg = "")
        {
            #region VARIABLES LOCALES
            var lc_result = string.Empty;
            var lc_srvpar = string.Empty;
            var lc_auxitem = string.Empty;
            int CodigoBebidas = 1244;
            int CodigoBebidas2 = 2444;
            int CodigoComidas = 246;
            List<(decimal CodigoBotella, string NombreFinalBotella, decimal PrecioFinalBotella, string frecuenciaBotella, decimal categoria)> datosFinalesBotella = new List<(decimal, string, decimal, string, decimal)>();
            List<(decimal CodigoComida, string NombreFinalComida, decimal PrecioFinalComida, string frecuenciaComida, decimal categoria)> datosFinalesComida = new List<(decimal, string, decimal, string, decimal)>();

            List<Producto> ob_return = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            Secuencia ob_scopre = new Secuencia();
            Producto ob_datpro = new Producto();
            General ob_fncgrl = new General();
            #endregion

            //Inicializar variables
            ViewBag.ListaM = null;
            ViewBag.alertS = false;
            ViewBag.UrlRetailImg = config.Value.UrlRetailImg;
            ViewBag.CantidadProductos = config.Value.CantProductos;
            ViewBag.Secuencia = pr_secpro;
            ViewBag.SwitchVenta = pr_swtven;
            ViewBag.Tipo = pr_tiplog;
            ViewBag.Censura = pr_cenprg;
            ViewBag.SwitchAdd = pr_swtadd;
            ViewBag.ListaB = null;
            ViewBag.ListaC = null;

            try
            {
                URLPortal(config);
                ListCarrito();

                //Validar seleccion de teatro
                if (Session.GetString("Teatro") == null)
                {
                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
                }
                else
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
                }

                //Validar productos retail
                if (ViewBag.ListCarritoR != null)
                    pr_tiplog = "M";

                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente"); //"No";

                ob_datpro.Codigo = Convert.ToDecimal(pr_keypro);
                ob_datpro.SwtVenta = pr_swtven;
                ob_datpro.EmailEli = Session.GetString("Usuario");
                ob_datpro.NombreEli = Session.GetString("Nombre");
                ob_datpro.KeyTeatro = Session.GetString("Teatro");
                ob_datpro.DesTeatro = Session.GetString("TeatroNombre");
                ob_datpro.ApellidoEli = Session.GetString("Apellido");
                ob_datpro.TelefonoEli = Session.GetString("Telefono");
                ob_datpro.KeySecuencia = pr_secpro;

                #region SERVICIO SCOPRE
                //Asignar valores PRE
                ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                ob_scopre.Teatro = Convert.ToInt32(ob_datpro.KeyTeatro);
                ob_scopre.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio PRE
                lc_srvpar = ob_fncgrl.JsonConverter(ob_scopre);
                lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                //Encriptar Json PRE
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio PRE
                if (ViewBag.ClientFrecnt == "No")
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);
                else
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopcf/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/DetailsCon";
                logSales.Metodo = "SCOPRE";
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
                    ob_return = (List<Producto>)JsonConvert.DeserializeObject(ob_diclst["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                    if (ob_diclst.ContainsKey("Validación"))
                        return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString(), pr_flag = "PR" });
                    else
                        ViewBag.ListaM = ob_return;
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    return RedirectToAction("Error", "Pages", new { pr_message = lc_result, pr_flag = "ER" });
                }
                #endregion

                //Recorrido por productos para obtener el seleccionado y sus valores
                foreach (var itepro in ob_return)
                {
                    if (itepro.Codigo == ob_datpro.Codigo)
                    {
                        switch (itepro.Tipo)
                        {
                            case "P": //PRODUCTOS
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Precios = itepro.Precios;
                                break;

                            case "C": //COMBOS
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Receta = itepro.Receta;
                                List<Precios> precio_Combo = new List<Precios>();
                                ob_datpro.Precios = precio_Combo;

                                bool condicionCumplida = false;

                                foreach (var itecat in itepro.Receta)
                                {
                                    var precio_Combo_Receta = itecat.Codigo;
                                    if (precio_Combo_Receta == CodigoBebidas || precio_Combo_Receta == CodigoBebidas2)
                                    {
                                        foreach (var i in itecat.RecetaCategoria)
                                        {
                                            var CodioBotella = i.Codigo;
                                            var NombreFinalBotella = i.Descripcion.ToString();
                                            var precioFinalBotella = i.Precios.Sum(precio => precio.General);
                                            var frecuenciaBotella = i.Frecuente.ToString();
                                            // Hacer algo con precioFinalCombo
                                            datosFinalesBotella.Add((CodioBotella, NombreFinalBotella, precioFinalBotella, frecuenciaBotella, itecat.Codigo));
                                        }
                                    }
                                    else if (precio_Combo_Receta == CodigoComidas)
                                    {
                                        foreach (var i in itecat.RecetaCategoria)
                                        {
                                            var CodioComida = i.Codigo;
                                            var NombreFinalComida = i.Descripcion.ToString();
                                            var precioFinalComida = i.Precios.Sum(precio => precio.General);
                                            var frecuenciaComida = i.Frecuente.ToString();

                                            // Hacer algo con precioFinalCombo
                                            datosFinalesComida.Add((CodioComida, NombreFinalComida, precioFinalComida, frecuenciaComida, itecat.Codigo));
                                        }
                                    }
                                    //Valido que las listas se hayan llenado
                                    if (datosFinalesBotella.Count > 0 && datosFinalesComida.Count > 0)
                                    {
                                        // Establecer el indicador para salir del bucle
                                        condicionCumplida = true;
                                    }
                                    // Si se cumplió la condición, salir del bucle foreach
                                    if (condicionCumplida)
                                    {
                                        break;
                                    }
                                }
                                break;

                            case "A": //CATEGORIAS
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Check = string.Empty;
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;

                                List<Receta> ob_recpro = new List<Receta>();
                                List<Precios> ob_prepro = new List<Precios>();
                                List<Producto> ob_lispro = new List<Producto>();
                                List<Pantallas> ob_panpro = new List<Pantallas>();

                                ob_datpro.Receta = ob_recpro;
                                ob_datpro.Precios = ob_prepro;
                                ob_datpro.Pantallas = ob_panpro;
                                ob_datpro.LisProducto = ob_lispro;

                                foreach (var itecat in itepro.Receta)
                                {
                                    Producto ob_itecat = new Producto();

                                    ob_itecat.Tipo = itecat.Tipo;
                                    ob_itecat.Check = string.Empty;
                                    ob_itecat.Codigo = itecat.Codigo;
                                    ob_itecat.Precios = itecat.Precios;
                                    ob_itecat.Cantidad = itecat.Cantidad;
                                    ob_itecat.Descripcion = itecat.Descripcion;

                                    ob_datpro.LisProducto.Add(ob_itecat);
                                }

                                break;
                        }

                        //Romper el ciclo
                        break;
                    }
                }
                ViewBag.ListaB = datosFinalesBotella.Distinct().ToList();
                ViewBag.ListaC = datosFinalesComida.Distinct().ToList();
                //Asignar valores encriptados
                ob_datpro.SwtVenta = pr_swtven;
                ob_datpro.EmailEli = Session.GetString("Usuario");
                ob_datpro.NombreEli = Session.GetString("Nombre");
                ob_datpro.KeyTeatro = Session.GetString("Teatro");
                ob_datpro.DesTeatro = Session.GetString("TeatroNombre");
                ob_datpro.TipoCompra = pr_tiplog;
                ob_datpro.ApellidoEli = Session.GetString("Apellido");
                ob_datpro.TelefonoEli = Session.GetString("Telefono");
                ob_datpro.KeySecuencia = pr_secpro;

                return View(ob_datpro);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/DetailsCon";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }
        #endregion

        #region POST
        /// <summary>
        /// POST: Room -- Proceso de ejecución SCOGRU para dejar ubicaciones en preventa
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("RoomBol")]
        public ActionResult RoomBol(BolVenta pr_bolvta)
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            string lc_auxitm = string.Empty;
            string lc_auxtel = string.Empty;
            string lc_auxsec = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string[] ls_lstsel = new string[5];

            List<string> ls_lstubi = new List<string>();
            List<Ubicaciones> ob_ubiprg = new List<Ubicaciones>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            Secuencia ob_secsec = new Secuencia();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                if (Session.GetString("Secuencia") != null)
                {
                    lc_auxsec = Session.GetString("Secuencia");
                }
                else
                {
                    #region SERVICIO SCOSEC
                    //Asignar valores SEC
                    ob_secsec.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                    ob_secsec.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                    ob_secsec.Tercero = config.Value.ValorTercero;

                    //Generar y encriptar JSON para servicio SEC
                    lc_srvpar = ob_fncgrl.JsonConverter(ob_secsec);
                    lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                    lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                    lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                    //Encriptar Json SEC
                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                    //Consumir servicio SEC
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosec/"), lc_srvpar);

                    //Validar respuesta
                    if (lc_result.Substring(0, 1) == "0")
                    {
                        //Quitar switch
                        lc_result = lc_result.Replace("0-", "");
                        lc_result = lc_result.Replace("[", "");
                        lc_result = lc_result.Replace("]", "");

                        //Deserializar Json y validar respuesta SEC
                        ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                        if (ob_diclst.ContainsKey("Validación"))
                        {
                            return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString(), pr_flag = "IR" });
                        }
                        else
                        {
                            lc_auxsec = ob_diclst["Secuencia"].ToString().Substring(0, ob_diclst["Secuencia"].ToString().IndexOf("."));
                            Session.SetString("Secuencia", lc_auxsec);
                        }

                        ob_diclst.Clear();
                    }
                    else
                    {
                        ViewBag.alertS = false;

                        lc_result = lc_result.Replace("1-", "");
                        ModelState.AddModelError("", lc_result);
                    }
                    #endregion
                }

                #region SERVICIO SCOGRU
                //Validar secuencia
                if (lc_auxsec != "0")
                {
                    //Asignar valores
                    pr_bolvta.Funcion = "";
                    pr_bolvta.Horario = "";
                    pr_bolvta.message = "";
                    pr_bolvta.FechaPrg = "";
                    pr_bolvta.FechaDia = "";
                    pr_bolvta.ValorTarifa = "";
                    pr_bolvta.IdTarifa = "";
                    pr_bolvta.MapaSala = new Ubicaciones[1, 1];

                    pr_bolvta.EmailEli = Session.GetString("Usuario");
                    pr_bolvta.NombreEli = Session.GetString("Nombre");
                    pr_bolvta.ApellidoEli = Session.GetString("Apellido");
                    pr_bolvta.TelefonoEli = Session.GetString("Telefono");

                    pr_bolvta.Tipo = "B";
                    pr_bolvta.NombreTarifa = pr_bolvta.NombreTar;
                    pr_bolvta.NombrePelicula = pr_bolvta.NombrePel;

                    pr_bolvta.Sala = Convert.ToInt32(pr_bolvta.KeySala);
                    pr_bolvta.KeySala = pr_bolvta.KeySala;
                    pr_bolvta.Telefono = Convert.ToInt64(Session.GetString("Telefono"));

                    pr_bolvta.Nombre = Session.GetString("Nombre");
                    pr_bolvta.Apellido = Session.GetString("Apellido");

                    pr_bolvta.FecProg = pr_bolvta.FecProg;
                    pr_bolvta.HorProg = pr_bolvta.HorProg;
                    pr_bolvta.KeyTarifa = pr_bolvta.KeyTarifa;
                    pr_bolvta.KeyTeatro = Session.GetString("Teatro");
                    pr_bolvta.KeyPelicula = pr_bolvta.KeyPelicula;
                    pr_bolvta.KeySecuencia = lc_auxsec;

                    pr_bolvta.Tercero = config.Value.ValorTercero;
                    pr_bolvta.Secuencia = Convert.ToInt32(lc_auxsec);
                    pr_bolvta.PuntoVenta = Convert.ToInt32(config.Value.PuntoVenta);
                    pr_bolvta.IdFuncion = pr_bolvta.HorProg.ToString().Length == 4 ? Convert.ToInt32(pr_bolvta.HorProg.ToString().Substring(0, 2)) : Convert.ToInt32(pr_bolvta.HorProg.ToString().Substring(0, 1));

                    //Obtener ubicaciones de vista
                    char[] ar_charst = pr_bolvta.SelUbicaciones.ToCharArray();
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

                        ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = Convert.ToInt32(pr_bolvta.KeyTarifa), FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                    }

                    pr_bolvta.Ubicaciones = ob_ubiprg;

                    //Validar cantidad de sillas
                    if (pr_bolvta.Ubicaciones.Count > Convert.ToInt32(config.Value.CantSillasBol))
                        return RedirectToAction("Error", "Pages", new { pr_message = "Solo se pueden seleccionar hasta " + config.Value.CantSillasBol + " sillas por transacción.", pr_flag = "IR" });

                    //Generar y encriptar JSON para servicio
                    lc_srvpar = ob_fncgrl.JsonConverter(pr_bolvta);

                    lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                    lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                    lc_srvpar = lc_srvpar.Replace("nombre", "Nombre");
                    lc_srvpar = lc_srvpar.Replace("tarifa", "Tarifa");
                    lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                    lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                    lc_srvpar = lc_srvpar.Replace("keyTeatro", "teatro");
                    lc_srvpar = lc_srvpar.Replace("apellido", "Apellido");
                    lc_srvpar = lc_srvpar.Replace("telefono", "Telefono");
                    lc_srvpar = lc_srvpar.Replace("keyPelicula", "Pelicula");
                    lc_srvpar = lc_srvpar.Replace("secuencia", "Secuencia");
                    lc_srvpar = lc_srvpar.Replace("fecProg", "FechaFuncion");
                    lc_srvpar = lc_srvpar.Replace("\"id\"", "\"IdMessage\"");
                    lc_srvpar = lc_srvpar.Replace("horProg", "InicioFuncion");
                    lc_srvpar = lc_srvpar.Replace("idFuncion", "HoraFuncion");
                    lc_srvpar = lc_srvpar.Replace("puntoVenta", "PuntoVenta");
                    lc_srvpar = lc_srvpar.Replace("ubicaciones", "Ubicaciones");
                    lc_srvpar = lc_srvpar.Replace("NombrePelicula", "Descripcion");

                    //Encriptar Json GRU
                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                    //Consumir servicio GRU
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scogru/"), lc_srvpar);

                    //Generar Log
                    LogSales logSales = new LogSales();
                    LogAudit logAudit = new LogAudit(config);
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "FastSales/RoomBol";
                    logSales.Metodo = "SCOGRU";
                    logSales.ExceptionMessage = lc_srvpar;
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    logAudit.LogApp(logSales);

                    //Validar respuesta GRU
                    if (lc_result.Substring(0, 1) == "0")
                    {
                        //Quitar switch
                        lc_result = lc_result.Replace("0-", "");

                        //Deserializar Json y validar respuesta
                        Dictionary<string, string>[] ob_result = (Dictionary<string, string>[])JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>[])));

                        //Validar respuesta llave 1
                        for (int lc_idxiii = 0; lc_idxiii < ob_result.Length; lc_idxiii++)
                        {
                            Dictionary<string, string> ob_auxrta = ob_result[lc_idxiii];

                            if (ob_auxrta.ContainsKey("Validación"))
                            {
                                return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString(), pr_flag = "PR" });
                            }
                            else
                            {
                                //Validar respuesta llave 2
                                if (ob_auxrta.ContainsKey("Respuesta"))
                                {
                                    if (ob_auxrta["Respuesta"].ToString() == "Proceso exitoso")
                                    {
                                        string lc_auxtar = pr_bolvta.NombreTar;
                                        double lc_valtar = Convert.ToDouble(lc_auxtar.Substring(lc_auxtar.IndexOf(";") + 1, lc_auxtar.Length - (lc_auxtar.IndexOf(";") + 1)));

                                        //Inicializar instancia de BD
                                        using (var context = new DataDB(config))
                                        {
                                            //Agregar valores a tabla ReportSales
                                            string TelefonoEli = string.Concat(Session.GetString("Telefono"), ";", Session.GetString("Documento"), "*", Session.GetString("Direccion"));
                                            var reportSales = new ReportSales
                                            {
                                                Secuencia = pr_bolvta.KeySecuencia,
                                                KeySala = pr_bolvta.KeySala,
                                                KeyTeatro = Session.GetString("Teatro"),
                                                KeyPelicula = pr_bolvta.KeyPelicula,
                                                SelUbicaciones = pr_bolvta.SelUbicaciones,
                                                Precio = (lc_valtar * pr_bolvta.Ubicaciones.Count),
                                                HorProg = pr_bolvta.HorProg,
                                                FecProg = pr_bolvta.FecProg,
                                                EmailEli = Session.GetString("Usuario"),
                                                NombreEli = Session.GetString("Nombre"),
                                                ApellidoEli = Session.GetString("Apellido"),
                                                TelefonoEli = TelefonoEli,
                                                NombrePel = pr_bolvta.NombrePel,
                                                NombreFec = pr_bolvta.NombreFec,
                                                NombreHor = pr_bolvta.NombreHor,
                                                NombreTar = pr_bolvta.NombreTar,
                                                KeyTarifa = pr_bolvta.KeyTarifa,
                                                Transaccion = pr_bolvta.Censura,
                                                Referencia = pr_bolvta.Imagen,
                                                FechaCreado = DateTime.Now,
                                                FechaModificado = DateTime.Now,
                                                KeyPunto = config.Value.PuntoVenta
                                            };

                                            //Adicionar y guardar registro a tabla
                                            context.ReportSales.Add(reportSales);
                                            context.SaveChanges();
                                        }

                                        //Paso datos de paso url
                                        if (pr_bolvta.NombrePel.Contains("3D"))
                                            return RedirectToAction("PreOnboarding", "SalesBol", new { pr_keypel = pr_bolvta.KeyPelicula, pr_fecprg = pr_bolvta.FecProg, pr_horprg = pr_bolvta.HorProg, pr_tarprg = pr_bolvta.KeyTarifa, pr_salprg = pr_bolvta.KeySala + ";" + pr_bolvta.TipoSilla, pr_nompel = pr_bolvta.NombrePel, pr_nomfec = pr_bolvta.NombreFec, pr_nomhor = pr_bolvta.NombreHor, pr_nomtar = pr_bolvta.NombreTar, pr_cenprg = pr_bolvta.Censura, pr_secsec = pr_bolvta.KeySecuencia, pr_selubi = pr_bolvta.SelUbicaciones, pr_flag3d = "F" });
                                        else
                                            return RedirectToAction("ListCon", "FastSales", new { pr_secpro = pr_bolvta.KeySecuencia, pr_swtven = "V", pr_tiplog = "B", pr_tbview = "tab-combos", pr_cenprg = pr_bolvta.Censura });

                                    }
                                    else
                                    {
                                        return RedirectToAction("Error", "Pages", new { pr_message = ob_auxrta["Respuesta"].ToString(), pr_flag = "ER" });
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        lc_result = lc_result.Replace("1-", "");
                        return RedirectToAction("Error", "Pages", new { pr_message = lc_result, pr_flag = "ER" });
                    }
                }
                else
                {
                    return RedirectToAction("Error", "Pages", new { pr_message = "Error en Secuencia", pr_flag = "ER" });
                }
                #endregion

                //Devolver a vista
                return View(pr_bolvta);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/RoomBol";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }

        [HttpPost]
        [Route("DetallesCon")]
        public ActionResult DetailsCon(Producto pr_datpro)
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxite = string.Empty;
            string lc_secsec = string.Empty;

            List<Producto> ob_return = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
            Dictionary<string, string> ob_seclst = new Dictionary<string, string>();

            Secuencia ob_secsec = new Secuencia();
            Secuencia ob_scopre = new Secuencia();
            Producto ob_datpro = new Producto();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente");

                #region SERVICIO SCOSEC
                lc_secsec = pr_datpro.KeySecuencia;
                if (lc_secsec == "0")
                {
                    //Asignar valores SEC
                    ob_secsec.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                    ob_secsec.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                    ob_secsec.Tercero = config.Value.ValorTercero;

                    //Generar y encriptar JSON para servicio SEC
                    lc_srvpar = ob_fncgrl.JsonConverter(ob_secsec);
                    lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                    lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                    lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                    //Encriptar Json SEC
                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                    //Consumir servicio SEC
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosec/"), lc_srvpar);

                    //Generar Log
                    LogSales logSales = new LogSales();
                    LogAudit logAudit = new LogAudit(config);
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "FastSales/DetailsCon";
                    logSales.Metodo = "SCOSEC";
                    logSales.ExceptionMessage = lc_srvpar;
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    logAudit.LogApp(logSales);

                    //Validar respuesta
                    if (lc_result.Substring(0, 1) == "0")
                    {
                        //Quitar switch
                        lc_result = lc_result.Replace("0-", "");
                        lc_result = lc_result.Replace("[", "");
                        lc_result = lc_result.Replace("]", "");

                        //Deserializar Json y validar respuesta SEC
                        ob_seclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                        //Validar respuesta del servicio
                        if (ob_seclst.ContainsKey("Validación"))
                            return RedirectToAction("Error", "Pages", new { pr_message = ob_seclst["Validación"].ToString(), pr_flag = "PR" });
                        else
                        {
                            lc_secsec = ob_seclst["Secuencia"].ToString().Substring(0, ob_seclst["Secuencia"].ToString().IndexOf("."));
                            Session.SetString("Secuencia", lc_secsec);
                        }


                        //Limpiar objeto
                        ob_seclst.Clear();
                    }
                    else
                    {
                        //Devolver a vista
                        lc_result = lc_result.Replace("1-", "");
                        ModelState.AddModelError("", lc_result);
                        return RedirectToAction("Error", "Pages", new { pr_message = lc_result, pr_flag = "ER" });
                    }
                }
                #endregion

                //Validar Categoria 
                if (pr_datpro.Tipo == "A")
                {
                    if (pr_datpro.Check != null)
                    {
                        pr_datpro.ProCategoria_1 = Convert.ToDecimal(pr_datpro.Check);
                        pr_datpro.Check1 = "0";
                        pr_datpro.Check2 = "0";
                        pr_datpro.Check3 = "0";
                        pr_datpro.Check4 = "0";
                        pr_datpro.Check5 = "0";

                        pr_datpro.CanCategoria_1 = pr_datpro.Cantidad;
                        pr_datpro.Cantidad1 = 0;
                        pr_datpro.Cantidad2 = 0;
                        pr_datpro.Cantidad3 = 0;
                        pr_datpro.Cantidad4 = 0;
                        pr_datpro.Cantidad5 = 0;
                    }
                    else
                    {
                        //Obtener detalle del producto seleccionado
                        ob_datpro = GetDetails(pr_datpro);
                        if (ob_datpro.Codigo == -1)
                        {
                            //Cargar mensaje de error
                            lc_auxite = ob_datpro.Descripcion;
                            ModelState.AddModelError("", lc_auxite);

                            //Devolver a vista
                            ob_datpro.Codigo = pr_datpro.Codigo;
                            ob_datpro.Descripcion = pr_datpro.Descripcion;
                            return View(ob_datpro);
                        }
                        else
                        {
                            //Devolver a vista
                            ModelState.AddModelError("", "Debe seleccionar un ítem de la categoría para continuar");
                            return View(ob_datpro);
                        }
                    }
                }

                //Inicializar instancia de BD
                using (var context = new DataDB(config))
                {
                    //Agregar valores a tabla ReportSales
                    var retailSales = new RetailSales
                    {
                        Tipo = pr_datpro.Tipo,
                        Precio = Convert.ToDecimal(pr_datpro.Valor),
                        Cantidad = pr_datpro.Cantidad,
                        Secuencia = Convert.ToDecimal(lc_secsec),
                        PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                        KeyProducto = pr_datpro.Codigo,
                        Descripcion = pr_datpro.Descripcion,
                        ProProducto1 = pr_datpro.ProProducto_1,
                        ProProducto2 = pr_datpro.ProProducto_2,
                        ProProducto3 = pr_datpro.ProProducto_3,
                        ProProducto4 = pr_datpro.ProProducto_4,
                        ProProducto5 = pr_datpro.ProProducto_5,
                        CanProducto1 = pr_datpro.ProCantidad_1,
                        CanProducto2 = pr_datpro.ProCantidad_2,
                        CanProducto3 = pr_datpro.ProCantidad_3,
                        CanProducto4 = pr_datpro.ProCantidad_4,
                        CanProducto5 = pr_datpro.ProCantidad_5,
                        ProCategoria1 = pr_datpro.ProCategoria_1,
                        ProCategoria2 = pr_datpro.ProCategoria_2,
                        ProCategoria3 = pr_datpro.ProCategoria_3,
                        ProCategoria4 = pr_datpro.ProCategoria_4,
                        ProCategoria5 = pr_datpro.ProCategoria_5,
                        CanCategoria1 = pr_datpro.CanCategoria_1,
                        CanCategoria2 = pr_datpro.CanCategoria_2,
                        CanCategoria3 = pr_datpro.CanCategoria_3,
                        CanCategoria4 = pr_datpro.CanCategoria_4,
                        CanCategoria5 = pr_datpro.CanCategoria_5,
                        FechaRegistro = DateTime.Now,
                        KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro")),
                        SwitchAdd = pr_datpro.SwitchAdd
                    };

                    //Adicionar y guardar registro a tabla
                    context.RetailSales.Add(retailSales);
                    context.SaveChanges();
                }

                //Validar Combo
                if (pr_datpro.Tipo == "C")
                {
                    //Obtener id de prodcutos de combo
                    int IdRetail = 0;
                    using (var context = new DataDB(config))
                        IdRetail = context.RetailSales.Max(u => u.Id);

                    //Recorrer la cantidad maxima de categorias por combo
                    for (int lc_variii = 0; lc_variii < 4; lc_variii++)
                    {
                        switch (lc_variii)
                        {
                            case 0:
                                RetailDet(Convert.ToDecimal(lc_secsec), IdRetail, pr_datpro.ProCategoria_1, pr_datpro.Check1, pr_datpro.Check11, pr_datpro.Check111, pr_datpro.Check1111);
                                break;
                            case 1:
                                RetailDet(Convert.ToDecimal(lc_secsec), IdRetail, pr_datpro.ProCategoria_2, pr_datpro.Check2, pr_datpro.Check22, pr_datpro.Check222, pr_datpro.Check2222);
                                break;
                            case 2:
                                RetailDet(Convert.ToDecimal(lc_secsec), IdRetail, pr_datpro.ProCategoria_3, pr_datpro.Check3, pr_datpro.Check33, pr_datpro.Check333, pr_datpro.Check3333);
                                break;
                            case 3:
                                RetailDet(Convert.ToDecimal(lc_secsec), IdRetail, pr_datpro.ProCategoria_4, pr_datpro.Check4, pr_datpro.Check44, pr_datpro.Check444, pr_datpro.Check4444);
                                break;
                            case 4:
                                RetailDet(Convert.ToDecimal(lc_secsec), IdRetail, pr_datpro.ProCategoria_5, pr_datpro.Check5, pr_datpro.Check44, pr_datpro.Check555, pr_datpro.Check5555);
                                break;
                        }
                    }
                }

                //Devolver a vista
                return RedirectToAction("ListCon", "FastSales", new { pr_secpro = lc_secsec.ToString(), pr_swtven = "V", pr_tiplog = pr_datpro.TipoCompra, pr_cenprg = pr_datpro.Censura });

            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/DetailsCon";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "ER" });
            }
        }
        #endregion

        #region MÉTODOS DE CLASE
        /// <summary>
        /// Método para cargar ciudades y teatros seleccionados forma externa
        /// </summary>
        /// <param name="pr_flag">Parámetro de ciudad</param>
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
        private void Selteatros(string pr_ciudad, string pr_teatro, string pr_nomteatro)
        {
            //Cargar ciudad seleccionada
            Ciuteatros(pr_ciudad);

            //Cargar Teatro
            Session.SetString("Teatro", pr_teatro);
            Session.SetString("TeatroNombre", pr_nomteatro);
            ViewBag.NombreCiudadTeatro = pr_nomteatro;
        }

        /// <summary>
        /// Método para cargar URL de Header y Footer del portal
        /// </summary>
        /// <returns></returns>
        private void URLPortal(IOptions<MyConfig> config)
        {
            #region VARIABLES LOCALES
            General ob_fncgrl = new General();
            #endregion

            //Obtener ciudades y teatros
            //ViewBag.Ciudades = ob_fncgrl.Ciudades(config.Value.Ciudades41, "C");
            //ViewBag.Teatros = ob_fncgrl.Ciudades(config.Value.Ciudades41, "T");

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
            var PuntoVenta = config.Value.PuntoVenta;
            var KeyTeatro = Session.GetString("Teatro");
            if (Session.GetString("Secuencia") != null)
            {
                //Obtener productos carrito de compra
                lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                using (var context = new DataDB(config))
                {
                    PuntoVenta = config.Value.PuntoVenta;
                    KeyTeatro = Session.GetString("Teatro");
                    var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).ToList();
                    ViewBag.ListCarritoR = RetailSales;
                    ViewBag.SwitchAddBtn = RetailSales.Any(x => x.SwitchAdd == "S");

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
        }

        /// <summary>
        /// Método para obtener listado de fechas de cartelera de películas
        /// </summary>
        /// <param name="pr_fecprg">fecha seleccionada</param>
        /// <returns></returns>
        private List<DateCartelera> DatePortal(string pr_fecprg)
        {
            #region VARIABLES LOCALES
            string st_fecpro = string.Empty;
            string lc_fechoy = DateTime.Now.ToString("yyyyMMdd");

            DateTime dt_fecpro;
            DateTime dt_fechoy = DateTime.Now;

            List<DateCartelera> ob_fechas = new List<DateCartelera>();
            #endregion

            //Validar día seleccioando
            if (pr_fecprg == "")
                pr_fecprg = lc_fechoy;

            //Recorrer y asignar valores
            for (int lc_variii = 0; lc_variii < 8; lc_variii++)
            {
                //Creabr objeto para asignar fecha
                DateCartelera ob_cartelera = new DateCartelera();
                dt_fecpro = dt_fechoy.AddDays(lc_variii);

                //Validar día
                if (lc_variii == 0)
                    ob_cartelera.DiaLt = "HOY";
                else
                    ob_cartelera.DiaLt = DiaMes(dt_fecpro.DayOfWeek.ToString(), "D");

                //Validar flag
                if (pr_fecprg == dt_fecpro.ToString("yyyyMMdd"))
                    ob_cartelera.Flags = "S";
                else
                    ob_cartelera.Flags = "N";

                //Asignar valores
                ob_cartelera.FecDt = dt_fecpro;
                ob_cartelera.FecSt = dt_fecpro.ToString("yyyyMMdd");
                ob_cartelera.DiaNb = dt_fecpro.Day.ToString().Length < 2 ? "0" + dt_fecpro.Day.ToString() : dt_fecpro.Day.ToString();
                ob_cartelera.MesLt = DiaMes(dt_fecpro.Month.ToString(), "M");
                ob_fechas.Add(ob_cartelera);
            }

            //Devolver valores
            ViewBag.Mes = DiaMes(pr_fecprg.Substring(4, 2), "M");
            return ob_fechas;
        }

        /// <summary>
        /// Método para obtener dia de la semana
        /// </summary>
        /// <param name="pr_daynum">id del día</param>
        /// <returns></returns>
        private string DiaMes(string pr_daynum, string pr_flag)
        {
            #region VARIABLES LOCALES
            string lc_daystr = string.Empty;
            #endregion

            if (pr_flag == "D")
            {
                //Selección de día.
                switch (pr_daynum)
                {
                    case "Sunday":
                        lc_daystr = "DOM";
                        break;
                    case "Monday":
                        lc_daystr = "LUN";
                        break;
                    case "Tuesday":
                        lc_daystr = "MAR";
                        break;
                    case "Wednesday":
                        lc_daystr = "MIE";
                        break;
                    case "Thursday":
                        lc_daystr = "JUE";
                        break;
                    case "Friday":
                        lc_daystr = "VIE";
                        break;
                    case "Saturday":
                        lc_daystr = "SAB";
                        break;
                }
            }
            else
            {
                //Selección de día.
                switch (pr_daynum)
                {
                    case "01":
                        lc_daystr = "ENERO";
                        break;
                    case "02":
                        lc_daystr = "FEBRERO";
                        break;
                    case "03":
                        lc_daystr = "MARZO";
                        break;
                    case "04":
                        lc_daystr = "ABRIL";
                        break;
                    case "05":
                        lc_daystr = "MAYO";
                        break;
                    case "06":
                        lc_daystr = "JUNIO";
                        break;
                    case "07":
                        lc_daystr = "JULIO";
                        break;
                    case "08":
                        lc_daystr = "AGOSTO";
                        break;
                    case "09":
                        lc_daystr = "SEPTIEMBRE";
                        break;
                    case "10":
                        lc_daystr = "OCTUBRE";
                        break;
                    case "11":
                        lc_daystr = "NOVIEMBRE";
                        break;
                    case "12":
                        lc_daystr = "DICIEMBRE";
                        break;
                }
            }

            //Devovler Valores
            return lc_daystr;
        }

        /// <summary>
        /// Método para obtener las horas y tarifas de la fecha seleccionada de la película
        /// </summary>
        /// <param name="pr_keypel">ID película</param>
        /// <param name="pr_fecprg">Fecha seleccionada</param>
        /// <returns></returns>
        private List<sala> GetHora(string pr_keypel, string pr_fecprg)
        {
            #region VARIABLES LOCALES
            int lc_keypel = 0;
            int lc_auxpel = 0;
            int lc_keytea = 0;
            int lc_auxtea = 0;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;

            XmlDocument ob_xmldoc = new XmlDocument();
            Dictionary<string, string> dc_zonas = new Dictionary<string, string>();

            BolVenta ob_datprg = new BolVenta();
            List<sala> ob_lisprg = new List<sala>();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //Validar fecha
                if (pr_fecprg == "")
                    pr_fecprg = DateTime.Now.ToString("yyyyMMdd");

                ViewBag.Fecha3 = pr_fecprg;

                //Obtener información de la web
                ob_xmldoc.Load(config.Value.Variables41);
                XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

                //Recorrer xml y obtener datos
                foreach (XmlElement item in pelicula)
                {
                    //Validar película
                    lc_keypel = Convert.ToInt32(item.GetAttribute("id"));
                    lc_auxpel = Convert.ToInt32(pr_keypel);

                    if (lc_keypel == lc_auxpel)
                    {
                        //Datos de nodo pelicula
                        ViewBag.Pelicula = item.GetAttribute("nombre").ToString();

                        //Datos de nodo pelicula/data
                        XmlNodeList data = item.GetElementsByTagName("data");
                        foreach (XmlElement itemdata in data)
                            ViewBag.Imagen = itemdata.GetAttribute("Imagen").ToString();

                        // Datos de nodo pelicula / cinemas
                        XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
                        foreach (XmlElement item2 in cinemas)
                        {
                            // Datos de nodo pelicula / cinemas / cinema
                            XmlNodeList cinema = item2.GetElementsByTagName("cinema");
                            foreach (XmlElement itemT in cinema)
                            {
                                //Validar Teatro
                                lc_keytea = Convert.ToInt32(itemT.GetAttribute("id"));
                                lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));

                                if (lc_keytea == lc_auxtea)
                                {
                                    // Datos de nodo pelicula / salas
                                    XmlNodeList salas = itemT.GetElementsByTagName("salas");
                                    foreach (XmlElement item3 in salas)
                                    {
                                        //Datos de nodo pelicula / salas / sala
                                        XmlNodeList sala = item3.GetElementsByTagName("sala");
                                        foreach (XmlElement itemS in sala)
                                        {
                                            //Obtener datos
                                            sala ob_room = new sala();
                                            ob_room.tipoSala = itemS.GetAttribute("tipoSala");
                                            ob_room.numeroSala = itemS.GetAttribute("numeroSala");
                                            ob_lisprg.Add(ob_room);

                                            //Datos de nodo pelicula / salas / dia
                                            XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
                                            foreach (XmlElement item4 in Fecha)
                                            {
                                                //Validar fecha
                                                if (pr_fecprg == item4.GetAttribute("univ").ToString())
                                                {
                                                    //Datos Nodo fecha
                                                    ViewBag.Fecha2 = item4.GetAttribute("dia").ToString();

                                                    // Datos de nodo pelicula / salas / dia / Hora
                                                    IList<hora> ls_prgtmp = new List<hora>();
                                                    XmlNodeList hora = item4.GetElementsByTagName("hora");
                                                    foreach (XmlElement item5 in hora)
                                                    {
                                                        if (item5.GetAttribute("webserviceVentas") == "Si")
                                                        {
                                                            //Obtener horas
                                                            string horuno = item5.GetAttribute("militar");
                                                            DateTime FechaHoraInicio = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy") + " " + horuno.Substring(0, 2) + ":" + horuno.Substring(2, 2) + ":00");
                                                            DateTime FechaHoraTermino = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                                                            //Validar pintada de la hora
                                                            hora ob_prgtmp = new hora();
                                                            if (config.Value.MinDifHora != "0")
                                                            {
                                                                //Validar hora vs valor programado solo para hoy
                                                                if (DateTime.Now.ToString("yyyyMMdd") == pr_fecprg)
                                                                {
                                                                    //Diferencia de tiempo entre hora funcion y hora del dia 
                                                                    TimeSpan diferencia = FechaHoraInicio - FechaHoraTermino;
                                                                    var diferenciaenminutos = diferencia.TotalMinutes;

                                                                    if (diferenciaenminutos > Convert.ToDouble(config.Value.MinDifHora))
                                                                    {
                                                                        ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
                                                                        ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
                                                                    }
                                                                    else
                                                                    {
                                                                        continue;
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
                                                                    ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
                                                                }
                                                            }
                                                            else
                                                            {
                                                                ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
                                                                ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
                                                            }

                                                            //Datos de nodo pelicula / salas / dia / hora / TipoZona
                                                            XmlNodeList zona = item5.GetElementsByTagName("TipoZona");
                                                            IList<TipoZona> ls_zontmp = new List<TipoZona>();
                                                            foreach (XmlElement item6 in zona)
                                                            {
                                                                TipoZona lc_zona = new TipoZona();
                                                                lc_zona.nombreZona = item6.GetAttribute("nombreZona");

                                                                if (!dc_zonas.ContainsKey(lc_zona.nombreZona))
                                                                    dc_zonas.Add(lc_zona.nombreZona, lc_zona.nombreZona);

                                                                //Datos de nodo peliculas / salas / dia / hora / TipoZona / TipoSilla
                                                                XmlNodeList silla = item6.GetElementsByTagName("TipoSilla");
                                                                IList<TipoSilla> ls_siltmp = new List<TipoSilla>();
                                                                foreach (XmlElement item7 in silla)
                                                                {
                                                                    TipoSilla lc_silla = new TipoSilla();
                                                                    lc_silla.nombreTipoSilla = item7.GetAttribute("nombreTipoSilla");

                                                                    //Datos de nodo peliculas / salas / dia / hora / TipoZona / TipoSilla / Tarifa
                                                                    IList<Tarifa> ls_tartmp = new List<Tarifa>();
                                                                    XmlNodeList tarifa = item7.GetElementsByTagName("Tarifa");
                                                                    foreach (XmlElement item8 in tarifa)
                                                                    {

                                                                        //Validar tarifas terceros
                                                                        if (item8.GetAttribute("validoTeceros") == "Si" && item8.GetAttribute("clienteFrecuente") == Session.GetString("ClienteFrecuente"))
                                                                        {
                                                                            Tarifa ob_tartmp = new Tarifa();
                                                                            ob_tartmp.codigoTarifa = item8.GetAttribute("codigoTarifa").ToString();
                                                                            ob_tartmp.nombreTarifa = item8.GetAttribute("nombreTarifa").ToString();
                                                                            ob_tartmp.valor = item8.GetAttribute("valor").ToString().Substring(0, item8.GetAttribute("valor").ToString().Length - 2);

                                                                            //Adiconar a lista para mostrar
                                                                            ls_tartmp.Add(ob_tartmp);
                                                                        }
                                                                    }

                                                                    lc_silla.Tarifa = ls_tartmp;
                                                                    ls_siltmp.Add(lc_silla);
                                                                }

                                                                lc_zona.TipoSilla = ls_siltmp;
                                                                ls_zontmp.Add(lc_zona);

                                                            }
                                                            ob_prgtmp.TipoZonaOld = ls_zontmp;
                                                            ls_prgtmp.Add(ob_prgtmp);
                                                        }
                                                        else
                                                        {
                                                            continue;
                                                        }

                                                        //Adiconar a lista para mostrar
                                                        ob_room.hora = ls_prgtmp;

                                                        //Cortar el ciclo
                                                        //break;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Cortar el ciclo
                                    break;
                                }
                            }
                        }

                        //Cortar el ciclo
                        break;
                    }
                }
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/GetHora";
                logSales.Metodo = "METHOD";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }

            //Devolver valores
            ViewBag.Zonas = dc_zonas;
            return ob_lisprg;
        }

        /// <summary>
        /// Método para poder obtener el detalle del producto seleccionado para retornar a vista
        /// </summary>
        /// <param name="pr_datpro">PARM entidad de producto seleccionado</param>
        /// <returns></returns>
        private Producto GetDetails(Producto pr_datpro)
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitem = string.Empty;

            List<Producto> ob_return = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            Secuencia ob_scopre = new Secuencia();
            Producto ob_datpro = new Producto();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                ob_datpro.Codigo = pr_datpro.Codigo;
                ob_datpro.SwtVenta = ob_fncgrl.DecryptStringAES(pr_datpro.SwtVenta);
                ob_datpro.EmailEli = Session.GetString("Usuario");
                ob_datpro.NombreEli = Session.GetString("Nombre");
                ob_datpro.KeyTeatro = Session.GetString("Teatro");
                ob_datpro.DesTeatro = Session.GetString("TeatroNombre");
                ob_datpro.TipoCompra = ob_fncgrl.DecryptStringAES(pr_datpro.TipoCompra);
                ob_datpro.ApellidoEli = Session.GetString("Apellido");
                ob_datpro.TelefonoEli = Session.GetString("Telefono");
                ob_datpro.KeySecuencia = ob_fncgrl.DecryptStringAES(pr_datpro.KeySecuencia);

                #region SERVICIO SCOPRE
                //Asignar valores PRE
                ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                ob_scopre.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                ob_scopre.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio PRE
                lc_srvpar = ob_fncgrl.JsonConverter(ob_scopre);
                lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                //Encriptar Json PRE
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio PRE
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/GetDetails";
                logSales.Metodo = "SCOPRE";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                    ob_return = (List<Producto>)JsonConvert.DeserializeObject(ob_diclst["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                    if (ob_diclst.ContainsKey("Validación"))
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
                    else
                        ViewBag.ListaM = ob_return;
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);
                }
                #endregion

                //Recorrido por productos para obtener el seleccionado y sus valores
                foreach (var itepro in ob_return)
                {
                    if (itepro.Codigo == ob_datpro.Codigo)
                    {
                        switch (itepro.Tipo)
                        {
                            case "P": //PRODUCTOS
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Precios = itepro.Precios;
                                break;

                            case "C": //COMBOS
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Receta = itepro.Receta;
                                break;

                            case "A": //CATEGORIAS
                                ob_datpro.Tipo = itepro.Tipo;
                                ob_datpro.Check = string.Empty;
                                ob_datpro.Codigo = itepro.Codigo;
                                ob_datpro.Descripcion = itepro.Descripcion;

                                List<Receta> ob_recpro = new List<Receta>();
                                List<Precios> ob_prepro = new List<Precios>();
                                List<Producto> ob_lispro = new List<Producto>();
                                List<Pantallas> ob_panpro = new List<Pantallas>();

                                ob_datpro.Receta = ob_recpro;
                                ob_datpro.Precios = ob_prepro;
                                ob_datpro.Pantallas = ob_panpro;
                                ob_datpro.LisProducto = ob_lispro;

                                foreach (var itecat in itepro.Receta)
                                {
                                    Producto ob_itecat = new Producto();

                                    ob_itecat.Tipo = itecat.Tipo;
                                    ob_itecat.Check = string.Empty;
                                    ob_itecat.Codigo = itecat.Codigo;
                                    ob_itecat.Precios = itecat.Precios;
                                    ob_itecat.Cantidad = itecat.Cantidad;
                                    ob_itecat.Descripcion = itecat.Descripcion;

                                    ob_datpro.LisProducto.Add(ob_itecat);
                                }

                                break;
                        }

                        //Romper el ciclo
                        break;
                    }
                }

                //Asignar valores encriptados
                ob_datpro.SwtVenta = pr_datpro.SwtVenta;
                ob_datpro.EmailEli = ob_fncgrl.EncryptStringAES(Session.GetString("Usuario"));
                ob_datpro.NombreEli = ob_fncgrl.EncryptStringAES(Session.GetString("Nombre"));
                ob_datpro.KeyTeatro = ob_fncgrl.EncryptStringAES(Session.GetString("Teatro"));
                ob_datpro.DesTeatro = ob_fncgrl.EncryptStringAES(Session.GetString("TeatroNombre"));
                ob_datpro.TipoCompra = pr_datpro.TipoCompra;
                ob_datpro.ApellidoEli = ob_fncgrl.EncryptStringAES(Session.GetString("Apellido"));
                ob_datpro.TelefonoEli = ob_fncgrl.EncryptStringAES(Session.GetString("Telefono"));
                ob_datpro.KeySecuencia = pr_datpro.KeySecuencia;

                return ob_datpro;
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "FastSales/GetDetails";
                logSales.Metodo = "METHOD";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                ob_datpro.Codigo = -1;
                ob_datpro.Descripcion = lc_syserr.Message;

                return ob_datpro;
            }
        }

        /// <summary>
        /// Método para almacenar detalle de categorias en combos
        /// </summary>
        /// <param name="secuencia">id de venta</param>
        /// <param name="categoria">id categoria combo</param>
        /// <param name="valor1">id prodcuto categoria combo seleccionado</param>
        /// <param name="valor2">id prodcuto categoria combo seleccionado</param>
        /// <param name="valor3">id prodcuto categoria combo seleccionado</param>
        /// <param name="valor4">id prodcuto categoria combo seleccionado</param>
        private void RetailDet(decimal secuencia, int idRetail, decimal categoria, string valor1, string valor2, string valor3, string valor4)
        {
            //Recorrido para guardar valores en base de datos
            for (int lc_variii = 0; lc_variii < 3; lc_variii++)
            {
                switch (lc_variii)
                {
                    case 0:
                        if (valor1 != string.Empty && valor1 != null)
                        {
                            //Inicializar instancia de BD
                            using (var context = new DataDB(config))
                            {
                                //Agregar valores a tabla RetailDet
                                var retailDet = new RetailDet
                                {
                                    Secuencia = secuencia,
                                    IdRetailSales = idRetail,
                                    ProCategoria = categoria,
                                    ProItem = valor1
                                };

                                //Adicionar y guardar registro a tabla
                                context.RetailDet.Add(retailDet);
                                context.SaveChanges();
                            }
                        }
                        break;
                    case 1:
                        if (valor2 != string.Empty && valor2 != null)
                        {
                            //Inicializar instancia de BD
                            using (var context = new DataDB(config))
                            {
                                //Agregar valores a tabla RetailDet
                                var retailDet = new RetailDet
                                {
                                    Secuencia = secuencia,
                                    IdRetailSales = idRetail,
                                    ProCategoria = categoria,
                                    ProItem = valor2
                                };

                                //Adicionar y guardar registro a tabla
                                context.RetailDet.Add(retailDet);
                                context.SaveChanges();
                            }
                        }
                        break;
                    case 2:
                        if (valor3 != string.Empty && valor3 != null)
                        {
                            //Inicializar instancia de BD
                            using (var context = new DataDB(config))
                            {
                                //Agregar valores a tabla RetailDet
                                var retailDet = new RetailDet
                                {
                                    Secuencia = secuencia,
                                    IdRetailSales = idRetail,
                                    ProCategoria = categoria,
                                    ProItem = valor3
                                };

                                //Adicionar y guardar registro a tabla
                                context.RetailDet.Add(retailDet);
                                context.SaveChanges();
                            }
                        }
                        break;
                    case 3:
                        if (valor4 != string.Empty && valor4 != null)
                        {
                            //Inicializar instancia de BD
                            using (var context = new DataDB(config))
                            {
                                //Agregar valores a tabla RetailDet
                                var retailDet = new RetailDet
                                {
                                    Secuencia = secuencia,
                                    IdRetailSales = idRetail,
                                    ProCategoria = categoria,
                                    ProItem = valor4
                                };

                                //Adicionar y guardar registro a tabla
                                context.RetailDet.Add(retailDet);
                                context.SaveChanges();
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Método para obtener el tipo de sala desde el nombre de la película
        /// </summary>
        /// <param name="pr_nompel">Llave para obtener nombre</param>
        /// <returns></returns>
        private string TipPelicula(string pr_nompel)
        {
            #region VARIABLES LOCALES
            string lc_return = string.Empty;
            #endregion

            //Validar tipo sala GEN
            if (pr_nompel.Contains(" GEN "))
            {
                lc_return = "GENERAL";
                return lc_return;
            }

            //Validar tipo sala SNV
            if (pr_nompel.Contains(" SNV "))
            {
                lc_return = "SUPERNOVA";
                return lc_return;
            }

            //Validar tipo sala SK
            if (pr_nompel.Contains(" SK "))
            {
                lc_return = "STAR KIDS";
                return lc_return;
            }

            //Validar tipo sala 4DX
            if (pr_nompel.Contains(" 4DX "))
            {
                lc_return = "4DX";
                return lc_return;
            }

            //Validar tipo sala CA
            if (pr_nompel.Contains(" CA "))
            {
                lc_return = "CINE ARTE";
                return lc_return;
            }

            //Validar tipo sala AUT
            if (pr_nompel.Contains(" AUT "))
            {
                lc_return = "AUTOCINE";
                return lc_return;
            }

            //Validar tipo sala BS
            if (pr_nompel.Contains(" BS "))
            {
                lc_return = "BACK STAR";
                return lc_return;
            }

            //Devolver valor vacio
            return lc_return;
        }
        #endregion

        private List<DateCartelera> SalaPelicualFechaPortal(string pr_fecprg, string pr_tippel, string pr_keypel = "0")
        {

            DateTime dt_fechoy = DateTime.Now;
            var helper = new Helper();


            if (string.IsNullOrEmpty(pr_fecprg))
                pr_fecprg = dt_fechoy.ToString("yyyyMMdd");

            // Construir la URL completa con el valor de la sesión del teatro
            string url = config.Value.Variables41TP;

            XDocument xdoc = XDocument.Load(url.Replace("xxx", Session.GetString("Teatro"))
                                                   .Replace("yyy", (pr_keypel.Length >= 5 ? pr_keypel.Substring(0, pr_keypel.Length - 5) : pr_keypel)));

            var ob_fechas = (

                from pelicula in xdoc.Descendants("pelicula")
                where pelicula.Attribute("tipo")?.Value == pr_tippel && pelicula.Attribute("id")?.Value == pr_keypel
                from cinema in pelicula.Descendants("cinema")
                where cinema.Attribute("id")?.Value == Session.GetString("Teatro").ToString()
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

        public List<SelectListItem> DataPeliculas(String Pelicula, XmlDocument ob_xmldoc, List<hora> ob_horflg)
        {

            var peliculas = new List<SelectListItem>();
            string urlPeliculas = config.Value.Variables41T + Session.GetString("Teatro");
            ob_xmldoc.Load(urlPeliculas);
            XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");
            foreach (XmlElement item in pelicula)
            {
                //Validar si es cartelera
                if (item.GetAttribute("tipo").ToString() == "Normal" || item.GetAttribute("tipo").ToString() == "Preventa" || item.GetAttribute("tipo").ToString() == "Estreno")
                {

                    //Datos de nodo pelicula/data
                    XmlNodeList data = item.GetElementsByTagName("data");
                    foreach (XmlElement item2 in data)
                    {
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
                                    peliculas.Add(new SelectListItem() { Text = item.GetAttribute("nombre").ToString(), Value = item.GetAttribute("id").ToString() });

                                    // Datos de nodo pelicula / salas
                                    XmlNodeList salas = item4.GetElementsByTagName("salas");
                                    foreach (XmlElement itemS in salas)
                                    {
                                        // Datos de nodo pelicula / salas / sala
                                        XmlNodeList sala = itemS.GetElementsByTagName("sala");
                                        foreach (XmlElement itemSS in sala)
                                        {
                                            //Datos de nodo pelicula / salas / dia
                                            XmlNodeList Fechas = itemS.GetElementsByTagName("Fecha");
                                            foreach (XmlElement item5 in Fechas)
                                            {
                                                XmlNodeList Hora = item5.GetElementsByTagName("hora");
                                                foreach (XmlElement item6 in Hora)
                                                {
                                                    string dia = item5.GetAttribute("univ").ToString().Substring(6, 2);
                                                    string mes = item5.GetAttribute("univ").ToString().Substring(4, 2);
                                                    string ano = item5.GetAttribute("univ").ToString().Substring(0, 4);
                                                    DateTime fechayhora = Convert.ToDateTime(dia + "/" + mes + "/" + ano + " " + item6.GetAttribute("horario").ToString());
                                                    bool flag2 = ob_horflg.Any(x => x.fechayhora == fechayhora);
                                                    if (!flag2)
                                                        ob_horflg.Add(new hora { fechayhora = fechayhora });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                }
            }

            return peliculas;
        }
    }
}
