/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Cinefans.cs                                                              *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad cinefans  del portal web                                         *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using Microsoft.AspNetCore.SignalR.Protocol;
using System;
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad Cinefans de datos generales
    /// </summary>
    public class Cinefans
    {
        public decimal puntos_acumulados { get; set; }
        public decimal puntos_redimidos { get; set; }
        public decimal puntos_vencidos { get; set; }
        public decimal puntos_disponibles { get; set; }
    }

    /// <summary>
    /// Entidad Cinefans de detalle de transacciones
    /// </summary>
    public class CinefansDET
    {
        public string Fecha { get; set; }
        public string Cinema { get; set; }
        public string TipoTransaccion { get; set; }
        public decimal Saldo { get; set; }
        public decimal Total { get; set; }
        public decimal PagoInterno { get; set; }
        public decimal Otros { get; set; }
        public string Descripcion { get; set; }
        public decimal Punto { get; set; }
        public decimal Transaccion { get; set; }
    }

    public class CinefansINI
    {
        public List<object> Historia { get; set; }
        public List<object> Puntos { get; set; }
        public List<CinefansDET> Saldo { get; set; }
        public string CineFan { get; set; }
        public string VenCineFan { get; set; }
        public string VenCliFrec { get; set; }
        public string Reclasificacion { get; set; }
        public string VenCashBack { get; set; }
        public int NivelCliente { get; set; }
        public decimal Visitas { get; set; }
    }

    public class CinefansSRV : OUTMessage
    {
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string tercero { get; set; }

        public string Fecha1 { get; set; }
        public string Fecha2 { get; set; }
    }
}
