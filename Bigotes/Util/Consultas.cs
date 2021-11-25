using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public static string QueEs(string es)
        {
            #region Propiedades
            string answer = "`EXTRAYENDO FRAGMENTO DE LA BASE DE DATOS DE RATA SUM` ";
            string resultado = String.Empty;
            string parrafoFinal = String.Empty;
            string url = Constantes.WIKI_GW2;
            string busqueda = es.Substring(3);
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
            catch(Exception ex)
            {
                resultado = "`[ERROR: " + ex.Message.ToUpper() + "]`";
            }

            return resultado;
        }

        /// <summary>
        /// Método para obtener el canal por el nombre
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static DiscordChannel GetChannel(CommandContext ctx, string channelName)
        {
            DiscordChannel channel = null;

            List<DiscordGuild> guilds = ctx.Client.Guilds.Values.ToList<DiscordGuild>();

            foreach (var guild in guilds)
            {
                foreach (var _channel in guild.Channels.Values)
                {
                    String[] tagsID = { "#", "<", ">" };

                    //Se compara nombre si se ha puesto únicamente el nombre ("general") o el ID en caso de haber puesto el tag entero ("#general")
                    if (_channel.Name == channelName.Trim() || channel.Id.ToString() == channelName.Split(tagsID, StringSplitOptions.RemoveEmptyEntries)[0].Trim())
                    {
                        channel = _channel;
                    }
                }
            }

            return channel;
        }
    }
}
