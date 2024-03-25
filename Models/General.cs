/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : General.cs                                                               *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase para manejo de procesos generales del sistema                      *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System;
using QRCoder;
using System.IO;
using Nancy.Json;
using System.Net;
using System.Text;
using System.Drawing;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;


namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Clase para manejo de procesos generales
    /// </summary>
    public class General
    {
        private readonly IOptions<MyConfig> config;

        #region MÉTODOS CLASE
        /// <summary>
        /// Encriptar cadena string en arreglo de bytes
        /// </summary>
        /// <param name="plainText">Parámetro de texto a encriptar</param>
        /// <param name="key">Parámetro de llave de algoritmo</param>
        /// <param name="iv">Parámetro de llave de algoritmo</param>
        /// <returns></returns>
        private byte[] EncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            #region VARIABLES LOCALES
            byte[] encrypted;
            #endregion

            //Validar argumentos
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");

            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");

            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("key");


            //Crear objeto RijndaelManaged con llaves específicas KEY y IV
            using (var rijAlg = new RijndaelManaged())
            {
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                //Crear un descifrador para realizar la transformación de flujo
                var encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                //Crear las transmisiones utilizadas para el cifrado
                using (var msEncrypt = new MemoryStream())
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                        swEncrypt.Write(plainText); //Escribir todos los datos en la transmisión

                    encrypted = msEncrypt.ToArray();
                }
            }

            //Devuelve los bytes cifrados
            return encrypted;
        }

        /// <summary>
        /// Desencriptar cadena string en arreglo de bytes
        /// </summary>
        /// <param name="cipherText">Parámetro de texto a desencriptar</param>
        /// <param name="key">Parámetro de llave de algoritmo</param>
        /// <param name="iv">Parámetro de llave de algoritmo</param>
        /// <returns></returns>
        private string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            #region VARIABLES LOCALES
            string plaintext = null;
            #endregion

            //Validar argumentos
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");

            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");

            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException("key");

            //Crear objeto RijndaelManaged con llaves específicas KEY y IV
            using (var rijAlg = new RijndaelManaged())
            {
                //Opciones
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                //Crear un descifrador para realizar la transformación de flujo
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    //Cree las transmisiones utilizadas para el descifrado
                    using (var msDecrypt = new MemoryStream(cipherText))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                        plaintext = srDecrypt.ReadToEnd(); // Lee los bytes descifrados del flujo de descifrado y los ubica en la cadena string
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            //Devuleve cadena string
            return plaintext;
        }

        /// <summary>
        /// Desencriptar cadena algoritmo AES
        /// </summary>
        /// <param name="cipherText">Parámetro cadena a encriptar</param>
        /// <returns></returns>
        public string DecryptStringAES(string cipherText)
        {
            //Llave de algoritmo
            var keybytes = Encoding.UTF8.GetBytes("tHIrd!sc0R3Is.00");
            var iv = Encoding.UTF8.GetBytes("tHIrd!sc0R3Is.00");

            //Clave del cifrado y desencriptar
            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromJavascript = DecryptStringFromBytes(encrypted, keybytes, iv);

            //Devolver cadena descifrada
            return Convert.ToString(decriptedFromJavascript);
        }

        /// <summary>
        /// Encriptar cadena algoritmo AES
        /// </summary>
        /// <param name="plaintext">Parámetro cadena a encriptar</param>
        /// <returns></returns>
        public string EncryptStringAES(string plaintext)
        {
            //Llave de algoritmo
            var keybytes = Encoding.UTF8.GetBytes("tHIrd!sc0R3Is.00");
            var iv = Encoding.UTF8.GetBytes("tHIrd!sc0R3Is.00");

            //Clave del cifrado y encriptar
            byte[] encryptedBytes = EncryptStringToBytes(plaintext, keybytes, iv);
            string b64ciphertext = Convert.ToBase64String(encryptedBytes);

            //Devolver cadena cifrada
            return b64ciphertext;
        }

        /// <summary>
        /// Método para seleccionar consulta 
        /// </summary>
        /// <param name="pr_selsql">Id de consulta a tratar</param>
        /// <returns></returns>
        public string SqlQuery(int pr_selsql)
        {
            #region VARIABLES LOCALES
            string lc_sqlstring = string.Empty;
            #endregion

            //Selección de consulta
            switch (pr_selsql)
            {
                case 1:
                    break;
            }

            //Devolver datos
            return lc_sqlstring;
        }

        /// <summary>
        /// Método para seleccionar proc. almacenado 
        /// </summary>
        /// <param name="pr_selsql">Id de SP a tratar</param>
        /// <returns></returns>
        public string SqlStoreProc(int pr_selsql)
        {
            #region VARIABLES LOCALES
            string lc_sqlstring = string.Empty;
            #endregion

            //Selección de SP
            switch (pr_selsql)
            {
                case 1:
                    break;
            }

            //Devolver datos
            return lc_sqlstring;
        }

        /// <summary>
        /// Método para convertir Entidad en Json
        /// </summary>
        /// <param name="pr_objson">Parm Entidad a tratar</param>
        /// <returns></returns>
        public string JsonConverter(object pr_objson)
        {
            #region VARIABLES LOCALES
            string lc_jsonch = string.Empty;
            #endregion

            //Inicializar y convertir entidad a Json
            var jsonStringName = new JavaScriptSerializer();
            lc_jsonch = jsonStringName.Serialize(pr_objson);

            //Devolver valores
            return lc_jsonch;
        }

        /// <summary>
        /// Método para consumir servicios de SCORE Dictionaty <string, string>
        /// </summary>
        /// <param name="pr_rutsrv">Parm Ruta de servicio</param>
        /// <param name="pr_objson">Parm objeto Json</param>
        public string WebServices(string pr_rutsrv, string pr_objson)
        {
            #region VARIABLES LOCALES
            string lc_jsonst = string.Empty;
            string lc_result = string.Empty;

            Dictionary<string, string> ob_diclst;
            #endregion

            var lc_strurl = string.Concat($"", pr_rutsrv, "");
            lc_jsonst = string.Concat($"\"", pr_objson, "\"");

            //Inicializar consumo POST
            HttpWebRequest ob_request = (HttpWebRequest)WebRequest.Create(lc_strurl);
            ob_request.Method = "POST";
            ob_request.ContentType = "application/json";
            ob_request.ContentLength = lc_jsonst.Length;
            ob_request.ServicePoint.Expect100Continue = false;
            ob_request.ServicePoint.UseNagleAlgorithm = false;

            //Ejecutar consumo POST
            StreamWriter requestWriter = new StreamWriter(ob_request.GetRequestStream(), System.Text.Encoding.ASCII);
            requestWriter.Write(lc_jsonst);
            requestWriter.Close();

            try
            {
                //Obtener respuesta 
                WebResponse ob_response = ob_request.GetResponse();
                Stream ob_stream = ob_response.GetResponseStream();
                StreamReader ob_reader = new StreamReader(ob_stream);

                //Leer respuesta POST
                lc_result = ob_reader.ReadToEnd();
                lc_result = lc_result.Replace("[", "");
                lc_result = lc_result.Replace("]", "");
                ob_reader.Close();

                //Leer fichero JSON, recorrer diccionario y desencriptar respuesta
                ob_diclst = (Dictionary<string, string>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, string>)));
                foreach (var lc_diclst in ob_diclst)
                    lc_result = "0-" + DecryptStringAES(lc_diclst.Value);
            }
            catch (Exception lc_syserr)
            {
                lc_result = "1-" + lc_syserr.Message;
            }

            //Devolver valores
            return lc_result;
        }

        /// <summary>
        /// Método para consumir servicios de SCORE Dictionary <string, List<object>>
        /// </summary>
        /// <param name="pr_rutsrv">Parm Ruta de servicio</param>
        /// <param name="pr_objson">Parm objeto Json</param>
        public Dictionary<string, List<object>> WebServices2(string pr_rutsrv, string pr_objson)
        {
            #region VARIABLES LOCALES
            string lc_jsonst = string.Empty;
            string lc_result = string.Empty;

            Dictionary<string, List<object>> ob_diclst = new Dictionary<string, List<object>>();
            #endregion

            var lc_strurl = string.Concat($"", pr_rutsrv, "");
            lc_jsonst = string.Concat($"\"", pr_objson, "\"");

            //Inicializar consumo POST
            HttpWebRequest ob_request = (HttpWebRequest)WebRequest.Create(lc_strurl);
            ob_request.Method = "POST";
            ob_request.ContentType = "application/json";
            ob_request.ContentLength = lc_jsonst.Length;

            //Ejecutar consumo POST
            StreamWriter requestWriter = new StreamWriter(ob_request.GetRequestStream(), System.Text.Encoding.ASCII);
            requestWriter.Write(lc_jsonst);
            requestWriter.Close();

            try
            {
                //Obtener respuesta 
                WebResponse ob_response = ob_request.GetResponse();
                Stream ob_stream = ob_response.GetResponseStream();
                StreamReader ob_reader = new StreamReader(ob_stream);

                //Leer respuesta POST
                lc_result = ob_reader.ReadToEnd();
                lc_result = lc_result.Replace("[", "");
                lc_result = lc_result.Replace("]", "");
                ob_reader.Close();

                //Leer fichero JSON, recorrer diccionario y desencriptar respuesta
                ob_diclst = (Dictionary<string, List<object>>)JsonConvert.DeserializeObject(lc_result, (typeof(Dictionary<string, List<object>>)));
                //foreach (var lc_diclst in ob_diclst)
                //    lc_result = "0-" + DecryptStringAES(lc_diclst.Value);
            }
            catch (Exception lc_syserr)
            {
                lc_result = "1-" + lc_syserr.Message;
            }

            //Devolver valores
            return ob_diclst;
        }

        /// <summary>
        /// Método para convertir fecha a formmato YYYYMMDD
        /// </summary>
        /// <param name="pr_auxfec">Parm fecha a tratar</param>
        /// <returns></returns>
        public string ConvertDate(DateTime pr_auxfec)
        {
            #region VARIABLES LOCALES
            string lc_auxano = string.Empty;
            string lc_auxmes = string.Empty;
            string lc_auxdia = string.Empty;
            #endregion

            //Obtener año, mes, dia
            lc_auxano = pr_auxfec.Year.ToString();
            lc_auxdia = pr_auxfec.Day.ToString().Length < 2 ? "0" + pr_auxfec.Day.ToString() : pr_auxfec.Day.ToString();
            lc_auxmes = pr_auxfec.Month.ToString().Length < 2 ? "0" + pr_auxfec.Month.ToString() : pr_auxfec.Month.ToString();

            //Devolver fecha
            return string.Concat(lc_auxano, lc_auxmes, lc_auxdia);
        }

        public byte[] GenerarQR(string textoQR)
        {
            // Codigo para poder generar el QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQR, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Codigo para poder convertir el Bitmap del QR a byte[]
            ImageConverter converter = new ImageConverter();
            byte[] QRbyte = (byte[])converter.ConvertTo(qrCodeImage, typeof(byte[]));

            return QRbyte;
        }

        /// <summary>
        /// Método para obtener ciudades y teatros por ciudad
        /// </summary>
        /// <param name="pr_flag">flag</param>
        /// <returns></returns>
        public List<teatro> Ciudades(string pr_url)
        {
            #region VARIABLES LOCALES
            XmlDocument ob_xmldoc = new XmlDocument();
            List<teatro> ob_ciuteatros = new List<teatro>();
            #endregion

            //Obtener información de la web
            ob_xmldoc.Load(pr_url);
            XmlNodeList teatros = ob_xmldoc.GetElementsByTagName("teatros");

            //Recorrer XML
            foreach (XmlElement item in teatros)
            {
                //Datos de nodo teatros/teatro
                XmlNodeList teatro = item.GetElementsByTagName("teatro");
                foreach (XmlElement item2 in teatro)
                {
                    //Obtener valores
                    XmlNodeList nodelist = item2.ChildNodes;
                    
                    teatro ob_ciutea = new teatro();
                    ob_ciutea.id = nodelist[0].InnerText;
                    ob_ciutea.nombre = nodelist[1].InnerText;
                    ob_ciutea.ciudad = nodelist[2].InnerText;
                    ob_ciutea.Habilitado = nodelist[3].InnerText;

                    ob_ciuteatros.Add(ob_ciutea);
                }
            }

            //devolver listado
            return ob_ciuteatros;
        }
        #endregion
    }

    /// <summary>
    /// Clase/Entidad para manejo de sentencias SQL
    /// </summary>
    public class SQLExec
    {
        [DataMember]
        public string sp_name { get; set; }

        [DataMember]
        public List<MySqlParameter> sp_parm { get; set; }

        //public DbQuery<T> MySuperQuery { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para retornar mensajes de error
    /// </summary>
    public class OUTMessage
    {
        public int id { get; set; }
        public string message { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para manejo de datos para proceso de boletas
    /// </summary>
    public class PARMBoletas : OUTMessage
    {
        public string KeySala { get; set; }
        public string FecProg { get; set; }
        public string HorProg { get; set; }
        public string SwtVenta { get; set; }
        public string EmailEli { get; set; }
        public string NombrePel { get; set; }
        public string NombreFec { get; set; }
        public string NombreHor { get; set; }
        public string NombreTar { get; set; }
        public string KeyTeatro { get; set; }
        public string KeyTarifa { get; set; }
        public string NombreEli { get; set; }
        public string KeyPelicula { get; set; }
        public string ApellidoEli { get; set; }
        public string TelefonoEli { get; set; }
        public string KeySecuencia { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para manejo de datos para proceso de boletas
    /// </summary>
    public class PARMConfites
    {
        public string TipoCompra { get; set; }
        public string SwtVenta { get; set; }
        public string EmailEli { get; set; }
        public string KeyTeatro { get; set; }
        public string DesTeatro { get; set; }
        public string NombreEli { get; set; }
        public string ApellidoEli { get; set; }
        public string TelefonoEli { get; set; }
        public string KeySecuencia { get; set; }

        public List<Producto> LisProducto { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para manejo de datos de carrito de compras
    /// </summary>
    public class OrderItem
    {
        public int Cantidad { get; set; }
        public int KeyProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para manejo de datos para secuencia de venta
    /// </summary>
    public class Secuencia : OUTMessage
    {
        public int Punto { get; set; }
        public int Teatro { get; set; }
        public string Tercero { get; set; }
    }

    /// <summary>
    /// Clase/Entidad pra manejo de datos de cartelera web
    /// </summary>
    public class Cartelera 
    {
        public string Teatro { get; set; }
        public string tercero { get; set; }
        public string IdPelicula { get; set; }
        public string FcPelicula { get; set; }
        public string TpPelicula { get; set; }
        public string FgPelicula { get; set; }
        public string CfPelicula { get; set; }
    }

    /// <summary>
    /// Entidad para manejo de datos reporte de vetas e integración paymentez
    /// </summary>
    public class Paymentez : PARMBoletas
    {
        public string url_api { get; set; }
        public string client_app_code { get; set; }
        public string client_app_key { get; set; }
        public string env_mode { get; set; }
        public string locale { get; set; }
        public string user_id { get; set; }
        public string user_email { get; set; }
        public string user_phone { get; set; }
        public string order_description { get; set; }
        public string order_amount { get; set; }
        public string order_taxable_amount { get; set; }
        public string order_tax_percentage { get; set; }
        public string order_vat { get; set; }
        public string order_reference { get; set; }

        public string response { get; set; }

        public int Id { get; set; }
        public string Tipo { get; set; }
        public string SelUbicaciones { get; set; }
        public DateTime FechaCreado { get; set; }
        public DateTime FechaModificado { get; set; }
    }

    /// <summary>
    /// Entidad para manejo de datos servicios Epayco
    /// </summary>
    public class EpaycoApi
    {
        public bool sucess { get; set; }
        public string titleResponse { get; set; }
        public string textResponse { get; set; }
        public string lastAction { get; set; }

        public List<EpaycoApiData> data { get; set; }
    }

    /// <summary>
    /// Entidad para manejo de datos servicios Epayco
    /// </summary>
    public class EpaycoApiGet
    {
        public bool sucess { get; set; }
        public string titleResponse { get; set; }
        public string textResponse { get; set; }
        public string lastAction { get; set; }

        public EpaycoApiData data { get; set; }
    }

    /// <summary>
    /// Entidad para manejo de datos detalle servicio Epayco
    /// </summary>
    public class EpaycoApiData
    {
        #region VARIABLES POST
        public int tax { get; set; }
        public int amount { get; set; }
        public int dollars { get; set; }
        public int amountNet { get; set; }
        public int taxBaseClient { get; set; }
        public int referencePayco { get; set; }

        public string ip { get; set; }
        public string test { get; set; }
        public string city { get; set; }
        public string bill { get; set; }
        public string bank { get; set; }
        public string email { get; set; }
        public string status { get; set; }
        public string company { get; set; }
        public string address { get; set; }
        public string country { get; set; }
        public string receipt { get; set; }
        public string currency { get; set; }
        public string lastName { get; set; }
        public string response { get; set; }
        public string document { get; set; }
        public string firstName { get; set; }
        public string telephone { get; set; }
        public string department { get; set; }
        public string numberCard { get; set; }
        public string ipoconsumo { get; set; }
        public string urlResponse { get; set; }
        public string mobilePhone { get; set; }
        public string description { get; set; }
        public string authorization { get; set; }
        public string paymentMethod { get; set; }
        public string urlConfirmation { get; set; }
        public string transactionDate { get; set; }
        #endregion

        #region VARIABLES GET
        public double x_tax { get; set; }
        public double x_amount { get; set; }
        public double x_tax_ico { get; set; }
        public double x_amount_ok { get; set; }
        public double x_ref_payco { get; set; }
        public double x_amount_base { get; set; }
        public double x_cod_response { get; set; }
        public double x_cod_respuesta { get; set; }
        public double x_amount_country { get; set; }
        public double x_cust_id_cliente { get; set; }
        public double x_cod_transaction_state { get; set; }

        public string x_extra1 { get; set; }
        public string x_extra2 { get; set; }
        public string x_extra3 { get; set; }
        public string x_extra4 { get; set; }
        public string x_extra5 { get; set; }
        public string x_extra6 { get; set; }
        public string x_extra7 { get; set; }
        public string x_extra8 { get; set; }
        public string x_extra9 { get; set; }
        public string x_extra10 { get; set; }
        public string x_quotas { get; set; }
        public string x_business { get; set; }
        public string x_response { get; set; }
        public string x_respuesta { get; set; }
        public string x_errorcode { get; set; }
        public string x_franchise { get; set; }
        public string x_signature { get; set; }
        public string x_bank_name { get; set; }
        public string x_cardnumber { get; set; }
        public string x_id_factura { get; set; }
        public string x_id_invoice { get; set; }
        public string x_description { get; set; }
        public string x_customer_ip { get; set; }
        public string x_test_request { get; set; }
        public string x_customer_city { get; set; }
        public string x_currency_code { get; set; }
        public string x_approval_code { get; set; }
        public string x_customer_name { get; set; }
        public string x_transaction_id { get; set; }
        public string x_customer_email { get; set; }
        public string x_customer_phone { get; set; }
        public string x_customer_movil { get; set; }
        public string x_customer_doctype { get; set; }
        public string x_customer_address { get; set; }
        public string x_transaction_date { get; set; }
        public string x_customer_country { get; set; }
        public string x_customer_document { get; set; }
        public string x_fecha_transaccion { get; set; }
        public string x_transaction_state { get; set; }
        public string x_customer_lastname { get; set; }
        public string x_customer_ind_pais { get; set; }
        public string x_response_reason_text { get; set; }
        #endregion

        #region LISTADO DE OBJETOS
        public List<object> log { get; set; }
        public List<object> allLogs { get; set; }
        public List<object> movements { get; set; }
        public List<object> extras { get; set; }
        #endregion
    }

    /// <summary>
    /// Entidad para manejo de datos de reporte de ventas e integracion operador de pagos
    /// </summary>
    public class Payment : PaymentType
    {
        public int Id { get; set; }

        public string Tipo { get; set; }
        public string response { get; set; }
        public string SelProductos { get; set; }
        public string SelUbicaciones { get; set; }

        public DateTime FechaCreado { get; set; }
        public DateTime FechaModificado { get; set; }

        public string KeySala { get; set; }
        public string FecProg { get; set; }
        public string HorProg { get; set; }
        public string SwtVenta { get; set; }
        public string EmailEli { get; set; }
        public string NombrePel { get; set; }
        public string NombreFec { get; set; }
        public string NombreHor { get; set; }
        public string NombreTar { get; set; }
        public string KeyTeatro { get; set; }
        public string DesTeatro { get; set; }
        public string KeyTarifa { get; set; }
        public string NombreEli { get; set; }
        public string TipoCompra { get; set; }
        public string KeyPelicula { get; set; }
        public string ApellidoEli { get; set; }
        public string TelefonoEli { get; set; }
        public string KeySecuencia { get; set; }

        public List<Producto> LisProducto { get; set; }
    }

    /// <summary>
    /// Entidad para manejo de datos operadores de pago
    /// </summary>
    public class PaymentType : OUTMessage
    {
        //Paymentez
        public string url_api { get; set; }
        public string client_app_code { get; set; }
        public string client_app_key { get; set; }
        public string env_mode { get; set; }
        public string locale { get; set; }
        public string user_id { get; set; }
        public string user_email { get; set; }
        public string user_phone { get; set; }
        public string order_description { get; set; }
        public string order_amount { get; set; }
        public string order_taxable_amount { get; set; }
        public string order_tax_percentage { get; set; }
        public string order_vat { get; set; }
        public string order_reference { get; set; }

        //Epayco
        public string src_epayco { get; set; }
        public string class_epayco { get; set; }
        public string data_epayco_key { get; set; }
        public string data_epayco_amount { get; set; }
        public string data_epayco_tax_base { get; set; }
        public string data_epayco_tax { get; set; }
        public string data_epayco_tax_ico { get; set; }
        public string data_epayco_name { get; set; }
        public string data_epayco_description { get; set; }
        public string data_epayco_currency { get; set; }
        public string data_epayco_country { get; set; }
        public string data_epayco_test { get; set; }
        public string data_epayco_external { get; set; }
        public string data_epayco_extra1 { get; set; }
        public string data_epayco_extra2 { get; set; }
        public string data_epayco_extra3 { get; set; }
        public string data_epayco_response { get; set; }
        public string data_epayco_confirmation { get; set; }
    }

    /// <summary>
    /// Clase/Entidad para valores de configuración personalizada
    /// </summary>
    public class MyConfig
    {
        public string URLfb { get; set; }
        public string URLig { get; set; }
        public string URLtw { get; set; }
        public string URLyb { get; set; }
        public string URLtk { get; set; }
        public string URLfaqs { get; set; }
        public string URLblog { get; set; }
        public string URLtarifas { get; set; }
        public string URLprocinal { get; set; }
        public string URLcontacto { get; set; }
        public string URLtermycond { get; set; }
        public string URLpoliticas { get; set; }
        public string URLservicios { get; set; }
        public string URLprotocolos { get; set; }
        public string URLexperiencias { get; set; }
        public string URLsobreprocinal { get; set; }
        public string URLpromociones { get; set; }
        public string URLeticaytra { get; set; }
        public string URLlaft { get; set; }
        public string URLresoluccn { get; set; }
        public string URLcinefans { get; set; }

        public string UrlCClave { get; set; }
        public string UrlCorreo { get; set; }
        public string UrlCRegistro { get; set; }
        public string PuntoVenta { get; set; }
        public string PortalWebDB { get; set; }
        public string CarteleraWP { get; set; }
        public string Ciudades41 { get; set; }
        public string Variables41 { get; set; }
        public string UrlRetailImg { get; set; }
        public string ValorTercero { get; set; }
        public string ScoreServices { get; set; }

        public string Variables41T { get; set; }
        public string Variables41TP { get; set; }
        public string Variables41TPF { get; set; }

        public string NomEmpresa { get; set; }
        public string DirEmpresa { get; set; }
        public string CiuEmpresa { get; set; }
        public string TelEmpresa { get; set; }
        public string CorEmpresa { get; set; }
        public string TeaDefault { get; set; }
        public string NomDefault { get; set; }
        public string CiuDefault { get; set; }

        public string VistasCF { get; set; }
        public string MinDifHora { get; set; }
        public string MinDifConf { get; set; }
        public string PwdLogin2 { get; set; }
        public string UsuLogin2 { get; set; }
        public string ComprarBTN { get; set; }
        public string CodMedioPago { get; set; }
        public string CantProductos { get; set; }
        public string CantSillasBol { get; set; }
        public string CodMedioPagoCB { get; set; }
        public string MessageException { get; set; }


        public string url_apify { get; set; }
        public string col_clint { get; set; }
        public string col_login { get; set; }
        public string col_paswd { get; set; }
        public string pro_clint { get; set; }
        public string pro_login { get; set; }
        public string pro_paswd { get; set; }
        public string src_epayco { get; set; }
        public string class_epayco { get; set; }
        public string data_epayco_key_col { get; set; }
        public string data_epayco_key_pro { get; set; }
        public string data_epayco_currency { get; set; }
        public string data_epayco_country { get; set; }
        public string data_epayco_test { get; set; }
        public string data_epayco_external { get; set; }
        public string data_epayco_secure { get; set; }
        public string data_epayco_response { get; set; }
        public string data_epayco_confirmation { get; set; }
    }

    /// <summary>
    /// Clase para manejo de variables de sesion
    /// </summary>
    public static class SessionExtensions
    {
        /// <summary>
        /// Método para cargar variable de sessión
        /// </summary>
        /// <param name="session">Parm sesion activa</param>
        /// <param name="key">Parm llave de objeto</param>
        /// <param name="value">Parm objeto</param>
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        /// <summary>
        /// Método para devolver variable de sesión
        /// </summary>
        /// <typeparam name="T">Parm objeto</typeparam>
        /// <param name="session">Parm sesion activa</param>
        /// <param name="key">Parm llave de objeto</param>
        /// <returns></returns>
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}
