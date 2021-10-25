using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Bigotes.Util
{
    public static class Consultas
    {
        /// <summary>
        /// Método para la consulta a la wikipedia de GW2
        /// </summary>
        /// <param name="eres">Nombre a consultar</param>
        /// <returns></returns>
        public static string QueEs(string eres)
        {
            #region Propiedades
            string answer = "`EXTRAYENDO FRAGMENTO DE LA BASE DE DATOS DE RATA SUM` ";
            string resultado = String.Empty;
            string parrafoFinal = String.Empty;
            string url = Constantes.WIKI_GW2;
            string busqueda = eres.Substring(3);
            #endregion

            try
            {
                busqueda = busqueda.Replace("?", String.Empty);
                busqueda = busqueda.Replace(' ', '_');

                using (WebClient client = new WebClient())
                {
                    resultado = client.DownloadString(url + busqueda);
                }

                String[] separadoresParrafo = { "<p>", "</p>" };

                answer += "```" + Regex.Replace(resultado.Split(separadoresParrafo, StringSplitOptions.RemoveEmptyEntries)[1], "<.*?>", String.Empty).Replace(' ', '-') + "```";
                //+ "Fuente: " + url + busqueda).ConfigureAwait(false);
            }
            catch
            {
                answer = "`ERROR` ```No-se-han-encontrado-resultados-de-la-consulta.```";
            }

            return answer;
        }

        /// <summary>
        /// Método para convertir una fecha de calendario Gregoriano a calendario Mouveliano
        /// </summary>
        /// <param name="fechaGregoriana"></param>
        public static string ConversionCalendario(DateTime fechaGregoriana)
        {
            #region Propiedades
            string resultado = String.Empty;
            string dia = String.Empty;
            string mes = String.Empty;
            string año = String.Empty;
            #endregion

            try
            {
                DateTime dia1 = new DateTime(fechaGregoriana.Year, 1, 1);
                double diasTotales = (fechaGregoriana - dia1).TotalDays + 1;

                switch (diasTotales)
                {
                    case double n when n < 91:
                        mes = "Céfiro";
                        dia = diasTotales.ToString();
                        break;

                    case double n when n > 90 && n < 181:
                        mes = "Fénix";
                        dia = (diasTotales - 90).ToString();
                        break;

                    case double n when n > 180 && n < 271:
                        mes = "Vástago";
                        dia = (diasTotales - 180).ToString();
                        break;

                    case double n when n > 270 && n < 366:
                        mes = "Coloso";
                        dia = (diasTotales - 270).ToString();
                        break;

                    default:
                        throw(new Exception());
                }

                año = (fechaGregoriana.Year - 687).ToString();

                resultado = dia + " " + mes + " de " + año;
            }
            catch
            {
                resultado = "ERROR";
            }

            return resultado;
        }
    }
}
