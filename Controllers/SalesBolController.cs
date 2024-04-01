/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : SalesBolController.cs                                                    *
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
using APIPortalKiosco.Models;
using Microsoft.AspNetCore.Mvc;
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
    [Route("api/[controller]")]
    [ApiController]
    public class SalesBolController : ControllerBase
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
        public SalesBolController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        #region GET
        /// <summary>
        /// GET: Detail -- Cargar vista con el detalle de la película ciudad-teatro-fecha-hora-tarifa
        /// </summary>
        /// <param name="pr_keypel">Párametro ID de película para obtener información</param>
        /// <param name="pr_fecprg">Párametro fecha de película para obtener información</param>
        /// <returns></returns>
        /// 
        //[HttpGet]
        //[Route("CargarDetPel")]
        //public ActionResult Detail(string pr_keypel, string pr_fecprg, string pr_tippel = "")
        //{
        //    #region VARIABLES LOCALES
        //    int lc_keypel = 0;
        //    int lc_auxpel = 0;
        //    int lc_keytea = 0;
        //    int lc_auxtea = 0;
        //    int lc_swtflg = 0;
        //    string Variables41TPF = string.Empty;
        //    string lc_auxitem = string.Empty;
        //    string lc_fecitem = string.Empty;
        //    string lc_flgpre = "S";

        //    string lc_result = string.Empty;
        //    string lc_srvpar = string.Empty;

        //    DateTime dt_fecpro;

        //    List<DateCartelera> ob_fechas = new List<DateCartelera>();

        //    XmlDocument ob_xmldoc = new XmlDocument();
        //    Billboard ob_bilmov = new Billboard();
        //    General ob_fncgrl = new General();

        //    Cartelera ob_carprg = new Cartelera();
        //    Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
        //    Dictionary<string, object> ob_lsala = new Dictionary<string, object>();
        //    List<sala> ob_lisprg = new List<sala>();
        //    #endregion

        //    try
        //    {
        //        URLPortal(config);

        //        //Validar inicio de sesión
        //        if (Session.GetString("Usuario") == null)
        //        {
        //            Session.Remove("IdPelicula");
        //            Session.Remove("FcPelicula");
        //            Session.Remove("TpPelicula");
        //            Session.SetString("IdPelicula", pr_keypel);
        //            Session.SetString("FcPelicula", pr_fecprg);
        //            Session.SetString("TpPelicula", pr_tippel);
        //            return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "PL" });
        //        }

        //        //Validar seleccion de teatro
        //        if (Session.GetString("Teatro") == null)
        //        {
        //            //Devolver vista de error
        //            return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
        //        }
        //        else
        //        {
        //            //Cargar ciudades home y teatro por defecto si aplica
        //            if (Session.GetString("Teatro") != null)
        //            {
        //                Ciuteatros("SEL");
        //            }
        //            else
        //            {
        //                if (Session.GetString("CiudadTeatro") != null)
        //                    Ciuteatros(Session.GetString("CiudadTeatro"));
        //                else
        //                    Ciuteatros();
        //            }
        //        }

        //        ListCarrito();

        //        //ViewBag.Fechaprog = pr_fecprg;
        //        //ViewBag.PelCodigo = pr_keypel;
        //        //ViewBag.TipoPelic = pr_tippel;

        //        List<DateCartelera> datePortal = DatePortal(pr_fecprg, pr_tippel, pr_keypel);
        //        bool flag = datePortal.Any(x => x.Flags == "S");

        //        //Validar flago rojo
        //        if (!flag)
        //            datePortal[0].Flags = "S";

        //        ////Obtener fechas de cartelera
        //        //ViewBag.Cartelera = datePortal;
        //        //if (pr_tippel != "Preventa")
        //        //    ViewBag.Cartelera[0].DiaLt = "HOY";
        //        //else
        //        //    ViewBag.Mes = ViewBag.Cartelera[0].MesLt;

        //        //Obtener información de la web
        //        #region SCOCAR
        //        ob_carprg.Teatro = Session.GetString("Teatro");
        //        ob_carprg.tercero = config.Value.ValorTercero;
        //        ob_carprg.IdPelicula = pr_keypel;
        //        ob_carprg.FcPelicula = pr_tippel != "Preventa" ? pr_fecprg : ViewBag.Cartelera[0].FecSt;
        //        ob_carprg.TpPelicula = pr_tippel;
        //        ob_carprg.FgPelicula = "2";
        //        ob_carprg.CfPelicula = Session.GetString("ClienteFrecuente");

        //        //Generar y encriptar JSON para servicio PRE
        //        lc_srvpar = ob_fncgrl.JsonConverter(ob_carprg);
        //        lc_srvpar = lc_srvpar.Replace("teatro", "Teatro");
        //        lc_srvpar = lc_srvpar.Replace("idPelicula", "IdPelicula");
        //        lc_srvpar = lc_srvpar.Replace("fcPelicula", "FcPelicula");
        //        lc_srvpar = lc_srvpar.Replace("tpPelicula", "TpPelicula");
        //        lc_srvpar = lc_srvpar.Replace("fgPelicula", "FgPelicula");
        //        lc_srvpar = lc_srvpar.Replace("cfPelicula", "CfPelicula");

        //        //Encriptar Json
        //        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //        //Consumir servicio
        //        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocar/"), lc_srvpar);

        //        //Generar Log
        //        //LogSales logSales = new LogSales();
        //        //LogAudit logAudit = new LogAudit(config);
        //        //logSales.Id = Guid.NewGuid().ToString();
        //        //logSales.Fecha = DateTime.Now;
        //        //logSales.Programa = "SalesBol/Detail";
        //        //logSales.Metodo = "SCOCAR";
        //        //logSales.ExceptionMessage = lc_srvpar;
        //        //logSales.InnerExceptionMessage = lc_result;

        //        //Escribir Log
        //        //logAudit.LogApp(logSales);

        //        //Validar respuesta
        //        if (lc_result.Substring(0, 1) == "0")
        //        {
        //            //Quitar switch
        //            lc_result = lc_result.Replace("0-", "");
        //            ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
        //            ob_bilmov = (Billboard)JsonConvert.DeserializeObject(ob_diclst["Billboard"].ToString(), (typeof(Billboard)));
        //            ob_lsala = (Dictionary<string, object>)JsonConvert.DeserializeObject(ob_diclst["GetHora"].ToString(), (typeof(Dictionary<string, object>)));

        //            ob_lisprg = (List<sala>)JsonConvert.DeserializeObject(ob_lsala["Lsala"].ToString(), (typeof(List<sala>)));
        //            ViewBag.Zonas = (Dictionary<string, string>)JsonConvert.DeserializeObject(ob_lsala["Zonas"].ToString(), (typeof(Dictionary<string, string>)));
        //            ViewBag.Hora = ob_lisprg;
        //            ViewBag.Fecha3 = ob_carprg.FcPelicula;
        //            ViewBag.Fecha2 = Convert.ToDateTime(pr_fecprg.Substring(6, 2) + "/" + pr_fecprg.Substring(4, 2) + "/" + pr_fecprg.Substring(0, 4)).ToString("ddd, dd MMMM yyyy");

        //        }
        //        else
        //        {
        //            //Devolver vista de error
        //            return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
        //        }
        //        #endregion

        //        //lc_fecitem = ViewBag.Fecha3;

        //        //Devolver a vista
        //        Session.Remove("IdPelicula");
        //        Session.Remove("FcPelicula");
        //        Session.Remove("TpPelicula");
        //        return View(ob_bilmov);
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        return RedirectToAction("Error", "Pages", new { pr_message = lc_syserr.Message });
        //    }
        //}

        /// <summary>
        /// GET: RoomProg -- Proceso de ejecución SCOGRU para dejar ubicaciones en preventa
        /// </summary>
        /// <returns></returns>
        /// 
        //[HttpGet]
        //[Route("ProcesoSCOGRU")]
        //public BolVenta Room(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_imgpel)
        //{
        //    #region VARIABLES LOCALES
        //    int lc_maxcol = 0;
        //    int lc_maxfil = 0;
        //    int lc_idxrow = 0;
        //    string lc_auxval = string.Empty;
        //    string lc_auxtar = string.Empty;
        //    string lc_auxhor = string.Empty;
        //    string lc_srvpar = string.Empty;
        //    string lc_result = string.Empty;

        //    DataTable ob_datubi = new DataTable();

        //    XmlDocument ob_xmldoc = new XmlDocument();

        //    Dictionary<string, object> ob_estsil = new Dictionary<string, object>();
        //    Dictionary<string, object> ob_diclst = new Dictionary<string, object>();
        //    Dictionary<string, object>[] ob_diclst2;
        //    List<BolVenta> ob_lisprg = new List<BolVenta>();

        //    Peliculas peliculas = new Peliculas();
        //    MapaSala mapaSala = new MapaSala();
        //    BolVenta boletaVenta = new BolVenta();
        //    General ob_fncgrl = new General();
        //    #endregion

        //    try
        //    {
        //        //URLPortal(config);
        //        //ListCarrito();

        //        ////Cargar ciudades home y teatro por defecto si aplica
        //        //if (Session.GetString("Teatro") != null)
        //        //{
        //        //    Ciuteatros("SEL");
        //        //}
        //        //else
        //        //{
        //        //    if (Session.GetString("CiudadTeatro") != null)
        //        //        Ciuteatros(Session.GetString("CiudadTeatro"));
        //        //    else
        //        //        Ciuteatros();
        //        //}

        //        ////Validar inicio de sesión
        //        //if (Session.GetString("Usuario") == null)
        //        //    return RedirectToAction("Error", "Pages", new { pr_message = "Se debe iniciar Sesión para Continuar", pr_flag = "P" });

        //        ////Validar seleccion de teatro
        //        //if (Session.GetString("Teatro") == null)
        //        //{
        //        //    //Devolver vista de error
        //        //    return RedirectToAction("Error", "Pages", new { pr_message = "Debe seleccionar un teatro para continuar", pr_flag = "P" });
        //        //}
        //        //else
        //        //{
        //        //    //Cargar ciudades home y teatro por defecto si aplica
        //        //    if (Session.GetString("Teatro") != null)
        //        //    {
        //        //        Ciuteatros("SEL");
        //        //    }
        //        //    else
        //        //    {
        //        //        if (Session.GetString("CiudadTeatro") != null)
        //        //            Ciuteatros(Session.GetString("CiudadTeatro"));
        //        //        else
        //        //            Ciuteatros();
        //        //    }
        //        //}

        //        //Asignar valores url
        //        boletaVenta.HorProg = pr_horprg;
        //        boletaVenta.FecProg = pr_fecprg;
        //        boletaVenta.SwtVenta = "V";
        //        boletaVenta.KeyTarifa = pr_tarprg;
        //        boletaVenta.NombrePel = pr_nompel;
        //        boletaVenta.NombreFec = pr_nomfec;
        //        boletaVenta.NombreHor = pr_nomhor;
        //        boletaVenta.NombreTar = pr_nomtar;
        //        boletaVenta.KeyPelicula = pr_keypel;

        //        boletaVenta.KeySala = pr_salprg;
        //        boletaVenta.TipoSilla = boletaVenta.KeySala.Substring(boletaVenta.KeySala.IndexOf(";") + 1);
        //        boletaVenta.KeySala = boletaVenta.KeySala.Substring(0, boletaVenta.KeySala.IndexOf(";"));

        //        lc_auxtar = pr_nomtar.Substring(0, pr_nomtar.IndexOf(";"));
        //        lc_auxval = pr_nomtar.Substring(pr_nomtar.IndexOf(";") + 1);
        //        lc_auxval = lc_auxval.Substring(0, lc_auxval.Length - 3);

        //        //Asignar valores
        //        boletaVenta.Sala = boletaVenta.Sala;
        //        ViewBag.Hora = pr_nomhor;
        //        ViewBag.Fecha = pr_nomfec;
        //        ViewBag.Imagen = pr_imgpel;
        //        peliculas.Teatro = Session.GetString("TeatroNombre");
        //        ViewBag.Tarifa = lc_auxtar;
        //        ViewBag.NumValor = lc_auxval;
        //        ViewBag.Tarvalor = String.Format("{0:C0}", Convert.ToInt32(lc_auxval));
        //        ViewBag.Censura = pr_cenprg;
        //        ViewBag.TipoSilla = boletaVenta.TipoSilla;
        //        ViewBag.NombreUsuario = Session.GetString("Nombre") + " " + Session.GetString("Apellido");
        //        ViewBag.CantSillasBol = config.Value.CantSillasBol;


        //        #region SERVICIO SCOMAP
        //        //Asignar valores MAP
        //        mapaSala.Sala = Convert.ToInt32(boletaVenta.KeySala);
        //        mapaSala.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
        //        mapaSala.Tercero = config.Value.ValorTercero;
        //        mapaSala.Correo = "";
        //        mapaSala.FechaFuncion = "";

        //        //Generar y encriptar JSON para servicio MAP
        //        lc_srvpar = ob_fncgrl.JsonConverter(mapaSala);
        //        lc_srvpar = lc_srvpar.Replace("sala", "Sala");

        //        //Encriptar Json MAP
        //        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //        //Consumir servicio MAP
        //        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scomap/"), lc_srvpar);

        //        ////Generar Log
        //        //LogSales logSales = new LogSales();
        //        //LogAudit logAudit = new LogAudit(config);
        //        //logSales.Id = Guid.NewGuid().ToString();
        //        //logSales.Fecha = DateTime.Now;
        //        //logSales.Programa = "SalesBol/Room";
        //        //logSales.Metodo = "SCOMAP";
        //        //logSales.ExceptionMessage = lc_srvpar;
        //        //logSales.InnerExceptionMessage = lc_result;

        //        //Escribir Log
        //        //logAudit.LogApp(logSales);

        //        //Validar respuesta
        //        if (lc_result.Substring(0, 1) == "0")
        //        {
        //            //Quitar switch
        //            lc_result = lc_result.Replace("0-", "");

        //            //Deserializar Json y validar respuesta MAP
        //            ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
        //        }
        //        else
        //        {
        //            lc_result = lc_result.Replace("1-", "");
        //            //return RedirectToAction("Error", "Pages", new { pr_message = "SCOMAP - " + lc_result });
        //        }
        //        #endregion

        //        #region SERVICIO SCOEST
        //        //Asignar valores EST
        //        mapaSala.Sala = Convert.ToInt32(boletaVenta.KeySala);
        //        mapaSala.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
        //        mapaSala.Tercero = config.Value.ValorTercero;

        //        mapaSala.Correo = Session.GetString("Usuario");
        //        mapaSala.FechaFuncion = boletaVenta.FecProg;

        //        lc_auxhor = boletaVenta.HorProg;
        //        mapaSala.Funcion = Convert.ToInt32(lc_auxhor.Substring(0, 2));

        //        //Generar y encriptar JSON para servicio EST
        //        lc_srvpar = ob_fncgrl.JsonConverter(mapaSala);
        //        lc_srvpar = lc_srvpar.Replace("sala", "Sala");
        //        lc_srvpar = lc_srvpar.Replace("correo", "Correo");
        //        lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
        //        lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");

        //        //Encriptar Json EST
        //        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //        //Consumir servicio EST
        //        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoest/"), lc_srvpar);

        //        ////Generar Log
        //        //logSales.Id = Guid.NewGuid().ToString();
        //        //logSales.Fecha = DateTime.Now;
        //        //logSales.Programa = "SalesBol/Room";
        //        //logSales.Metodo = "GEt";
        //        //logSales.ExceptionMessage = lc_srvpar;
        //        //logSales.InnerExceptionMessage = lc_result;

        //        //Escribir Log
        //        //logAudit.LogApp(logSales);

        //        //Validar respuesta
        //        if (lc_result.Substring(0, 1) == "0")
        //        {
        //            //Quitar switch
        //            lc_result = lc_result.Replace("0-", "");

        //            //Deserializar Json y validar respuesta EST
        //            ob_diclst2 = (Dictionary<string, object>[])JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>[])));
        //        }
        //        else
        //        {
        //            lc_result = lc_result.Replace("1-", "");
        //            //return RedirectToAction("Error", "Pages", new { pr_message = "SCOEST - " + lc_result });
        //        }
        //        #endregion

        //        #region MATRIZ SALA
        //        //Obtener maximo de filas, columnas y estado de sillas para matriz de sala
        //        foreach (var item in ob_diclst2)
        //        {
        //            lc_maxcol = Convert.ToInt32(item["maxCol"]);
        //            lc_maxfil = ob_diclst2.Length;
        //            ob_estsil.Add(item["filRel"].ToString(), item["DescripcionSilla"]);
        //        }

        //        //Obtener arreglos de filas y columnas de la matriz SCOMAP
        //        double[] ColumnaTotal = (double[])JsonConvert.DeserializeObject(ob_diclst["ColumnaTotal"].ToString(), (typeof(double[])));
        //        double[] ColumnaRelativa = (double[])JsonConvert.DeserializeObject(ob_diclst["ColumnaRelativa"].ToString(), (typeof(double[])));
        //        string[] FilaTotal = (string[])JsonConvert.DeserializeObject(ob_diclst["FilaTotal"].ToString(), (typeof(string[])));
        //        string[] FilaRelativa = (string[])JsonConvert.DeserializeObject(ob_diclst["FilaRelativa"].ToString(), (typeof(string[])));
        //        string[] TipoSilla = (string[])JsonConvert.DeserializeObject(ob_diclst["TipoSilla"].ToString(), (typeof(string[])));
        //        string[] TipoZona = (string[])JsonConvert.DeserializeObject(ob_diclst["TipoZona"].ToString(), (typeof(string[])));

        //        //Recorrer y cargar matriz de sala (filas)
        //        Ubicaciones[,] mt_datsal = new Ubicaciones[lc_maxfil, lc_maxcol];
        //        for (int lc_idxiii = 0; lc_idxiii < lc_maxfil; lc_idxiii++)
        //        {
        //            //Recorrer y cargar matriz de sala (columnas)
        //            for (int lc_idxjjj = 0; lc_idxjjj < lc_maxcol; lc_idxjjj++)
        //            {
        //                //Inicializar objeto de ubicaciones 
        //                Ubicaciones ob_ubisal = new Ubicaciones();

        //                //Cargar valores numericos de los arreglos al objeto
        //                ob_ubisal.Columna = Convert.ToInt32(ColumnaTotal[lc_idxrow]);
        //                ob_ubisal.ColRelativa = Convert.ToInt32(ColumnaRelativa[lc_idxrow]);

        //                //Cargar valores string de los arreglos al objeto
        //                ob_ubisal.Fila = FilaTotal[lc_idxrow];
        //                ob_ubisal.FilRelativa = FilaRelativa[lc_idxrow];
        //                ob_ubisal.TipoSilla = TipoSilla[lc_idxrow];
        //                ob_ubisal.TipoZona = TipoZona[lc_idxrow];

        //                //Recorrer y buscar fila en ciclo de matriz
        //                List<EstadoDeSilla> ls_estsil = new List<EstadoDeSilla>((List<EstadoDeSilla>)JsonConvert.DeserializeObject(ob_estsil[FilaRelativa[lc_idxrow]].ToString(), (typeof(List<EstadoDeSilla>))));
        //                foreach (var item in ls_estsil)
        //                {
        //                    //Validar columna en ciclo de matriz
        //                    if (Convert.ToInt32(item.Columna) == ColumnaRelativa[lc_idxrow])
        //                    {
        //                        //Asignar valor y romper ciclo
        //                        ob_ubisal.EstadoSilla = item.EstadoSilla;
        //                        break;
        //                    }
        //                }

        //                //Cargar objeto ubicaciones a la matriz
        //                mt_datsal[lc_idxiii, lc_idxjjj] = ob_ubisal;
        //                lc_idxrow++;
        //            }
        //        }

        //        //Asignar Sala a Objeto
        //        boletaVenta.FilSala = lc_maxfil;
        //        boletaVenta.ColSala = lc_maxcol;
        //        boletaVenta.MapaSala = mt_datsal;
        //        #endregion

        //        //Devolver a vista
        //        return boletaVenta;
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        ////Generar Log
        //        //LogSales logSales = new LogSales();
        //        //LogAudit logAudit = new LogAudit(config);
        //        //logSales.Id = Guid.NewGuid().ToString();
        //        //logSales.Fecha = DateTime.Now;
        //        //logSales.Programa = "SalesBol/Room";
        //        //logSales.Metodo = "GET";
        //        //logSales.ExceptionMessage = lc_syserr.Message;
        //        //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

        //        ////Escribir Log
        //        //logAudit.LogApp(logSales);

        //        //Devolver vista de error
        //        //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
        //        return null;
        //    }
        //}

        /// <summary>
        /// GET: Onboarding -- Proceso de resumen de compra en preventa
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("ProcesoResumenCompra")]
        public Producto Onboarding(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_secsec, string pr_selubi, string pr_flggaf = "")
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            double lc_valtar = 0;
            string lc_auxsal = string.Empty;
            string lc_auxtel = string.Empty;
            string lc_auxtar = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string lc_auxitm = string.Empty;
            string[] ls_lstsel = new string[5];

            string TelefonoEli = string.Empty;

            PARMBoletas parmboletas = new PARMBoletas();
            Producto producto = new Producto();
            List<string> ls_lstubi = new List<string>();
            List<Ubicaciones> ob_ubisel = new List<Ubicaciones>();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                ////Sesion para carrito de compras
                //Session.Remove("pr_keypel");
                //Session.SetString("pr_keypel", pr_keypel);
                //Session.Remove("pr_fecprg");
                //Session.SetString("pr_fecprg", pr_fecprg);
                //Session.Remove("pr_horprg");
                //Session.SetString("pr_horprg", pr_horprg);
                //Session.Remove("pr_tarprg");
                //Session.SetString("pr_tarprg", pr_tarprg);
                //Session.Remove("pr_salprg");
                //Session.SetString("pr_salprg", pr_salprg);
                //Session.Remove("pr_nompel");
                //Session.SetString("pr_nompel", pr_nompel);
                //Session.Remove("pr_nomfec");
                //Session.SetString("pr_nomfec", pr_nomfec);
                //Session.Remove("pr_nomhor");
                //Session.SetString("pr_nomhor", pr_nomhor);
                //Session.Remove("pr_nomtar");
                //Session.SetString("pr_nomtar", pr_nomtar);
                //Session.Remove("pr_cenprg");
                //Session.SetString("pr_cenprg", pr_cenprg);
                //Session.Remove("pr_secsec");
                //Session.SetString("pr_secsec", pr_secsec);
                //Session.Remove("pr_selubi");
                //Session.SetString("pr_selubi", pr_selubi);

                //URLPortal(config);
                //ListCarrito();

                ////Cargar ciudades home y teatro por defecto si aplica
                //if (Session.GetString("Teatro") != null)
                //{
                //    Ciuteatros("SEL");
                //}
                //else
                //{
                //    if (Session.GetString("CiudadTeatro") != null)
                //        Ciuteatros(Session.GetString("CiudadTeatro"));
                //    else
                //        Ciuteatros();
                //}

                parmboletas.SwtVenta = "V";
                //ViewBag.pr_tiplog = "B";
                parmboletas.KeyPelicula = pr_keypel;
                parmboletas.FecProg = pr_fecprg;
                parmboletas.HorProg = pr_horprg;
                parmboletas.KeyTarifa = pr_tarprg;
                parmboletas.KeySala = pr_salprg;
                parmboletas.NombrePel = pr_nompel;
                parmboletas.NombreFec = pr_nomfec;
                parmboletas.NombreHor = pr_nomhor;
                parmboletas.NombreTar = pr_nomtar;
                //ViewBag.pr_cenprg = pr_cenprg;
                //ViewBag.pr_secsec = pr_secsec;
                //ViewBag.pr_selubi = pr_selubi;
                //ViewBag.pr_flggaf = pr_flggaf;

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
                //logSales.Programa = "SalesBol/Onboarding";
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
        [Route("LiberarSillas")]
        public LiberaSilla RoomReverse(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_secsec, string pr_selubi)
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitm = string.Empty;
            string[] ls_lstsel = new string[5];

            LiberaSilla liberarSilla = new LiberaSilla();
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
                    
                    liberarSilla.Fila = ls_lstsel[3];
                    liberarSilla.Sala = Convert.ToInt32(pr_salprg.Substring(0, pr_salprg.IndexOf(";")));
                    liberarSilla.teatro = Convert.ToInt32(Session.GetString("Teatro"));
                    liberarSilla.Funcion = Convert.ToInt32(pr_horprg.Length == 4 ? pr_horprg.Substring(0, 2) : pr_horprg.Substring(0, 1));
                    liberarSilla.Columna = Convert.ToInt32(ls_lstsel[4]);
                    liberarSilla.Usuario = 777;
                    liberarSilla.tercero = config.Value.ValorTercero;
                    liberarSilla.FechaFuncion = pr_fecprg;

                    //Generar y encriptar JSON para servicio
                    lc_srvpar = ob_fncgrl.JsonConverter(liberarSilla);

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

                    ////Generar Log
                    //LogSales logSales = new LogSales();
                    //LogAudit logAudit = new LogAudit(config);
                    //logSales.Id = Guid.NewGuid().ToString();
                    //logSales.Fecha = DateTime.Now;
                    //logSales.Programa = "SalesBol/RoomReverse";
                    //logSales.Metodo = "SCOSIL";
                    //logSales.ExceptionMessage = lc_srvpar;
                    //logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    //logAudit.LogApp(logSales);

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
                            //return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
                            return liberarSilla;
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
                                    //return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Respuesta"].ToString() });
                                    return liberarSilla;
                                }
                            }
                        }
                    }
                    //else
                    //{
                    //    return RedirectToAction("Error", "Pages", new { pr_message = "Error al liberar silla SCOLIB" });
                    //}
                    #endregion
                }

                //Obtener objeto BD para eliminar item
                using (var context = new DataDB(config))
                {
                    //Obtener valores a tabla ReportSales
                    string KeyTeatro = Session.GetString("Teatro");
                    var ReportSales = context.ReportSales.Where(x => x.KeyTeatro == KeyTeatro).Where(x => x.Secuencia == pr_secsec).Where(x => x.KeyPunto == config.Value.PuntoVenta).Where(x => x.KeyPelicula == pr_keypel).ToList();
                    foreach (var report in ReportSales)
                    {
                        //Eliminar item y guardar registro a tabla
                        context.ReportSales.Remove(report);
                        context.SaveChanges();
                    }
                }

                //Validar acción
                //return RedirectToAction("Room", "SalesBol", new { pr_keypel = pr_keypel, pr_fecprg = pr_fecprg, pr_horprg = pr_horprg, pr_tarprg = pr_tarprg, pr_salprg = pr_salprg, pr_nompel = pr_nompel, pr_nomfec = pr_nomfec, pr_nomhor = pr_nomhor, pr_nomtar = pr_nomtar, pr_cenprg = pr_cenprg });
                return liberarSilla;
            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesBol/RoomReverse";
                //logSales.Metodo = "GET";
                //logSales.ExceptionMessage = lc_syserr.Message;
                //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                ////Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                return liberarSilla;
            }
        }

        /// <summary>
        /// GET: Onboarding -- Proceso de adicionar gafas
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpGet]
        [Route("AdicionarGafas")]
        public Producto PreOnboarding(string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_secsec, string pr_selubi, string pr_flag3d)
        {
            #region VARIABLES LOCALES
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_auxitem = string.Empty;

            List<Producto> ob_return = new List<Producto>();
            Dictionary<string, object> ob_diclst = new Dictionary<string, object>();

            Secuencia ob_scopre = new Secuencia();
            Producto producto = new Producto();
            General ob_fncgrl = new General();
            #endregion

            ////Sesion para carrito de compras
            //Session.Remove("pr_keypel");
            //Session.SetString("pr_keypel", pr_keypel);
            //Session.Remove("pr_fecprg");
            //Session.SetString("pr_fecprg", pr_fecprg);
            //Session.Remove("pr_horprg");
            //Session.SetString("pr_horprg", pr_horprg);
            //Session.Remove("pr_tarprg");
            //Session.SetString("pr_tarprg", pr_tarprg);
            //Session.Remove("pr_salprg");
            //Session.SetString("pr_salprg", pr_salprg);
            //Session.Remove("pr_nompel");
            //Session.SetString("pr_nompel", pr_nompel);
            //Session.Remove("pr_nomfec");
            //Session.SetString("pr_nomfec", pr_nomfec);
            //Session.Remove("pr_nomhor");
            //Session.SetString("pr_nomhor", pr_nomhor);
            //Session.Remove("pr_nomtar");
            //Session.SetString("pr_nomtar", pr_nomtar);
            //Session.Remove("pr_cenprg");
            //Session.SetString("pr_cenprg", pr_cenprg);
            //Session.Remove("pr_secsec");
            //Session.SetString("pr_secsec", pr_secsec);
            //Session.Remove("pr_selubi");
            //Session.SetString("pr_selubi", pr_selubi);

            //Incializar valores
            //ViewBag.pr_keypel = pr_keypel;
            //ViewBag.pr_fecprg = pr_fecprg;
            //ViewBag.pr_horprg = pr_horprg;
            //ViewBag.pr_tarprg = pr_tarprg;
            //ViewBag.pr_salprg = pr_salprg;
            //ViewBag.pr_nompel = pr_nompel;
            //ViewBag.pr_nomfec = pr_nomfec;
            //ViewBag.pr_nomhor = pr_nomhor;
            //ViewBag.pr_nomtar = pr_nomtar;
            //ViewBag.pr_cenprg = pr_cenprg;
            //ViewBag.pr_secsec = pr_secsec;
            //ViewBag.pr_selubi = pr_selubi;
            //ViewBag.UrlRetailImg = config.Value.UrlRetailImg;
            //ViewBag.pr_flag3d = pr_flag3d;

            try
            {
                //URLPortal(config);
                //ListCarrito();

                ////Cargar ciudades home y teatro por defecto si aplica
                //if (Session.GetString("Teatro") != null)
                //{
                //    Ciuteatros("SEL");
                //}
                //else
                //{
                //    if (Session.GetString("CiudadTeatro") != null)
                //        Ciuteatros(Session.GetString("CiudadTeatro"));
                //    else
                //        Ciuteatros();
                //}

                //ViewBag.ClientFrecnt = Session.GetString("ClienteFrecuente");

                producto.Codigo = 379;
                producto.SwtVenta = "V";
                producto.TipoCompra = "B";
                producto.EmailEli = Session.GetString("Usuario");
                producto.NombreEli = Session.GetString("Nombre");
                producto.KeyTeatro = Session.GetString("Teatro");
                producto.DesTeatro = Session.GetString("TeatroNombre");
                producto.ApellidoEli = Session.GetString("Apellido");
                producto.TelefonoEli = Session.GetString("Telefono");
                producto.KeySecuencia = pr_secsec;

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
                //logSales.Programa = "SalesBol/PreOnboarding";
                //logSales.Metodo = "SCOPRE";
                //logSales.ExceptionMessage = lc_srvpar;
                //logSales.InnerExceptionMessage = lc_result;

                //Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_diclst = (Dictionary<string, object>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, object>)));
                    ob_return = (List<Producto>)JsonConvert.DeserializeObject(ob_diclst["Lista_Productos"].ToString(), (typeof(List<Producto>)));

                    if (ob_diclst.ContainsKey("Validación"))
                    {
                        //Devolver vista de error
                        //return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
                        return producto;
                    }
                    else
                    {
                        //Recorrido por productos para obtener el seleccionado y sus valores
                        foreach (var itepro in ob_return)
                        {
                            if (itepro.Codigo == producto.Codigo)
                            {
                                producto.Codigo = itepro.Codigo;
                                producto.Descripcion = itepro.Descripcion;
                                producto.Tipo = itepro.Tipo;
                                producto.Precios = itepro.Precios;

                                //Romper el ciclo
                                break;
                            }
                        }
                    }
                }
                //else
                //{
                //    lc_result = lc_result.Replace("1-", "");

                //    //Devolver vista de error
                //    return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
                //}
                #endregion

                //Devolver vista
                return producto;

            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesBol/PreOnboarding";
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
        /// POST: Room -- Proceso de ejecución SCOGRU para dejar ubicaciones en preventa
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("DejarUbicaciones")]
        public BolVenta Room(BolVenta boletaVenta)
        {
            #region VARIABLES LOCALES
            int lc_idearr = 0;
            double lc_valtar = 0;
            string lc_auxitm = string.Empty;
            string lc_auxtel = string.Empty;
            string lc_auxsec = string.Empty;
            string lc_srvpar = string.Empty;
            string lc_result = string.Empty;
            string[] ls_lstsel = new string[5];

            string TelefonoEli = string.Empty;

            List<string> ls_lstubi = new List<string>();
            List<Ubicaciones> ob_ubiprg = new List<Ubicaciones>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            Secuencia secuencia = new Secuencia();
            General ob_fncgrl = new General();
            #endregion

            try
            {
                //URLPortal(config);
                //ListCarrito();

                if (Session.GetString("Secuencia") != null)
                {
                    lc_auxsec = Session.GetString("Secuencia");
                }
                else
                {
                    #region SERVICIO SCOSEC
                    //Asignar valores SEC
                    secuencia.Punto = Convert.ToInt32(config.Value.PuntoVenta);
                    secuencia.Teatro = Convert.ToInt32(Session.GetString("Teatro"));
                    secuencia.Tercero = config.Value.ValorTercero;

                    //Generar y encriptar JSON para servicio SEC
                    lc_srvpar = ob_fncgrl.JsonConverter(secuencia);
                    lc_srvpar = lc_srvpar.Replace("Teatro", "teatro");
                    lc_srvpar = lc_srvpar.Replace("Tercero", "tercero");
                    lc_srvpar = lc_srvpar.Replace("punto", "Punto");

                    //Encriptar Json SEC
                    lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                    //Consumir servicio SEC
                    lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosec/"), lc_srvpar);

                    ////Generar Log
                    //LogSales logSales = new LogSales();
                    //LogAudit logAudit = new LogAudit(config);
                    //logSales.Id = Guid.NewGuid().ToString();
                    //logSales.Fecha = DateTime.Now;
                    //logSales.Programa = "SalesBol/Room";
                    //logSales.Metodo = "SCOSEC";
                    //logSales.ExceptionMessage = lc_srvpar;
                    //logSales.InnerExceptionMessage = lc_result;

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

                        if (ob_diclst.ContainsKey("Validación"))
                        {
                            //return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
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
                        //ViewBag.alertS = false;

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
                    boletaVenta.Funcion = "";
                    boletaVenta.Horario = "";
                    boletaVenta.message = "";
                    boletaVenta.FechaPrg = "";
                    boletaVenta.FechaDia = "";
                    boletaVenta.ValorTarifa = "";
                    boletaVenta.IdTarifa = "";
                    boletaVenta.MapaSala = new Ubicaciones[1, 1];

                    boletaVenta.EmailEli = Session.GetString("Usuario");
                    boletaVenta.NombreEli = Session.GetString("Nombre");
                    boletaVenta.ApellidoEli = Session.GetString("Apellido");
                    boletaVenta.TelefonoEli = Session.GetString("Telefono");

                    boletaVenta.Tipo = "B";
                    boletaVenta.NombreTarifa = boletaVenta.NombreTar;
                    boletaVenta.NombrePelicula = boletaVenta.NombrePel;

                    boletaVenta.Sala = Convert.ToInt32(boletaVenta.KeySala);
                    boletaVenta.KeySala = boletaVenta.KeySala;
                    var telefonodefault = (Session.GetString("Telefono") == null ? 0 : Convert.ToDecimal(Session.GetString("Telefono")));
                    boletaVenta.Telefono = Convert.ToInt64(telefonodefault);

                    boletaVenta.Nombre = Session.GetString("Nombre");
                    boletaVenta.Apellido = Session.GetString("Apellido");

                    boletaVenta.FecProg = boletaVenta.FecProg;
                    boletaVenta.HorProg = boletaVenta.HorProg;
                    boletaVenta.KeyTarifa = boletaVenta.KeyTarifa;
                    boletaVenta.KeyTeatro = Session.GetString("Teatro");
                    boletaVenta.KeyPelicula = boletaVenta.KeyPelicula;
                    boletaVenta.KeySecuencia = lc_auxsec;

                    boletaVenta.Tercero = config.Value.ValorTercero;
                    boletaVenta.Secuencia = Convert.ToInt32(lc_auxsec);
                    boletaVenta.PuntoVenta = Convert.ToInt32(config.Value.PuntoVenta);
                    boletaVenta.IdFuncion = boletaVenta.HorProg.ToString().Length == 4 ? Convert.ToInt32(boletaVenta.HorProg.ToString().Substring(0, 2)) : Convert.ToInt32(boletaVenta.HorProg.ToString().Substring(0, 1));

                    //Obtener ubicaciones de vista
                    char[] ar_charst = boletaVenta.SelUbicaciones.ToCharArray();
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

                        ob_ubiprg.Add(new Ubicaciones() { Fila = ls_lstsel[3], Columna = Convert.ToInt32(ls_lstsel[4]), Tarifa = Convert.ToInt32(boletaVenta.KeyTarifa), FilRelativa = ls_lstsel[1], ColRelativa = Convert.ToInt32(ls_lstsel[2]), TipoSilla = "", TipoZona = "", EstadoSilla = "" });
                    }

                    boletaVenta.Ubicaciones = ob_ubiprg;

                    if (boletaVenta.Imagen == null)
                        boletaVenta.Imagen = "NA";

                    //Validar cantidad de sillas
                    if (boletaVenta.Ubicaciones.Count > Convert.ToInt32(config.Value.CantSillasBol))
                        //return RedirectToAction("Error", "Pages", new { pr_message = "Solo se pueden seleccionar hasta " + config.Value.CantSillasBol + " sillas por transacción." });
                        return boletaVenta;

                    //Generar y encriptar JSON para servicio
                    lc_srvpar = ob_fncgrl.JsonConverter(boletaVenta);

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

                    ////Generar Log
                    //LogSales logSales = new LogSales();
                    //LogAudit logAudit = new LogAudit(config);
                    //logSales.Id = Guid.NewGuid().ToString();
                    //logSales.Fecha = DateTime.Now;
                    //logSales.Programa = "SalesBol/Room";
                    //logSales.Metodo = "SCOGRU";
                    //logSales.ExceptionMessage = lc_srvpar;
                    //logSales.InnerExceptionMessage = lc_result;

                    //Escribir Log
                    //logAudit.LogApp(logSales);

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
                                //return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
                                return boletaVenta;
                            }
                            else
                            {
                                //Validar respuesta llave 2
                                if (ob_auxrta.ContainsKey("Respuesta"))
                                {
                                    if (ob_auxrta["Respuesta"].ToString() == "Proceso exitoso")
                                    {
                                        lc_valtar = Convert.ToDouble(boletaVenta.NombreTar.Substring(boletaVenta.NombreTar.IndexOf(";") + 1, boletaVenta.NombreTar.Length - (boletaVenta.NombreTar.IndexOf(";") + 1)));

                                        //Inicializar instancia de BD
                                        using (var context = new DataDB(config))
                                        {
                                            //Agregar valores a tabla ReportSales
                                            TelefonoEli = string.Concat(Session.GetString("Telefono"), ";", Session.GetString("Documento"), "*", Session.GetString("Direccion"));
                                            var reportSales = new ReportSales
                                            {
                                                Secuencia = boletaVenta.KeySecuencia,
                                                KeySala = boletaVenta.KeySala,
                                                KeyTeatro = Session.GetString("Teatro"),
                                                KeyPelicula = boletaVenta.KeyPelicula,
                                                SelUbicaciones = boletaVenta.SelUbicaciones,
                                                Precio = (lc_valtar * boletaVenta.Ubicaciones.Count),
                                                HorProg = boletaVenta.HorProg,
                                                FecProg = boletaVenta.FecProg,
                                                EmailEli = Session.GetString("Usuario"),
                                                NombreEli = Session.GetString("Nombre"),
                                                ApellidoEli = Session.GetString("Apellido"),
                                                TelefonoEli = TelefonoEli,
                                                NombrePel = boletaVenta.NombrePel,
                                                NombreFec = boletaVenta.NombreFec,
                                                NombreHor = boletaVenta.NombreHor,
                                                NombreTar = boletaVenta.NombreTar,
                                                KeyTarifa = boletaVenta.KeyTarifa,
                                                //Transaccion = boletaVenta.Censura,
                                                Referencia = boletaVenta.Imagen,
                                                FechaCreado = DateTime.Now,
                                                FechaModificado = DateTime.Now,
                                                KeyPunto = config.Value.PuntoVenta
                                            };

                                            //Adicionar y guardar registro a tabla
                                            context.ReportSales.Add(reportSales);
                                            context.SaveChanges();
                                        }

                                        //Paso datos de paso url
                                        //if (boletaVenta.NombrePel.Contains("3D"))
                                        //    return RedirectToAction("PreOnboarding", "SalesBol", new { pr_keypel = boletaVenta.KeyPelicula, pr_fecprg = boletaVenta.FecProg, pr_horprg = boletaVenta.HorProg, pr_tarprg = boletaVenta.KeyTarifa, pr_salprg = boletaVenta.KeySala + ";" + boletaVenta.TipoSilla, pr_nompel = boletaVenta.NombrePel, pr_nomfec = boletaVenta.NombreFec, pr_nomhor = boletaVenta.NombreHor, pr_nomtar = boletaVenta.NombreTar, /*pr_cenprg = boletaVenta.Censura,*/ pr_secsec = boletaVenta.KeySecuencia, pr_selubi = boletaVenta.SelUbicaciones, pr_flag3d = "N" });
                                        //else
                                        //    return RedirectToAction("Onboarding", "SalesBol", new { pr_keypel = boletaVenta.KeyPelicula, pr_fecprg = boletaVenta.FecProg, pr_horprg = boletaVenta.HorProg, pr_tarprg = boletaVenta.KeyTarifa, pr_salprg = boletaVenta.KeySala + ";" + boletaVenta.TipoSilla, pr_nompel = boletaVenta.NombrePel, pr_nomfec = boletaVenta.NombreFec, pr_nomhor = boletaVenta.NombreHor, pr_nomtar = boletaVenta.NombreTar, pr_cenprg = boletaVenta.Censura, pr_secsec = boletaVenta.KeySecuencia, pr_selubi = boletaVenta.SelUbicaciones });
                                    }
                                    else
                                    {
                                        //return RedirectToAction("Error", "Pages", new { pr_message = ob_auxrta["Respuesta"].ToString() });
                                        return boletaVenta;
                                    }
                                }
                            }
                        }
                    }
                    //else
                    //{
                    //    lc_result = lc_result.Replace("1-", "");
                    //    return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
                    //}
                }
                //else
                //{
                //    return RedirectToAction("Error", "Pages", new { pr_message = "Error en Secuencia" });
                //}
                //#endregion

                //Devolver a vista
                return boletaVenta;
            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesBol/Room";
                //logSales.Metodo = "POST";
                //logSales.ExceptionMessage = lc_syserr.Message;
                //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                ////Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                return boletaVenta;
            }
        }

        /// <summary>
        /// POST: PreOnboarding -- Proceso para adicionar gafas a la compra actual
        /// </summary>
        /// <param name="pr_datpro"></param>
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
        [HttpPost]
        [Route("AdiccionarGafasCompraActual")]
        public Producto PreOnboarding(Producto producto, string pr_keypel, string pr_fecprg, string pr_horprg, string pr_tarprg, string pr_salprg, string pr_nompel, string pr_nomfec, string pr_nomhor, string pr_nomtar, string pr_cenprg, string pr_secsec, string pr_selubi, string pr_flag3d)
        {
            try
            {
                //URLPortal(config);
                //ListCarrito();

                //Inicializar instancia de BD
                using (var context = new DataDB(config))
                {
                    //Agregar valores a tabla ReportSales
                    var retailSales = new RetailSales
                    {
                        Tipo = "P",
                        Precio = Convert.ToDecimal(producto.Valor),
                        Cantidad = producto.Cantidad,
                        //Secuencia = Convert.ToDecimal(pr_secsec),
                        PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta),
                        KeyProducto = producto.Codigo,
                        Descripcion = producto.Descripcion,
                        ProProducto1 = producto.ProProducto_1,
                        ProProducto2 = producto.ProProducto_2,
                        ProProducto3 = producto.ProProducto_3,
                        ProProducto4 = producto.ProProducto_4,
                        ProProducto5 = producto.ProProducto_5,
                        CanProducto1 = producto.ProCantidad_1,
                        CanProducto2 = producto.ProCantidad_2,
                        CanProducto3 = producto.ProCantidad_3,
                        CanProducto4 = producto.ProCantidad_4,
                        CanProducto5 = producto.ProCantidad_5,
                        ProCategoria1 = producto.ProCategoria_1,
                        ProCategoria2 = producto.ProCategoria_2,
                        ProCategoria3 = producto.ProCategoria_3,
                        ProCategoria4 = producto.ProCategoria_4,
                        ProCategoria5 = producto.ProCategoria_5,
                        CanCategoria1 = producto.CanCategoria_1,
                        CanCategoria2 = producto.CanCategoria_2,
                        CanCategoria3 = producto.CanCategoria_3,
                        CanCategoria4 = producto.CanCategoria_4,
                        CanCategoria5 = producto.CanCategoria_5,
                        FechaRegistro = DateTime.Now,
                        KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"))
                    };

                    //Adicionar y guardar registro a tabla
                    context.RetailSales.Add(retailSales);
                    context.SaveChanges();
                }

                //Paso datos de paso url
                //if (pr_flag3d == "N")
                //    return RedirectToAction("OnBoarding", "SalesBol", new { pr_keypel = pr_keypel, pr_fecprg = pr_fecprg, pr_horprg = pr_horprg, pr_tarprg = pr_tarprg, pr_salprg = pr_salprg, pr_nompel = pr_nompel, pr_nomfec = pr_nomfec, pr_nomhor = pr_nomhor, pr_nomtar = pr_nomtar, pr_cenprg = pr_cenprg, pr_secsec = pr_secsec, pr_selubi = pr_selubi, pr_flggaf = "M" });
                //else
                //    return RedirectToAction("ListCon", "FastSales", new { pr_secpro = pr_secsec, pr_swtven = "V", pr_tiplog = "B", pr_cenprg = pr_cenprg });
                return producto;
            }
            catch (Exception lc_syserr)
            {
                ////Generar Log
                //LogSales logSales = new LogSales();
                //LogAudit logAudit = new LogAudit(config);
                //logSales.Id = Guid.NewGuid().ToString();
                //logSales.Fecha = DateTime.Now;
                //logSales.Programa = "SalesBol/PreOnboarding";
                //logSales.Metodo = "POST";
                //logSales.ExceptionMessage = lc_syserr.Message;
                //logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                ////Escribir Log
                //logAudit.LogApp(logSales);

                //Devolver vista de error
                //return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
                return null;
            }
        }
        #endregion

        #region MÉTODOS DE CLASE
        ///// <summary>
        ///// Método para cargar ciudades y teatros
        ///// </summary>
        ///// <param name="pr_flag">Parámetro de ciudad</param>
        //private void Ciuteatros(string pr_flag = "")
        //{
        //    #region VARIABLES LOCALES
        //    General ob_fncgrl = new General();
        //    List<teatro> ls_ciudades = new List<teatro>();
        //    #endregion

        //    //Obtener listado de ciudades, teatros y recorrer ciudades
        //    List<teatro> ls_ciuteatros = ob_fncgrl.Ciudades(config.Value.Ciudades41);
        //    var ls_auxciudad = ls_ciuteatros.Where(x => x.Habilitado == "S").Select(x => x.ciudad).Distinct().ToList();
        //    foreach (var item in ls_auxciudad)
        //    {
        //        //Asignar objeto ciudades y validar ciudad por defecto o selecionada
        //        teatro ob_auxitem = new teatro();

        //        //Validar flag
        //        if (pr_flag == "SEL")
        //        {
        //            ob_auxitem.id = "0";
        //            ob_auxitem.ciudad = item.ToString();

        //            ViewBag.NombreCiudad = Session.GetString("CiudadTeatro");
        //            ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
        //        }
        //        else
        //        {
        //            if (item == pr_flag)
        //            {
        //                ob_auxitem.id = "1";
        //                ob_auxitem.ciudad = item.ToString();

        //                ViewBag.NombreCiudad = item.ToString();
        //                if (Session.GetString("TeatroNombre") != null)
        //                    ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
        //                else
        //                    ViewBag.NombreCiudadTeatro = "Seleccionar Teatro";

        //                Session.Remove("Teatro");
        //                Session.Remove("TeatroNombre");
        //                Session.Remove("CiudadTeatro");
        //                Session.SetString("CiudadTeatro", item.ToString());
        //            }
        //            else
        //            {
        //                //Normalizar valores y validar ciudad
        //                string auxCiudad = Regex.Replace(item.Normalize(NormalizationForm.FormD), @"[^a-zA-z0-9 ]+", "");
        //                if (pr_flag == "" && auxCiudad == config.Value.CiuDefault)
        //                {
        //                    ob_auxitem.id = "1";
        //                    ob_auxitem.ciudad = item.ToString();

        //                    ViewBag.NombreCiudad = item.ToString();
        //                    ViewBag.NombreCiudadTeatro = config.Value.NomDefault;

        //                    Session.Remove("Teatro");
        //                    Session.Remove("TeatroNombre");
        //                    Session.Remove("CiudadTeatro");

        //                    Session.SetString("Teatro", config.Value.TeaDefault);
        //                    Session.SetString("TeatroNombre", config.Value.NomDefault);
        //                    Session.SetString("CiudadTeatro", item.ToString());
        //                }
        //                else
        //                {
        //                    ob_auxitem.id = "0";
        //                    ob_auxitem.ciudad = item.ToString();
        //                }
        //            }
        //        }

        //        //Adicionar ciudad a lista
        //        ls_ciudades.Add(ob_auxitem);
        //    }

        //    ViewBag.Ciudades = ls_ciudades;
        //    ViewBag.TeatrosList = ls_ciuteatros;
        //}

        ///// <summary>
        ///// Método para asignar teatro
        ///// </summary>
        ///// <param name="pr_ciudad">Parámetro de ciudad</param>
        ///// <param name="pr_teatro">Parámetro de id teatro</param>
        ///// <param name="pr_nomteatro">Parámetro de nombre teatro</param>
        //private void Selteatros(string pr_ciudad, string pr_teatro, string pr_nomteatro, string pr_url)
        //{
        //    //Validar si la selecion es en sala
        //    if (pr_url.Contains("room") || pr_url.Contains("detail"))
        //    {
        //        #region VARIABLES LOCALES
        //        int lc_idearr = 0;
        //        string lc_result = string.Empty;
        //        string lc_srvpar = string.Empty;
        //        string lc_auxitm = string.Empty;
        //        string[] ls_lstsel = new string[5];
        //        var PuntoVenta = config.Value.PuntoVenta;
        //        var KeyTeatro = Session.GetString("Teatro");
        //        General ob_fncgrl = new General();
        //        List<string> ls_lstubi = new List<string>();
        //        Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
        //        #endregion

        //        //Validar secuenicia
        //        if (Session.GetString("Secuencia") != null)
        //        {
        //            //Borrar retail
        //            decimal lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
        //            using (var context = new DataDB(config))
        //            {
        //                //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
        //                PuntoVenta = config.Value.PuntoVenta;
        //                KeyTeatro = Session.GetString("Teatro");
        //                var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec1).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).ToList();
        //                foreach (var retail in RetailSales)
        //                {
        //                    //Borrar retail detcombo
        //                    lc_secsec1 = Convert.ToDecimal(Session.GetString("Secuencia"));
        //                    using (var context1 = new DataDB(config))
        //                    {
        //                        //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
        //                        var RetailDet = context.RetailDet.Where(x => x.IdRetailSales == retail.Id).ToList();
        //                        foreach (var retaildet in RetailDet)
        //                        {
        //                            //Eliminar item y guardar registro a tabla
        //                            context.RetailDet.Remove(retaildet);
        //                            context.SaveChanges();
        //                        }
        //                    }

        //                    //Eliminar item y guardar registro a tabla
        //                    context.RetailSales.Remove(retail);
        //                    context.SaveChanges();
        //                }


        //                //Borrar boletas
        //                string lc_secsec2 = Session.GetString("Secuencia");

        //                var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec2).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
        //                foreach (var report in ReportSales)
        //                {
        //                    //Obtener ubicaciones de vista
        //                    char[] ar_charst = report.SelUbicaciones.ToCharArray();
        //                    for (int lc_iditem = 0; lc_iditem < ar_charst.Length; lc_iditem++)
        //                    {
        //                        //Concatenar caracteres
        //                        lc_auxitm += ar_charst[lc_iditem].ToString();

        //                        //Obtener parámetro
        //                        if (ar_charst[lc_iditem].ToString() == ";")
        //                        {
        //                            ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
        //                            lc_auxitm = string.Empty;
        //                        }
        //                    }

        //                    //Cargar ubicaciones al modelo JSON
        //                    lc_auxitm = string.Empty;
        //                    foreach (var item in ls_lstubi)
        //                    {
        //                        lc_idearr = 0;
        //                        char[] ar_chars2 = item.ToCharArray();
        //                        for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
        //                        {
        //                            //Concatenar caracteres
        //                            lc_auxitm += ar_chars2[lc_iditem].ToString();

        //                            //Obtener parámetro
        //                            if (ar_chars2[lc_iditem].ToString() == "_")
        //                            {
        //                                ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);

        //                                lc_idearr++;
        //                                lc_auxitm = string.Empty;
        //                            }
        //                        }

        //                        #region SCOSIL
        //                        LiberaSilla ob_libsrv = new LiberaSilla();
        //                        ob_libsrv.Fila = ls_lstsel[3];
        //                        ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
        //                        ob_libsrv.teatro = Convert.ToInt32(Session.GetString("Teatro"));
        //                        ob_libsrv.Funcion = Convert.ToInt32(report.HorProg);
        //                        ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
        //                        ob_libsrv.Usuario = 777;
        //                        ob_libsrv.tercero = config.Value.ValorTercero;
        //                        ob_libsrv.FechaFuncion = report.FecProg;

        //                        //Generar y encriptar JSON para servicio
        //                        lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);

        //                        lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion");
        //                        lc_srvpar = lc_srvpar.Replace("sala", "Sala");
        //                        lc_srvpar = lc_srvpar.Replace("funcion", "Funcion");
        //                        lc_srvpar = lc_srvpar.Replace("fila", "Fila");
        //                        lc_srvpar = lc_srvpar.Replace("columna", "Columna");
        //                        lc_srvpar = lc_srvpar.Replace("usuario", "Usuario");

        //                        //Encriptar Json LIB
        //                        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //                        //Consumir servicio LIB
        //                        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
        //                        #endregion
        //                    }

        //                    //Eliminar item y guardar registro a tabla
        //                    context.ReportSales.Remove(report);
        //                    context.SaveChanges();
        //                }
        //            }

        //            //Quitar secuencia
        //            Session.Remove("Secuencia");
        //        }
        //    }
        //    /*if (pr_url.Contains("room") || pr_url.Contains("detail"))
        //    {
        //        #region VARIABLES LOCALES
        //        int lc_idearr = 0;
        //        string lc_result = string.Empty;
        //        string lc_srvpar = string.Empty;
        //        string lc_auxitm = string.Empty;
        //        string[] ls_lstsel = new string[5];
        //        var PuntoVenta = config.Value.PuntoVenta;
        //        var KeyTeatro = Session.GetString("Teatro");
        //        General ob_fncgrl = new General();
        //        List<string> ls_lstubi = new List<string>();
        //        Dictionary<string, string> ob_diclst = new Dictionary<string, string>();
        //        #endregion

        //        // Validar secuencia
        //        var secuencia = Session.GetString("Secuencia");
        //        if (!string.IsNullOrEmpty(secuencia))
        //        {
        //            decimal lc_secsec1 = Convert.ToDecimal(secuencia);

        //            using (var context = new DataDB(config))
        //            {
        //                // Borrar retail
        //                var RetailSales = context.RetailSales
        //                    .Where(x => x.Secuencia == lc_secsec1 && x.PuntoVenta == Convert.ToDecimal(PuntoVenta) && x.KeyTeatro == Convert.ToDecimal(KeyTeatro))
        //                    .ToList();

        //                foreach (var retail in RetailSales)
        //                {
        //                    // Borrar retail detcombo
        //                    var RetailDet = context.RetailDet
        //                        .Where(x => x.IdRetailSales == retail.Id)
        //                        .ToList();

        //                    foreach (var retaildet in RetailDet)
        //                    {
        //                        context.RetailDet.Remove(retaildet);
        //                    }

        //                    context.RetailSales.Remove(retail);
        //                }

        //                // Borrar boletas
        //                var ReportSales = context.ReportSales
        //                    .Where(x => x.Secuencia == secuencia && x.KeyPunto == PuntoVenta && x.KeyTeatro == KeyTeatro)
        //                    .ToList();

        //                foreach (var report in ReportSales)
        //                {
        //                    var selUbicaciones = report.SelUbicaciones;

        //                    for (int lc_iditem = 0; lc_iditem < selUbicaciones.Length; lc_iditem++)
        //                    {
        //                        lc_auxitm += selUbicaciones[lc_iditem];

        //                        if (selUbicaciones[lc_iditem] == ';')
        //                        {
        //                            ls_lstubi.Add(lc_auxitm.Substring(0, lc_auxitm.Length - 1));
        //                            lc_auxitm = string.Empty;
        //                        }
        //                    }

        //                    lc_auxitm = string.Empty;

        //                    foreach (var item in ls_lstubi)
        //                    {
        //                        lc_idearr = 0;
        //                        var ar_chars2 = item.ToCharArray();

        //                        for (int lc_iditem = 0; lc_iditem < ar_chars2.Length; lc_iditem++)
        //                        {
        //                            lc_auxitm += ar_chars2[lc_iditem];

        //                            if (ar_chars2[lc_iditem] == '_')
        //                            {
        //                                ls_lstsel[lc_idearr] = lc_auxitm.Substring(0, lc_auxitm.Length - 1);
        //                                lc_idearr++;
        //                                lc_auxitm = string.Empty;
        //                            }
        //                        }

        //                        // SCOSIL
        //                        LiberaSilla ob_libsrv = new LiberaSilla();
        //                        ob_libsrv.Fila = ls_lstsel[3];
        //                        ob_libsrv.Sala = Convert.ToInt32(report.KeySala);
        //                        ob_libsrv.teatro = Convert.ToInt32(KeyTeatro);
        //                        ob_libsrv.Funcion = Convert.ToInt32(report.HorProg);
        //                        ob_libsrv.Columna = Convert.ToInt32(ls_lstsel[4]);
        //                        ob_libsrv.Usuario = 777;
        //                        ob_libsrv.tercero = config.Value.ValorTercero;
        //                        ob_libsrv.FechaFuncion = report.FecProg;

        //                        // Generar y encriptar JSON para servicio
        //                        lc_srvpar = ob_fncgrl.JsonConverter(ob_libsrv);
        //                        lc_srvpar = lc_srvpar.Replace("fechaFuncion", "FechaFuncion")
        //                                           .Replace("sala", "Sala")
        //                                           .Replace("funcion", "Funcion")
        //                                           .Replace("fila", "Fila")
        //                                           .Replace("columna", "Columna")
        //                                           .Replace("usuario", "Usuario");

        //                        // Encriptar Json LIB
        //                        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //                        // Consumir servicio LIB
        //                        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scosil/"), lc_srvpar);
        //                    }

        //                    // Eliminar item y guardar registro a tabla
        //                    context.ReportSales.Remove(report);
        //                }

        //                // Guardar cambios en la base de datos
        //                context.SaveChanges();
        //            }

        //            // Quitar secuencia
        //            Session.Remove("Secuencia");
        //        }
        //    }*/
        //    //Cargar ciudad seleccionada
        //    Ciuteatros(pr_ciudad);

        //    //Cargar Teatro
        //    Session.SetString("Teatro", pr_teatro);
        //    Session.SetString("TeatroNombre", pr_nomteatro);
        //    ViewBag.NombreCiudadTeatro = pr_nomteatro;
        //}

        ///// <summary>
        ///// Método para obtener las horas y tarifas de la fecha seleccionada de la película
        ///// </summary>
        ///// <param name="pr_keypel">ID película</param>
        ///// <param name="pr_fecprg">Fecha seleccionada</param>
        ///// <returns></returns>
        //private List<sala> GetHora(string pr_keypel, string pr_fecprg)
        //{
        //    #region VARIABLES LOCALES
        //    int lc_keypel = 0;
        //    int lc_auxpel = 0;
        //    int lc_keytea = 0;
        //    int lc_auxtea = 0;
        //    string lc_srvpar = string.Empty;
        //    string lc_result = string.Empty;

        //    XmlDocument ob_xmldoc = new XmlDocument();
        //    Dictionary<string, string> dc_zonas = new Dictionary<string, string>();

        //    BolVenta ob_datprg = new BolVenta();
        //    List<sala> ob_lisprg = new List<sala>();
        //    General ob_fncgrl = new General();
        //    #endregion

        //    try
        //    {
        //        //Validar fecha
        //        if (pr_fecprg == "")
        //            pr_fecprg = DateTime.Now.ToString("yyyyMMdd");

        //        ViewBag.Fecha3 = pr_fecprg;

        //        //Obtener información de la web
        //        ob_xmldoc.Load(config.Value.Variables41);
        //        XmlNodeList pelicula = ob_xmldoc.GetElementsByTagName("pelicula");

        //        //Recorrer xml y obtener datos
        //        foreach (XmlElement item in pelicula)
        //        {
        //            //Validar película
        //            lc_keypel = Convert.ToInt32(item.GetAttribute("id"));
        //            lc_auxpel = Convert.ToInt32(pr_keypel);

        //            if (lc_keypel == lc_auxpel)
        //            {
        //                //Datos de nodo pelicula
        //                ViewBag.Pelicula = item.GetAttribute("nombre").ToString();

        //                //Datos de nodo pelicula/data
        //                XmlNodeList data = item.GetElementsByTagName("data");
        //                foreach (XmlElement itemdata in data)
        //                    ViewBag.Imagen = itemdata.GetAttribute("Imagen").ToString();

        //                // Datos de nodo pelicula / cinemas
        //                XmlNodeList cinemas = item.GetElementsByTagName("cinemas");
        //                foreach (XmlElement item2 in cinemas)
        //                {
        //                    // Datos de nodo pelicula / cinemas / cinema
        //                    XmlNodeList cinema = item2.GetElementsByTagName("cinema");
        //                    foreach (XmlElement itemT in cinema)
        //                    {
        //                        //Validar Teatro
        //                        lc_keytea = Convert.ToInt32(itemT.GetAttribute("id"));
        //                        lc_auxtea = Convert.ToInt32(Session.GetString("Teatro"));

        //                        if (lc_keytea == lc_auxtea)
        //                        {
        //                            // Datos de nodo pelicula / salas
        //                            XmlNodeList salas = itemT.GetElementsByTagName("salas");
        //                            foreach (XmlElement item3 in salas)
        //                            {
        //                                //Datos de nodo pelicula / salas / sala
        //                                XmlNodeList sala = item3.GetElementsByTagName("sala");
        //                                foreach (XmlElement itemS in sala)
        //                                {
        //                                    //Obtener datos
        //                                    sala ob_room = new sala();
        //                                    ob_room.tipoSala = itemS.GetAttribute("tipoSala");
        //                                    ob_room.numeroSala = itemS.GetAttribute("numeroSala");
        //                                    ob_lisprg.Add(ob_room);

        //                                    //Datos de nodo pelicula / salas / dia
        //                                    XmlNodeList Fecha = itemS.GetElementsByTagName("Fecha");
        //                                    foreach (XmlElement item4 in Fecha)
        //                                    {
        //                                        //Validar fecha
        //                                        if (pr_fecprg == item4.GetAttribute("univ").ToString())
        //                                        {
        //                                            //Datos Nodo fecha
        //                                            ViewBag.Fecha2 = item4.GetAttribute("dia").ToString();

        //                                            // Datos de nodo pelicula / salas / dia / Hora
        //                                            IList<hora> ls_prgtmp = new List<hora>();
        //                                            XmlNodeList hora = item4.GetElementsByTagName("hora");
        //                                            foreach (XmlElement item5 in hora)
        //                                            {
        //                                                if (item5.GetAttribute("webserviceVentas") == "Si")
        //                                                {
        //                                                    //Obtener horas
        //                                                    string horuno = item5.GetAttribute("militar");
        //                                                    DateTime FechaHoraInicio = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy") + " " + horuno.Substring(0, 2) + ":" + horuno.Substring(2, 2) + ":00");
        //                                                    DateTime FechaHoraTermino = DateTime.ParseExact(DateTime.Now.ToString("HH:mm"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);

        //                                                    //Validar pintada de la hora
        //                                                    hora ob_prgtmp = new hora();
        //                                                    if (config.Value.MinDifHora != "0")
        //                                                    {
        //                                                        //Validar hora vs valor programado solo para hoy
        //                                                        if (DateTime.Now.ToString("yyyyMMdd") == pr_fecprg)
        //                                                        {
        //                                                            //Diferencia de tiempo entre hora funcion y hora del dia 
        //                                                            TimeSpan diferencia = FechaHoraInicio - FechaHoraTermino;
        //                                                            var diferenciaenminutos = diferencia.TotalMinutes;

        //                                                            if (diferenciaenminutos > Convert.ToDouble(config.Value.MinDifHora))
        //                                                            {
        //                                                                ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
        //                                                                ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
        //                                                            }
        //                                                            else
        //                                                            {
        //                                                                continue;
        //                                                            }
        //                                                        }
        //                                                        else
        //                                                        {
        //                                                            ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
        //                                                            ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
        //                                                        }
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        ob_prgtmp.militar = item5.GetAttribute("militar").ToString();
        //                                                        ob_prgtmp.horario = item5.GetAttribute("horario").ToString();
        //                                                    }

        //                                                    //Datos de nodo pelicula / salas / dia / hora / TipoZona
        //                                                    XmlNodeList zona = item5.GetElementsByTagName("TipoZona");
        //                                                    IList<TipoZona> ls_zontmp = new List<TipoZona>();
        //                                                    foreach (XmlElement item6 in zona)
        //                                                    {
        //                                                        TipoZona lc_zona = new TipoZona();
        //                                                        lc_zona.nombreZona = item6.GetAttribute("nombreZona");

        //                                                        if (!dc_zonas.ContainsKey(lc_zona.nombreZona))
        //                                                            dc_zonas.Add(lc_zona.nombreZona, lc_zona.nombreZona);

        //                                                        //Datos de nodo peliculas / salas / dia / hora / TipoZona / TipoSilla
        //                                                        XmlNodeList silla = item6.GetElementsByTagName("TipoSilla");
        //                                                        IList<TipoSilla> ls_siltmp = new List<TipoSilla>();
        //                                                        foreach (XmlElement item7 in silla)
        //                                                        {
        //                                                            TipoSilla lc_silla = new TipoSilla();
        //                                                            lc_silla.nombreTipoSilla = item7.GetAttribute("nombreTipoSilla");

        //                                                            //Datos de nodo peliculas / salas / dia / hora / TipoZona / TipoSilla / Tarifa
        //                                                            IList<Tarifa> ls_tartmp = new List<Tarifa>();
        //                                                            XmlNodeList tarifa = item7.GetElementsByTagName("Tarifa");
        //                                                            foreach (XmlElement item8 in tarifa)
        //                                                            {

        //                                                                //Validar tarifas terceros
        //                                                                if (item8.GetAttribute("validoTeceros") == "Si" && item8.GetAttribute("clienteFrecuente") == Session.GetString("ClienteFrecuente"))
        //                                                                {
        //                                                                    Tarifa ob_tartmp = new Tarifa();
        //                                                                    ob_tartmp.codigoTarifa = item8.GetAttribute("codigoTarifa").ToString();
        //                                                                    ob_tartmp.nombreTarifa = item8.GetAttribute("nombreTarifa").ToString();
        //                                                                    ob_tartmp.valor = item8.GetAttribute("valor").ToString().Substring(0, item8.GetAttribute("valor").ToString().Length - 2);

        //                                                                    //Adiconar a lista para mostrar
        //                                                                    ls_tartmp.Add(ob_tartmp);
        //                                                                }
        //                                                            }

        //                                                            lc_silla.Tarifa = ls_tartmp;
        //                                                            ls_siltmp.Add(lc_silla);
        //                                                        }

        //                                                        lc_zona.TipoSilla = ls_siltmp;
        //                                                        ls_zontmp.Add(lc_zona);

        //                                                    }
        //                                                    ob_prgtmp.TipoZonaOld = ls_zontmp;
        //                                                    ls_prgtmp.Add(ob_prgtmp);
        //                                                }
        //                                                else
        //                                                {
        //                                                    continue;
        //                                                }

        //                                                //Adiconar a lista para mostrar
        //                                                ob_room.hora = ls_prgtmp;

        //                                                //Cortar el ciclo
        //                                                //break;
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }

        //                            //Cortar el ciclo
        //                            break;
        //                        }
        //                    }
        //                }

        //                //Cortar el ciclo
        //                break;
        //            }
        //        }
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        //Generar Log
        //        LogSales logSales = new LogSales();
        //        LogAudit logAudit = new LogAudit(config);
        //        logSales.Id = Guid.NewGuid().ToString();
        //        logSales.Fecha = DateTime.Now;
        //        logSales.Programa = "SalesBol/GetHora";
        //        logSales.Metodo = "METHOD";
        //        logSales.ExceptionMessage = lc_syserr.Message;
        //        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

        //        //Escribir Log
        //        logAudit.LogApp(logSales);

        //        //Devolver vista de error
        //        RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
        //    }

        //    //Devolver valores
        //    ViewBag.Zonas = dc_zonas;
        //    return ob_lisprg;
        //}

        ///// <summary>
        ///// Método para cargar URL de Header y Footer del portal
        ///// </summary>
        ///// <returns></returns>
        //private void URLPortal(IOptions<MyConfig> config)
        //{
        //    //Inicializar valores
        //    ViewBag.URLength = HttpContext.Request.Path.ToString().Length;
        //    ViewBag.URLcartelerawp = config.Value.CarteleraWP;

        //    if (Session.GetString("Finhora") != null)
        //        ViewBag.FlagConf = Session.GetString("Finhora");
        //    else
        //        ViewBag.FlagConf = "S";

        //    ViewBag.URLfb = config.Value.URLfb;
        //    ViewBag.URLig = config.Value.URLig;
        //    ViewBag.URLtw = config.Value.URLtw;
        //    ViewBag.URLyb = config.Value.URLyb;
        //    ViewBag.URLtk = config.Value.URLtk;
        //    ViewBag.URLfaqs = config.Value.URLfaqs;
        //    ViewBag.URLblog = config.Value.URLblog;
        //    ViewBag.URLtarifas = config.Value.URLtarifas;
        //    ViewBag.URLprocinal = config.Value.URLprocinal;
        //    ViewBag.URLcontacto = config.Value.URLcontacto;
        //    ViewBag.URLtermycond = config.Value.URLtermycond;
        //    ViewBag.URLpoliticas = config.Value.URLpoliticas;
        //    ViewBag.URLservicios = config.Value.URLservicios;
        //    ViewBag.URLprotocolos = config.Value.URLprotocolos;
        //    ViewBag.URLexperiencias = config.Value.URLexperiencias;
        //    ViewBag.URLsobreprocinal = config.Value.URLsobreprocinal;
        //    ViewBag.URLpromociones = config.Value.URLpromociones;
        //    ViewBag.URLeticaytra = config.Value.URLeticaytra;
        //    ViewBag.URLlaft = config.Value.URLlaft;
        //    ViewBag.URLresoluccn = config.Value.URLresoluccn;
        //    ViewBag.URLcinefans = config.Value.URLcinefans;

        //    //Validar ciudad
        //    if (Session.GetString("CiudadTeatro") != null)
        //        ViewBag.NombreCiudad = Session.GetString("CiudadTeatro");
        //    else
        //        ViewBag.NombreCiudad = "Sin Ciudad";

        //    //Validar teatro
        //    if (Session.GetString("TeatroNombre") != null)
        //        ViewBag.NombreCiudadTeatro = Session.GetString("TeatroNombre");
        //    else
        //        ViewBag.NombreCiudadTeatro = "Sin Teatro";

        //    ViewBag.FlagLogin = Session.GetString("FlagLogin");

        //    //Validar inicio de sesión
        //    ViewBag.NombreUsuario = null;
        //    if (Session.GetString("Usuario") != null)
        //    {
        //        ViewBag.NombreUsuario = "Bienvenido " + Session.GetString("Nombre");

        //        ViewBag.USUNombre = Session.GetString("Nombre");
        //        ViewBag.USUApellido = Session.GetString("Apellido");
        //        ViewBag.USUTelefono = Session.GetString("Telefono");
        //        ViewBag.USUDireccion = Session.GetString("Direccion");
        //        ViewBag.USUDocumento = Session.GetString("Documento");
        //    }
        //}

        ///// <summary>
        ///// Método para obtener lista de carrito de compras
        ///// </summary>
        //private void ListCarrito()
        //{
        //    #region VARIABLES LOCALES
        //    decimal lc_secsec = 0;
        //    var PuntoVenta = config.Value.PuntoVenta;
        //    var KeyTeatro = Session.GetString("Teatro");
        //    #endregion

        //    //Validar secuencia y asignar valores
        //    ViewBag.Venta = "V";
        //    ViewBag.Secuencia = Session.GetString("Secuencia");
        //    ViewBag.ListCarritoB = null;
        //    ViewBag.ListCarritoR = null;
        //    ViewBag.NombreTeatro = Session.GetString("TeatroNombre");
        //    lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
        //    if (Session.GetString("Secuencia") != null)
        //    {
        //        //Obtener productos carrito de compra
        //        lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
        //        using (var context = new DataDB(config))
        //        {
        //            var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == Convert.ToDecimal(PuntoVenta)).Where(x => x.KeyTeatro == Convert.ToDecimal(KeyTeatro)).ToList();
        //            ViewBag.ListCarritoR = RetailSales;

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

        ///// <summary>
        ///// Método para obtener listado de fechas de cartelera de películas
        ///// </summary>
        ///// <param name="pr_fecprg">fecha seleccionada</param>
        ///// <returns></returns>
        //private List<DateCartelera> DatePortal(string pr_fecprg, string pr_tippel, string pr_keypel = "0")
        //{

        //    DateTime dt_fechoy = DateTime.Now;
        //    var helper = new Helper();


        //    if (string.IsNullOrEmpty(pr_fecprg))
        //        pr_fecprg = dt_fechoy.ToString("yyyyMMdd");

        //    // Construir la URL completa con el valor de la sesión del teatro
        //    string url = config.Value.Variables41TP;

        //    XDocument xdoc = XDocument.Load(url.Replace("xxx", Session.GetString("Teatro"))
        //                                           .Replace("yyy", (pr_keypel.Length >= 5 ? pr_keypel.Substring(0, pr_keypel.Length - 5) : pr_keypel)));

        //    var ob_fechas = (

        //        from pelicula in xdoc.Descendants("pelicula")
        //        where pelicula.Attribute("tipo")?.Value == pr_tippel && pelicula.Attribute("id")?.Value == pr_keypel
        //        from cinema in pelicula.Descendants("cinema")
        //        where cinema.Attribute("id")?.Value == Session.GetString("Teatro").ToString()
        //        from dia in pelicula.Descendants("DiasDisponiblesTodosCinemas").Descendants("dia")
        //        let auxFec = dia.Attribute("univ")?.Value
        //        where !string.IsNullOrEmpty(auxFec)
        //        let dtAuxFec = DateTime.ParseExact(auxFec, "yyyyMMdd", CultureInfo.InvariantCulture)
        //        group new { dtAuxFec, auxFec } by dtAuxFec.Date into grouped
        //        select new DateCartelera
        //        {
        //            DiaLt = helper.DiaMes(grouped.Key.DayOfWeek.ToString(), "D"),
        //            Flags = (pr_fecprg == grouped.First().auxFec) ? "S" : "N",
        //            FecDt = grouped.Key,
        //            FecSt = grouped.First().auxFec,
        //            DiaNb = grouped.Key.ToString("dd"),
        //            MesLt = helper.DiaMes(grouped.First().auxFec.Substring(4, 2), "M")
        //        }
        //    ).OrderBy(o => o.FecDt).ToList();

        //    ViewBag.Mes = helper.DiaMes(pr_fecprg.Substring(4, 2), "M");
        //    return ob_fechas;
        //}



        #endregion
    }
    #endregion
}