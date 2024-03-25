/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Json.cs                                                                  *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad cartelera del portal web                                         *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System;
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de película cartelera Json
    /// </summary>
    public class Peliculas
    {
        public string? UltimaActualizacion { get; set; }
        public IList<pelicula> pelicula { get; set; }
    }

    /// <summary>
    /// Entidad para datos por película cartelera Json
    /// </summary>
    public class pelicula
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string tipo { get; set; }
        public string sinopsis { get; set; }
        public data data { get; set; }
        public DiasDisponiblesTodosCinemas DiasDisponiblesTodosCinemas { get; set; }
        public cinemas cinemas { get; set; }
    }

    /// <summary>
    /// Entidad para datos de data cinema Json
    /// </summary>
    public class data
    {
        public string TituloOriginal { get; set; }
        public string Imagen { get; set; }
        public string Censura { get; set; }
        public string idioma { get; set; }
        public string genero { get; set; }
        public string pais { get; set; }
        public string duracion { get; set; }
        public string medio { get; set; }
        public string formato { get; set; }
        public string versión { get; set; }
        public string trailer1 { get; set; }
        public string trailer2 { get; set; }
        public string fechaEstreno { get; set; }
        public string reparto { get; set; }
        public string director { get; set; }
        public string distribuidor { get; set; }
        public string Habilitado { get; set; }
    }

    /// <summary>
    /// Entidad para datos de DiasDisponibles Tcinema Json
    /// </summary>
    public class DiasDisponiblesTodosCinemas
    {
        public IList<dia> dia { get; set; }
    }

    /// <summary>
    /// Entidad para datos de cinema cartelera Json
    /// </summary>
    public class cinemas
    {
        public cinema cinema { get; set; }
    }

    /// <summary>
    /// Entidad para datos por cinema cartelera Json
    /// </summary>
    public class cinema
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string ciudad { get; set; }
        public DiasDisponiblesCinema DiasDisponiblesCinema { get; set; }
        public salas Salas { get; set; }
    }

    /// <summary>
    /// Entidad para datos de dias Cinema Json
    /// </summary>
    public class DiasDisponiblesCinema 
    {
        public IList<dia> dia { get; set; }
    }

    /// <summary>
    /// Entidad para datos de dias en cinema cartelera Json
    /// </summary>
    public class dia
    {
        public string fecha { get; set; }
        public string univ { get; set; }
    }

    /// <summary>
    /// Entidad para datos de salas cartelera Json
    /// </summary>
    public class salas
    {
        public Dictionary<string, object> sala { get; set; }
    }

    /// <summary>
    /// Entidad para datos por salas cartelera Json
    /// </summary>
    public class sala
    {
        public string numeroSala { get; set; }
        public string tipoSala { get; set; }
        public IList<Fecha> Fecha { get; set; }
        public IList<hora> hora { get; set; }
    }

    /// <summary>
    /// Entidad para datos de fechas cartelera Json 
    /// </summary>
    public class Fecha
    {
        public string dia { get; set; }
        public string univ { get; set; }
        public IList<hora> hora { get; set; }
    }

    /// <summary>
    /// Entidad para datos de horas cartelera Json
    /// </summary>
    public class hora
    {
        public string idFuncion { get; set; }
        public string horario { get; set; }
        public string reservasONline { get; set; }
        public string ventasONline { get; set; }
        public string militar { get; set; }
        public string validaciones { get; set; }
        public string cortoNacional { get; set; }
        public string capacidad { get; set; }
        public string disponibilidad { get; set; }
        public string nombreZona { get; set; }
        public DateTime fechayhora { get; set; }
        public TipoZona TipoZona { get; set; }
        public IList<TipoZona> TipoZonaOld { get; set; }
    }

    /// <summary>
    /// Entidad para datos de tipos de zonas cartelera Json
    /// </summary>
    public class TipoZona
    {
        public string nombreZona { get; set; }
        public string idZona { get; set; }
        public IList<TipoSilla> TipoSilla { get; set; }
    }

    /// <summary>
    /// Entidad para datos de tipos de sillas cartelera Json
    /// </summary>
    public class TipoSilla
    {
        public string nombreTipoSilla { get; set; }
        public string idTipoSilla { get; set; }
        public IList<Tarifa> Tarifa { get; set; }
    }

    /// <summary>
    /// Entidad para datos de tarifas cartelera Json
    /// </summary>
    public class Tarifa
    {
        public string codigoTarifa { get; set; }
        public string nombreTarifa { get; set; }
        public string documentoEnCaja { get; set; }
        public string medioPago { get; set; }
        public string validokiosko { get; set; }
        public string validoTeceros { get; set; }
        public string validoInternet { get; set; }
        public string validoPos { get; set; }
        public string validoApp { get; set; }
        public string validoPosMovil { get; set; }
        public string clienteFrecuente { get; set; }
        public string listaEspecial { get; set; }
        public string valor { get; set; }
    }

    /// <summary>
    /// Entidad para datos de fechas en cartelera portal
    /// </summary>
    public class DateCartelera
    {
        public string FecSt { get; set; }
        public string MesLt { get; set; }
        public string DiaNb { get; set; }
        public string DiaLt { get; set; }
        public string Flags { get; set; }
        public DateTime FecDt { get; set; }
    }

    /// <summary>
    /// Entidad para datos de pelicula-fechas en cartelera compra rapida
    /// </summary>
    public class DateCompraRapida
    {
        public string Pelicula { get; set; }
        public string Fecha { get; set; }
        public string Teatro { get; set; }
        public string Referencia { get; set; }
        public string FechaRef { get; set; }
    }
}
