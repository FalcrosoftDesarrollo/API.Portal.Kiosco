/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : EntityDB.cs                                                              *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase para manejo de entidades de BD                                     *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/
using System;


namespace APIPortalKiosco.Data
{
    public class EntityDB
    {
    }

    public class ReportSales
    {
        public int Id { get; set; }
        public double Precio { get; set; }
        public string Secuencia { get; set; }
        public string KeySala { get; set; }
        public string KeyTeatro { get; set; }
        public string KeyPelicula { get; set; }
        public string SelUbicaciones { get; set; }
        public string HorProg { get; set; }
        public string FecProg { get; set; }
        public string EmailEli { get; set; }
        public string NombreEli { get; set; }
        public string ApellidoEli { get; set; }
        public string TelefonoEli { get; set; }
        public string NombrePel { get; set; }
        public string NombreFec { get; set; }
        public string NombreHor { get; set; }
        public string NombreTar { get; set; }
        public string KeyTarifa { get; set; }
        public string Transaccion { get; set; }
        public string Referencia { get; set; }
        public DateTime FechaCreado { get; set; }
        public DateTime FechaModificado { get; set; }
        public string KeyPunto { get; set; }
    }

    public class RetailSales
    {
        public int Id { get; set; }
        public string Tipo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Secuencia { get; set; }
        public decimal PuntoVenta { get; set; }
        public decimal KeyProducto { get; set; }
        public decimal ProProducto1 { get; set; }
        public decimal CanProducto1 { get; set; }
        public decimal ProProducto2 { get; set; }
        public decimal CanProducto2 { get; set; }
        public decimal ProProducto3 { get; set; }
        public decimal CanProducto3 { get; set; }
        public decimal ProProducto4 { get; set; }
        public decimal CanProducto4 { get; set; }
        public decimal ProProducto5 { get; set; }
        public decimal CanProducto5 { get; set; }
        public decimal ProCategoria1 { get; set; }
        public decimal CanCategoria1 { get; set; }
        public decimal ProCategoria2 { get; set; }
        public decimal CanCategoria2 { get; set; }
        public decimal ProCategoria3 { get; set; }
        public decimal CanCategoria3 { get; set; }
        public decimal ProCategoria4 { get; set; }
        public decimal CanCategoria4 { get; set; }
        public decimal ProCategoria5 { get; set; }
        public decimal CanCategoria5 { get; set; }
        public DateTime FechaRegistro { get; set; }

        public decimal KeyTeatro { get; set; }
        public string SwitchAdd { get; set; }
    }

    public class RetailDet
    {
        public int Id { get; set; }
        public int IdRetailSales { get; set; }
        public decimal Secuencia { get; set; }
        public decimal ProCategoria { get; set; }
        public string ProItem { get; set; }
    }

    public class TransactionSales
    {
        public int Id { get; set; }
        public decimal Secuencia { get; set; }
        public decimal PuntoVenta { get; set; }
        public decimal Teatro { get; set; }
        public string EmailEli { get; set; }
        public string NombreEli { get; set; }
        public string DocumentoEli { get; set; }
        public string TelefonoEli { get; set; }
        public string EstadoTx { get; set; }
        public DateTime FechaTx { get; set; }
        public decimal ValorTx { get; set; }
        public decimal BaseTx { get; set; }
        public decimal IvaTx { get; set; }
        public decimal IcoTx { get; set; }
        public string AutorizacionTx { get; set; }
        public string ReferenciaTx { get; set; }
        public string ReferenciaEx { get; set; }
        public string BancoTx { get; set; }
        public string Observaciones { get; set; }
        public DateTime FechaCreado { get; set; }
        public DateTime FechaModificado { get; set; }
    }

    public class LogSales
    {
        public string Id { get; set;}
        public DateTime Fecha { get; set; }
        public string Programa { get; set; }
        public string Metodo { get; set; }
        public string ExceptionMessage { get; set; }
        public string InnerExceptionMessage { get; set; }
    }          
}
