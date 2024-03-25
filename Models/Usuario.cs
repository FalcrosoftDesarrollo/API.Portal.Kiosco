/******************************************************************************************
*   Autor      : Daniel Páez Puentes - UNIFIC D&I GROUP                                   *
*   Módulo     : Usuario.cs                                                               *
*   Entidad    : Portal Web - Score 4.1                                                   *
*   Fecha      : 15/10/2020                                                               *
*   Descripción: Entidad Usuarios del portal web                                          *
*                                                                                         *
*   Detalle Cambios: -> Creación - DPP - 15/10/2020                                       *
******************************************************************************************/

namespace APIPortalKiosco.Models
{
    /// <summary>
    /// Entidad para datos de usuario
    /// </summary>
    public class Usuario : PARMBoletas
    {
        public int Edad { get; set; }

        public bool Terminos { get; set; }
        public bool Politicas { get; set; }

        public string Tipo { get; set; }
        public string Sexo { get; set; }
        public string Login { get; set; }
        public string Accion { get; set; }
        public string Barrio { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Genero { get; set; }
        public string Cinema { get; set; }
        public string Celular { get; set; }
        public string Tercero { get; set; }
        public string Password { get; set; }
        public string Reservas { get; set; }
        public string Noticias { get; set; }
        public string Apellido { get; set; }
        public string Cartelera { get; set; }
        public string Direccion { get; set; }
        public string Documento { get; set; }
        public string Municipio { get; set; }
        public string Telefono { get; set; }
        public string Contacto { get; set; }
        public string Otras_Salas { get; set; }
        public string Fecha_Nacimiento { get; set; }
    }

    /// <summary>
    /// Entidad para datos de login de usuario
    /// </summary>
    public class Login : PARMBoletas
    {
        public string Password { get; set; }
        public string Correo { get; set; }
        public string Usuario { get; set; }
        public string Tercero { get; set; }
        public string Tipo { get; set; }
    }

    public class UserDocumento
    {
        public string Documento { get; set; }
        public string tercero { get; set; }

    }
}
