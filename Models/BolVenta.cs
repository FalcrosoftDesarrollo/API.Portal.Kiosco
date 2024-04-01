/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : BolVenta.cs                                                              *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad Boletas del portal web                                           *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de boleta de cine
    /// </summary>
    public class BolVenta : PARMBoletas
    {
        public int Secuencia { get; set; }
        public int PuntoVenta { get; set; }
        public int IdFuncion { get; set; }
        public long Telefono { get; set; }
        public int Sala { get; set; }
        public string Tipo { get; set; }
        public string Imagen { get; set; }
        public static string Censura { get; set; }
        public string FechaPrg { get; set; }
        public string FechaDia { get; set; }
        public string Funcion { get; set; }
        public string Horario { get; set; }
        public string Tercero { get; set; }
        public string IdTarifa { get; set; }
        public string ValorTarifa { get; set; }
        public string NombreTarifa { get; set; }
        public string NombrePelicula { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public List<Ubicaciones> Ubicaciones { get; set; }

        public int FilSala { get; set; }
        public int ColSala { get; set; }
        public string TipoSilla { get; set; }
        public string SelUbicaciones { get; set; }
        public Ubicaciones[,] MapaSala { get; set; }
    }

    /// <summary>
    /// Entidad para datos de ubicaciones de cine
    /// </summary>
    public class Ubicaciones
    {
        public int Tarifa { get; set; }
        public int Columna { get; set; }
        public int ColRelativa { get; set; }
        public string Fila { get; set; }
        public string FilRelativa { get; set; }
        public string TipoSilla { get; set; }
        public string TipoZona { get; set; }
        public string EstadoSilla { get; set; }
    }

    /// <summary>
    /// Entidad para datos de cargar sala de cine
    /// </summary>
    public class MapaSala
    {
        public int Sala { get; set; }
        public int Teatro { get; set; }
        public int Funcion { get; set; }

        public string Correo { get; set; }
        public string Tercero { get; set; }
        public string FechaFuncion { get; set; }
    }

    /// <summary>
    /// Entidad para datos de carga estado de silla de sala de cine
    /// </summary>
    public class EstadoDeSilla
    {
        public string TipoSilla { get; set; }
        public string EstadoSilla { get; set; }
        public double Columna { get; set; }
    }

    /// <summary>
    /// Entidad para datos de teatro
    /// </summary>
    public class Teatro : PARMBoletas
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Ciudad { get; set; }
        public string Tipo { get; set; }
    }

    /// <summary>
    /// Entidad para liberar silla
    /// </summary>
    
}
public class LiberaSilla
{
    public string FechaFuncion { get; set; }
    public int Sala { get; set; }
    public int Funcion { get; set; }
    public string Fila { get; set; }
    public int Columna { get; set; }
    public int Usuario { get; set; }
    public int teatro { get; set; }
    public string tercero { get; set; }

}