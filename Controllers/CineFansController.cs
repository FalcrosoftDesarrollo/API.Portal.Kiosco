/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : CineFansController.cs                                                    *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase controlador que contiene los métodos para interactuar con las      *
*                páginas de la vista                                                      *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
*   Detalle Cambio: Refactorizacion código -> (Antoine Román - Falcrosoft) 02/01/2024     *
******************************************************************************************/

using APIPortalKiosco.Data;
using APIPortalKiosco.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace APIPortalKiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CineFansController : ControllerBase
    {
        #region CONSTRUCTOR
        public ISession Session => this.HttpContext.Session;
        private readonly IOptions<MyConfig> config;
        public CineFansController(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        #endregion

        #region GET
        /// <summary>
        /// GET: CineFans -- Perfil del usuario logueado/registrado
        /// </summary>
        /// <returns></returns>
        /// 
        
        [HttpGet]
        [Route("CineFans")]
        public CineFansData CineFans(string Documento, string Passwrd)
        {

            #region VARIABLES LOCALES
            var cinefansData = new CineFansData();
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;
            string Usuario = string.Empty;

            List<CinefansDET> ob_cfsdet = new List<CinefansDET>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            Login pr_datlog = new Login();
            General ob_fncgrl = new General();
            Cinefans ob_rtacnfs = new Cinefans();
            CinefansSRV ob_servicio = new CinefansSRV();
            CinefansINI ob_cfinicio = new CinefansINI();
            #endregion

            try
            {
                
                var userDocument = new UserDocumento();
                userDocument.Documento = Documento;
                userDocument.tercero = "1";

                //Generar y encriptar JSON para servicio
                lc_srvpar = JsonConvert.SerializeObject(userDocument);

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scoced/"), lc_srvpar);
                if (lc_result.Substring(0, 1) == "0")
                {
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result.Replace("0-[", "["), (typeof(Dictionary<string, string>)));
                }

                Usuario = ob_diclst["Login"].ToString();

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

                #region SCOMOV
                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scomov/"), lc_srvpar);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
                    if (ob_diclst.ContainsKey("puntos_acumulados"))
                    {
                        ob_rtacnfs.puntos_vencidos = Convert.ToDecimal(ob_diclst["puntos_vencidos"]);
                        ob_rtacnfs.puntos_redimidos = Convert.ToDecimal(ob_diclst["puntos_redimidos"]);
                        ob_rtacnfs.puntos_acumulados = Convert.ToDecimal(ob_diclst["puntos_acumulados"]);
                        ob_rtacnfs.puntos_disponibles = Convert.ToDecimal(ob_diclst["puntos_disponibles"]);
                    }
                    else
                    {
                        ob_rtacnfs.puntos_vencidos = 0;
                        ob_rtacnfs.puntos_redimidos = 0;
                        ob_rtacnfs.puntos_acumulados = 0;
                        ob_rtacnfs.puntos_disponibles = 0;
                    }
                }

                #endregion

                #region SCODES
                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scodes/"), lc_srvpar);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-[", "[");
                    ob_cfsdet = (List<CinefansDET>)JsonConvert.DeserializeObject(lc_result, (typeof(List<CinefansDET>))); //Deserializar Json y validar respuesta
                }
                
                #endregion

                #region SCOHIS
                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scohis/"), lc_srvpar);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    if (!lc_result.Contains("Validación"))
                    {
                        //Quitar switch
                        lc_result = lc_result.Replace("0-{", "{");
                        ob_cfinicio = (CinefansINI)JsonConvert.DeserializeObject(lc_result, (typeof(CinefansINI))); //Deserializar Json y validar respuesta

                        cinefansData.FechaCF = ob_cfinicio.VenCineFan;
                        cinefansData.FechaCL = ob_cfinicio.VenCliFrec;
                        cinefansData.FechaCB = ob_cfinicio.VenCashBack;
                        cinefansData.FechaCBDD = Convert.ToDateTime(ob_cfinicio.VenCashBack).Day.ToString();
                        cinefansData.FechaCBMM = Convert.ToDateTime(ob_cfinicio.VenCashBack).Month < 10 ? DiaMes("0" + Convert.ToDateTime(ob_cfinicio.VenCashBack).Month.ToString(), "M") : DiaMes(Convert.ToDateTime(ob_cfinicio.VenCashBack).Month.ToString(), "M");
                        //cinefansData.Saldo = ob_cfinicio.Saldo;
                        cinefansData.Nivel = ob_cfinicio.NivelCliente;
                        cinefansData.ReclsfcDD = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Day.ToString();
                        cinefansData.ReclsfcMM = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month < 10 ? DiaMes("0" + Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month.ToString(), "M") : DiaMes(Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month.ToString(), "M");
                        cinefansData.ReclsfcYY = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Year.ToString();
                        //cinefansData.Visitas = Convert.ToInt32(ob_cfinicio.Visitas).ToString();
                        //cinefansData.VisitasTotal = config.Value.VistasCF;
                        cinefansData.VisitasFalta = Convert.ToString(Convert.ToInt32(config.Value.VistasCF) - Convert.ToInt32(ob_cfinicio.Visitas));
                        cinefansData.NivelCF = ob_cfinicio.NivelCliente.ToString();

                        foreach (var item in ob_cfinicio.Saldo)
                            cinefansData.Acumulado = String.Format("{0:C0}", Convert.ToInt32(item.Saldo));

                    }
                    else
                    {
                        cinefansData.FechaCF = DateTime.Now.Year.ToString() + "0101";
                        cinefansData.FechaCL = DateTime.Now.Year.ToString() + "0101";
                        cinefansData.FechaCB = DateTime.Now.Year.ToString() + "0101";
                        cinefansData.FechaCBDD = "DD";
                        cinefansData.FechaCBMM = "MM";
                        cinefansData.Saldo = 0;
                        cinefansData.Nivel = 1;
                        cinefansData.ReclsfcDD = "DD";
                        cinefansData.ReclsfcMM = "MM";
                        cinefansData.ReclsfcYY = "YYYY";
                        cinefansData.Visitas = 0;
                        //cinefansData.VisitasTotal = config.Value.VistasCF;
                        cinefansData.VisitasFalta = config.Value.VistasCF;
                    }
                }
       
                #endregion

                #region SCOLOG
                //Asignar valores
                pr_datlog.Correo = Session.GetString("Usuario");
                pr_datlog.Password = Session.GetString("Passwrd2");
                pr_datlog.Tercero = config.Value.ValorTercero;

                //Generar y encriptar JSON para servicio
                lc_srvpar = ob_fncgrl.JsonConverter(pr_datlog);
                lc_srvpar = lc_srvpar.Replace("correo", "Correo");
                lc_srvpar = lc_srvpar.Replace("password", "Clave");

                //Encriptar Json
                lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

                //Consumir servicio
                lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scolog/"), lc_srvpar);

                //Validar respuesta
                if (lc_result.Substring(0, 1) == "0")
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    lc_result = lc_result.Replace("[", "");
                    lc_result = lc_result.Replace("]", "");

                    //Deserializar Json y validar respuesta
                    ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
                    if (ob_diclst.ContainsKey("VÁLIDA"))
                    {
                        //Devolver a vista
                        ModelState.AddModelError("", ob_diclst["Validación"].ToString());                       
                    }
                    else
                    {
                        if (ob_diclst["Valor"].ToString() == "VÁLIDA")
                        {
                            cinefansData.Genero = ob_diclst["Genero"].ToString();
                            cinefansData.Barrio = ob_diclst["Barrio"].ToString();
                            cinefansData.Municipio = ob_diclst["Municipio"].ToString();
                            cinefansData.Celular = ob_diclst["Celular"].ToString();
                        }
                    }
                }

                #endregion

                //Valores a mostrar
                cinefansData.Correo = Usuario;
                //cinefansData.Nombre = Session.GetString("Nombre");
                //cinefansData.Apellido = Session.GetString("Apellido");
                //cinefansData.Documento = Session.GetString("Documento");

                //cinefansData.Movimientos = ob_cfsdet.OrderByDescending(m => m.Fecha).ToList();
                //cinefansData.NombreCompleto = Session.GetString("Nombre") + " " + Session.GetString("Apellido");
            }
            catch (Exception lc_syserr)
            {
                
            }


            //Devolver a Vista
            return cinefansData;
        }

        /// <summary>
        /// GET: Registro -- Inscripción en el programa cinefans
        /// </summary>
        /// <returns></returns>
        /// 
        //[HttpGet]
        //[Route("RegistroCineFans")]
        //public ActionResult Registro(string pr_flgusu = "")
        //{
        //    //URLPortal(config);
        //    //ListCarrito();

        //    ViewBag.pr_usuflg = pr_flgusu;

        //    //Validar inicio de sesión
        //    if (Session.GetString("Usuario") != null)
        //        return RedirectToAction("CineFans", "CineFans");

        //    //Devolver a Vista
        //    return View();
        //}


        /// <summary>
        /// GET: Beneficios -- Beneficios de club cinefans otorgados al usuario
        /// </summary>
        /// <returns></returns>
        /// 
        
        [HttpGet]
        [Route("Beneficios")]
        public CineFansData Beneficios()
        {
            #region VARIABLES LOCALES
            var cinefansData = new CineFansData();
            string lc_result = string.Empty;
            string lc_srvpar = string.Empty;

            List<CinefansDET> ob_cfsdet = new List<CinefansDET>();
            Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

            Login pr_datlog = new Login();
            General ob_fncgrl = new General();
            Cinefans ob_rtacnfs = new Cinefans();
            CinefansSRV ob_servicio = new CinefansSRV();
            CinefansINI ob_cfinicio = new CinefansINI();
            #endregion

            //URLPortal(config);
            //ListCarrito();

            // Asignar Valores
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

            #region SCOHIS
            //Consumir servicio
            lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scohis/"), lc_srvpar);

            //Validar respuesta
            if (lc_result.Substring(0, 1) == "0")
            {
                if (!lc_result.Contains("Validación"))
                {
                    //Quitar switch
                    lc_result = lc_result.Replace("0-", "");
                    ob_cfinicio = (CinefansINI)JsonConvert.DeserializeObject(lc_result, (typeof(CinefansINI))); //Deserializar Json y validar respuesta

                    cinefansData.FechaCF = ob_cfinicio.VenCineFan;
                    cinefansData.FechaCL = ob_cfinicio.VenCliFrec;
                    cinefansData.FechaCB = ob_cfinicio.VenCashBack;
                    cinefansData.FechaCBDD = Convert.ToDateTime(ob_cfinicio.VenCashBack).Day.ToString();
                    cinefansData.FechaCBMM = Convert.ToDateTime(ob_cfinicio.VenCashBack).Month < 10 ? DiaMes("0" + Convert.ToDateTime(ob_cfinicio.VenCashBack).Month.ToString(), "M") : DiaMes(Convert.ToDateTime(ob_cfinicio.VenCashBack).Month.ToString(), "M");
                    //cinefansData.Saldo = ob_cfinicio.Saldo;
                    cinefansData.Nivel = ob_cfinicio.NivelCliente;
                    cinefansData.ReclsfcDD = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Day.ToString();
                    cinefansData.ReclsfcMM = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month < 10 ? DiaMes("0" + Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month.ToString(), "M") : DiaMes(Convert.ToDateTime(ob_cfinicio.Reclasificacion).Month.ToString(), "M");
                    cinefansData.ReclsfcYY = Convert.ToDateTime(ob_cfinicio.Reclasificacion).Year.ToString();
                    //cinefansData.Visitas = Convert.ToInt32(ob_cfinicio.Visitas);
                    //cinefansData.VisitasTotal = config.Value.VistasCF;
                    cinefansData.VisitasFalta = Convert.ToString(Convert.ToInt32(config.Value.VistasCF) - Convert.ToInt32(ob_cfinicio.Visitas));

                
                    foreach (var item in ob_cfinicio.Saldo)
                        cinefansData.Acumulado = String.Format("{0:C0}", Convert.ToInt32(item.Saldo));

                }
                else
                {
                    cinefansData.FechaCF = DateTime.Now.Year.ToString() + "0101";
                    cinefansData.FechaCL = DateTime.Now.Year.ToString() + "0101";
                    cinefansData.FechaCB = DateTime.Now.Year.ToString() + "0101";
                    cinefansData.FechaCBDD = "DD";
                    cinefansData.FechaCBMM = "MM";
                    //cinefansData.Saldo = "0";
                    cinefansData.Nivel = 1;
                    cinefansData.ReclsfcDD = "DD";
                    cinefansData.ReclsfcMM = "MM";
                    cinefansData.ReclsfcYY = "YYYY";
                    //cinefansData.Visitas = "0";
                    //cinefansData.VisitasTotal = config.Value.VistasCF;
                    cinefansData.VisitasFalta = config.Value.VistasCF;
                }
            }
            
            #endregion

            //Valores a mostrar
            cinefansData.Correo = Session.GetString("Usuario");
            cinefansData.Nombre = Session.GetString("Nombre");
            cinefansData.Apellido = Session.GetString("Apellido");
            cinefansData.Documento = Session.GetString("Documento");

            //cinefansData.Movimientos = ob_cfsdet.OrderByDescending(m => m.Fecha).ToList();

            //cinefansData.NombreCompleto = Session.GetString("Nombre") + " " + Session.GetString("Apellido");

            //Devolver a Vista
            return cinefansData;
        }

        /// <summary>
        /// GET: Plus -- Perfil de beneficios del club cinefans
        /// </summary>
        /// <returns></returns>
        /// 
        ///[HttpGet]
        //[Route("Plus")]
        //public ActionResult Plus()
        //{
        //    URLPortal(config);
        //    ListCarrito();

        //    //Devolver a Vista
        //    return View();
        //}

      
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
        #endregion

        #region POST
        /// <summary>
        /// POST: Registro -- Crear usuario en portal web
        /// </summary>
        /// <param name="pr_dateli">Parm Entidad usuario</param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("CrearUsuarioPortalWeb")]
        //public ActionResult Registro(Usuario pr_dateli)
        //{
        //    #region VARIABLES LOCALES
        //    int lc_auxedi = 0;
        //    int lc_auxedf = 0;
        //    string lc_srvpar = string.Empty;
        //    string lc_result = string.Empty;

        //    Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

        //    General ob_fncgrl = new General();
        //    #endregion

        //    URLPortal(config);
        //    ListCarrito();

        //    //Inicializar variables
        //    ViewBag.AlertS = false;

        //    try
        //    {
        //        //Validar clave
        //        if (pr_dateli.Password != pr_dateli.Barrio)
        //            ModelState.AddModelError("", "La contraseña y la confirmación no coinciden");

        //        //Asignar valores
        //        pr_dateli.Tipo = "";
        //        pr_dateli.SwtVenta = "";
        //        pr_dateli.KeyPelicula = "";

        //        pr_dateli.Terminos = true;
        //        pr_dateli.Politicas = true;

        //        pr_dateli.Edad = 0;
        //        pr_dateli.Sexo = "M";
        //        pr_dateli.Login = pr_dateli.Correo;
        //        pr_dateli.Barrio = "";
        //        pr_dateli.Genero = "";
        //        pr_dateli.Cinema = "";
        //        pr_dateli.Accion = "C";

        //        pr_dateli.Cinema = config.Value.ValorTercero;
        //        pr_dateli.Tercero = config.Value.ValorTercero;
        //        pr_dateli.Reservas = "";
        //        pr_dateli.Noticias = "";
        //        pr_dateli.Celular = pr_dateli.Celular;
        //        pr_dateli.Noticias = "";
        //        pr_dateli.Telefono = pr_dateli.Telefono;
        //        pr_dateli.Cartelera = "";
        //        pr_dateli.Direccion = "";
        //        pr_dateli.Municipio = "";
        //        pr_dateli.Otras_Salas = "N";
        //        pr_dateli.Fecha_Nacimiento = "20000101";

        //        pr_dateli.TelefonoEli = "";
        //        pr_dateli.ApellidoEli = "";
        //        pr_dateli.KeyTeatro = "";
        //        pr_dateli.NombreEli = "";
        //        pr_dateli.EmailEli = "";
        //        pr_dateli.KeySala = "";
        //        pr_dateli.FecProg = "";
        //        pr_dateli.HorProg = "";
        //        pr_dateli.message = "";

        //        pr_dateli.NombrePel = "";
        //        pr_dateli.NombreFec = "";
        //        pr_dateli.NombreHor = "";
        //        pr_dateli.NombreTar = "";
        //        pr_dateli.KeyTarifa = "";
        //        pr_dateli.KeySecuencia = "";

        //        //Generar y encriptar JSON para servicio
        //        lc_srvpar = ob_fncgrl.JsonConverter(pr_dateli);
        //        lc_srvpar = lc_srvpar.Replace("sexo", "Sexo");
        //        lc_srvpar = lc_srvpar.Replace("edad", "Edad");
        //        lc_srvpar = lc_srvpar.Replace("login", "Login");
        //        lc_srvpar = lc_srvpar.Replace("barrio", "Barrio");
        //        lc_srvpar = lc_srvpar.Replace("genero", "Genero");
        //        lc_srvpar = lc_srvpar.Replace("cinema", "Cinema");
        //        lc_srvpar = lc_srvpar.Replace("correo", "Correo");
        //        lc_srvpar = lc_srvpar.Replace("nombre", "Nombre");
        //        lc_srvpar = lc_srvpar.Replace("accion", "Accion");
        //        lc_srvpar = lc_srvpar.Replace("celular", "Celular");
        //        lc_srvpar = lc_srvpar.Replace("password", "Clave");
        //        lc_srvpar = lc_srvpar.Replace("telefono", "Telefono");
        //        lc_srvpar = lc_srvpar.Replace("apellido", "Apellido");
        //        lc_srvpar = lc_srvpar.Replace("reservas", "Reservas");
        //        lc_srvpar = lc_srvpar.Replace("noticias", "Noticias");
        //        lc_srvpar = lc_srvpar.Replace("cartelera", "Cartelera");
        //        lc_srvpar = lc_srvpar.Replace("documento", "Documento");
        //        lc_srvpar = lc_srvpar.Replace("direccion", "Direccion");
        //        lc_srvpar = lc_srvpar.Replace("municipio", "Municipio");
        //        lc_srvpar = lc_srvpar.Replace("\"id\"", "\"IdMessage\"");
        //        lc_srvpar = lc_srvpar.Replace("otras_Salas", "Otras_Salas");
        //        lc_srvpar = lc_srvpar.Replace("fecha_Nacimiento", "Fecha_Nacimiento");

        //        //Encriptar Json
        //        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //        //Consumir servicio
        //        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocya/"), lc_srvpar);


        //        //Validar respuesta
        //        if (lc_result.Substring(0, 1) == "0")
        //        {
        //            //Quitar switch
        //            lc_result = lc_result.Replace("0-", "");
        //            lc_result = lc_result.Replace("[", "");
        //            lc_result = lc_result.Replace("]", "");

        //            //Deserializar Json y validar respuesta
        //            ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
        //            if (ob_diclst.ContainsKey("Validación"))
        //            {
        //                ModelState.AddModelError("", ob_diclst["Validación"].ToString());
        //            }
        //            else
        //            {
        //                if (ob_diclst["Respuesta"].ToString() == "Proceso realizado con éxito.")
        //                    ViewBag.AlertS = true;
        //                else
        //                    ModelState.AddModelError("", ob_diclst["Valor"].ToString());
        //            }
        //        }
        //        else
        //        {
        //            lc_result = lc_result.Replace("1-", "");
        //            ModelState.AddModelError("", lc_result);
        //        }

        //        //Devolver a vista
        //        return View(pr_dateli);
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        //Generar Log
        //        LogSales logSales = new LogSales();
        //        LogAudit logAudit = new LogAudit(config);
        //        logSales.Id = Guid.NewGuid().ToString();
        //        logSales.Fecha = DateTime.Now;
        //        logSales.Programa = "CineFans/Registro";
        //        logSales.Metodo = "POST";
        //        logSales.ExceptionMessage = lc_syserr.Message;
        //        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

        //        //Escribir Log
        //        logAudit.LogApp(logSales);

        //        //Devolver vista de error
        //        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
        //    }
        //}

        /// <summary>
        /// POST: UPDuser -- Crear usuario en portal web cinefans
        /// </summary>
        /// <param name="pr_dateli">Parm Entidad usuario</param>
        /// <returns></returns>
        //[HttpPost]
        //[Route("CrearUsuarioPortalWebCineFans")]
        //public ActionResult UPDuser(string Correo, string Nombre, string Apellido, string Documento, string Genero, string Municipio, string Barrio, string Celular)
        //{
        //    #region VARIABLES LOCALES
        //    string lc_srvpar = string.Empty;
        //    string lc_result = string.Empty;

        //    Dictionary<string, string> ob_diclst = new Dictionary<string, string>();

        //    Usuario pr_dateli = new Usuario();
        //    General ob_fncgrl = new General();
        //    #endregion

        //    URLPortal(config);
        //    ListCarrito();

        //    //Inicializar variables
        //    ViewBag.AlertS = false;

        //    Correo = Session.GetString("Usuario");
        //    Documento = Session.GetString("Documento");

        //    try
        //    {
        //        //Asignar valores
        //        pr_dateli.Tipo = "";
        //        pr_dateli.SwtVenta = "";
        //        pr_dateli.KeyPelicula = "";

        //        pr_dateli.Terminos = true;
        //        pr_dateli.Politicas = true;
        //        pr_dateli.Password = Session.GetString("Passwrd");

        //        pr_dateli.Edad = 1;
        //        pr_dateli.Sexo = "M";
        //        pr_dateli.Login = Correo;
        //        pr_dateli.Correo = Correo;
        //        pr_dateli.Nombre = Nombre;
        //        pr_dateli.Barrio = Barrio;
        //        pr_dateli.Genero = Genero;
        //        pr_dateli.Accion = "U";
        //        pr_dateli.Apellido = Apellido;

        //        pr_dateli.Cinema = Session.GetString("Teatro");
        //        pr_dateli.Tercero = config.Value.ValorTercero;
        //        pr_dateli.Reservas = "";
        //        pr_dateli.Noticias = "";
        //        pr_dateli.Telefono = "0";
        //        pr_dateli.Cartelera = "";
        //        pr_dateli.Direccion = "0";
        //        pr_dateli.Municipio = Municipio;
        //        pr_dateli.Documento = Documento;
        //        pr_dateli.Celular = Celular;
        //        pr_dateli.Otras_Salas = "N";
        //        pr_dateli.Fecha_Nacimiento = "20000101";

        //        pr_dateli.TelefonoEli = "";
        //        pr_dateli.ApellidoEli = "";
        //        pr_dateli.KeyTeatro = "";
        //        pr_dateli.NombreEli = "";
        //        pr_dateli.EmailEli = "";
        //        pr_dateli.KeySala = "";
        //        pr_dateli.FecProg = "";
        //        pr_dateli.HorProg = "";
        //        pr_dateli.message = "";

        //        pr_dateli.NombrePel = "";
        //        pr_dateli.NombreFec = "";
        //        pr_dateli.NombreHor = "";
        //        pr_dateli.NombreTar = "";
        //        pr_dateli.KeyTarifa = "";
        //        pr_dateli.KeySecuencia = "";

        //        //Generar y encriptar JSON para servicio
        //        lc_srvpar = ob_fncgrl.JsonConverter(pr_dateli);
        //        lc_srvpar = lc_srvpar.Replace("sexo", "Sexo");
        //        lc_srvpar = lc_srvpar.Replace("edad", "Edad");
        //        lc_srvpar = lc_srvpar.Replace("login", "Login");
        //        lc_srvpar = lc_srvpar.Replace("barrio", "Barrio");
        //        lc_srvpar = lc_srvpar.Replace("genero", "Genero");
        //        lc_srvpar = lc_srvpar.Replace("cinema", "Cinema");
        //        lc_srvpar = lc_srvpar.Replace("correo", "Correo");
        //        lc_srvpar = lc_srvpar.Replace("nombre", "Nombre");
        //        lc_srvpar = lc_srvpar.Replace("accion", "Accion");
        //        lc_srvpar = lc_srvpar.Replace("celular", "Celular");
        //        lc_srvpar = lc_srvpar.Replace("password", "Clave");
        //        lc_srvpar = lc_srvpar.Replace("telefono", "Telefono");
        //        lc_srvpar = lc_srvpar.Replace("apellido", "Apellido");
        //        lc_srvpar = lc_srvpar.Replace("reservas", "Reservas");
        //        lc_srvpar = lc_srvpar.Replace("noticias", "Noticias");
        //        lc_srvpar = lc_srvpar.Replace("cartelera", "Cartelera");
        //        lc_srvpar = lc_srvpar.Replace("documento", "Documento");
        //        lc_srvpar = lc_srvpar.Replace("direccion", "Direccion");
        //        lc_srvpar = lc_srvpar.Replace("municipio", "Municipio");
        //        lc_srvpar = lc_srvpar.Replace("\"id\"", "\"IdMessage\"");
        //        lc_srvpar = lc_srvpar.Replace("otras_Salas", "Otras_Salas");
        //        lc_srvpar = lc_srvpar.Replace("fecha_Nacimiento", "Fecha_Nacimiento");

        //        //Encriptar Json
        //        lc_srvpar = ob_fncgrl.EncryptStringAES(lc_srvpar);

        //        //Consumir servicio
        //        lc_result = ob_fncgrl.WebServices(string.Concat(config.Value.ScoreServices, "scocya/"), lc_srvpar);

        //        //Generar Log
        //        LogSales logSales = new LogSales();
        //        LogAudit logAudit = new LogAudit(config);
        //        logSales.Id = Guid.NewGuid().ToString();
        //        logSales.Fecha = DateTime.Now;
        //        logSales.Programa = "CineFans/UPDuser";
        //        logSales.Metodo = "SCOCYA";
        //        logSales.ExceptionMessage = lc_srvpar;
        //        logSales.InnerExceptionMessage = lc_result;

        //        //Escribir Log
        //        //logAudit.LogApp(logSales);

        //        //Validar respuesta
        //        if (lc_result.Substring(0, 1) == "0")
        //        {
        //            //Quitar switch
        //            lc_result = lc_result.Replace("0-", "");
        //            lc_result = lc_result.Replace("[", "");
        //            lc_result = lc_result.Replace("]", "");

        //            //Deserializar Json y validar respuesta
        //            ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
        //            if (ob_diclst.ContainsKey("Validación"))
        //            {
        //                return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Validación"].ToString() });
        //            }
        //            else
        //            {
        //                if (ob_diclst["Respuesta"].ToString() == "Proceso realizado con éxito.")
        //                    return RedirectToAction("CineFans", "CineFans");
        //                else
        //                    return RedirectToAction("Error", "Pages", new { pr_message = ob_diclst["Valor"].ToString() });
        //            }
        //        }
        //        else
        //        {
        //            lc_result = lc_result.Replace("1-", "");
        //            return RedirectToAction("Error", "Pages", new { pr_message = lc_result });
        //        }
        //    }
        //    catch (Exception lc_syserr)
        //    {
        //        //Generar Log
        //        LogSales logSales = new LogSales();
        //        LogAudit logAudit = new LogAudit(config);
        //        logSales.Id = Guid.NewGuid().ToString();
        //        logSales.Fecha = DateTime.Now;
        //        logSales.Programa = "CineFans/UPDuser";
        //        logSales.Metodo = "POST";
        //        logSales.ExceptionMessage = lc_syserr.Message;
        //        logSales.InnerExceptionMessage = logSales.ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

        //        //Escribir Log
        //        logAudit.LogApp(logSales);

        //        //Devolver vista de error
        //        return RedirectToAction("Error", "Pages", new { pr_message = config.Value.MessageException + logSales.Id });
        //    }
        //}
        #endregion

        #region MÉTODOS DE CLASE
        ///// <summary>
        ///// Método para cargar URL de Header y Footer del portal
        ///// </summary>
        ///// <returns></returns>
        ///// 
        //[HttpGet]
        //[Route("CargarURLPortal")]
        //private void URLPortal(IOptions<MyConfig> config)
        //{
        //    //Cargar ciudades home y teatro por defecto si aplica
        //    if (Session.GetString("Teatro") != null)
        //    {
        //        Ciuteatros("SEL");
        //    }
        //    else
        //    {
        //        if (Session.GetString("CiudadTeatro") != null)
        //            Ciuteatros(Session.GetString("CiudadTeatro"));
        //        else
        //            Ciuteatros();
        //    }

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
        ///// 
        //[HttpGet]
        //[Route("ListarCarrito")]
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

        //    //if (Session.GetString("Secuencia") != null)
        //    //{
        //    //Obtener productos carrito de compra
        //    lc_secsec = Convert.ToDecimal(Session.GetString("Secuencia"));
        //    using (var context = new DataDB(config))
        //    {
        //        //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
        //        decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
        //        decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
        //        var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
        //        ViewBag.ListCarritoR = RetailSales;
        //    }

        //    //Obtener boletas carrito de compra
        //    using (var context = new DataDB(config))
        //    {
        //        //Select * From ReportSales Where Secuencia == ob_datpro.KeySecuencia
        //        string PuntoVenta = config.Value.PuntoVenta;
        //        string KeyTeatro = Session.GetString("Teatro");
        //        var ReportSales = context.ReportSales.Where(x => x.Secuencia == lc_secsec.ToString()).Where(x => x.KeyPunto == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
        //        ViewBag.ListCarritoB = ReportSales;
        //    }

        //    if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count == 0)
        //        ViewBag.TipoV = "B";
        //    if (ViewBag.ListCarritoB.Count == 0 && ViewBag.ListCarritoR.Count != 0)
        //        ViewBag.TipoV = "P";
        //    if (ViewBag.ListCarritoB.Count != 0 && ViewBag.ListCarritoR.Count != 0)
        //        ViewBag.TipoV = "M";
        //    //}
        //}

        ///// <summary>
        ///// Método para obtener dia de la semana
        ///// </summary>
        ///// <param name="pr_daynum">id del día</param>
        ///// <returns></returns>
        ///// 
        //[HttpGet]
        //[Route("ObtenerDiaDeLaSemana")]
        //private string DiaMes(string pr_daynum, string pr_flag)
        //{
        //    #region VARIABLES LOCALES
        //    string lc_daystr = string.Empty;
        //    #endregion

        //    if (pr_flag == "D")
        //    {
        //        //Selección de día.
        //        switch (pr_daynum)
        //        {
        //            case "Sunday":
        //                lc_daystr = "DOM";
        //                break;
        //            case "Monday":
        //                lc_daystr = "LUN";
        //                break;
        //            case "Tuesday":
        //                lc_daystr = "MAR";
        //                break;
        //            case "Wednesday":
        //                lc_daystr = "MIE";
        //                break;
        //            case "Thursday":
        //                lc_daystr = "JUE";
        //                break;
        //            case "Friday":
        //                lc_daystr = "VIE";
        //                break;
        //            case "Saturday":
        //                lc_daystr = "SAB";
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        //Selección de día.
        //        switch (pr_daynum)
        //        {
        //            case "01":
        //                lc_daystr = "ENERO";
        //                break;
        //            case "02":
        //                lc_daystr = "FEBRERO";
        //                break;
        //            case "03":
        //                lc_daystr = "MARZO";
        //                break;
        //            case "04":
        //                lc_daystr = "ABRIL";
        //                break;
        //            case "05":
        //                lc_daystr = "MAYO";
        //                break;
        //            case "06":
        //                lc_daystr = "JUNIO";
        //                break;
        //            case "07":
        //                lc_daystr = "JULIO";
        //                break;
        //            case "08":
        //                lc_daystr = "AGOSTO";
        //                break;
        //            case "09":
        //                lc_daystr = "SEPTIEMBRE";
        //                break;
        //            case "10":
        //                lc_daystr = "OCTUBRE";
        //                break;
        //            case "11":
        //                lc_daystr = "NOVIEMBRE";
        //                break;
        //            case "12":
        //                lc_daystr = "DICIEMBRE";
        //                break;
        //        }
        //    }

        //    //Devovler Valores
        //    return lc_daystr;
        //}

        ///// <summary>
        ///// Método para cargar ciudades y teatros
        ///// </summary>
        ///// <param name="pr_flag">Parámetro de ciudad</param>
        ///// 
        //[HttpGet]
        //[Route("ObtenerListadoCiudadesyTeatros")]
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
        ///// 
        //[HttpGet]
        //[Route("SeleccionarTeatro")]
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
        //                decimal PuntoVenta = Convert.ToDecimal(config.Value.PuntoVenta);
        //                decimal KeyTeatro = Convert.ToDecimal(Session.GetString("Teatro"));
        //                var RetailSales = context.RetailSales.Where(x => x.Secuencia == lc_secsec1).Where(x => x.PuntoVenta == PuntoVenta).Where(x => x.KeyTeatro == KeyTeatro).ToList();
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
        //            }

        //            //Borrar boletas
        //            string lc_secsec2 = Session.GetString("Secuencia");
        //            using (var context = new DataDB(config))
        //            {
        //                //Select * From RetailSales Where Secuencia == ob_datpro.KeySecuencia
        //                string PuntoVenta = config.Value.PuntoVenta;
        //                string KeyTeatro = Session.GetString("Teatro");
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

        //    //Cargar ciudad seleccionada
        //    Ciuteatros(pr_ciudad);

        //    //Cargar Teatro
        //    Session.SetString("Teatro", pr_teatro);
        //    Session.SetString("TeatroNombre", pr_nomteatro);
        //    ViewBag.NombreCiudadTeatro = pr_nomteatro;
        //}
        #endregion
    }
}
