/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : InternetSales.cs                                                         *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad Ventas por internet del portal web                               *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de ventas por internet
    /// </summary>
    public class InternetSales
    {
        public int Sala { get; set; }
        public int Teatro { get; set; }
        public int Tercero { get; set; }
        public int Factura { get; set; }
        public int Funcion { get; set; }
        public int Pelicula { get; set; }
        public int InicioFun { get; set; }
        public int PuntoVenta { get; set; }
        public int AudiPrev { get; set; }
        public int TipoBono { get; set; }
        public int CodMedioPago { get; set; }
        public int ClienteFrecuente { get; set; }

        public long Telefono { get; set; }
        public long DocIdentidad { get; set; }

        public double TotalVenta { get; set; }
        public double PagoInterno { get; set; }
        public double PagoCredito { get; set; }
        public double PagoEfectivo { get; set; }

        public string Obs1 { get; set; }
        public string Obs2 { get; set; }
        public string Obs3 { get; set; }
        public string Obs4 { get; set; }
        public string Placa { get; set; }
        public string Accion { get; set; }
        public string Nombre { get; set; }
        public string FechaFun { get; set; }
        public string Apellido { get; set; }
        public string Cortesia { get; set; }
        public string Direccion { get; set; }
        public string TipoEntrega { get; set; }
        public string CorreoCliente { get; set; }

        public List<Productos> Productos { get; set; }
        public List<Ubicaciones> Ubicaciones { get; set; }
    }
}
