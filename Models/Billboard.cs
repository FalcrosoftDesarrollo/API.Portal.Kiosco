/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Billboard.cs                                                             *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad cartelera del portal web                                         *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using APIPortalKiosco.Models;
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de cartelera
    /// </summary>
    public class Billboard
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Pais { get; set; }
        public string Medio { get; set; }
        public string Imagen { get; set; }
        public string Idioma { get; set; }
        public string Genero { get; set; }
        public string Auxids { get; set; }
        public string Switch { get; set; }
        public string Nombre { get; set; }
        public string Formato { get; set; }
        public string Reparto { get; set; }
        public string Version { get; set; }
        public string Censura { get; set; }
        public string Director { get; set; }
        public string Sinopsis { get; set; }
        public string Duracion { get; set; }
        public string Trailer1 { get; set; }
        public string Trailer2 { get; set; }
        public string TipoSala { get; set; }
        public string Distribuidor { get; set; }
        public string FechaEstreno { get; set; }
        public string TituloOriginal { get; set; }
        public List<Fechas> Fechafunc { get; set; }
    }

    /// <summary>
    /// Entidad tipo lista para obtener cartelera
    /// </summary>
    public class LisBillboard : OUTMessage
    {
        public List<Billboard> ListaM { get; set; }
    }

    /// <summary>
    /// Entidad tpo lista para fechas de pelicula
    /// </summary>
    public class Fechas
    {
        public string fecham { get; set; }
        public string fecunv { get; set; }
        public List<Hora> horafun { get; set; }
    }

    /// <summary>
    /// Entidad tipo lista para horario de película 
    /// </summary>
    public class Hora
    {
        public string fecunv { get; set; }
        public string idFuncion { get; set; }
        public string horario { get; set; }
    }
}
