/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Teatros.cs                                                               *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad Teatros y ciudades del portal web                                *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/

using System.Collections.Generic;
using System.Xml.Serialization;

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de teatros y cuidades
    /// </summary>
    public class teatro
    {
        public string id { get; set; }
        public string nombre { get; set; }
        public string ciudad { get; set; }
        public string Habilitado { get; set; }
    }
}
