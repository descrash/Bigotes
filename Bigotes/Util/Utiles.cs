using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bigotes.Util
{
    public static class Utiles
    {
        #region Propiedades
        /// <summary>
        /// Índice de cordialidad para el saludo
        /// </summary>
        public static int cordialidad;

        /// <summary>
        /// Canal determinado para la encuesta
        /// </summary>
        public static DiscordChannel canalEventos;

        /// <summary>
        /// Enumerado para elegir género de personaje en creación de ficha
        /// </summary>
        public enum Genero
        {
            MASCULINO,
            FEMENINO,
            NOBINARIO
        }

        /// <summary>
        /// Enumerado para elegir raza de personaje en creación de ficha
        /// </summary>
        public enum Raza
        {
            HUMANO,
            CHARR,
            NORN,
            ASURA,
            SYLVARI,
            OTROS
        }
        #endregion

        #region Métodos
        /// <summary>
        /// Método estático para ajustar la cordialidad del bot
        /// </summary>
        /// <param name="porcentaje">Porcentaje de cordialidad (positivo)</param>
        /// <returns></returns>
        public static string AjustarCordialidad(string porcentaje)
        {
            string msg = String.Empty;

            try
            {
                if (!Regex.IsMatch(porcentaje.Trim(), @"\d%"))
                {
                    msg = "`[CARGANDO MENSAJE DE ERROR]` ```Error-en-formato.``` `[CARGANDO SUGERENCIA]` ```Usar-formato-00%.```";
                }
                else
                {
                    int nivel = Int32.Parse(porcentaje.Trim().Split('%')[0]);

                    Utiles.cordialidad = nivel;

                    msg = "`[CARGANDO MENSAJE DE CONFIRMACIÓN]` ```Ajustada-cordialidad-al-" + nivel + "%.```";
                }
            }
            catch
            {
                msg = "`[CARGANDO MENSAJE DE ERROR]` ```Error-en-realización.-Recomendable-consultar-código.```";
            }

            return msg;
        }
        #endregion
    }
}
