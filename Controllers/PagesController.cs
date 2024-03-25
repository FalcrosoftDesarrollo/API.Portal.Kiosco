/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : PagesController.cs                                                       *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase controlador que contiene los métodos para interactuar con las      *
*                páginas de la vista                                                      *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
*   Detalle Cambio: Refactorizacion código -> (Antoine Román - Falcrosoft) 02/01/2024     *
******************************************************************************************/
using APIPortalKiosco.Data;
using APIPortalKiosco.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;

namespace APIPortalKiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagesController : Controller
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
        public PagesController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        #region GET
        /// <summary>
        /// GET: RememberPwd -- Inicar vista de recordar clave
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("RecordarPwd")]
        public ActionResult RememberPwd()
        {
            try
            {
                URLPortal(config);
                ListCarrito();

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
                logSales.Programa = "Pages/RememberPwd";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// GET: Users -- Iniciar vista de formulario de registro
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("MostrarFormularioRegistro")]
        public ActionResult Users()
        {
            try
            {
                URLPortal(config);
                ListCarrito();

                //Inicializar variables
                ViewBag.AlertS = false;

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
                logSales.Programa = "Pages/Users";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// GET: TermConditions -- Proceso de aceptar terminos y condiciones del portal web
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("AceptarTerminosyCondiciones")]
        public ActionResult TermConditions(string pr_secsec, string pr_swtven, string pr_tiplog, string pr_cenprg = "")
        {
            try
            {
                //Validar producto seleccionado P
                if (pr_secsec == "0")
                    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un producto retail para continuar", pr_flag = "P" });

                URLPortal(config);
                ListCarrito();

                //Session para carrito de compras
                Session.Remove("pr_secsec");
                Session.SetString("pr_secsec", pr_secsec);
                Session.Remove("pr_swtven");
                Session.SetString("pr_swtven", pr_swtven);
                Session.Remove("pr_tiplog");
                Session.SetString("pr_tiplog", pr_tiplog);
                Session.Remove("pr_cenprg");
                Session.SetString("pr_cenprg", pr_cenprg);

                ViewBag.pr_secsec = pr_secsec;
                ViewBag.pr_swtven = pr_swtven;
                ViewBag.pr_tiplog = pr_tiplog;
                ViewBag.pr_cenprg = pr_cenprg;

                ViewBag.Correo = Session.GetString("Usuario");
                ViewBag.Nombre = Session.GetString("Nombre") + " " + Session.GetString("Apellido");
                ViewBag.Telefono = Session.GetString("Telefono");
                ViewBag.Documento = Session.GetString("Documento");

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
                logSales.Programa = "Pages/TermConditions";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// GET: Payment -- Proceso de pago del portal web
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ProcesoPago")]
        public ActionResult Payment(string pr_secsec, string pr_swtven, string pr_tiplog, string pr_cenprg = "", string pr_nomeli = "", string pr_doceli = "", string pr_coreli = "", string pr_teleli = "")
        {
            #region VARIABLES LOCALES
            var lc_idearr = 0;
            var lc_cntubi = 0;
            var lc_keytar = 0;
            var lc_keypel = 0;
            var lc_auxpel = 0;
            var KeyPelicula = 0;

            var lc_boltot = 0.00;
            var lc_valtar = 0.00;
            var lc_auxtar = string.Empty;
            var lc_despel = string.Empty;
            var lc_auxitm = string.Empty;
            var lc_ubiprg = string.Empty;
            var lc_result = string.Empty;
            var lc_srvpar = string.Empty;
            var lc_auxite = string.Empty;
            var fechaprg = string.Empty;
            var lc_ubilbl = "Ubicaciones: ";
            string[] ls_lstsel = new string[5];

            var KeySala = string.Empty;
            var EmailEli = string.Empty;
            var NombreHor = string.Empty;
            var NombreFec = string.Empty;
            var NombreEli = string.Empty;
            var NombreTar = string.Empty;
            var ApellidoEli = string.Empty;
            var TelefonoEli = string.Empty;
            var KeySecuencia = string.Empty;

            var lc_secsec = 0;

            var PuntoVenta = config.Value.PuntoVenta;
            var KeyTeatro = Session.GetString("Teatro");

            var ob_xmldoc = new XmlDocument();

            var ls_lstubi = new List<string>();
            var ob_ordite = new List<OrderItem>();
            var ob_ubiprg = new List<Ubicaciones>();
            var ob_return = new List<Producto>();
            var ob_diclst = new Dictionary<string, object>();

            var ob_servicio = new CinefansSRV();
            var ob_cfinicio = new CinefansINI();
            var ob_scopre = new Secuencia();
            var ob_datpro = new Producto();
            var ob_fncgrl = new General();
            #endregion

            try
            {
                if (pr_cenprg == null)
                    pr_cenprg = "";

                //Session para carrito de compras
                Session.Remove("pr_secsec");
                Session.SetString("pr_secsec", pr_secsec);
                Session.Remove("pr_swtven");
                Session.SetString("pr_swtven", pr_swtven);
                Session.Remove("pr_tiplog");
                Session.SetString("pr_tiplog", pr_tiplog);
                Session.Remove("pr_cenprg");
                Session.SetString("pr_cenprg", pr_cenprg);

                //Validar si es invitado
                if (pr_nomeli != "")
                {
                    Session.Remove("Usuario");
                    Session.SetString("Usuario", pr_coreli);

                    Session.Remove("Nombre");
                    Session.SetString("Nombre", pr_nomeli);

                    Session.Remove("Documento");
                    Session.SetString("Documento", pr_doceli);

                    Session.Remove("Telefono");
                    Session.SetString("Telefono", pr_teleli);
                }

                URLPortal(config);
                ListCarrito();

                //Asignar tipo de compra validada
                pr_tiplog = ViewBag.TipoV;

                //Inicilaizar valores
                ViewBag.Sala = null;
                ViewBag.Imagen = null;
                ViewBag.HoraNom = null;
                ViewBag.Formato = null;
                ViewBag.Duracion = null;
                ViewBag.FechaNom = null;
                ViewBag.Pelicula = null;
                ViewBag.TarfaNom = null;
                ViewBag.TarfaVal = null;
                ViewBag.NombreEli = null;
                ViewBag.ApellidoEli = null;
                ViewBag.TelefonoEli = null;
                ViewBag.ProductosRetail = null;
                ViewBag.TeatroNom = Session.GetString("TeatroNombre");
                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente");

                ViewBag.AlertS = false;
                ViewBag.TipoVenta = pr_tiplog;
                ViewBag.Acumulado = "0";
                ViewBag.valorCashback = 0;

                EmailEli = Session.GetString("Usuario");
                NombreEli = Session.GetString("Nombre");
                ApellidoEli = Session.GetString("Apellido");
                TelefonoEli = Session.GetString("Telefono");

                if (Session.GetString("Secuencia") != null)
                    KeySecuencia = Session.GetString("Secuencia");
                else
                    KeySecuencia = pr_secsec;

                //Inicializar variables
                lc_secsec = Convert.ToInt32(KeySecuencia);
                switch (pr_tiplog)
                {
                    case "M":
                        //Obtener productos carrito de compra
                        using (var context = new DataDB(config))
                        {


                            var rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).Where(x => x.Tipo == "C").ToList();
                            List<RetailSales> retailsales = rs
                                .GroupBy(l => l.KeyProducto)
                                .Select(cl => new RetailSales
                                {
                                    Descripcion = cl.First().Descripcion,
                                    Cantidad = cl.Sum(c => c.Cantidad),
                                    Precio = cl.Sum(c => c.Precio)
                                }).ToList();

                            foreach (var vr_itevta in retailsales)
                            {
                                //Adicionar a lista
                                ob_ordite.Add(new OrderItem
                                {
                                    Precio = vr_itevta.Precio,
                                    Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                    Descripcion = vr_itevta.Descripcion
                                });
                            }

                            rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).Where(x => x.Tipo != "C").ToList();

                            foreach (var vr_itevta in rs)
                            {
                                //Adicionar a lista
                                ob_ordite.Add(new OrderItem
                                {
                                    Precio = vr_itevta.Precio * Convert.ToInt32(vr_itevta.Cantidad),
                                    Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                    Descripcion = vr_itevta.Descripcion
                                });
                            }

                            ViewBag.ProductosRetail = ob_ordite;
                        }

                        //Obtener boletas carrito de compra
                        using (var context = new DataDB(config))
                        {
                            //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia

                            var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                            foreach (var vr_itevta in ReportSales)
                            {
                                KeySala = vr_itevta.KeySala;
                                lc_boltot = vr_itevta.Precio;
                                lc_ubiprg = vr_itevta.SelUbicaciones;
                                lc_keytar = Convert.ToInt32(vr_itevta.KeyTarifa);
                                lc_despel = vr_itevta.NombrePel;
                                NombreHor = vr_itevta.NombreHor;
                                NombreFec = vr_itevta.NombreFec;
                                NombreTar = vr_itevta.NombreTar;
                                KeyPelicula = Convert.ToInt32(vr_itevta.KeyPelicula);
                                pr_cenprg = vr_itevta.Transaccion;
                                fechaprg = vr_itevta.FecProg;
                            }

                            //Asignar Valores
                            ViewBag.HoraNom = NombreHor;
                            ViewBag.FechaNom = NombreFec;
                            ViewBag.Pelicula = lc_despel;

                            ViewBag.NombreEli = Session.GetString("Nombre");
                            ViewBag.ApellidoEli = Session.GetString("Apellido");
                            ViewBag.TelefonoEli = Session.GetString("Telfono");

                            //Obtener ubicaciones de vista
                            char[] ar_charst = lc_ubiprg.ToCharArray();
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
                                lc_ubilbl += string.Concat("Fila: ", ls_lstsel[1], " Columna: ", ls_lstsel[2], ";");
                                ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = lc_keytar, FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                            }

                            ViewBag.Ubicaciones = lc_cntubi;

                            lc_auxtar = NombreTar;
                            ViewBag.TarifaNom = lc_auxtar.Substring(0, lc_auxtar.IndexOf(";"));

                            lc_valtar = Convert.ToDouble((lc_auxtar.Substring(lc_auxtar.IndexOf(";") + 1, lc_auxtar.Length - (lc_auxtar.IndexOf(";") + 1))));
                            ViewBag.TarifaVal = Convert.ToString(lc_valtar * lc_cntubi);

                            //Obtener información de la web
                            //Obtener información de la web
                            string apiUrl = config.Value.Variables41TPF;

                            // Construir la URL final con los valores reemplazados
                            string urlFinal = apiUrl.Replace("xxx", Session.GetString("Teatro"))
                                                   .Replace("yyy", KeyPelicula.ToString())
                                                   .Replace("zzz", fechaprg);

                            ob_xmldoc.Load(urlFinal);

                            // Obtener la película específica del XML usando LINQ to XML
                            var peliculaSeleccionada = ob_xmldoc.GetElementsByTagName("pelicula")
                                .Cast<XmlElement>()
                                .FirstOrDefault(item => Convert.ToInt32(item.GetAttribute("id")) == KeyPelicula);

                            if (peliculaSeleccionada != null)
                            {
                                // Obtener datos del nodo "data"
                                var data = peliculaSeleccionada.GetElementsByTagName("data").Cast<XmlElement>().FirstOrDefault();
                                if (data != null)
                                {
                                    ViewBag.Imagen = data.GetAttribute("Imagen");
                                    ViewBag.Duracion = data.GetAttribute("duracion");
                                    ViewBag.Formato = data.GetAttribute("formato");
                                    // ViewBag.Censura = data.GetAttribute("censura");
                                }

                                // Obtener información sobre la sala usando LINQ to XML
                                var cinema = peliculaSeleccionada.GetElementsByTagName("cinema")
                                    .Cast<XmlElement>()
                                    .FirstOrDefault(item => Convert.ToInt32(item.GetAttribute("id")) == Convert.ToInt32(Session.GetString("Teatro")));

                                if (cinema != null)
                                {
                                    var salas = cinema.GetElementsByTagName("salas").Cast<XmlElement>().FirstOrDefault();
                                    var sala = salas?.GetElementsByTagName("sala")
                                        .Cast<XmlElement>()
                                        .FirstOrDefault(item => item.GetAttribute("numeroSala") == KeySala);

                                    if (sala != null)
                                    {
                                        ViewBag.Sala = $"{KeySala}-{sala.GetAttribute("tipoSala")}";
                                    }
                                }
                            }
                        }

                        ob_datpro.SwtVenta = pr_swtven;
                        ob_datpro.TipoCompra = pr_tiplog;
                        ob_datpro.KeySecuencia = pr_secsec;

                        break;
                    case "B":
                        //Obtener boletas carrito de compra
                        using (var context = new DataDB(config))
                        {
                            //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia

                            var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                            foreach (var vr_itevta in ReportSales)
                            {
                                KeySala = vr_itevta.KeySala;
                                lc_boltot = vr_itevta.Precio;
                                lc_ubiprg = vr_itevta.SelUbicaciones;
                                lc_keytar = Convert.ToInt32(vr_itevta.KeyTarifa);
                                lc_despel = vr_itevta.NombrePel;
                                NombreHor = vr_itevta.NombreHor;
                                NombreFec = vr_itevta.NombreFec;
                                NombreTar = vr_itevta.NombreTar;
                                KeyPelicula = Convert.ToInt32(vr_itevta.KeyPelicula);
                                pr_cenprg = vr_itevta.Transaccion;
                                fechaprg = vr_itevta.FecProg;
                            }

                            //Asignar Valores
                            ViewBag.HoraNom = NombreHor;
                            ViewBag.FechaNom = NombreFec;
                            ViewBag.Pelicula = lc_despel;

                            ViewBag.NombreEli = Session.GetString("Nombre");
                            ViewBag.ApellidoEli = Session.GetString("Apellido");
                            ViewBag.TelefonoEli = Session.GetString("Telfono");

                            //Obtener ubicaciones de vista
                            char[] ar_charst = lc_ubiprg.ToCharArray();
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
                                lc_ubilbl += string.Concat("Fila: ", ls_lstsel[1], " Columna: ", ls_lstsel[2], ";");
                                ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = lc_keytar, FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                            }

                            ViewBag.Ubicaciones = lc_cntubi;

                            lc_auxtar = NombreTar;
                            ViewBag.TarifaNom = lc_auxtar.Substring(0, lc_auxtar.IndexOf(";"));

                            lc_valtar = Convert.ToDouble(lc_auxtar.Substring(lc_auxtar.IndexOf(";") + 1, lc_auxtar.Length - (lc_auxtar.IndexOf(";") + 1)));
                            ViewBag.TarifaVal = Convert.ToString(lc_valtar * lc_cntubi);

                            //Obtener información de la web
                            string apiUrl = config.Value.Variables41TPF;

                            // Construir la URL final con los valores reemplazados
                            string urlFinal = apiUrl.Replace("xxx", Session.GetString("Teatro"))
                                                   .Replace("yyy", KeyPelicula.ToString())
                                                   .Replace("zzz", fechaprg);

                            ob_xmldoc.Load(urlFinal);

                            // Obtener la película específica del XML usando LINQ to XML
                            var peliculaSeleccionada = ob_xmldoc.GetElementsByTagName("pelicula")
                                .Cast<XmlElement>()
                                .FirstOrDefault(item => Convert.ToInt32(item.GetAttribute("id")) == KeyPelicula);

                            if (peliculaSeleccionada != null)
                            {
                                // Obtener datos del nodo "data"
                                var data = peliculaSeleccionada.GetElementsByTagName("data").Cast<XmlElement>().FirstOrDefault();
                                if (data != null)
                                {
                                    ViewBag.Imagen = data.GetAttribute("Imagen");
                                    ViewBag.Duracion = data.GetAttribute("duracion");
                                    ViewBag.Formato = data.GetAttribute("formato");
                                    // ViewBag.Censura = data.GetAttribute("censura");
                                }

                                // Obtener información sobre la sala usando LINQ to XML
                                var cinema = peliculaSeleccionada.GetElementsByTagName("cinema")
                                    .Cast<XmlElement>()
                                    .FirstOrDefault(item => Convert.ToInt32(item.GetAttribute("id")) == Convert.ToInt32(Session.GetString("Teatro")));

                                if (cinema != null)
                                {
                                    var salas = cinema.GetElementsByTagName("salas").Cast<XmlElement>().FirstOrDefault();
                                    var sala = salas?.GetElementsByTagName("sala")
                                        .Cast<XmlElement>()
                                        .FirstOrDefault(item => item.GetAttribute("numeroSala") == KeySala);

                                    if (sala != null)
                                    {
                                        ViewBag.Sala = $"{KeySala}-{sala.GetAttribute("tipoSala")}";
                                    }
                                }
                            }
                        }

                        ob_datpro.SwtVenta = pr_swtven;
                        ob_datpro.TipoCompra = pr_tiplog;
                        ob_datpro.KeySecuencia = pr_secsec;

                        break;
                    case "P":
                        //Obtener productos carrito de compra
                        using (var context = new DataDB(config))
                        {
                            //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia


                            var rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).Where(x => x.Tipo == "C").ToList();
                            List<RetailSales> retailsales = rs
                                .GroupBy(l => l.KeyProducto)
                                .Select(cl => new RetailSales
                                {
                                    Descripcion = cl.First().Descripcion,
                                    Cantidad = cl.Sum(c => c.Cantidad),
                                    Precio = cl.Sum(c => c.Precio)
                                }).ToList();

                            foreach (var vr_itevta in retailsales)
                            {
                                //Adicionar a lista
                                ob_ordite.Add(new OrderItem
                                {
                                    Precio = vr_itevta.Precio,
                                    Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                    Descripcion = vr_itevta.Descripcion
                                });
                            }

                            rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).Where(x => x.Tipo != "C").ToList();

                            foreach (var vr_itevta in rs)
                            {
                                //Adicionar a lista
                                ob_ordite.Add(new OrderItem
                                {
                                    Precio = vr_itevta.Precio * Convert.ToInt32(vr_itevta.Cantidad),
                                    Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                    Descripcion = vr_itevta.Descripcion
                                });
                            }

                            ViewBag.ProductosRetail = ob_ordite;
                        }

                        ob_datpro.SwtVenta = pr_swtven;
                        ob_datpro.TipoCompra = pr_tiplog;
                        ob_datpro.KeySecuencia = pr_secsec;
                        break;
                }

                if (Session.GetString("ClienteFrecuente") == "Si")
                {
                    #region SCOHIS
                    //Asignar Valores
                    ob_servicio.Clave = Session.GetString("Passwrd");
                    ob_servicio.Correo = Session.GetString("Usuario");
                    ob_servicio.Fecha1 = Convert.ToString(DateTime.Now.Year - 1) + "0101";
                    ob_servicio.Fecha2 = Convert.ToString(DateTime.Now.Year + 1) + "1231";
                    ob_servicio.tercero = config.Value.ValorTercero;


                    // Generar y encriptar JSON para servicio
                    lc_srvpar = ob_fncgrl.JsonConverter(ob_servicio);
                    lc_srvpar = lc_srvpar.Replace("correo", "Correo");
                    lc_srvpar = lc_srvpar.Replace("clave", "Clave");
                    lc_srvpar = lc_srvpar.Replace("fecha1", "Fecha1");
                    lc_srvpar = lc_srvpar.Replace("fecha2", "Fecha2");

                    //Encriptar Json
                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                    //Consumir servicio
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scohis/"), lc_srvpar);

                    //Generar Log
                    LogSales logSales = new LogSales();
                    LogAudit logAudit = new LogAudit(config);
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "Pages/Payment";
                    logSales.Metodo = "SCOHIS";
                    logSales.ExceptionMessage = lc_srvpar;
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    //logAudit.LogApp(logSales);

                    //Validar respuesta
                    if (lc_result.Substring(0, 1) == "0")
                    {
                        if (!lc_result.Contains("Validación"))
                        {
                            //Quitar switch
                            lc_result = lc_result.Replace("0-", "");
                            ob_cfinicio = (CinefansINI)JsonConvert.DeserializeObject(lc_result, (typeof(CinefansINI))); //Deserializar Json y validar respuesta
                            foreach (var item in ob_cfinicio.Saldo)
                            {
                                ViewBag.Acumulado = String.Format("{0:C0}", Convert.ToInt32(item.Saldo));
                                ViewBag.valorCashback = item.Saldo;
                            }

                        }
                        else
                        {
                            ViewBag.Acumulado = "0";
                        }
                    }
                    else
                    {
                        lc_result = lc_result.Replace("1-", "");

                        //Generar Log
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Payment";
                        logSales.Metodo = "SCOHIS_R";
                        logSales.ExceptionMessage = lc_result;
                        logSales.InnerExceptionMessage = "null";

                        //Escribir Log
                        logAudit.LogApp(logSales);

                        //Devolver vista de error
                        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                    }
                    #endregion
                }

                ViewBag.Censura = pr_cenprg;
                ob_datpro.SwitchCashback = "N";

                //Devolver a vista
                return View(ob_datpro);
            }
            catch (Exception lc_syserr)
            {
                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = lc_syserr.Message });
            }
        }

        /// <summary>
        /// GET: Finish -- Proceso de pago del portal web con operador de pago
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ProcesoPagoConOperadorPago")]
        public ActionResult Finish(string pr_secpro, string pr_swtven, decimal pr_valven = 0, decimal pr_valimp = 0, decimal pr_valiva = 0, decimal pr_valbas = 0, decimal pr_casbck = 0)
        {
            #region VARIABLES LOCALES
            string lc_auxtel = string.Empty;
            string lc_auxtar = string.Empty;

            //Inicializar valores
            ViewBag.AlertP = true;
            ViewBag.AlertS = false;
            ViewBag.Status = string.Empty;
            ViewBag.Detail = string.Empty;
            ViewBag.CarteleraWP = string.Empty;

            ViewBag.HoraNom = null;
            ViewBag.FechaNom = null;
            ViewBag.Pelicula = null;
            ViewBag.TarfaNom = null;
            ViewBag.TarfaVal = null;
            ViewBag.NombreEli = null;
            ViewBag.ApellidoEli = null;
            ViewBag.TelefonoEli = null;

            ViewBag.ListCarritoB = null;
            ViewBag.ListCarritoR = null;
            ViewBag.PuntoVenta = config.Value.PuntoVenta;

            XmlDocument ob_xmldoc = new XmlDocument();
            Payment ob_datpay = new Payment();

            List<BolVenta> ob_lisprg = new List<BolVenta>();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);

                //Asignar valores url
                ob_datpay.SwtVenta = pr_swtven;
                ob_datpay.EmailEli = Session.GetString("Usuario");
                ob_datpay.NombreEli = Session.GetString("Nombre");
                ob_datpay.KeyTeatro = Session.GetString("Teatro");
                ob_datpay.DesTeatro = Session.GetString("TeatroNombre");
                ob_datpay.ApellidoEli = Session.GetString("Apellido");
                ob_datpay.TelefonoEli = Session.GetString("Telefono");
                ob_datpay.KeySecuencia = pr_secpro;

                ob_datpay.src_epayco = config.Value.src_epayco;
                ob_datpay.class_epayco = config.Value.class_epayco;
                ob_datpay.data_epayco_test = config.Value.data_epayco_test;
                ob_datpay.data_epayco_country = config.Value.data_epayco_country;
                ob_datpay.data_epayco_currency = config.Value.data_epayco_currency;
                ob_datpay.data_epayco_external = config.Value.data_epayco_external;
                ob_datpay.data_epayco_response = config.Value.data_epayco_response;
                ob_datpay.data_epayco_confirmation = config.Value.data_epayco_confirmation;

                ob_datpay.data_epayco_name = "Venta Internet - " + ob_datpay.KeySecuencia.ToString();

                ob_datpay.data_epayco_tax = pr_valiva.ToString();
                ob_datpay.data_epayco_tax = ob_datpay.data_epayco_tax.Replace(",", ".");
                ob_datpay.data_epayco_tax_ico = pr_valimp.ToString();
                ob_datpay.data_epayco_tax_ico = ob_datpay.data_epayco_tax_ico.Replace(",", ".");
                ob_datpay.data_epayco_amount = pr_valven.ToString();
                ob_datpay.data_epayco_amount = ob_datpay.data_epayco_amount.Replace(",", ".");
                ob_datpay.data_epayco_tax_base = pr_valbas.ToString();
                ob_datpay.data_epayco_tax_base = ob_datpay.data_epayco_tax_base.Replace(",", ".");

                ob_datpay.data_epayco_description = ob_datpay.data_epayco_name;

                //Validar teatro IMPORTANTE FALTA COLOCAR LOS OTROS TEATROS
                if (ob_datpay.KeyTeatro == "302" || ob_datpay.KeyTeatro == "304" || ob_datpay.KeyTeatro == "305" ||
                    ob_datpay.KeyTeatro == "306" || ob_datpay.KeyTeatro == "323" || ob_datpay.KeyTeatro == "327")
                {
                    ob_datpay.data_epayco_key = config.Value.data_epayco_key_pro;
                }
                else
                {
                    ob_datpay.data_epayco_key = config.Value.data_epayco_key_col;
                }

                //Cargar Casback
                Session.SetString("CashBack_Acumulado", pr_casbck.ToString());

                //devolver a vista
                return View(ob_datpay);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Finish";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// GET: Responses -- Respuesta pago por internet Epayco con operador de pago
        /// </summary>
        /// 
        [HttpGet]
        [Route("RespuestaPagoEpaycoConOperadorPago")]
        public ActionResult Responses(string ref_payco = "")
        {
            #region VARIABLES LOCALES
            string lc_fectra = string.Empty;
            string lc_valtra = string.Empty;
            string lc_idsepy = string.Empty;
            string lc_status = string.Empty;
            string lc_coreli = string.Empty;
            string lc_jsnrst = string.Empty;
            string lc_objson = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_refepy = string.Empty;
            string lc_bankpy = string.Empty;
            string lc_urlcor = config.Value.UrlCorreo;

            decimal lc_secsec = 0;
            decimal lc_keytea = 0;
            decimal lc_puntea = Convert.ToDecimal(config.Value.PuntoVenta);

            List<OrderItem> ob_ordite = new List<OrderItem>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            TransactionSales ob_repsle = new TransactionSales();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //Validar si esta la sesion activa
                if (Session.GetString("ClienteFrecuente") != null)
                {
                    URLPortal(config);
                    ListCarrito();

                    ViewBag.ClienteFrecuente = Session.GetString("ClienteFrecuente");
                    ViewBag.CashBack_Acumulado = String.Format("{0:C0}", Convert.ToDecimal(Session.GetString("CashBack_Acumulado")));
                }

                //Inicializar instancia web client para leer respuesta
                using (WebClient wc = new WebClient())
                {
                    //Obtener información de epayco
                    var ob_json = wc.DownloadString(config.Value.data_epayco_secure + ref_payco);
                    var ob_response = JsonConvert.DeserializeObject<EpaycoApiGet>(ob_json);

                    //validar rta y Obtener y deserializar respuesta
                    if (!ref_payco.Contains("CashBack"))
                    {
                        //Obtener valores de rta Epayco y consultar registro en la bd
                        lc_secsec = Convert.ToDecimal(ob_response.data.x_extra1);
                        lc_keytea = Convert.ToDecimal(ob_response.data.x_extra2);
                        lc_coreli = ob_response.data.x_customer_email.ToString();
                        lc_valtra = ob_response.data.x_amount.ToString();
                        lc_status = ob_response.data.x_response.ToString();
                        lc_idsepy = ob_response.data.x_transaction_id.ToString();
                        lc_fectra = ob_response.data.x_fecha_transaccion.ToString();
                        lc_refepy = ob_response.data.x_ref_payco.ToString();
                        lc_bankpy = ob_response.data.x_bank_name.ToString();
                    }
                    else
                    {
                        //Obtener valores de rta cashback y consultar registro en la bd
                        lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                        lc_keytea = Convert.ToDecimal(Session.GetString("Teatro"));
                        lc_coreli = Session.GetString("Usuario");
                        lc_valtra = ref_payco.Substring(ref_payco.IndexOf(":") + 1);
                        lc_status = "Cashback";
                        lc_idsepy = "Cashback:SEC-" + Session.GetString("Secuencia");
                        lc_fectra = DateTime.Now.ToString();
                        lc_refepy = config.Value.PuntoVenta + "-" + Session.GetString("Secuencia");
                        lc_bankpy = "CashBack Procinal";
                    }

                    //Inicializar instancia de BD
                    using (var context = new DataDB(config))
                    {
                        //Consultar registro de venta en BD transacciones
                        var ob_repsl1 = context.TransactionSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == lc_puntea).Where(x => x.Teatro == lc_keytea);
                        foreach (var TransactionSales in ob_repsl1)
                            ob_repsle = context.TransactionSales.Find(TransactionSales.Id);

                        //Inicializar valores
                        switch (lc_status)
                        {
                            case "Cashback":
                                ob_repsle.EstadoTx = "CBK";
                                ob_repsle.Observaciones = "VENTA SCORE/PROCINAL";

                                ViewBag.Status = "success";
                                break;

                            case "Aceptada":
                                ob_repsle.EstadoTx = "EPY";
                                ob_repsle.Observaciones = "VENTA SCORE/EPAYCO";

                                ViewBag.Status = "success";
                                break;

                            case "Rechazada":
                            case "Fallida":
                                ob_repsle.EstadoTx = "REX";
                                ob_repsle.Observaciones = "VENTA RECHAZADA EPAYCO";

                                ViewBag.Status = "failure";
                                break;

                            case "Pendiente":
                                ob_repsle.EstadoTx = "EPX";
                                ob_repsle.Observaciones = "VENTA PENDIENTE EPAYCO";

                                ViewBag.Status = "pending";
                                break;
                        }

                        ob_repsle.FechaTx = Convert.ToDateTime(lc_fectra);
                        ob_repsle.AutorizacionTx = string.Concat(lc_idsepy, ",", lc_status);
                        ob_repsle.ReferenciaTx = lc_refepy;
                        ob_repsle.ReferenciaEx = ref_payco;
                        ob_repsle.BancoTx = lc_bankpy;
                        ob_repsle.FechaModificado = DateTime.Now;

                        //Validar si la sesion esta activa
                        if (Session.GetString("Usuario") != null)
                        {
                            ob_repsle.EmailEli = Session.GetString("Usuario");
                            ob_repsle.NombreEli = Session.GetString("Nombre") + " " + Session.GetString("Apellido");
                            ob_repsle.TelefonoEli = Session.GetString("Telefono");
                            ob_repsle.DocumentoEli = Session.GetString("Documento");
                        }

                        //Actualizar estado de transacción
                        context.TransactionSales.Update(ob_repsle);
                        context.SaveChanges();

                        ViewBag.Teatro = lc_keytea.ToString();
                        ViewBag.PtoVenta = lc_puntea.ToString();
                        ViewBag.Secuencia = lc_secsec.ToString();
                    }
                }

                ViewBag.CarteleraWP = config.Value.CarteleraWP;

                //Adicionar valores de envio de correo Score
                lc_urlcor = lc_urlcor.Replace("#xxx", lc_keytea.ToString());
                lc_urlcor = lc_urlcor.Replace("#yyy", config.Value.PuntoVenta);
                lc_urlcor = lc_urlcor.Replace("#zzz", lc_secsec.ToString());
                lc_urlcor = lc_urlcor.Replace("#ccc", lc_coreli);

                //Estado Exitoso
                if (lc_status == "Aceptada" || lc_status == "Cashback")
                {
                    //Obtener resumen de compra
                    ViewBag.ListB = ViewBag.ListCarritoB;

                    using (var context = new DataDB(config))
                    {
                        var rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == lc_puntea).Where(x => x.KeyTeatro == lc_keytea).Where(x => x.Tipo == "C").ToList();
                        List<RetailSales> retailsales = rs
                            .GroupBy(l => l.KeyProducto)
                            .Select(cl => new RetailSales
                            {
                                Descripcion = cl.First().Descripcion,
                                Cantidad = cl.Sum(c => c.Cantidad),
                                Precio = cl.Sum(c => c.Precio)
                            }).ToList();

                        foreach (var vr_itevta in retailsales)
                        {
                            //Adicionar a lista
                            ob_ordite.Add(new OrderItem
                            {
                                Precio = vr_itevta.Precio,
                                Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                Descripcion = vr_itevta.Descripcion
                            });
                        }

                        rs = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == lc_puntea).Where(x => x.KeyTeatro == lc_keytea).Where(x => x.Tipo != "C").ToList();

                        foreach (var vr_itevta in rs)
                        {
                            //Adicionar a lista
                            ob_ordite.Add(new OrderItem
                            {
                                Precio = vr_itevta.Precio * Convert.ToInt32(vr_itevta.Cantidad),
                                Cantidad = Convert.ToInt32(vr_itevta.Cantidad),
                                Descripcion = vr_itevta.Descripcion
                            });
                        }

                        ViewBag.ListR = ob_ordite; //ViewBag.ListCarritoR;
                    }

                    ViewBag.ListCarritoB = null;
                    ViewBag.ListCarritoR = null;

                    if (Session.GetString("Secuencia") != null)
                    {
                        try
                        {
                            //Envio de correo Score
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Envío de correo compra APROBADA: Exitoso";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                        }
                        catch (Exception)
                        {
                            ViewBag.EnvioCorreo = "Fallo envío de correo compra APROBADA, por favor comunicarse con el teatro.";

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Fallo envío de correo compra APROBADA, por favor comunicarse con el teatro.";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            logAudit.LogApp(logSales);
                        }
                    }
                }

                //Estado Fallido
                //Validar venta
                if (lc_status == "Rechazada" || lc_status == "Fallida")
                {
                    if (Session.GetString("Secuencia") != null)
                    {
                        #region SERVICO SCORET
                        //Json de servicio RET
                        lc_objson = "{\"Punto\":" + Convert.ToInt32(config.Value.PuntoVenta) + ",\"Pedido\":" + Convert.ToInt32(lc_secsec) + ",\"teatro\":\"" + Convert.ToInt32(lc_keytea) + "\",\"tercero\":\"" + config.Value.ValorTercero + "\"}";

                        //Encriptar Json RET
                        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_objson);

                        //Consumir servicio RET
                        lc_jsnrst = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoret/"), lc_srvpar);

                        //Generar Log
                        LogSales logSales = new LogSales();
                        LogAudit logAudit = new LogAudit(config);
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Responses";
                        logSales.Metodo = "SCORET";
                        logSales.ExceptionMessage = lc_srvpar;
                        logSales.InnerExceptionMessage = lc_jsnrst;

                        //Escribir Log
                        //logAudit.LogApp(logSales);

                        //Validar respuesta
                        if (lc_jsnrst.Substring(0, 1) == "0")
                        {
                            //Quitar switch
                            lc_jsnrst = lc_jsnrst.Replace("0-", "");
                            lc_jsnrst = lc_jsnrst.Replace("[", "");
                            lc_jsnrst = lc_jsnrst.Replace("]", "");

                            //Deserializar Json y validar respuesta SEC
                            ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_jsnrst, (typeof(Dictionary<string, string>)));

                            //Validar respuesta llave 1
                            if (ob_diclst.ContainsKey("Validación"))
                            {
                                ModelState.AddModelError("", ob_diclst["Validación"].ToString());
                                return View();
                            }
                            else
                            {
                                //Validar respuesta llave 2
                                if (ob_diclst.ContainsKey("Respuesta"))
                                {
                                    if (ob_diclst["Respuesta"].ToString() != "Proceso exitoso")
                                    {
                                        ModelState.AddModelError("", ob_diclst["Respuesta"].ToString());
                                        return View();
                                    }
                                }
                            }

                            ob_diclst.Clear();
                        }
                        else
                        {
                            ModelState.AddModelError("", "Reembolso no culminado con exito SCORET");
                            return View();
                        }
                        #endregion

                        try
                        {
                            //Envio de correo Score
                            lc_urlcor = lc_urlcor.Replace("compra", "Fallida");
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            //Generar Log
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Envío de correo compra RECHAZADA: Exitoso";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                        }
                        catch (Exception)
                        {
                            ViewBag.EnvioCorreo = "Fallo envío de correo compra RECHAZADA, por favor comunicarse con el teatro.";

                            //Generar Log
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Fallo envío de correo compra RECHAZADA, por favor comunicarse con el teatro.";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            logAudit.LogApp(logSales);
                        }
                    }

                    ViewBag.ListB = null;
                    ViewBag.ListR = null;
                }

                //Estado Pendiente
                //Validar venta
                if (lc_status == "Pendiente")
                {
                    ViewBag.ListB = null;
                    ViewBag.ListR = null;

                    if (Session.GetString("Secuencia") != null)
                    {
                        try
                        {
                            //Envio de correo Score
                            lc_urlcor = lc_urlcor.Replace("compra", "Pendiente");
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Envío de correo compra PENDIENTE: Exitoso";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                        }
                        catch (Exception)
                        {
                            ViewBag.EnvioCorreo = "Fallo envío de correo compra PENDIENTE, por favor comunicarse con el teatro.";

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Responses";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Fallo envío de correo compra PENDIENTE, por favor comunicarse con el teatro.";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            logAudit.LogApp(logSales);
                        }
                    }
                }

                //Validar y remover sesion invitada
                if (Session.GetString("FlagLogin") == "INV")
                {
                    Session.Remove("Nombre");
                    Session.Remove("Passwrd");
                    Session.Remove("Usuario");
                    Session.Remove("Apellido");
                    Session.Remove("Telefono");
                    Session.Remove("Direccion");
                    Session.Remove("Documento");
                    Session.Remove("ClienteFrecuente");
                    Session.Remove("FlagLogin");
                    ViewBag.ListCarritoR = null;
                    ViewBag.ListCarritoB = null;
                }

                //Quitar secuencia
                Session.Remove("Secuencia");
                return View();
            }
            catch (Exception lc_syserr)
            {
                #region SERVICO SCORET
                //Json de servicio RET
                //lc_objson = "{\"Punto\":" + Convert.ToInt32(config.Value.PuntoVenta) + ",\"Pedido\":" + Convert.ToInt32(lc_secsec) + ",\"teatro\":\"" + Convert.ToInt32(lc_keytea) + "\",\"tercero\":\"" + config.Value.ValorTercero + "\"}";

                //Encriptar Json RET
                //lc_srvpar = ob_fncgrl.EncryptStringAES(lc_objson);

                //Consumir servicio RET
                //lc_jsnrst = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoret/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Responses";
                logSales.Metodo = "SCORET_CATCH";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_jsnrst;

                //Escribir Log
                //logAudit.LogApp(logSales);
                #endregion

                //Generar Log
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Responses";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar si esta la sesion activa y Devolver vista de error
                if (Session.GetString("ClienteFrecuente") != null)
                    return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "RSPNS" });
                else
                    return RedirectToAction("Home", "Home");
            }
        }

        /// <summary>
        /// GET: Confirmation -- Respuesta pago por internet Epayco con operador de pago TX prendiente
        /// </summary>
        /// 
        [HttpGet]
        [Route("RespuestaPagoEpaycoConOperadorPagoTX")]
        public ActionResult Confirmation(string ref_payco = "")
        {
            #region VARIABLES LOCALES
            string lc_fectra = string.Empty;
            string lc_valtra = string.Empty;
            string lc_idsepy = string.Empty;
            string lc_status = string.Empty;
            string lc_coreli = string.Empty;
            string lc_jsnrst = string.Empty;
            string lc_objson = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_refepy = string.Empty;
            string lc_bankpy = string.Empty;
            string lc_urlcor = config.Value.UrlCorreo;

            decimal lc_secsec = 0;
            decimal lc_keytea = 0;
            decimal lc_puntea = Convert.ToDecimal(config.Value.PuntoVenta);

            List<OrderItem> ob_ordite = new List<OrderItem>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            TransactionSales ob_repsle = new TransactionSales();
            General ob_fncgrl = new General();
            #endregion

            if (ref_payco != "")
            {
                try
                {
                    //Inicializar instancia web client para leer respuesta
                    using (WebClient wc = new WebClient())
                    {
                        //Obtener información de epayco
                        var ob_json = wc.DownloadString(config.Value.data_epayco_secure + ref_payco);
                        var ob_response = JsonConvert.DeserializeObject<EpaycoApiGet>(ob_json);

                        //Obtener valores de rta Epayco y consultar registro en la bd
                        lc_secsec = Convert.ToDecimal(ob_response.data.x_extra1);
                        lc_keytea = Convert.ToDecimal(ob_response.data.x_extra2);
                        lc_coreli = ob_response.data.x_extra3.ToString();
                        lc_valtra = ob_response.data.x_amount.ToString();
                        lc_status = ob_response.data.x_response.ToString();
                        lc_idsepy = ob_response.data.x_transaction_id.ToString();
                        lc_fectra = ob_response.data.x_fecha_transaccion.ToString();
                        lc_refepy = ob_response.data.x_ref_payco.ToString();
                        lc_bankpy = ob_response.data.x_bank_name.ToString();

                        //Inicializar instancia de BD
                        using (var context = new DataDB(config))
                        {
                            //Consultar registro de venta en BD transacciones
                            var ob_repsl1 = context.TransactionSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == lc_puntea).Where(x => x.Teatro == lc_keytea);
                            foreach (var TransactionSales in ob_repsl1)
                                ob_repsle = context.TransactionSales.Find(TransactionSales.Id);

                            //Inicializar valores
                            switch (lc_status)
                            {
                                case "Aceptada":
                                    ob_repsle.EstadoTx = "EPY";
                                    ob_repsle.Observaciones = "VENTA SCORE/EPAYCO";
                                    break;

                                case "Rechazada":
                                case "Fallida":
                                    ob_repsle.EstadoTx = "REX";
                                    ob_repsle.Observaciones = "VENTA RECHAZADA EPAYCO";
                                    break;

                                case "Pendiente":
                                    ob_repsle.EstadoTx = "EPX";
                                    ob_repsle.Observaciones = "VENTA PENDIENTE EPAYCO";
                                    break;
                            }

                            ob_repsle.FechaTx = Convert.ToDateTime(lc_fectra);
                            ob_repsle.AutorizacionTx = string.Concat(lc_idsepy, ",", lc_status);
                            ob_repsle.ReferenciaTx = lc_refepy;
                            ob_repsle.ReferenciaEx = ref_payco;
                            ob_repsle.BancoTx = lc_bankpy;
                            ob_repsle.FechaModificado = DateTime.Now;

                            //Actualizar estado de transacción
                            context.TransactionSales.Update(ob_repsle);
                            context.SaveChanges();
                        }
                    }

                    //Adicionar valores de envio de correo Score
                    lc_urlcor = lc_urlcor.Replace("#xxx", lc_keytea.ToString());
                    lc_urlcor = lc_urlcor.Replace("#yyy", config.Value.PuntoVenta);
                    lc_urlcor = lc_urlcor.Replace("#zzz", lc_secsec.ToString());
                    lc_urlcor = lc_urlcor.Replace("#ccc", lc_coreli);

                    //Estado Exitoso
                    if (lc_status == "Aceptada")
                    {
                        try
                        {
                            //Envio de correo Score
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Confirmation";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Envío de correo compra APROBADA: Exitoso";
                            logSales.InnerExceptionMessage = "null";

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                        }
                        catch (Exception lc_syserr)
                        {
                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Confirmation";
                            logSales.Metodo = "EMAIL";
                            logSales.ExceptionMessage = "Fallo envío de correo compra APROBADA: " + lc_syserr.Message;
                            logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                            //Escribir Log
                            logAudit.LogApp(logSales);
                        }
                    }

                    //Estado Fallido
                    if (lc_status == "Rechazada" || lc_status == "Fallida")
                    {
                        if (Session.GetString("Secuencia") != null)
                        {
                            #region SERVICO SCORET
                            //Json de servicio RET
                            lc_objson = "{\"Punto\":" + Convert.ToInt32(config.Value.PuntoVenta) + ",\"Pedido\":" + Convert.ToInt32(lc_secsec) + ",\"teatro\":\"" + Convert.ToInt32(lc_keytea) + "\",\"tercero\":\"" + config.Value.ValorTercero + "\"}";

                            //Encriptar Json RET
                            lc_srvpar = ob_fncgrl.EncryptStringAES(lc_objson);

                            //Consumir servicio RET
                            lc_jsnrst = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoret/"), lc_srvpar);

                            //Generar Log
                            LogSales logSales = new LogSales();
                            LogAudit logAudit = new LogAudit(config);
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/Confirmation";
                            logSales.Metodo = "SCORET";
                            logSales.ExceptionMessage = lc_srvpar;
                            logSales.InnerExceptionMessage = lc_jsnrst;

                            //Escribir Log
                            //logAudit.LogApp(logSales);
                            #endregion

                            try
                            {
                                //Envio de correo Score
                                lc_urlcor = lc_urlcor.Replace("compra", "Fallida");
                                var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                                request.GetResponse();

                                //Generar Log
                                LogSales logSales1 = new LogSales();
                                LogAudit logAudit1 = new LogAudit(config);
                                logSales1.Id = Guid.NewGuid().ToString();
                                logSales1.Fecha = DateTime.Now;
                                logSales1.Programa = "Pages/Confirmation";
                                logSales1.Metodo = "EMAIL";
                                logSales1.ExceptionMessage = "Envío de correo compra RECHAZADA/FALLIDA: Exitoso";
                                logSales1.InnerExceptionMessage = "null";

                                //Escribir Log
                                logAudit1.LogApp(logSales1);
                            }
                            catch (Exception lc_syserr)
                            {
                                //Generar Log
                                LogSales logSales1 = new LogSales();
                                LogAudit logAudit1 = new LogAudit(config);
                                logSales1.Id = Guid.NewGuid().ToString();
                                logSales1.Fecha = DateTime.Now;
                                logSales1.Programa = "Pages/Confirmation";
                                logSales1.Metodo = "EMAIL";
                                logSales1.ExceptionMessage = "Fallo envío de correo compra RECHAZADA/FALLIDA: " + lc_syserr.Message;
                                logSales1.InnerExceptionMessage = logSales1.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                                //Escribir Log
                                logAudit1.LogApp(logSales1);
                            }
                        }
                    }

                    //Estado Pendiente
                    if (lc_status == "Pendiente")
                    {
                        try
                        {
                            //Envio de correo Score
                            lc_urlcor = lc_urlcor.Replace("compra", "Pendiente");
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            //Generar Log
                            LogSales logSales2 = new LogSales();
                            LogAudit logAudit2 = new LogAudit(config);
                            logSales2.Id = Guid.NewGuid().ToString();
                            logSales2.Fecha = DateTime.Now;
                            logSales2.Programa = "Pages/Confirmation";
                            logSales2.Metodo = "EMAIL";
                            logSales2.ExceptionMessage = "Envío de correo compra PENDIENTE: Exitoso";
                            logSales2.InnerExceptionMessage = "null";

                            //Escribir Log
                            logAudit2.LogApp(logSales2);
                        }
                        catch (Exception)
                        {
                            //Generar Log
                            LogSales logSales2 = new LogSales();
                            LogAudit logAudit2 = new LogAudit(config);
                            logSales2.Id = Guid.NewGuid().ToString();
                            logSales2.Fecha = DateTime.Now;
                            logSales2.Programa = "Pages/Confirmation";
                            logSales2.Metodo = "EMAIL";
                            logSales2.ExceptionMessage = "Fallo envío de correo compra PENDIENTE: Exitoso";
                            logSales2.InnerExceptionMessage = "null";

                            //Escribir Log
                            logAudit2.LogApp(logSales2);
                        }
                    }
                }
                catch (Exception lc_syserr)
                {
                    //Generar Log
                    LogSales logSales3 = new LogSales();
                    LogAudit logAudit3 = new LogAudit(config);
                    logSales3.Id = Guid.NewGuid().ToString();
                    logSales3.Fecha = DateTime.Now;
                    logSales3.Programa = "Pages/Confirmation";
                    logSales3.Metodo = "GET";
                    logSales3.ExceptionMessage = lc_syserr.Message;
                    logSales3.InnerExceptionMessage = logSales3.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                    //Escribir Log
                    logAudit3.LogApp(logSales3);
                }
            }

            return View();
        }

        /// <summary>
        /// GET: Error -- Cargar vista de error
        /// </summary>
        /// <param name="pr_message">Parm mensaje de error</param>
        /// <returns></returns>
        /// 

        [HttpGet]
        [Route("CargarErrorVista")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public ActionResult Error(string pr_message, string pr_flag = "E")
        {
            #region VARIABLES LOCALES
            string lc_secuencia = Session.GetString("Secuencia");
            string lc_puntovta = config.Value.PuntoVenta;
            string lc_tercero = config.Value.ValorTercero;
            string lc_teatro = Session.GetString("Teatro");
            #endregion

            URLPortal(config);
            ListCarrito();

            //validar flag
            if (pr_flag.Length > 2)
                ViewBag.Flag = pr_flag.Substring(0, 2);
            else
                ViewBag.Flag = pr_flag;

            ViewBag.pr_usulog = config.Value.UsuLogin2;
            ViewBag.pr_pwdlog = config.Value.PwdLogin2;

            //Asignar valores 
            if (Session.GetString("Secuencia") != null)
                ViewBag.Message = pr_message + " SECUENCIA: " + lc_secuencia + "-PUNTOVTA: " + lc_puntovta;
            else
                ViewBag.Message = pr_message;

            //Validar error para URL
            ViewBag.Ciudad = null;
            ViewBag.Teatro = null;
            if (pr_flag.Length > 2)
            {
                ViewBag.pr_usuflg = pr_flag;
                ViewBag.Ciudad = pr_flag.Substring(pr_flag.IndexOf(";") + 1);
                ViewBag.Teatro = pr_flag.Substring(3, pr_flag.IndexOf(";") - 3);
                ViewBag.URLhome = "/FastSales/Home";

                //Validar ciudad y teatro desde web externa
                if (ViewBag.Teatro != "0")
                    Selteatros(ViewBag.Teatro);
            }
            else
            {
                if (pr_flag.Contains("R"))
                    ViewBag.URLhome = "/FastSales/Home";
                else
                    ViewBag.URLhome = "/Home/Home";
            }

            //Validar error
            if (pr_flag.Contains("E"))
            {
                //CancelPaymentError(Convert.ToInt32(lc_secuencia), Convert.ToInt32(lc_puntovta), Convert.ToInt32(lc_teatro), lc_tercero);
                Session.Remove("Secuencia");
            }

            //Devolver a vista
            return View();
        }

        [HttpGet]
        [Route("DescargaTerminosYCondiciones")]
        public FileResult Download(string pr_flag)
        {
            //Validar archivo
            string ruta = string.Empty;
            if (pr_flag == "1")
            {
                ruta = "/documents/TERMINOS_Y_CONDICIONES_DE_USO_DEL_SITIO_WEB.pdf";
                return File(ruta, "application/pdf", "TERMINOS Y CONDICIONES DE USO DEL SITIO WEB.pdf");
            }
            else
            {
                ruta = "/documents/TERMINOS_Y_CONDICIONES_CINEFANS.pdf";
                return File(ruta, "application/pdf", "TERMINOS Y CONDICIONES CINEFANS.pdf");
            }
        }
        #endregion

        #region POST
        /// <summary>
        /// POST: RememberPwd -- Proceso para recordar clave
        /// </summary>
        /// <param name="pr_datlog">Parm Entidad Login</param>
        /// <returns></returns>
        [HttpPost]
        [Route("RememberPwd")]
        public ActionResult RememberPwd(Login pr_datlog)
        {
            #region VARIABLES LOCALES
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string lc_urlcor = config.Value.UrlCClave;

            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                //Asignar valores
                pr_datlog.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(pr_datlog);
                lc_srvpar = lc_srvpar.Replace("correo", "Correo");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocsn/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/RememberPwd";
                logSales.Metodo = "SCOCSN";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        ViewBag.alertS = false;
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
                    }
                    else
                    {
                        if (ob_diclst["Valor"].ToString() != "Usuario no registrado")
                        {
                            //Adicionar valores de envio de correo Score
                            lc_urlcor = lc_urlcor.Replace("#ccc", pr_datlog.Correo);
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            ViewBag.alertS = true;
                            pr_datlog.Password = "";
                        }
                        else
                        {
                            ViewBag.alertS = false;
                            ModelState.AddModelError("", ob_diclst["Valor"].ToString() + " por favor verificar.");
                        }
                    }
                }
                else
                {
                    ViewBag.alertS = false;

                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);
                }

                //Devolver a vista
                return View(pr_datlog);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/RememberPwd";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// POST: Users -- Crear usuario en portal web
        /// </summary>
        /// <param name="pr_dateli">Parm Entidad usuario</param>
        /// <returns></returns>
        [HttpPost]
        [Route("CrearusuarioPortalWeb")]
        public ActionResult Users(Usuario pr_dateli)
        {
            #region VARIABLES LOCALES
            int lc_auxedi = 0;
            int lc_auxedf = 0;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string lc_urlcor = config.Value.UrlCRegistro;

            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                //Inicializar variables
                ViewBag.AlertS = false;

                //Validar fecha
                if (pr_dateli.Fecha_Nacimiento == null)
                {
                    ModelState.AddModelError("", "Debe ingresar una fecha de nacimiento para continuar");
                    return View(pr_dateli);
                }

                //Validar clave
                if (pr_dateli.Password != pr_dateli.Barrio)
                {
                    ModelState.AddModelError("", "La contraseña y la confirmación no coinciden");
                    return View(pr_dateli);
                }

                //Validar medio favorito de contacto
                if (pr_dateli.Contacto == null || pr_dateli.Contacto == "")
                {
                    ModelState.AddModelError("", "Debe seleccionar un medio favorito de contacto para continuar");
                    return View(pr_dateli);
                }

                //Validar terminos
                if (!pr_dateli.Terminos)
                {
                    ModelState.AddModelError("", "Debe aceptar los términos y condiciones, política de tratamiento de datos y cinefans para continuar");
                    return View(pr_dateli);
                }

                //Validar MFC
                if (!pr_dateli.Politicas)
                {
                    ModelState.AddModelError("", "Debe autorizar recibir información por medios de contacto para continuar");
                    return View(pr_dateli);
                }

                //Asignar valores
                pr_dateli.Tipo = "";
                pr_dateli.SwtVenta = "";
                pr_dateli.KeyPelicula = "";

                lc_auxedf = DateTime.Today.Year;
                lc_auxedi = Convert.ToInt32(pr_dateli.Fecha_Nacimiento.Substring(6, 4));

                //Asignar valores
                pr_dateli.Edad = (lc_auxedf - lc_auxedi);
                pr_dateli.Sexo = pr_dateli.Sexo;
                pr_dateli.Login = pr_dateli.Correo;
                pr_dateli.Barrio = "";
                pr_dateli.Genero = "";
                pr_dateli.Accion = "C";
                pr_dateli.Cinema = config.Value.ValorTercero;
                pr_dateli.Tercero = config.Value.ValorTercero;
                pr_dateli.Telefono = "0";
                pr_dateli.Reservas = "";
                pr_dateli.Noticias = "";
                pr_dateli.Cartelera = "";
                pr_dateli.Direccion = "";
                pr_dateli.Municipio = "";
                pr_dateli.Otras_Salas = "N";
                pr_dateli.Fecha_Nacimiento = ob_fncgrl.ConvertDate(Convert.ToDateTime(pr_dateli.Fecha_Nacimiento.Substring(0, 2) + "/" + pr_dateli.Fecha_Nacimiento.Substring(3, 2) + "/" + pr_dateli.Fecha_Nacimiento.Substring(6, 4)));

                pr_dateli.TelefonoEli = "";
                pr_dateli.ApellidoEli = "";
                pr_dateli.KeyTeatro = "";
                pr_dateli.NombreEli = "";
                pr_dateli.EmailEli = "";
                pr_dateli.KeySala = "";
                pr_dateli.FecProg = "";
                pr_dateli.HorProg = "";
                pr_dateli.message = "";

                pr_dateli.NombrePel = "";
                pr_dateli.NombreFec = "";
                pr_dateli.NombreHor = "";
                pr_dateli.NombreTar = "";
                pr_dateli.KeyTarifa = "";
                pr_dateli.KeySecuencia = "";

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(pr_dateli);
                lc_srvpar = lc_srvpar.Replace("sexo", "Sexo");
                lc_srvpar = lc_srvpar.Replace("edad", "Edad");
                lc_srvpar = lc_srvpar.Replace("login", "Login");
                lc_srvpar = lc_srvpar.Replace("barrio", "Barrio");
                lc_srvpar = lc_srvpar.Replace("genero", "Genero");
                lc_srvpar = lc_srvpar.Replace("cinema", "Cinema");
                lc_srvpar = lc_srvpar.Replace("correo", "Correo");
                lc_srvpar = lc_srvpar.Replace("nombre", "Nombre");
                lc_srvpar = lc_srvpar.Replace("accion", "Accion");
                lc_srvpar = lc_srvpar.Replace("celular", "Celular");
                lc_srvpar = lc_srvpar.Replace("password", "Clave");
                lc_srvpar = lc_srvpar.Replace("contacto", "Contacto");
                lc_srvpar = lc_srvpar.Replace("telefono", "Telefono");
                lc_srvpar = lc_srvpar.Replace("apellido", "Apellido");
                lc_srvpar = lc_srvpar.Replace("reservas", "Reservas");
                lc_srvpar = lc_srvpar.Replace("noticias", "Noticias");
                lc_srvpar = lc_srvpar.Replace("cartelera", "Cartelera");
                lc_srvpar = lc_srvpar.Replace("documento", "Documento");
                lc_srvpar = lc_srvpar.Replace("direccion", "Direccion");
                lc_srvpar = lc_srvpar.Replace("municipio", "Municipio");
                lc_srvpar = lc_srvpar.Replace("\"id\"", "\"IdMessage\"");
                lc_srvpar = lc_srvpar.Replace("otras_Salas", "Otras_Salas");
                lc_srvpar = lc_srvpar.Replace("fecha_Nacimiento", "Fecha_Nacimiento");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocya/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/User";
                logSales.Metodo = "SCOCYA";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
                        return View();
                    }
                    else
                    {
                        if (ob_diclst["Respuesta"].ToString() == "Proceso realizado con éxito.")
                        {
                            //Adicionar valores de envio de correo Score
                            lc_urlcor = lc_urlcor.Replace("#ccc", pr_dateli.Correo);
                            var request = (HttpWebRequest)WebRequest.Create(lc_urlcor);
                            request.GetResponse();

                            ViewBag.AlertS = true;
                        }
                        else
                        {
                            ModelState.AddModelError("", ob_diclst["Valor"].ToString());
                            return View();
                        }
                    }
                }
                else
                {
                    lc_result = lc_result.Replace("1-", "");
                    ModelState.AddModelError("", lc_result);
                    return View();
                }

                //Devolver a vista
                return View(pr_dateli);
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/User";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// POST: Login -- Validar inicio de sesión
        /// </summary>
        /// <param name="pr_usulog">Parm login</param>
        /// /// <param name="pr_pwdlog">Parm contraseña</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ValidarLogin")]
        public ActionResult Login(string pr_usulog, string pr_pwdlog, string pr_usuflg, string pr_usutea = "", string pr_ciutea = "")
        {
            #region VARIABLES LOCALES
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string lc_teleli = string.Empty;
            string lc_nomeli = string.Empty;
            string lc_apeeli = string.Empty;
            string lc_coreli = string.Empty;
            string lc_keytea = string.Empty;
            string lc_swtven = string.Empty;
            string lc_destea = string.Empty;
            string lc_tiplog = string.Empty;
            string lc_keypel = string.Empty;

            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            Login pr_datlog = new Login();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                //Asignar valores


                pr_datlog.Correo = pr_usulog;
                pr_datlog.Password = pr_pwdlog;
                pr_datlog.Tercero = config.Value.ValorTercero;

                Session.Remove("NroTarjeta");

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(pr_datlog);
                lc_srvpar = lc_srvpar.Replace("correo", "Correo");
                lc_srvpar = lc_srvpar.Replace("password", "Clave");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scolog/"), lc_srvpar);

                //Devolver a vista
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Login";
                logSales.Metodo = "SCOLOG";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        //Devolver a vista
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Login";
                        logSales.Metodo = "SCOLOG";
                        logSales.ExceptionMessage = "Validacion respuesta llave Validación";
                        logSales.InnerExceptionMessage = ob_diclst["Validación"].ToString();

                        //Escribir Log
                        logAudit.LogApp(logSales);

                        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id, pr_flag = "P" });
                    }
                    else
                    {
                        if (ob_diclst["Valor"].ToString() == "VÁLIDA")
                        {
                            //Validar si es cliente frecuente
                            Session.Remove("ClienteFrecuente");
                            if (ob_diclst["Estado"].ToString() == "Usuario sin tarjeta activa")
                            {
                                Session.SetString("ClienteFrecuente", "No");
                            }
                            else
                            {
                                Session.SetString("ClienteFrecuente", "Si");
                                Session.SetString("NroTarjeta", ob_diclst["No. Tarjeta"].ToString());
                            }

                            //Validar invitado
                            if (pr_usulog == "pol@scoreprojects.net")
                            {
                                Session.SetString("FlagLogin", "INV");
                                Session.SetString("FlagInv", "Si");
                                Session.SetString("FlagAdmin", "Si");

                            }
                            else if (pr_usulog == "admin@scoreprojects.net")
                            {
                                Session.SetString("FlagAdmin", "Si");

                            }
                            else
                            {
                                Session.SetString("FlagLogin", "USU");
                                Session.SetString("FlagInv", "No");
                                Session.SetString("FlagAdmin", "No");
                            }



                            //Cargar variables de sesión
                            Session.SetString("Passwrd", ob_diclst["Clave"].ToString());
                            Session.SetString("Passwrd2", pr_pwdlog);
                            Session.SetString("Usuario", pr_usulog);

                            Session.SetString("Nombre", ob_diclst["Nombre"].ToString());
                            Session.SetString("Apellido", ob_diclst["Apellido"].ToString());
                            Session.SetString("Telefono", ob_diclst["Celular"].ToString());
                            Session.SetString("Direccion", ob_diclst["Direccion"].ToString());
                            Session.SetString("Documento", ob_diclst["Documento"].ToString());
                            Session.SetString("Genero", ob_diclst["Genero"].ToString());
                            Session.SetString("Barrio", ob_diclst["Barrio"].ToString());
                            Session.SetString("Municipio", ob_diclst["Municipio"].ToString());

                            //Devolver a vista confi
                            if (pr_usuflg == "X")
                                return RedirectToAction("ProductList", "SalesCon", new { pr_secpro = "0", pr_swtven = "V", pr_tiplog = "P", pr_tbview = "", Teatro = pr_usutea, Ciudad = pr_ciutea });

                            //Devolver a vista
                            if (pr_usuflg == "L")
                            {
                                if (Session.GetString("FlagCompra") == "N")
                                {
                                    //Validar seleccion PEL sin logueo
                                    if (Session.GetString("IdPelicula") != null)
                                        return RedirectToAction("Detail", "SalesBol", new { pr_keypel = Session.GetString("IdPelicula"), pr_fecprg = Session.GetString("FcPelicula"), pr_tippel = Session.GetString("TpPelicula") });
                                    else
                                        return RedirectToAction("Home", "Home");
                                }
                                else
                                {
                                    if (pr_usutea != "")
                                    {
                                        return RedirectToAction("Home", "FastSales", new { Teatro = pr_usutea, Ciudad = pr_ciutea });
                                    }
                                    else
                                    {
                                        return RedirectToAction("Home", "FastSales");
                                    }
                                }
                            }
                            else
                            {
                                if (pr_usuflg == "P")
                                {

                                }
                                //Validar seleccion PEL sin logueo
                                if (Session.GetString("IdPelicula") != null)
                                    return RedirectToAction("Detail", "SalesBol", new { pr_keypel = Session.GetString("IdPelicula"), pr_fecprg = Session.GetString("FcPelicula"), pr_tippel = Session.GetString("TpPelicula") });
                                else
                                    return RedirectToAction("CineFans", "CineFans");
                            }
                        }
                        else
                        {
                            //Devolver a vista
                            return RedirectToAction("Error", "Pages", new { pr_message = "No pudimos iniciar sesión. Vuelve a intentarlo", pr_flag = "P" });
                        }
                    }
                }
                else
                {
                    //Devolver a vista
                    lc_result = lc_result.Replace("1-", "");

                    //Generar Log
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "Pages/Login";
                    logSales.Metodo = "SCOLOG";
                    logSales.ExceptionMessage = "Validacion respuesta llave 0";
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    //logAudit.LogApp(logSales);

                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                }
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Login";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// POST: Payment -- Proceso de ejecución SCOINT para generar una venta de boletas
        /// </summary>
        /// <param name="pr_datpay">Parm objeto de venta boletas</param>
        /// <returns></returns>
        [HttpPost]
        [Route("ProcesoGenerarVentaBoletas")]
        public ActionResult Payment(Producto pr_datpro)
        {
            #region VARIABLES LOCALES
            int lc_swtcat = 0;
            int lc_idearr = 0;
            int lc_cntubi = 0;
            int lc_keytar = 0;
            int lc_barclf = 0;
            double lc_boltot = 0;
            string lc_despel = string.Empty;
            string lc_auxitm = string.Empty;
            string lc_ubiprg = string.Empty;
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_tipven = string.Empty;
            string lc_doceli = string.Empty;
            string lc_direli = string.Empty;
            string lc_teleli = string.Empty;
            string lc_ubilbl = "Ubicaciones: ";
            string lc_fecven = DateTime.Now.ToString("dd/MM/yyyy");

            string SwtVenta = string.Empty;
            string EmailEli = string.Empty;
            string NombreEli = string.Empty;
            string KeyTeatro = string.Empty;
            string DesTeatro = string.Empty;
            string TipoCompra = string.Empty;
            string ApellidoEli = string.Empty;
            string TelefonoEli = string.Empty;
            string DireccionEli = string.Empty;
            string DocumentoEli = string.Empty;
            string KeySecuencia = string.Empty;
            string[] ls_lstsel = new string[5];

            decimal lc_secsec = 0;
            decimal lc_canrot = 0;
            decimal lc_prorot = 0;
            decimal lc_valpro = 0;

            decimal Base = 0;
            decimal Total = 0;
            decimal Impuesto_1 = 0;
            decimal Impuesto_2 = 0;
            decimal CashBack_Acumulado = 0;

            List<string> ls_lstubi = new List<string>();
            List<Producto> ob_retpro = new List<Producto>();
            List<Productos> ob_proven = new List<Productos>();
            List<OrderItem> ob_ordite = new List<OrderItem>();
            List<Ubicaciones> ob_ubiprg = new List<Ubicaciones>();
            Dictionary<string, object> ob_lstpro = new Dictionary<string, object>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            InternetSales ob_intvta = new InternetSales();
            Secuencia ob_scopre = new Secuencia();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                URLPortal(config);
                ListCarrito();

                //Inicializar valores
                ViewBag.AlertS = false;
                var PuntoVenta = config.Value.PuntoVenta;

                EmailEli = Session.GetString("Usuario");
                NombreEli = Session.GetString("Nombre");
                KeyTeatro = Session.GetString("Teatro");
                TipoCompra = pr_datpro.TipoCompra;
                ApellidoEli = Session.GetString("Apellido");
                TelefonoEli = Session.GetString("Telefono");
                DireccionEli = Session.GetString("Direccion");
                DocumentoEli = Session.GetString("Documento");
                KeySecuencia = pr_datpro.KeySecuencia;

                if (Session.GetString("NroTarjeta") != null)
                    lc_barclf = Convert.ToInt32(Session.GetString("NroTarjeta"));
                else
                    lc_barclf = 0;

                lc_tipven = TipoCompra;
                lc_secsec = Convert.ToDecimal(KeySecuencia);

                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);

                //Tipo de venta
                switch (lc_tipven)
                {
                    case "B": //VENTA BOLETAS
                        //Obtener boletas carrito de compra
                        using (var context = new DataDB(config))
                        {

                            var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                            foreach (var vr_itevta in ReportSales)
                            {
                                ob_intvta.Sala = Convert.ToInt32(vr_itevta.KeySala);
                                ob_intvta.Funcion = Convert.ToInt32(vr_itevta.HorProg.Substring(0, 2));
                                ob_intvta.Pelicula = Convert.ToInt32(vr_itevta.KeyPelicula);
                                ob_intvta.FechaFun = string.Concat(vr_itevta.FecProg.Substring(0, 4), "-", vr_itevta.FecProg.Substring(4, 2), "-", vr_itevta.FecProg.Substring(6, 2));
                                ob_intvta.InicioFun = Convert.ToInt32(vr_itevta.HorProg);

                                lc_boltot = vr_itevta.Precio;
                                lc_ubiprg = vr_itevta.SelUbicaciones;
                                lc_keytar = Convert.ToInt32(vr_itevta.KeyTarifa);
                            }

                            //Obtener ubicaciones de vista
                            char[] ar_charst = lc_ubiprg.ToCharArray();
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
                                lc_ubilbl += string.Concat("Fila: ", ls_lstsel[1], " Columna: ", ls_lstsel[2], ";");
                                ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = lc_keytar, FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                            }

                            //Adicionar a lista
                            ob_ordite.Add(new OrderItem
                            {
                                Precio = Convert.ToDecimal(lc_boltot),
                                Cantidad = lc_cntubi,
                                Descripcion = string.Concat(lc_despel, "-", lc_ubilbl),
                                KeyProducto = ob_intvta.Pelicula
                            });

                            //Asignar valores
                            ob_intvta.Productos = ob_proven;
                            ob_intvta.Ubicaciones = ob_ubiprg;
                            ob_intvta.Accion = pr_datpro.SwtVenta;
                            ob_intvta.TotalVenta = lc_boltot;

                            //Validar pago cashback
                            if (pr_datpro.SwitchCashback == "S")
                            {
                                ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPagoCB);
                                ob_intvta.PagoInterno = Convert.ToDouble(pr_datpro.Valor);
                                ob_intvta.PagoCredito = 0;
                            }
                            else
                            {
                                ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPago);
                                ob_intvta.PagoInterno = 0;
                                ob_intvta.PagoCredito = Convert.ToDouble(pr_datpro.Valor);
                            }
                        }

                        break;

                    case "P": //VENTA RETAIL
                        #region SERVICIO SCOPRE
                        //Asignar valores PRE
                        ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                        ob_scopre.Teatro = Convert.ToInt32(KeyTeatro);
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
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Payment";
                        logSales.Metodo = "SCOPRE_P";
                        logSales.ExceptionMessage = lc_srvpar;
                        logSales.InnerExceptionMessage = lc_result;

                        //Escribir Log
                        //logAudit.LogApp(logSales);

                        //Validar respuesta
                        if (lc_result.Substring(0, 1) == "0")
                        {
                            //Quitar switch
                            lc_result = lc_result.Replace("0-", "");
                            ob_lstpro = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                            ob_retpro = (List<Producto>)JsonConvert.DeserializeObject(ob_lstpro["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                            if (ob_lstpro.ContainsKey("Validación"))
                                ModelState.AddModelError("", ob_lstpro["Validación"].ToString());
                        }
                        else
                        {
                            lc_result = lc_result.Replace("1-", "");
                            ModelState.AddModelError("", lc_result);
                        }
                        #endregion

                        //Obtener productos carrito de compra
                        using (var context = new DataDB(config))
                        {
                            //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia

                            decimal IdTeatro = Convert.ToDecimal(KeyTeatro);
                            var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == IdTeatro).ToList();
                            foreach (var vr_itevta in RetailSales)
                            {
                                //Recorrer productos habilitados para internet y valodar carrito de compras
                                foreach (var vr_itepro in ob_retpro)
                                {
                                    //Valiar keypro vta VS keypro int
                                    if (vr_itevta.KeyProducto == vr_itepro.Codigo)
                                    {
                                        //Recorrer y asignar productos por cantidad seleccionada
                                        for (int j = 0; j < vr_itevta.Cantidad; j++)
                                        {
                                            //Asignar valores
                                            Productos ob_auxven = new Productos();
                                            List<Receta> ob_comrec = new List<Receta>();

                                            //Validar tipo
                                            if (vr_itevta.Tipo == "A")
                                            {
                                                ob_auxven.Codigo = vr_itevta.ProCategoria1;
                                                ob_auxven.Tipo = "P";
                                            }
                                            else
                                            {
                                                ob_auxven.Codigo = vr_itevta.KeyProducto;
                                                ob_auxven.Tipo = vr_itevta.Tipo;
                                            }

                                            //validar Cliente frecuente
                                            ob_auxven.Precio = 1;
                                            //if (Session.GetString("ClienteFrecuente") == "No")
                                            //    ob_auxven.Precio = 1;
                                            //else
                                            //    ob_auxven.Precio = 2;

                                            ob_auxven.Receta = null;
                                            ob_auxven.Descripcion = vr_itevta.Descripcion;

                                            //Validar receta del combo
                                            if (ob_auxven.Tipo == "C")
                                            {
                                                //Recorrer receta del combo y guardar
                                                lc_swtcat = 1;
                                                foreach (var vr_itecom in vr_itepro.Receta)
                                                {
                                                    List<Producto> ob_compro = new List<Producto>();


                                                    //Validar si es categoria
                                                    if (vr_itecom.Tipo == "A")
                                                    {
                                                        //Asignar valores
                                                        switch (lc_swtcat)
                                                        {
                                                            case 1:
                                                                lc_canrot = vr_itevta.CanCategoria1;
                                                                lc_prorot = vr_itevta.ProCategoria1;
                                                                break;
                                                            case 2:
                                                                lc_canrot = vr_itevta.CanCategoria2;
                                                                lc_prorot = vr_itevta.ProCategoria2;
                                                                break;
                                                            case 3:
                                                                lc_canrot = vr_itevta.CanCategoria3;
                                                                lc_prorot = vr_itevta.ProCategoria3;
                                                                break;
                                                            case 4:
                                                                lc_canrot = vr_itevta.CanCategoria4;
                                                                lc_prorot = vr_itevta.ProCategoria4;
                                                                break;
                                                            case 5:
                                                                lc_canrot = vr_itevta.CanCategoria5;
                                                                lc_prorot = vr_itevta.ProCategoria5;
                                                                break;
                                                        }




                                                        lc_swtcat++;

                                                        //Adicionar retail detcombo

                                                        decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                                                        using (var context2 = new DataDB(config))
                                                        {
                                                            var RetailDet = context.RetailDet.Where(x => x.IdRetailSales == vr_itevta.Id).Where(x => x.Secuencia == lc_secsec1).Where(x => x.ProCategoria == lc_prorot).ToList();
                                                            foreach (var retail in RetailDet)
                                                            {
                                                                //Adicionar producto seleccionado de la categoria la cantidad indicada

                                                                ob_compro.Add(new Producto
                                                                {
                                                                    Tipo = "P",
                                                                    Codigo = Convert.ToDecimal(retail.ProItem.Substring(0, retail.ProItem.IndexOf(","))),
                                                                    Cantidad = 1,
                                                                    Descripcion = retail.ProItem.Substring(retail.ProItem.IndexOf("-") + 1)
                                                                });

                                                            }
                                                        }

                                                        //Adicionar producto a receta del combo
                                                        ob_comrec.Add(new Receta
                                                        {
                                                            Tipo = vr_itecom.Tipo,
                                                            Codigo = vr_itecom.Codigo,
                                                            Cantidad = vr_itecom.Cantidad,
                                                            Descripcion = vr_itecom.Descripcion,
                                                            RecetaCategoria = ob_compro
                                                        });
                                                    }
                                                    else
                                                    {
                                                        //Adicionar producto a receta del combo
                                                        ob_comrec.Add(vr_itecom);
                                                    }
                                                }
                                            }

                                            //Cargar producto
                                            ob_auxven.Receta = ob_comrec;
                                            ob_proven.Add(ob_auxven);
                                        }

                                        break;
                                    }
                                }

                                //Asignar valor total de productos
                                lc_valpro += vr_itevta.Precio * vr_itevta.Cantidad;
                            }

                            //Asignar valores
                            ob_intvta.Productos = ob_proven;
                            ob_intvta.Ubicaciones = ob_ubiprg;

                            //Asignar valores
                            ob_intvta.Sala = 0;
                            ob_intvta.Funcion = 0;
                            ob_intvta.Pelicula = 0;
                            ob_intvta.FechaFun = string.Concat(lc_fecven.Substring(6, 4), "-", lc_fecven.Substring(3, 2), "-", lc_fecven.Substring(0, 2));
                            ob_intvta.InicioFun = 0;
                            ob_intvta.Accion = "C";
                            ob_intvta.TotalVenta = Convert.ToDouble(lc_valpro);

                            //Validar pago cashback
                            if (pr_datpro.SwitchCashback == "S")
                            {
                                ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPagoCB);
                                ob_intvta.PagoInterno = Convert.ToDouble(lc_valpro);
                                ob_intvta.PagoCredito = 0;
                            }
                            else
                            {
                                ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPago);
                                ob_intvta.PagoInterno = 0;
                                ob_intvta.PagoCredito = Convert.ToDouble(lc_valpro);
                            }
                        }

                        break;

                    case "M": //VENTA MIXTA (BOLETAS Y RETAIL)
                        #region SERVICIO SCOPRE
                        //Asignar valores PRE
                        ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                        ob_scopre.Teatro = Convert.ToInt32(KeyTeatro);
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
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Payment";
                        logSales.Metodo = "SCOPRE_M";
                        logSales.ExceptionMessage = lc_srvpar;
                        logSales.InnerExceptionMessage = lc_result;

                        //Escribir Log
                        //logAudit.LogApp(logSales);

                        //Validar respuesta
                        if (lc_result.Substring(0, 1) == "0")
                        {
                            //Quitar switch
                            lc_result = lc_result.Replace("0-", "");
                            ob_lstpro = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                            ob_retpro = (List<Producto>)JsonConvert.DeserializeObject(ob_lstpro["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                            if (ob_lstpro.ContainsKey("Validación"))
                                ModelState.AddModelError("", ob_lstpro["Validación"].ToString());
                        }
                        else
                        {
                            lc_result = lc_result.Replace("1-", "");
                            ModelState.AddModelError("", lc_result);
                        }
                        #endregion

                        //Obtener productos carrito de compra
                        using (var context = new DataDB(config))
                        {
                            //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia

                            decimal IdTeatro = Convert.ToDecimal(KeyTeatro);
                            var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == IdTeatro).ToList();
                            foreach (var vr_itevta in RetailSales)
                            {
                                //Recorrer productos habilitados para internet y valodar carrito de compras
                                foreach (var vr_itepro in ob_retpro)
                                {
                                    //Valiar keypro vta VS keypro int
                                    if (vr_itevta.KeyProducto == vr_itepro.Codigo)
                                    {
                                        //Recorrer y asignar productos por cantidad seleccionada
                                        for (int j = 0; j < vr_itevta.Cantidad; j++)
                                        {
                                            //Asignar valores
                                            Productos ob_auxven = new Productos();
                                            List<Receta> ob_comrec = new List<Receta>();

                                            //Validar tipo
                                            if (vr_itevta.Tipo == "A")
                                            {
                                                ob_auxven.Codigo = vr_itevta.ProCategoria1;
                                                ob_auxven.Tipo = "P";
                                            }
                                            else
                                            {
                                                ob_auxven.Codigo = vr_itevta.KeyProducto;
                                                ob_auxven.Tipo = vr_itevta.Tipo;
                                            }

                                            //validar Cliente frecuente
                                            ob_auxven.Precio = 1;
                                            //if (Session.GetString("ClienteFrecuente") == "No")
                                            //    ob_auxven.Precio = 1;
                                            //else
                                            //    ob_auxven.Precio = 2;

                                            ob_auxven.Receta = null;
                                            ob_auxven.Descripcion = vr_itevta.Descripcion;

                                            //Validar receta del combo
                                            if (ob_auxven.Tipo == "C")
                                            {
                                                //Recorrer receta del combo y guardar
                                                lc_swtcat = 1;
                                                foreach (var vr_itecom in vr_itepro.Receta)
                                                {
                                                    List<Producto> ob_compro = new List<Producto>();
                                                    //Validar si es categoria
                                                    if (vr_itecom.Tipo == "A")
                                                    {
                                                        //Asignar valores
                                                        switch (lc_swtcat)
                                                        {
                                                            case 1:
                                                                lc_canrot = vr_itevta.CanCategoria1;
                                                                lc_prorot = vr_itevta.ProCategoria1;
                                                                break;
                                                            case 2:
                                                                lc_canrot = vr_itevta.CanCategoria2;
                                                                lc_prorot = vr_itevta.ProCategoria2;
                                                                break;
                                                            case 3:
                                                                lc_canrot = vr_itevta.CanCategoria3;
                                                                lc_prorot = vr_itevta.ProCategoria3;
                                                                break;
                                                            case 4:
                                                                lc_canrot = vr_itevta.CanCategoria4;
                                                                lc_prorot = vr_itevta.ProCategoria4;
                                                                break;
                                                            case 5:
                                                                lc_canrot = vr_itevta.CanCategoria5;
                                                                lc_prorot = vr_itevta.ProCategoria5;
                                                                break;
                                                        }



                                                        lc_swtcat++;

                                                        //Adicionar retail detcombo

                                                        decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                                                        using (var context2 = new DataDB(config))
                                                        {
                                                            var RetailDet = context.RetailDet.Where(x => x.IdRetailSales == vr_itevta.Id).Where(x => x.Secuencia == lc_secsec1).Where(x => x.ProCategoria == lc_prorot).ToList();
                                                            foreach (var retail in RetailDet)
                                                            {
                                                                //Adicionar producto seleccionado de la categoria la cantidad indicada

                                                                ob_compro.Add(new Producto
                                                                {
                                                                    Tipo = "P",
                                                                    Codigo = Convert.ToDecimal(retail.ProItem.Substring(0, retail.ProItem.IndexOf(","))),
                                                                    Cantidad = 1,
                                                                    Descripcion = retail.ProItem.Substring(retail.ProItem.IndexOf("-") + 1)
                                                                });

                                                            }
                                                        }

                                                        //Adicionar producto a receta del combo
                                                        ob_comrec.Add(new Receta
                                                        {
                                                            Tipo = vr_itecom.Tipo,
                                                            Codigo = vr_itecom.Codigo,
                                                            Cantidad = vr_itecom.Cantidad,
                                                            Descripcion = vr_itecom.Descripcion,
                                                            RecetaCategoria = ob_compro
                                                        });
                                                    }
                                                    else
                                                    {
                                                        //Adicionar producto a receta del combo
                                                        ob_comrec.Add(vr_itecom);
                                                    }
                                                }
                                            }

                                            //Cargar producto
                                            ob_auxven.Receta = ob_comrec;
                                            ob_proven.Add(ob_auxven);
                                        }

                                        break;
                                    }
                                }

                                //Asignar valor total de productos
                                lc_valpro += vr_itevta.Precio * vr_itevta.Cantidad;
                            }


                            //Obtener boletas carrito de compra

                            //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia

                            var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
                            foreach (var vr_itevta in ReportSales)
                            {
                                ob_intvta.Sala = Convert.ToInt32(vr_itevta.KeySala);
                                ob_intvta.Funcion = Convert.ToInt32(vr_itevta.HorProg.Substring(0, 2));
                                ob_intvta.Pelicula = Convert.ToInt32(vr_itevta.KeyPelicula);
                                ob_intvta.FechaFun = string.Concat(vr_itevta.FecProg.Substring(0, 4), "-", vr_itevta.FecProg.Substring(4, 2), "-", vr_itevta.FecProg.Substring(6, 2));
                                ob_intvta.InicioFun = Convert.ToInt32(vr_itevta.HorProg);

                                lc_boltot = vr_itevta.Precio;
                                lc_ubiprg = vr_itevta.SelUbicaciones;
                                lc_keytar = Convert.ToInt32(vr_itevta.KeyTarifa);
                            }

                            //Obtener ubicaciones de vista
                            char[] ar_charst = lc_ubiprg.ToCharArray();
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
                                lc_ubilbl += string.Concat("Fila: ", ls_lstsel[1], " Columna: ", ls_lstsel[2], ";");
                                ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = lc_keytar, FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                            }

                            //Adicionar a lista
                            ob_ordite.Add(new OrderItem
                            {
                                Precio = Convert.ToDecimal(lc_boltot),
                                Cantidad = lc_cntubi,
                                Descripcion = string.Concat(lc_despel, "-", lc_ubilbl),
                                KeyProducto = ob_intvta.Pelicula
                            });
                        }

                        //Venta de boletas y confites
                        lc_boltot += Convert.ToDouble(lc_valpro);

                        //Asignar valores
                        ob_intvta.Productos = ob_proven;
                        ob_intvta.Ubicaciones = ob_ubiprg;
                        ob_intvta.Accion = pr_datpro.SwtVenta;
                        ob_intvta.TotalVenta = lc_boltot;

                        //Validar pago cashback
                        if (pr_datpro.SwitchCashback == "S")
                        {
                            ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPagoCB);
                            ob_intvta.PagoInterno = Convert.ToDouble(lc_boltot);
                            ob_intvta.PagoCredito = 0;
                        }
                        else
                        {
                            ob_intvta.CodMedioPago = Convert.ToInt32(config.Value.CodMedioPago);
                            ob_intvta.PagoInterno = 0;
                            ob_intvta.PagoCredito = Convert.ToDouble(lc_boltot);
                        }

                        break;
                }

                #region SERVICIO SCOINT
                //Asignar valores
                ob_intvta.Nombre = NombreEli;
                ob_intvta.Factura = Convert.ToInt32(KeySecuencia);
                ob_intvta.Apellido = ApellidoEli;
                ob_intvta.Telefono = Convert.ToInt64(TelefonoEli);
                ob_intvta.Direccion = DireccionEli;
                ob_intvta.PuntoVenta = Convert.ToInt32(config.Value.PuntoVenta);
                ob_intvta.DocIdentidad = Convert.ToInt64(DocumentoEli);
                ob_intvta.CorreoCliente = EmailEli;

                ob_intvta.Obs1 = "";
                ob_intvta.Obs2 = "";
                ob_intvta.Obs3 = "";
                ob_intvta.Obs4 = "";
                ob_intvta.Placa = "0";
                ob_intvta.Teatro = Convert.ToInt32(KeyTeatro);
                ob_intvta.Tercero = Convert.ToInt32(config.Value.ValorTercero);
                ob_intvta.AudiPrev = 0;
                ob_intvta.TipoBono = 0;
                ob_intvta.Cortesia = "";
                ob_intvta.TipoEntrega = "T";

                ob_intvta.PagoEfectivo = 0;
                ob_intvta.ClienteFrecuente = lc_barclf;

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(ob_intvta);
                lc_srvpar = lc_srvpar.Replace("puntoVenta", "PuntoVenta");
                lc_srvpar = lc_srvpar.Replace("factura", "Factura");
                lc_srvpar = lc_srvpar.Replace("correoCliente", "CorreoCliente");
                lc_srvpar = lc_srvpar.Replace("docIdentidad", "DocIdentidad");
                lc_srvpar = lc_srvpar.Replace("nombre", "Nombre");
                lc_srvpar = lc_srvpar.Replace("apellido", "Apellido");
                lc_srvpar = lc_srvpar.Replace("telefono", "Telefono");
                lc_srvpar = lc_srvpar.Replace("direccion", "Direccion");
                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                lc_srvpar = lc_srvpar.Replace("fechaFun", "FechaFun");
                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                lc_srvpar = lc_srvpar.Replace("inicioFun", "InicioFun");
                lc_srvpar = lc_srvpar.Replace("ubicaciones", "Ubicaciones");
                lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                lc_srvpar = lc_srvpar.Replace("tarifa", "Tarifa");
                lc_srvpar = lc_srvpar.Replace("filRelativa", "FilRelativa");
                lc_srvpar = lc_srvpar.Replace("colRelativa", "ColRelativa");
                lc_srvpar = lc_srvpar.Replace("pelicula", "Pelicula");
                lc_srvpar = lc_srvpar.Replace("productos", "Productos");

                lc_srvpar = lc_srvpar.Replace("receta", "Receta");
                lc_srvpar = lc_srvpar.Replace("RecetaCategoria", "Receta");
                lc_srvpar = lc_srvpar.Replace("codigo", "Codigo");
                lc_srvpar = lc_srvpar.Replace("tipo", "Tipo");
                lc_srvpar = lc_srvpar.Replace("descripcion", "Descripcion");
                lc_srvpar = lc_srvpar.Replace("precio", "Precio");
                lc_srvpar = lc_srvpar.Replace("precios", "Precio");
                lc_srvpar = lc_srvpar.Replace("Precios", "Precio");
                lc_srvpar = lc_srvpar.Replace("cantidad", "Cantidad");

                lc_srvpar = lc_srvpar.Replace("placa", "Placa");
                lc_srvpar = lc_srvpar.Replace("audiPrev", "AudiPrev");
                lc_srvpar = lc_srvpar.Replace("tipoEntrega", "TipoEntrega");
                lc_srvpar = lc_srvpar.Replace("cortesia", "Cortesia");
                lc_srvpar = lc_srvpar.Replace("tipoBono", "TipoBono");
                lc_srvpar = lc_srvpar.Replace("clienteFrecuente", "ClienteFrecuente");
                lc_srvpar = lc_srvpar.Replace("totalVenta", "TotalVenta");
                lc_srvpar = lc_srvpar.Replace("pagoInterno", "PagoInterno");
                lc_srvpar = lc_srvpar.Replace("pagoCredito", "PagoCredito");
                lc_srvpar = lc_srvpar.Replace("pagoEfectivo", "PagoEfectivo");
                lc_srvpar = lc_srvpar.Replace("codMedioPago", "CodMedioPago");
                lc_srvpar = lc_srvpar.Replace("obs1", "Obs1");
                lc_srvpar = lc_srvpar.Replace("obs2", "Obs2");
                lc_srvpar = lc_srvpar.Replace("obs3", "Obs3");
                lc_srvpar = lc_srvpar.Replace("obs4", "Obs4");
                lc_srvpar = lc_srvpar.Replace("accion", "Accion");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoint/"), lc_srvpar);

                //Generar Log
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Payment";
                logSales.Metodo = "SCOINT";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

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
                        //Devolver error a vista
                        Session.Remove("Secuencia");

                        //Generar Log
                        logSales.Id = Guid.NewGuid().ToString();
                        logSales.Fecha = DateTime.Now;
                        logSales.Programa = "Pages/Payment";
                        logSales.Metodo = "SCOINT";
                        logSales.ExceptionMessage = "Validacion respuesta servicio Llave Validación";
                        logSales.InnerExceptionMessage = ob_diclst["Validación"].ToString() + " SECUENCIA: " + KeySecuencia + "-PUNTOVTA: " + config.Value.PuntoVenta;

                        //Escribir Log
                        logAudit.LogApp(logSales);

                        //Devolver vista de error
                        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                    }
                    else
                    {
                        //Validar respuesta llave 2
                        if (ob_diclst.ContainsKey("Respuesta"))
                        {
                            if (ob_diclst["Respuesta"].ToString() != "Proceso exitoso.")
                            {
                                //Crear transacción del registro
                                using (var transaction = new DataDB(config))
                                {
                                    var transactionSales = new TransactionSales
                                    {
                                        Secuencia = Convert.ToDecimal(KeySecuencia),
                                        PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                                        Teatro = Convert.ToDecimal(KeyTeatro),
                                        EmailEli = EmailEli,
                                        NombreEli = NombreEli + " " + ApellidoEli,
                                        DocumentoEli = DocumentoEli,
                                        TelefonoEli = TelefonoEli,
                                        EstadoTx = "ERR",
                                        FechaTx = DateTime.Now,
                                        ValorTx = 0,
                                        BaseTx = 0,
                                        IvaTx = 0,
                                        IcoTx = 0,
                                        AutorizacionTx = "-",
                                        ReferenciaTx = "-",
                                        ReferenciaEx = "VENTA RECHAZADA SOLO SCORE ",
                                        BancoTx = "-",
                                        Observaciones = ob_diclst["Respuesta"].ToString(),
                                        FechaCreado = DateTime.Now,
                                        FechaModificado = DateTime.Now
                                    };

                                    //Adicionar y guardar registro a tabla
                                    transaction.TransactionSales.Add(transactionSales);
                                    transaction.SaveChanges();
                                }

                                //Devolver error a vista
                                Session.Remove("Secuencia");

                                //Generar Log
                                logSales.Id = Guid.NewGuid().ToString();
                                logSales.Fecha = DateTime.Now;
                                logSales.Programa = "Pages/Payment";
                                logSales.Metodo = "SCOINT";
                                logSales.ExceptionMessage = "Validacion respuesta servicio Venta Rechazada";
                                logSales.InnerExceptionMessage = ob_diclst["Respuesta"].ToString() + " SECUENCIA: " + KeySecuencia + "-PUNTOVTA: " + config.Value.PuntoVenta;

                                //Escribir Log
                                logAudit.LogApp(logSales);

                                //Devolver vista de error
                                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                            }
                            else
                            {
                                if (lc_tipven == "B")
                                {
                                    Base = Math.Round(Convert.ToDecimal(ob_diclst["Total"].ToString()), 2);
                                    Total = Math.Round(Convert.ToDecimal(ob_diclst["Total"].ToString()), 2);
                                }
                                else if (lc_tipven == "M")
                                {
                                    Base = Math.Round(Convert.ToDecimal(ob_diclst["Boletas+Base"].ToString()), 2);
                                    Total = Math.Round(Convert.ToDecimal(ob_diclst["Total"].ToString()), 2);
                                }
                                else
                                {
                                    Base = Math.Round(Convert.ToDecimal(ob_diclst["Base"].ToString()), 2);
                                    Total = Math.Round(Convert.ToDecimal(ob_diclst["Total"].ToString()), 2);
                                }

                                if (ob_diclst["Impuesto_1"].ToString() != "0")
                                {
                                    //Validar impoconsumo 1
                                    if (ob_diclst["TipoImpuesto_1"].ToString().Contains("8%"))
                                        Impuesto_1 = Math.Round(Convert.ToDecimal(ob_diclst["Impuesto_1"].ToString()), 2);

                                    //Validar iva 1
                                    if (ob_diclst["TipoImpuesto_1"].ToString().Contains("19%"))
                                        Impuesto_2 = Math.Round(Convert.ToDecimal(ob_diclst["Impuesto_1"].ToString()), 2);
                                }

                                if (ob_diclst["Impuesto_2"].ToString() != "0")
                                {
                                    //Validar impoconsumo 1
                                    if (ob_diclst["TipoImpuesto_2"].ToString().Contains("8%"))
                                        Impuesto_1 = Math.Round(Convert.ToDecimal(ob_diclst["Impuesto_2"].ToString()), 2);

                                    //Validar iva 1
                                    if (ob_diclst["TipoImpuesto_2"].ToString().Contains("19%"))
                                        Impuesto_2 = Math.Round(Convert.ToDecimal(ob_diclst["Impuesto_2"].ToString()), 2);
                                }

                                CashBack_Acumulado = Math.Round(Convert.ToDecimal(ob_diclst["CashBack_Acumulado"].ToString()), 2);

                                //Crear transacción del registro
                                using (var transaction = new DataDB(config))
                                {
                                    var transactionSales = new TransactionSales
                                    {
                                        Secuencia = Convert.ToDecimal(KeySecuencia),
                                        PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                                        Teatro = Convert.ToDecimal(KeyTeatro),
                                        EmailEli = EmailEli,
                                        NombreEli = NombreEli + " " + ApellidoEli,
                                        DocumentoEli = DocumentoEli,
                                        TelefonoEli = TelefonoEli,
                                        EstadoTx = "SCO",
                                        FechaTx = DateTime.Now,
                                        ValorTx = Total,
                                        BaseTx = Base,
                                        IvaTx = Impuesto_2,
                                        IcoTx = Impuesto_1,
                                        AutorizacionTx = "-",
                                        ReferenciaTx = "-",
                                        ReferenciaEx = "-",
                                        BancoTx = "-",
                                        Observaciones = "VENTA SOLO SCORE",
                                        FechaCreado = DateTime.Now,
                                        FechaModificado = DateTime.Now
                                    };

                                    //Adicionar y guardar registro a tabla
                                    transaction.TransactionSales.Add(transactionSales);
                                    transaction.SaveChanges();
                                }
                            }
                        }
                    }
                }
                else
                {
                    //Devolver error a vista
                    Session.Remove("Secuencia");

                    //Generar Log
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "Pages/Payment";
                    logSales.Metodo = "SCOINT";
                    logSales.ExceptionMessage = "Validacion respuesta servicio";
                    logSales.InnerExceptionMessage = "Error en Secuencia" + " SECUENCIA: " + KeySecuencia + "-PUNTOVTA: " + config.Value.PuntoVenta;

                    //Escribir Log
                    logAudit.LogApp(logSales);

                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                }
                #endregion

                //Validar redireccion si hay pago con cashback
                if (pr_datpro.SwitchCashback == "S")
                {
                    return RedirectToAction("Responses", "Pages", new { ref_payco = "CashBack:" + Total.ToString() });
                }
                else
                {
                    return RedirectToAction("Finish", "Pages", new { pr_secpro = pr_datpro.KeySecuencia, pr_swtven = pr_datpro.SwtVenta, pr_valven = Total, pr_valimp = Impuesto_1, pr_valiva = Impuesto_2, pr_valbas = Base, pr_casbck = CashBack_Acumulado });
                }
            }
            catch (Exception lc_syserr)
            {
                //Crear transacción del registro
                using (var transaction = new DataDB(config))
                {
                    var transactionSales = new TransactionSales
                    {
                        Secuencia = Convert.ToDecimal(KeySecuencia),
                        PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                        Teatro = Convert.ToDecimal(KeyTeatro),
                        EmailEli = EmailEli,
                        NombreEli = NombreEli + " " + ApellidoEli,
                        DocumentoEli = DocumentoEli,
                        TelefonoEli = TelefonoEli,
                        EstadoTx = "ERR",
                        FechaTx = DateTime.Now,
                        ValorTx = 0,
                        BaseTx = 0,
                        IvaTx = 0,
                        IcoTx = 0,
                        AutorizacionTx = "-",
                        ReferenciaTx = "-",
                        ReferenciaEx = "VENTA RECHAZADA SOLO SCORE ",
                        BancoTx = "-",
                        Observaciones = lc_syserr.Message,
                        FechaCreado = DateTime.Now,
                        FechaModificado = DateTime.Now
                    };

                    //Adicionar y guardar registro a tabla
                    transaction.TransactionSales.Add(transactionSales);
                    transaction.SaveChanges();
                }

                //Devolver vista de error
                Session.Remove("Secuencia");

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/Payment";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        /// <summary>
        /// POST: FinishProg -- Proceso de ejecución SCORET para devolver transaccion
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("DevolverTransaccion")]
        public ActionResult CancelPayment(Paymentez pr_datpay)
        {
            #region VARIABLES LOCALES
            string lc_usuepy = string.Empty;
            string lc_pwdepy = string.Empty;
            string lc_tokens = string.Empty;
            string lc_traepy = string.Empty;
            string lc_srvcod = string.Empty;
            string lc_srvkey = string.Empty;
            string lc_unvtok = string.Empty;
            string lc_auttok = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_jsonst = string.Empty;
            string lc_result = string.Empty;
            string pr_objson = string.Empty;
            string lc_feccod = string.Empty;

            Dictionary<string, string> ob_diclst;
            TransactionSales ob_repsle = new TransactionSales();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                #region SERVICO SCORET
                //Json de servicio RET
                pr_objson = "{\"Punto\":" + Convert.ToInt32(config.Value.PuntoVenta) + ",\"Pedido\":" + Convert.ToInt32(pr_datpay.KeySecuencia) + ",\"teatro\":\"" + Convert.ToInt32(pr_datpay.KeyTeatro) + "\",\"tercero\":\"" + config.Value.ValorTercero + "\"}";

                //Encriptar Json RET
                lc_srvpar = ob_fncgrl.EncryptStringAES(pr_objson);

                //Consumir servicio RET
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoret/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/CancelPayment";
                logSales.Metodo = "GET";
                logSales.ExceptionMessage = lc_srvpar;
                logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta SEC
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                    //Validar respuesta llave 1
                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
                    }
                    else
                    {
                        //Validar respuesta llave 2
                        if (ob_diclst.ContainsKey("Respuesta"))
                        {
                            if (ob_diclst["Respuesta"].ToString() == "Proceso exitoso")
                            {
                                //Inicializar instancia de BD
                                decimal Secuencia = Convert.ToDecimal(Session.GetString("Secuencia"));
                                decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                                decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
                                using (var context = new DataDB(config))
                                {
                                    //Consultar registro de venta en BD transacciones
                                    var ob_repsl1 = context.TransactionSales.Where(x => x.Secuencia == Secuencia).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.Teatro == KeyTeatro);
                                    foreach (var TransactionSales in ob_repsl1)
                                        ob_repsle = context.TransactionSales.Find(TransactionSales.Id);

                                    ob_repsle.EstadoTx = "REE";
                                    ob_repsle.Observaciones = "VENTA ANULADA SOLO SCORE";

                                    ob_repsle.DocumentoEli = Session.GetString("Documento");
                                    ob_repsle.AutorizacionTx = string.Concat(Secuencia, ",", "Cancel");
                                    ob_repsle.FechaModificado = DateTime.Now;

                                    //Actualizar estado de transacción
                                    context.TransactionSales.Update(ob_repsle);
                                    context.SaveChanges();
                                }

                                //Validar y remover sesion invitada
                                if (Session.GetString("FlagLogin") == "INV")
                                {
                                    Session.Remove("Nombre");
                                    Session.Remove("Passwrd");
                                    Session.Remove("Usuario");
                                    Session.Remove("Apellido");
                                    Session.Remove("Telefono");
                                    Session.Remove("Direccion");
                                    Session.Remove("Documento");
                                    Session.Remove("ClienteFrecuente");
                                    Session.Remove("FlagLogin");
                                    ViewBag.ListCarritoR = null;
                                    ViewBag.ListCarritoB = null;
                                }

                                //Quitar secuencia
                                Session.Remove("Secuencia");
                                Session.Remove("CashBack_Acumulado");
                                return RedirectToAction("Home", "Home");
                            }
                            else
                            {
                                //Generar Log
                                logSales.Id = Guid.NewGuid().ToString();
                                logSales.Fecha = DateTime.Now;
                                logSales.Programa = "Pages/CancelPayment";
                                logSales.Metodo = "SCORET";
                                logSales.ExceptionMessage = "Validacion servicio reembolso";
                                logSales.InnerExceptionMessage = ob_diclst["Respuesta"].ToString();

                                //Escribir Log
                                logAudit.LogApp(logSales);

                                //Devolver vista de error
                                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                            }
                        }
                        else
                        {
                            //Generar Log
                            logSales.Id = Guid.NewGuid().ToString();
                            logSales.Fecha = DateTime.Now;
                            logSales.Programa = "Pages/CancelPayment";
                            logSales.Metodo = "SCORET";
                            logSales.ExceptionMessage = "Validacion servicio reembolso llave Respuesta";
                            logSales.InnerExceptionMessage = "Proceso Fallido";

                            //Escribir Log
                            logAudit.LogApp(logSales);

                            //Devolver vista de error
                            return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                        }
                    }
                }
                else
                {
                    //Generar Log
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "Pages/CancelPayment";
                    logSales.Metodo = "SCORET";
                    logSales.ExceptionMessage = "Validacion servicio reembolso llave 0";
                    logSales.InnerExceptionMessage = "Reembolso no culminado con exito SCORET";

                    //Escribir Log
                    logAudit.LogApp(logSales);

                    //Devolver vista de error
                    return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                }
                #endregion
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/CancelPayment";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Quitar secuencia
                Session.Remove("Secuencia");
                Session.Remove("CashBack_Acumulado");
                return RedirectToAction("Home", "Home");
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
        /// Método para cargar URL de Header y Footer del portal
        /// </summary>
        /// <returns></returns>
        private void URLPortal(IOptions<MyConfig> config)
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

            //Validar ciudad
            if (Session.GetString("CiudadTeatro") != null)
                ViewBag.NombreCiudad = Session.GetString("CiudadTeatro");
            else
                ViewBag.NombreCiudad = "Sin Ciudad";

            //Validar teatro
            if (Session.GetString("TeatroNombre") != null)
                ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
            else
                ViewBag.NombreCiudadTeatro = "Sin Teatro";

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

            ViewBag.ListCarritoB = null;
            ViewBag.ListCarritoR = null;
        }

        /// <summary>
        /// Método para obtener lista de carrito de compras
        /// </summary>
        //private void ListCarrito()
        //{
        //    #region VARIABLES LOCALES
        //    decimal lc_secsec = 0;
        //    #endregion

        //    //Validar secuencia y asignar valores
        //    ViewBag.Venta = "V";
        //    ViewBag.Secuencia = Session.GetString("Secuencia");
        //    ViewBag.ListCarritoB = null;
        //    ViewBag.ListCarritoR = null;
        //    ViewBag.NombreTeatro = Session.GetString("TeatroNombre");

        //    if (Session.GetString("Secuencia") != null)
        //    {
        //        //Obtener productos carrito de compra
        //        lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
        //        using (var context = new DataDB(config))
        //        {
        //            //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
        //            decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
        //            decimal KeyTeatro =  Convert.ToDecimal(Session.GetString("Teatro"));
        //            var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
        //            ViewBag.ListCarritoR = RetailSales;
        //        }

        //        //Obtener boletas carrito de compra
        //        using (var context = new DataDB(config))
        //        {
        //            //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia
        //            string PuntoVenta = config.Value.PuntoVenta;
        //            string KeyTeatro = Session.GetString("Teatro");
        //            var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
        //            ViewBag.ListCarritoB = ReportSales;
        //        }

        //        if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count == 0)
        //            ViewBag.TipoV = "B";
        //        if (ViewBag.ListCarritoB.Count == 0 && ViewBag.ListCarritoR.Count != 0)
        //            ViewBag.TipoV = "P";
        //        if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count != 0)
        //            ViewBag.TipoV = "M";
        //    }
        //}

        private void ListCarrito()
        {
            #region VARIABLES LOCALES
            decimal lc_secsec = 0;
            var PuntoVenta = config.Value.PuntoVenta;
            var KeyTeatro = Session.GetString("Teatro");
            #endregion

            //Validar secuencia y asignar valores
            ViewBag.Venta = "V";
            ViewBag.Secuencia = Session.GetString("Secuencia");
            ViewBag.ListCarritoB = null;
            ViewBag.ListCarritoR = null;
            ViewBag.NombreTeatro = Session.GetString("TeatroNombre");

            if (Session.GetString("Secuencia") != null)
            {
                //Obtener productos carrito de compra
                lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
                using (var context = new DataDB(config))
                {
                    var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).ToList();
                    ViewBag.ListCarritoR = RetailSales;

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
        /// ELIMINAR TRANSACCION POR ERROR EN EL PROCESO
        /// </summary>
        /// <param name="pr_secuencia">Secuenta de venta</param>
        /// <param name="pr_puntovta">Punto de venta</param>
        /// <param name="pr_teatro">id teatro</param>
        /// <param name="pr_tercero">id tercero</param>
        private void CancelPaymentError(int pr_secuencia, int pr_puntovta, int pr_teatro, string pr_tercero)
        {
            #region VARIABLES LOCALES
            string lc_srvpar = string.Empty;
            string lc_jsonst = string.Empty;
            string lc_result = string.Empty;
            string pr_objson = string.Empty;

            bool lc_flgdel;

            ReportSales ob_repsle = new ReportSales();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //Validar si hay pago con epayco
                using (var context = new DataDB(config))
                {
                    decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
                    decimal KeyTeatro = Convert.ToDecimal(pr_teatro);
                    decimal Secuencia = Convert.ToDecimal(pr_secuencia);
                    var TransactionSales = context.TransactionSales.Where(x => x.Secuencia == Secuencia).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.Teatro == KeyTeatro).ToList();

                    if (TransactionSales.Count > 0)
                    {
                        string EstadoTx = string.Empty;
                        foreach (var transactionSales in TransactionSales)
                            EstadoTx = transactionSales.EstadoTx; //Obtener estado

                        //Validar estado
                        if (EstadoTx == "SCO")
                            lc_flgdel = true;
                        else
                            lc_flgdel = false;
                    }
                    else
                    {
                        lc_flgdel = true;
                    }


                }

                if (lc_flgdel)
                {
                    #region SERVICO SCORET
                    //Json de servicio RET
                    pr_objson = "{\"Punto\":" + pr_puntovta + ",\"Pedido\":" + pr_secuencia + ",\"teatro\":\"" + pr_teatro + "\",\"tercero\":\"" + pr_tercero + "\"}";

                    //Encriptar Json RET
                    lc_srvpar = ob_fncgrl.EncryptStringAES(pr_objson);

                    //Consumir servicio RET
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoret/"), lc_srvpar);

                    //Generar Log
                    LogSales logSales = new LogSales();
                    LogAudit logAudit = new LogAudit(config);
                    logSales.Id = Guid.NewGuid().ToString();
                    logSales.Fecha = DateTime.Now;
                    logSales.Programa = "Pages/CancelPaymentError";
                    logSales.Metodo = "SCORET";
                    logSales.ExceptionMessage = lc_srvpar;
                    logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    //logAudit.LogApp(logSales);

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
                            //Eliminar item y guardar registro a tabla
                            context.ReportSales.Remove(report);
                            context.SaveChanges();
                        }
                    }

                    //Validar y remover sesion invitada
                    if (Session.GetString("FlagLogin") == "INV")
                    {
                        Session.Remove("Nombre");
                        Session.Remove("Passwrd");
                        Session.Remove("Usuario");
                        Session.Remove("Apellido");
                        Session.Remove("Telefono");
                        Session.Remove("Direccion");
                        Session.Remove("Documento");
                        Session.Remove("ClienteFrecuente");
                        Session.Remove("FlagLogin");
                        ViewBag.ListCarritoR = null;
                        ViewBag.ListCarritoB = null;
                    }
                    #endregion
                }

            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "Pages/CancelPaymentError";
                logSales.Metodo = "METHOD";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);
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
                        string auxCiudad = Regex.Replace(item.Normalize(System.Text.NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
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
            /*if (pr_url.Contains("room") || pr_url.Contains("detail"))
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

                //Validar secuencia
                string secuencia = Session.GetString("Secuencia");

                if (!string.IsNullOrEmpty(secuencia))
                {
                    decimal lc_secsec1 = Convert.ToDecimal(secuencia);

                    using (var context = new DataDB(config))
                    {
                        var PuntoVenta = config.Value.PuntoVenta;
                        var KeyTeatro = Session.GetString("Teatro");

                        // Borrar retail
                        var RetailSalesToDelete = context.RetailSales
                            .Where(x => x.Secuencia == lc_secsec1 && x.PuntoVenta == Convert.ToDecimal(PuntoVenta) && x.KeyTeatro == Convert.ToDecimal(KeyTeatro))
                            .ToList();

                        foreach (var retail in RetailSalesToDelete)
                        {
                            // Borrar retail detcombo
                            var RetailDetToDelete = context.RetailDet
                                .Where(x => x.IdRetailSales == retail.Id)
                                .ToList();

                            foreach (var retaildet in RetailDetToDelete)
                            {
                                context.RetailDet.Remove(retaildet);
                            }

                            context.RetailSales.Remove(retail);
                        }

                        // Borrar boletas
                        var ReportSalesToDelete = context.ReportSales
                            .Where(x => x.Secuencia == secuencia && x.KeyPunto == PuntoVenta && x.KeyTeatro == Session.GetString("Teatro"))
                            .ToList();

                        foreach (var report in ReportSalesToDelete)
                        {
                            var selUbicaciones = report.SelUbicaciones;
                            char[] ar_charst = selUbicaciones.ToCharArray();

                            foreach (var item in ls_lstubi)
                            {
                                lc_idearr = 0;
                                char[] ar_chars2 = item.ToCharArray();

                                for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
                                {
                                    lc_auxitm += ar_chars2[lc_iditem].ToString();

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

                                // Generar y encriptar JSON para servicio
                                lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

                                lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
                                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                                lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                                lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                                lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

                                // Encriptar Json LIB
                                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                                // Consumir servicio LIB
                                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
                                #endregion
                            }

                            context.ReportSales.Remove(report);
                        }

                        // Quitar secuencia
                        Session.Remove("Secuencia");

                        context.SaveChanges();
                    }
                }
            }*/

            //Cargar ciudad seleccionada
            Ciuteatros(pr_ciudad);

            //Cargar Teatro
            Session.SetString("Teatro", pr_teatro);
            Session.SetString("TeatroNombre", pr_nomteatro);
            ViewBag.NombreCiudadTeatro = pr_nomteatro;
        }
        #endregion
    }
}