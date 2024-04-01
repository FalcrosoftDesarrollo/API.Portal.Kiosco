/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : SalesConController.cs                                                    *
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
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace APIPortalKiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesConController : Controller
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
        public SalesConController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        #region GET
        /// <summary>
        /// GET: ProductList -- Listado de productos para ventas por el portal web
        /// </summary>
        /// <param name="pr_secpro">Secuencia tran</param>
        /// <param name="pr_swtven">Switch Venta</param>
        /// <param name="pr_tiplog">Tipo Compra</param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ListarProductosVentasPortalWeb")]
        public Producto ProductList(string pr_secpro, string pr_swtven, string pr_tiplog, string pr_tbview = "", string Teatro = "0", string Ciudad = "0")
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitem = string.Empty;

            List<Producto> ob_return = new List<Producto>();
            List<Producto> ob_result = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            //LisBillboard listaBillboard = new LisBillboard();
            Producto producto = new Producto();
            Secuencia ob_scopre = new Secuencia();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //Validar redireccion externa
                if (pr_secpro == null)
                    pr_secpro = "0";
                if (pr_swtven == null)
                    pr_swtven = "V";
                if (pr_tiplog == null)
                    pr_tiplog = "P";
                if (pr_tbview == null)
                    pr_tbview = "";

                //Inicializar valores de entrada
                //ViewBag.pr_secpro = pr_secpro;
                //ViewBag.pr_swtven = pr_swtven;
                //ViewBag.pr_tiplog = pr_tiplog;
                //ViewBag.pr_tbview = pr_tbview;

                ////Session carrito de compras
                //Session.Remove("pr_tbview");
                //Session.SetString("pr_tbview", pr_tbview);
                //Session.Remove("pr_secpro");
                //Session.SetString("pr_secpro", pr_secpro);
                //Session.Remove("pr_swtven");
                //Session.SetString("pr_swtven", pr_swtven);
                //Session.Remove("pr_tiplog");
                //Session.SetString("pr_tiplog", pr_tiplog);

                //URLPortal(config);

                ////Validar ciudad y teatro desde web externa
                //if (Teatro != "0")
                //    Selteatros(Teatro);

                ////Validar seleccion de teatro
                //if (Session.GetString("Teatro") == null)
                //{
                //    //Devolver vista de error
                //    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
                //}


                //ListCarrito();

                //Inicializar variables
                //listaBillboard.ListaM = null;
                //ViewBag.alertS = false;
                //ViewBag.CantidadProductos = config.Value.CantProductos;
                //ViewBag.UrlRetailImg = config.Value.UrlRetailImg;
                //ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente");
                //ViewBag.Tipo = pr_tiplog;

                ViewBag.Teatro = Session.GetString("TeatroNombre");
                ViewBag.Correo = Session.GetString("Usuario");
                ViewBag.Nombre = Session.GetString("Nombre");
                ViewBag.Apellido = Session.GetString("Apellido");
                ViewBag.Telefono = Session.GetString("Telefoho");
                ViewBag.KeyTeatro = Session.GetString("Teatro");

                ViewBag.CombosWeb = null;
                ViewBag.AlimentosWeb = null;
                ViewBag.BebidassWeb = null;
                ViewBag.SnacksWeb = null;

                if (Session.GetString("Secuencia") != null)
                    ViewBag.Secuencia = Session.GetString("Secuencia");
                else
                    ViewBag.Secuencia = pr_secpro;

                ViewBag.SwitchVenta = pr_swtven;

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

                ////Consumir servicio PRE
                //if (ViewBag.ClientFrecnt == "No")
                //    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);
                //else
                //    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopcf/"), lc_srvpar);

                //Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesCon/ProductList";
                //logSales.Metodo = "GET";
                //logSales.ExceptionMessage = lc_srvpar;
                //logSales.InnerExceptionMessage = lc_result;

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
                    {
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
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
                return producto;
            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesCon/ProductList";
                //logSales.Metodo = "GET";
                //logSales.ExceptionMessage = lc_syserr.Message;
                //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                ////Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                return producto;
            }
        }

        /// <summary>
        /// GET: ProductList -- Listado de productos para ventas por el portal web
        /// </summary>
        /// <param name="pr_secpro">Secuencia tran</param>
        /// <param name="pr_swtven">Switch Venta</param>
        /// <param name="pr_tiplog">Tipo Compra</param>
        /// <returns></returns>

        [HttpGet]
        [Route("AddProduct")]
        public ActionResult AddProduct(string pr_secpro, string pr_swtven, string pr_tiplog, string pr_tbview = "")
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

                URLPortal(config);
                ListCarrito();

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

                ViewBag.CombosWeb = null;
                ViewBag.AlimentosWeb = null;
                ViewBag.BebidassWeb = null;
                ViewBag.SnacksWeb = null;

                //ViewBag.ClientFrecnt = "No";

                if (Session.GetString("Secuencia") != null)
                    ViewBag.Secuencia = Session.GetString("Secuencia");
                else
                    ViewBag.Secuencia = pr_secpro;

                ViewBag.SwitchVenta = pr_swtven;

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
                logSales.Programa = "SalesCon/Addproduct";
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
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
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
                        List<Producto> AdicionesWeb = new List<Producto>();

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
                                        break;

                                    case "ALIMENTOS WEB":
                                        int lc_cntali = AlimentosWeb.Count();

                                        AlimentosWeb.Add(item);
                                        AlimentosWeb[lc_cntali].OrdenView = pantallas.Orden;
                                        break;

                                    case "BEBIDAS WEB":
                                        int lc_cntbeb = BebidasWeb.Count();

                                        BebidasWeb.Add(item);
                                        BebidasWeb[lc_cntbeb].OrdenView = pantallas.Orden;
                                        break;

                                    case "SNACKS WEB":
                                        int lc_cntsnk = SnacksWeb.Count();

                                        SnacksWeb.Add(item);
                                        SnacksWeb[lc_cntsnk].OrdenView = pantallas.Orden;
                                        break;

                                    case "ADICIONES WEB":
                                        int lc_cntadd = AdicionesWeb.Count();

                                        AdicionesWeb.Add(item);
                                        AdicionesWeb[lc_cntadd].OrdenView = pantallas.Orden;
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

                        //Asignar lista de adiciones
                        ViewBag.ListaA = AdicionesWeb.OrderBy(o => o.OrdenView).ToList();
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
                logSales.Programa = "SalesCon/Addproduct";
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
        /// GET: Details -- Cargar vista de detalle producto seleccionado
        /// </summary>
        /// <param name="pr_keypro">Id Producto</param>
        /// <param name="pr_secpro">Secuencia tran</param>
        /// <param name="pr_swtven">Switch Venta</param>
        /// <param name="pr_tiplog">Tipo Compra</param>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("CargarVistaDetalleProducto")]
        public Producto Details(string pr_keypro, string pr_secpro, string pr_swtven, string pr_tiplog, string pr_swtadd)
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitem = string.Empty;
            int CodigoBebidas = 1244;
            int CodigoBebidas2 = 2444;
            int CodigoComidas = 246;
            List<(decimal CodigoBotella, string NombreFinalBotella, decimal PrecioFinalBotella, string frecuenciaBotella, decimal categoria)> datosFinalesBotella = new List<(decimal, string, decimal, string, decimal)>();
            List<(decimal CodigoComida, string NombreFinalComida, decimal PrecioFinalComida, string frecuenciaComida, decimal categoria)> datosFinalesComida = new List<(decimal, string, decimal, string, decimal)>();

            List<Producto> ob_return = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            LisBillboard listaBillboard = new LisBillboard();
            Secuencia ob_scopre = new Secuencia();
            Producto producto = new Producto();
            General ob_fncgrl = new General();
            #endregion

            ////Inicializar variables
            //listaBillboard.ListaM = null;
            ////ViewBag.alertS = false;
            //producto.UrlRetailImg = config.Value.UrlRetailImg;
            //producto.CantidadProductos = config.Value.CantProductos;
            //producto.Secuencia = pr_secpro;
            //producto.SwitchVenta = pr_swtven;
            //producto.Tipo = pr_tiplog;
            //producto.SwitchAdd = pr_swtadd;
            //ViewBag.ListaB = null;
            //ViewBag.ListaC = null;

            //Session carrito de compras
            //Session.Remove("pr_keypro");
            //Session.SetString("pr_keypro", pr_keypro);
            //Session.Remove("pr_secpro");
            //Session.SetString("pr_secpro", pr_secpro);
            //Session.Remove("pr_swtven");
            //Session.SetString("pr_swtven", pr_swtven);
            //Session.Remove("pr_tiplog");
            //Session.SetString("pr_tiplog", pr_tiplog);

            try
            {
                //URLPortal(config);
                //ListCarrito();

                //Validar inicio de sesión
                //if (Session.GetString("Usuario") == null)
                //    return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "PX" });

                //InternetSales.ClienteFrecuente = Session.GetString("ClienteFrecuente"); //"No;"

                producto.Codigo = Convert.ToDecimal(pr_keypro);
                producto.SwtVenta = pr_swtven;
                producto.EmailEli = Session.GetString("Usuario");
                producto.NombreEli = Session.GetString("Nombre");
                producto.KeyTeatro = Session.GetString("Teatro");
                producto.DesTeatro = Session.GetString("TeatroNombre");
                producto.ApellidoEli = Session.GetString("Apellido");
                producto.TelefonoEli = Session.GetString("Telefono");
                producto.KeySecuencia = pr_secpro;

                #region SERVICIO SCOPRE
                //Asignar valores PRE
                ob_scopre.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                ob_scopre.Teatro = Convert.ToInt32(producto.KeyTeatro);
                ob_scopre.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio PRE
                lc_srvpar = ob_fncgrl.JsonConverter(ob_scopre);
                lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                //Encriptar Json PRE
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                ////Consumir servicio PRE
                //if (ViewBag.ClientFrecnt == "No")
                //    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);
                //else
                //    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopcf/"), lc_srvpar);

                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesCon/Details";
                //logSales.Metodo = "SCOPRE";
                //logSales.ExceptionMessage = lc_srvpar;
                //logSales.InnerExceptionMessage = lc_result;

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
                    {
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());
                    }
                    else
                    {
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
                                        break;

                                    case "ALIMENTOS WEB":
                                        int lc_cntali = AlimentosWeb.Count();

                                        AlimentosWeb.Add(item);
                                        AlimentosWeb[lc_cntali].OrdenView = pantallas.Orden;
                                        break;

                                    case "BEBIDAS WEB":
                                        int lc_cntbeb = BebidasWeb.Count();

                                        BebidasWeb.Add(item);
                                        BebidasWeb[lc_cntbeb].OrdenView = pantallas.Orden;
                                        break;

                                    case "SNACKS WEB":
                                        int lc_cntsnk = SnacksWeb.Count();

                                        SnacksWeb.Add(item);
                                        SnacksWeb[lc_cntsnk].OrdenView = pantallas.Orden;
                                        break;
                                }
                            }
                        }

                        //producto.ListaM = CombosWeb.OrderBy(o => o.OrdenView).ToList();
                    }
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
                    if (itepro.Codigo == producto.Codigo)
                    {
                        switch (itepro.Tipo)
                        {

                            case "P": //PRODUCTOS
                                producto.Codigo = itepro.Codigo;
                                producto.Descripcion = itepro.Descripcion;
                                producto.Tipo = itepro.Tipo;
                                producto.Precios = itepro.Precios;
                                break;

                            case "C": //COMBOS
                                producto.Codigo = itepro.Codigo;
                                producto.Descripcion = itepro.Descripcion;
                                producto.Tipo = itepro.Tipo;
                                producto.Receta = itepro.Receta;
                                producto.Precios = itepro.Precios;
                                List<Precios> precio_Combo = new List<Precios>();
                                producto.Precios = precio_Combo;

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
                                producto.Tipo = itepro.Tipo;
                                producto.Check = string.Empty;
                                producto.Codigo = itepro.Codigo;
                                producto.Descripcion = itepro.Descripcion;

                                List<Receta> ob_recpro = new List<Receta>();
                                List<Precios> ob_prepro = new List<Precios>();
                                List<Producto> ob_lispro = new List<Producto>();
                                List<Pantallas> ob_panpro = new List<Pantallas>();

                                producto.Receta = ob_recpro;
                                producto.Precios = ob_prepro;
                                producto.Pantallas = ob_panpro;
                                producto.LisProducto = ob_lispro;

                                foreach (var itecat in itepro.Receta)
                                {
                                    Producto ob_itecat = new Producto();
                                    ob_itecat.Tipo = itecat.Tipo;
                                    ob_itecat.Check = string.Empty;
                                    ob_itecat.Codigo = itecat.Codigo;
                                    ob_itecat.Precios = itecat.Precios;
                                    ob_itecat.Cantidad = itecat.Cantidad;
                                    ob_itecat.Descripcion = itecat.Descripcion;

                                    producto.LisProducto.Add(ob_itecat);
                                }

                                break;
                        }

                        //Romper el ciclo
                        break;
                    }
                }
                //ViewBag.ListaB = datosFinalesBotella.Distinct().ToList();
                //ViewBag.ListaC = datosFinalesComida.Distinct().ToList();
                //Asignar valores encriptados
                producto.SwtVenta = pr_swtven;
                producto.EmailEli = Session.GetString("Usuario");
                producto.NombreEli = Session.GetString("Nombre");
                producto.KeyTeatro = Session.GetString("Teatro");
                producto.DesTeatro = Session.GetString("TeatroNombre");
                producto.TipoCompra = pr_tiplog;
                producto.ApellidoEli = Session.GetString("Apellido");
                producto.TelefonoEli = Session.GetString("Telefono");
                producto.KeySecuencia = pr_secpro;

                return producto;
            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesCon/Details";
                //logSales.Metodo = "GET";
                //logSales.ExceptionMessage = lc_syserr.Message;
                //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                ////Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                return producto;
            }
        }
        #endregion

        #region POST
        /// <summary>
        /// POST: Details -- Generar item a carrito de compra producto
        /// </summary>
        /// <param name="pr_datpro">Entidad producto seleccionado</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GenerarDetalleCarritoCompra")]
        public ActionResult Details(Producto pr_datpro)
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

                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente"); //"No";

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
                    logSales.Programa = "SalesCon/Details";
                    logSales.Metodo = "SCOSEC";
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
                        ob_seclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));

                        //Validar respuesta del servicio
                        if (ob_seclst.ContainsKey("Validación"))
                            ModelState.AddModelError("", ob_seclst["Validación"].ToString());
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
                        return View(new { pr_keypro = pr_datpro.Codigo.ToString(), pr_secpro = pr_datpro.KeySecuencia, pr_keytea = pr_datpro.KeyTeatro, pr_destea = pr_datpro.DesTeatro, pr_swtven = pr_datpro.SwtVenta, pr_tiplog = pr_datpro.TipoCompra, pr_coreli = pr_datpro.EmailEli, pr_nomeli = pr_datpro.NombreEli, pr_apeeli = pr_datpro.ApellidoEli, pr_teleli = pr_datpro.TelefonoEli });
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
                return RedirectToAction("ProductList", "SalesCon", new { pr_secpro = lc_secsec.ToString(), pr_swtven = "V", pr_tiplog = pr_datpro.TipoCompra });

            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "SalesCon/Details";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
            }
        }

        private string ObtenerCategoria(string input)
        {
            string pattern = @"Categoria:(\d+),\d+";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(input);

            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return "No se encontró la categoría en la cadena.";
            }
        }

        [HttpPost]
        [Route("AñadirProducto")]
        public ActionResult AddProduct(Adiciones pr_addpro)
        {
            #region VARIABLES LOCALES
            Producto ob_datpro = new Producto();
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

                ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente"); //"No";

                //inicializar instancia de BD
                using (var context = new DataDB(config))
                {
                    //Recorrido para saber si hay cantidades válidas para agregar a carrito
                    int lc_idxiii = 1;
                    while (lc_idxiii <= 6)
                    {
                        switch (lc_idxiii)
                        {
                            case 1:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_1 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_1.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_1;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_1;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_1;
                                break;

                            case 2:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_2 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_2.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_2;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_2;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_2;
                                break;

                            case 3:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_3 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_3.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_3;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_3;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_3;
                                break;

                            case 4:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_4 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_4.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_4;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_4;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_4;
                                break;

                            case 5:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_5 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_5.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_5;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_5;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_5;
                                break;

                            case 6:
                                //Validar cantidad producto
                                if (pr_addpro.Cantidad_6 <= 0)
                                {
                                    lc_idxiii++;
                                    continue;
                                }

                                //Asignar valores de vista
                                ob_datpro.Valor = pr_addpro.Precio_6.ToString();
                                ob_datpro.Codigo = pr_addpro.Codigo_6;
                                ob_datpro.Cantidad = pr_addpro.Cantidad_6;
                                ob_datpro.Descripcion = pr_addpro.Descripcion_6;
                                break;
                        }

                        //Agregar valores a yabla RetailSales
                        var retailSales = new RetailSales
                        {
                            Tipo = "P",
                            Precio = Convert.ToDecimal(ob_datpro.Valor),
                            Cantidad = ob_datpro.Cantidad,
                            Secuencia = pr_addpro.Secuencia,
                            PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                            KeyProducto = ob_datpro.Codigo,
                            Descripcion = ob_datpro.Descripcion,
                            ProProducto1 = ob_datpro.ProProducto_1,
                            ProProducto2 = ob_datpro.ProProducto_2,
                            ProProducto3 = ob_datpro.ProProducto_3,
                            ProProducto4 = ob_datpro.ProProducto_4,
                            ProProducto5 = ob_datpro.ProProducto_5,
                            CanProducto1 = ob_datpro.ProCantidad_1,
                            CanProducto2 = ob_datpro.ProCantidad_2,
                            CanProducto3 = ob_datpro.ProCantidad_3,
                            CanProducto4 = ob_datpro.ProCantidad_4,
                            CanProducto5 = ob_datpro.ProCantidad_5,
                            ProCategoria1 = ob_datpro.ProCategoria_1,
                            ProCategoria2 = ob_datpro.ProCategoria_2,
                            ProCategoria3 = ob_datpro.ProCategoria_3,
                            ProCategoria4 = ob_datpro.ProCategoria_4,
                            ProCategoria5 = ob_datpro.ProCategoria_5,
                            CanCategoria1 = ob_datpro.CanCategoria_1,
                            CanCategoria2 = ob_datpro.CanCategoria_2,
                            CanCategoria3 = ob_datpro.CanCategoria_3,
                            CanCategoria4 = ob_datpro.CanCategoria_4,
                            CanCategoria5 = ob_datpro.CanCategoria_5,
                            FechaRegistro = DateTime.Now,
                            KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"))
                        };

                        //Adicionar y guardar registro a tabla
                        context.RetailSales.Add(retailSales);
                        context.SaveChanges();
                        lc_idxiii++;
                    }
                }

                //Devolver a vista
                return RedirectToAction("TermConditions", "Pages", new { pr_secsec = pr_addpro.Secuencia, pr_swtven = pr_addpro.SwitchVenta, pr_tiplog = pr_addpro.Tipo });
            }
            catch (Exception lc_syserr)
            {
                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "SalesCon/AddProduct";
                logSales.Metodo = "POST";
                logSales.ExceptionMessage = lc_syserr.Message;
                logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                //Escribir Log
                logAudit.LogApp(logSales);

                //Devolver vista de error
                return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
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
                if (ViewBag.ClientFrecnt == "No")
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopre/"), lc_srvpar);
                else
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scopcf/"), lc_srvpar);

                //Generar Log
                LogSales logSales = new LogSales();
                LogAudit logAudit = new LogAudit(config);
                logSales.Id = Guid.NewGuid().ToString();
                logSales.Fecha = DateTime.Now;
                logSales.Programa = "SalesCon/GetDetails";
                logSales.Metodo = "METHOD";
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
                logSales.Programa = "SalesCon/GetDetails";
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
        }

        /// <summary>
        /// Método para obtener lista de carrito de compras
        /// </summary> 
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
                using (var context = new DataDB(config))
                {
                    switch (lc_variii)
                    {
                        case 0:
                            if (valor1 != string.Empty && valor1 != null)
                            {
                                //Inicializar instancia de BD


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
                            break;
                        case 1:
                            if (valor2 != string.Empty && valor2 != null)
                            {
                                //Inicializar instancia de BD

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
                            break;
                        case 2:
                            if (valor3 != string.Empty && valor3 != null)
                            {
                                //Inicializar instancia de BD
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
                            break;
                        case 3:
                            if (valor4 != string.Empty && valor4 != null)
                            {
                                //Inicializar instancia de BD

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
                            break;

                    }
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

                //Validar secuenicia
                if (Session.GetString("Secuencia") != null)
                {
                    decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
                    var puntoVenta = config.Value.PuntoVenta;
                    var teatro = Session.GetString("Teatro");

                    using (var context = new DataDB(config))
                    {
                        var retailSales = context.RetailSales
                            .Where(x => x.Secuencia == lc_secsec1 && x.PuntoVenta == Convert.ToDecimal(puntoVenta) && x.KeyTeatro == Convert.ToDecimal(teatro))
                            .ToList();

                        foreach (var retail in retailSales)
                        {
                            decimal retailId = retail.Id;

                            var retailDet = context.RetailDet.Where(x => x.IdRetailSales == retailId).ToList();
                            foreach (var retaildet in retailDet)
                            {
                                context.RetailDet.Remove(retaildet);
                            }

                            context.RetailSales.Remove(retail);
                        }
                        context.SaveChanges();
                    }

                    string lc_secsec2 = Session.GetString("Secuencia");

                    using (var context = new DataDB(config))
                    {
                        var reportSales = context.ReportSales
                            .Where(x => x.Secuencia == lc_secsec2 && x.KeyPunto == puntoVenta && x.KeyTeatro == teatro)
                            .ToList();

                        foreach (var report in reportSales)
                        {
                            string selUbicaciones = report.SelUbicaciones;
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

                                LiberaSilla ob_libsrv = new LiberaSilla();
                                ob_libsrv.Fila = ls_lstsel[3];
                                ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
                                ob_libsrv.teatro = Convert.ToInt32(teatro);
                                ob_libsrv.Funcion = Convert.ToInt32(report.HorProg);
                                ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
                                ob_libsrv.Usuario = 777;
                                ob_libsrv.tercero = config.Value.ValorTercero;
                                ob_libsrv.FechaFuncion = report.FecProg;

                                lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);
                                lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
                                lc_srvpar = lc_srvpar.Replace("sala", "Sala");
                                lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
                                lc_srvpar = lc_srvpar.Replace("fila", "Fila");
                                lc_srvpar = lc_srvpar.Replace("columna", "Columna");
                                lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

                                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);
                                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
                            }

                            context.ReportSales.Remove(report);
                        }
                        context.SaveChanges();
                    }

                    Session.Remove("Secuencia");
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
