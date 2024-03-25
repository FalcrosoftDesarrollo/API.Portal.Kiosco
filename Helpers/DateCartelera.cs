using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Collections.Generic;
using System.Xml.Linq;
using System;

namespace APIPortalKiosco.Helpers
{
    public class Helper
    {

        /// <summary>
        /// Método para obtener dia de la semana
        /// </summary>
        /// <param name="pr_daynum">id del día</param>
        /// <returns></returns>
        public string DiaMes(string pr_daynum, string pr_flag)
        {
            #region VARIABLES LOCALES
            string lc_daystr = string.Empty;
            #endregion

            if (pr_flag == "D")
            {
                //Selección de día.
                switch (pr_daynum)
                {
                    case "Sunday":
                        lc_daystr = "DOM";
                        break;
                    case "Monday":
                        lc_daystr = "LUN";
                        break;
                    case "Tuesday":
                        lc_daystr = "MAR";
                        break;
                    case "Wednesday":
                        lc_daystr = "MIE";
                        break;
                    case "Thursday":
                        lc_daystr = "JUE";
                        break;
                    case "Friday":
                        lc_daystr = "VIE";
                        break;
                    case "Saturday":
                        lc_daystr = "SAB";
                        break;
                }
            }
            else
            {
                //Selección de día.
                switch (pr_daynum)
                {
                    case "01":
                        lc_daystr = "ENERO";
                        break;
                    case "02":
                        lc_daystr = "FEBRERO";
                        break;
                    case "03":
                        lc_daystr = "MARZO";
                        break;
                    case "04":
                        lc_daystr = "ABRIL";
                        break;
                    case "05":
                        lc_daystr = "MAYO";
                        break;
                    case "06":
                        lc_daystr = "JUNIO";
                        break;
                    case "07":
                        lc_daystr = "JULIO";
                        break;
                    case "08":
                        lc_daystr = "AGOSTO";
                        break;
                    case "09":
                        lc_daystr = "SEPTIEMBRE";
                        break;
                    case "10":
                        lc_daystr = "OCTUBRE";
                        break;
                    case "11":
                        lc_daystr = "NOVIEMBRE";
                        break;
                    case "12":
                        lc_daystr = "DICIEMBRE";
                        break;
                }
            }

            //Devovler Valores
            return lc_daystr;
        }

        /// <summary>
        /// Método para obtener el tipo de sala desde el nombre de la película
        /// </summary>
        /// <param name="pr_nompel">Llave para obtener nombre</param>
        /// <returns></returns>
        public string TipPelicula(string pr_nompel)
        {
            #region VARIABLES LOCALES
            string lc_return = string.Empty;
            #endregion

            //Validar tipo sala GEN
            if (pr_nompel.Contains(" GEN "))
            {
                lc_return = "GENERAL";
                return lc_return;
            }

            //Validar tipo sala SNV
            if (pr_nompel.Contains(" SNV "))
            {
                lc_return = "SUPERNOVA";
                return lc_return;
            }

            //Validar tipo sala SK
            if (pr_nompel.Contains(" SK "))
            {
                lc_return = "STAR KIDS";
                return lc_return;
            }

            //Validar tipo sala 4DX
            if (pr_nompel.Contains(" 4DX "))
            {
                lc_return = "4DX";
                return lc_return;
            }

            //Validar tipo sala CA
            if (pr_nompel.Contains(" CA "))
            {
                lc_return = "CINE ARTE";
                return lc_return;
            }

            //Validar tipo sala AUT
            if (pr_nompel.Contains(" AUT "))
            {
                lc_return = "AUTOCINE";
                return lc_return;
            }

            //Validar tipo sala BS
            if (pr_nompel.Contains(" BS "))
            {
                lc_return = "BACK STAR";
                return lc_return;
            }

            //Devolver valor vacio
            return lc_return;
        }
    }
}
