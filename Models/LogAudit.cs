/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : LogAudit.cs                                                              *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 05/09/2023                                                               *
*   Descripción: Clase para manejo de logs de auditoria del sistema                       *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 05/09/2023                                       * 
*   Detalle Cambio: Refactorizacion código -> (Antoine Román - Falcrosoft) 02/01/2024     *
******************************************************************************************/
using APIPortalKiosco.Data;
using Microsoft.Extensions.Options;

namespace APIPortalKiosco.Models
{
    public class LogAudit
    {
        private readonly IOptions<MyConfig> config;

        public LogAudit(IOptions<MyConfig> config)
        {
            this.config = config;
        }

        /// <summary>
        /// Método para albergar log de transacciones
        /// </summary>
        /// <param name="logSales"></param>
        /// 
        public void LogApp(LogSales logSales)
        {
            var file = "LogPortal_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
            var path = Path.GetFullPath(file);

            try
            {
                logSales.ExceptionMessage ??= "null";
                logSales.InnerExceptionMessage ??= "null";

                using (var context = new DataDB(config))
                {
                    context.LogSales.Add(logSales);
                    context.SaveChanges();
                }

                //var logEntry = $"{logSales.Id} | {logSales.Fecha} | {logSales.Programa} | {logSales.Metodo} | {logSales.ExceptionMessage} | {logSales.InnerExceptionMessage}";

                //if (!File.Exists(path))
                //{
                //    using (StreamWriter sw = File.CreateText(path))
                //    {
                //        sw.WriteLine(logEntry);
                //    }
                //}
                //else
                //{
                //    using (StreamWriter sr = File.AppendText(path))
                //    {
                //        sr.WriteLine(logEntry);
                //    }
                //}
            }
            catch (Exception lc_syserr)
            {
                var ExceptionMessage = lc_syserr.Message;
                var InnerExceptionMessage = ExceptionMessage.Contains("Inner") ? lc_syserr.InnerException.Message : "null";

                var logEntry = $"{logSales.Id} | {logSales.Fecha} | {logSales.Programa} | {logSales.Metodo} | {logSales.ExceptionMessage} | {logSales.InnerExceptionMessage}";

                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine(logEntry);
                        sw.WriteLine($"{logSales.Id} | {logSales.Fecha} | {logSales.Programa} | {logSales.Metodo} Exeption unsave BD | {ExceptionMessage} | {InnerExceptionMessage}");
                    }
                }
                else
                {
                    using (StreamWriter sr = File.AppendText(path))
                    {
                        sr.WriteLine(logEntry);
                        sr.WriteLine($"{logSales.Id} | {logSales.Fecha} | {logSales.Programa} | {logSales.Metodo} Exeption unsave BD | {ExceptionMessage} | {InnerExceptionMessage}");
                    }
                }
            }
        }
   }
}