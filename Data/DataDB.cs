/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Data.cs                                                                  *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Clase para manejo de BD y persistencia                                   *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
*   Detalle Cambio: Refactorizacion código -> (Antoine Román - Falcrosoft) 02/01/2024     *
******************************************************************************************/
using APIPortalKiosco.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Linq;

namespace APIPortalKiosco.Data
{
    /// <summary>
    /// Clase para manejo de BD que hereda de DbContext
    /// </summary>
    public class DataDB : DbContext
    {
        #region VARIABLES DE INSTANCIA
        private string it_sqlparm = string.Empty;
        private string it_message = string.Empty;

        private MySqlParameter it_mslparm { get; set; }
        #endregion

        #region ENTIDADES DB
        public DbSet<ReportSales> ReportSales { get; set; }
        public DbSet<RetailSales> RetailSales { get; set; }
        public DbSet<RetailDet> RetailDet { get; set; }
        public DbSet<TransactionSales> TransactionSales { get; set; }
        public DbSet<LogSales> LogSales { get; set; }
        #endregion

        #region CONSTRUCTOR
        /// <summary>
        /// Valor privado de parámetro de configuración
        /// </summary>
        public readonly IOptions<MyConfig> config;

        /// <summary>
        /// Constructor de la clase DataDB
        /// </summary>
        /// <param name="options">Párametro de contexto</param>
        public DataDB(IOptions<MyConfig> config)
        {
            this.config = config;
        }
        //public DataDB(DbContextOptions<DataDB> options, IOptions<MyConfig> config) : base(options)
        //{
        //    this.config = config;
        //}
        #endregion

        #region MÉTODOS DE CLASE
        /// <summary>
        /// Método para cargar cadena de conexión BD
        /// </summary>
        /// <param name="optionsBuilder">Parm valores de cadena</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(config.Value.PortalWebDB);
        }

        /// <summary>
        /// Método para crear los modelos de la BD
        /// </summary>
        /// <param name="modelBuilder">Parm entidades de BD</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReportSales>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Secuencia).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.KeySala).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.KeyTeatro).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.KeyPelicula).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.SelUbicaciones).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.HorProg).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.FecProg).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.EmailEli).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.NombreEli).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.ApellidoEli).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.TelefonoEli).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.NombrePel).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.NombreFec).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.NombreHor).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.NombreTar).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Transaccion).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.Referencia).HasMaxLength(1000).IsRequired();
                entity.Property(e => e.FechaCreado).IsRequired();
                entity.Property(e => e.FechaModificado).IsRequired();
            });
        }

        /// <summary>
        /// Genera string de los parametros para el SP
        /// </summary>
        /// <param name="pr_items">Listado de parámetros </param>
        /// <returns></returns>
        private string SqlString(List<MySqlParameter> pr_items)
        {
            #region VARIABLES LOCALES
            var lc_nameparm = string.Empty;
            #endregion

            //Inicializar variables
            it_sqlparm = string.Empty;

            //Recorrido por la lista de parámetros
            foreach (var lc_item in pr_items)
            {
                lc_nameparm = (lc_item.Direction.Equals(System.Data.ParameterDirection.Output)) ? lc_item.ParameterName + " OUT" : lc_item.ParameterName;
                it_sqlparm = (!lc_item.Equals(pr_items.Last())) ? this.it_sqlparm + " " + lc_nameparm + ", " : this.it_sqlparm + " " + lc_nameparm;
            }

            //Devolver valores
            return it_sqlparm;
        }

        public List<T> ExeQueryList<T>(SQLExec pr_sqlexec)
        {
            #region VARIABLES LOCALES
            List<T> lc_result = new List<T>();
            #endregion

            //Inicializar variables
            it_message = string.Empty;

            try
            {
                //Ejecutar sentencia SQL
                //lc_result = Database.SqlQuery<T>(pr_sqlexec.sp_name + SqlString(pr_sqlexec.sp_parm), pr_sqlexec.sp_parm.ToArray()).ToList();
            }
            catch (Exception lc_syserr)
            {
                //Genera excepción
                it_message = "Error ejecución ExeQueryList" + Environment.NewLine + Environment.NewLine + lc_syserr.Message;
                throw new ArgumentException(it_message, "ExeQueryList");
            }

            //Devolver datos
            return lc_result;
        }


        #endregion
    }
}