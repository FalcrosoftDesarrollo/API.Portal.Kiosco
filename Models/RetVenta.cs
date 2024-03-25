/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : RetVenta.cs                                                              *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad Retail del portal web                                            *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System.Collections.Generic;

namespace APIPortalKiosco.Models
{
    public class RetVenta
    {
    }

    /// <summary>
    /// Entidad para datos de productos para adiciones retail SCOINT
    /// </summary>
    public class Adiciones
    {
        public decimal Secuencia { get; set; }
        public decimal Codigo_1 { get; set; }
        public decimal Codigo_2 { get; set; }
        public decimal Codigo_3 { get; set; }
        public decimal Codigo_4 { get; set; }
        public decimal Codigo_5 { get; set; }
        public decimal Codigo_6 { get; set; }

        public decimal Precio_1 { get; set; }
        public decimal Precio_2 { get; set; }
        public decimal Precio_3 { get; set; }
        public decimal Precio_4 { get; set; }
        public decimal Precio_5 { get; set; }
        public decimal Precio_6 { get; set; }

        public decimal Cantidad_1 { get; set; }
        public decimal Cantidad_2 { get; set; }
        public decimal Cantidad_3 { get; set; }
        public decimal Cantidad_4 { get; set; }
        public decimal Cantidad_5 { get; set; }
        public decimal Cantidad_6 { get; set; }

        public string Tipo { get; set; }
        public string SwitchVenta { get; set; }
        public string Descripcion_1 { get; set; }
        public string Descripcion_2 { get; set; }
        public string Descripcion_3 { get; set; }
        public string Descripcion_4 { get; set; }
        public string Descripcion_5 { get; set; }
        public string Descripcion_6 { get; set; }

    }

    /// <summary>
    /// Entidad para datos de productos para venta retail SCOINT
    /// </summary>
    public class Productos
    {
        public decimal Codigo { get; set; }
        public decimal Precio { get; set; }

        public string Tipo { get; set; }
        public string Descripcion { get; set; }

        public List<Receta> Receta { get; set; }
    }

    /// <summary>
    /// Entidad para datos de productos de Retail
    /// </summary>
    public class Producto : PARMConfites
    {
        public string SwitchCashback { get; set; }

        public decimal ProProducto_1 { get; set; }
        public decimal ProProducto_2 { get; set; }
        public decimal ProProducto_3 { get; set; }
        public decimal ProProducto_4 { get; set; }
        public decimal ProProducto_5 { get; set; }

        public decimal ProCantidad_1 { get; set; }
        public decimal ProCantidad_2 { get; set; }
        public decimal ProCantidad_3 { get; set; }
        public decimal ProCantidad_4 { get; set; }
        public decimal ProCantidad_5 { get; set; }

        public decimal ProCategoria_1 { get; set; }
        public decimal ProCategoria_2 { get; set; }
        public decimal ProCategoria_3 { get; set; }
        public decimal ProCategoria_4 { get; set; }
        public decimal ProCategoria_5 { get; set; }

        public decimal CanCategoria_1 { get; set; }
        public decimal CanCategoria_2 { get; set; }
        public decimal CanCategoria_3 { get; set; }
        public decimal CanCategoria_4 { get; set; }
        public decimal CanCategoria_5 { get; set; }

        public decimal Cantidad1 { get; set; }
        public decimal Cantidad11 { get; set; }
        public decimal Cantidad111 { get; set; }
        public decimal Cantidad1111 { get; set; }

        public decimal Cantidad2 { get; set; }
        public decimal Cantidad22 { get; set; }
        public decimal Cantidad222 { get; set; }
        public decimal Cantidad2222 { get; set; }

        public decimal Cantidad3 { get; set; }
        public decimal Cantidad33 { get; set; }
        public decimal Cantidad333 { get; set; }
        public decimal Cantidad3333 { get; set; }

        public decimal Cantidad4 { get; set; }
        public decimal Cantidad44 { get; set; }
        public decimal Cantidad444 { get; set; }
        public decimal Cantidad4444 { get; set; }

        public decimal Cantidad5 { get; set; }
        public decimal Cantidad55 { get; set; }
        public decimal Cantidad555 { get; set; }
        public decimal Cantidad5555 { get; set; }

        public string Check1 { get; set; }
        public string Check11 { get; set; }
        public string Check111 { get; set; }
        public string Check1111 { get; set; }


        public string Check2 { get; set; }
        public string Check22 { get; set; }
        public string Check222 { get; set; }
        public string Check2222 { get; set; }

        public string Check3 { get; set; }
        public string Check33 { get; set; }
        public string Check333 { get; set; }
        public string Check3333 { get; set; }

        public string Check4 { get; set; }
        public string Check44 { get; set; }
        public string Check444 { get; set; }
        public string Check4444 { get; set; }

        public string Check5 { get; set; }
        public string Check55 { get; set; }
        public string Check555 { get; set; }
        public string Check5555 { get; set; }

        public decimal Codigo { get; set; }
        public decimal Cantidad { get; set; }

        public string Tipo { get; set; }
        public string Check { get; set; }
        public string Valor { get; set; }
        public string ValorCashback { get; set; }
        public string Descripcion { get; set; }

        public string RecetaResumen { get; set; }

        public string Censura { get; set; }
        public decimal OrdenView { get; set; }
        public string Descripcion_Web { get; set; }
        public string Flag { get; set; }
        public string SwitchAdd { get; set; }
        public string Frecuente { get; set; }

        public List<Receta> Receta { get; set; }
        public List<Precios> Precios { get; set; }
        public List<Pantallas> Pantallas { get; set; }
    }

    /// <summary>
    /// Entidad para datos de recetas de productos de retail
    /// </summary>
    public class Receta
    {
        public decimal Codigo { get; set; }
        public decimal Cantidad { get; set; }

        public string Tipo { get; set; }
        public string Descripcion { get; set; }

        public List<Precios> Precios { get; set; }
        public List<Producto> RecetaCategoria { get; set; }
    }

    /// <summary>
    /// Entidad para datos de precios de productos de retail
    /// </summary>
    public class Precios
    {
        public decimal General { get; set; }
        public decimal OtroPago { get; set; }
        public decimal HorasDias { get; set; }
        public decimal PagoInterno { get; set; }
        public decimal PrecioAtencion { get; set; }
    }

    /// <summary>
    /// Entidad para datos de pantallas de productos de retail
    /// </summary>
    public class Pantallas
    {
        public decimal Pantalla { get; set; }
        public string Descripcion { get; set; }
        public decimal Orden { get; set; }
        public string Descripcion_Web { get; set; }
        public string Flag { get; set; }
    }
}
