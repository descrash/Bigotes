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

        /// <summary>
        /// Lista de roles administradores que se iniciarán al inicio del bot para recoger
        /// aquellos roles con permisos de ADMIN
        /// </summary>
        public static List<DiscordRole> rolesAdministradores;

        /// <summary>
        /// Lista de roles asignables por los usuarios.
        /// </summary>
        public static List<DiscordRole> rolesAsignables = new List<DiscordRole>();
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

        #region Creación de botones
        /// <summary>
        /// Método para la creación de lista de botones.
        /// Los emojis son opcionales, pero los puede crear desde los nombres.
        /// Todos los botones estarán HABILITADOS
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="msgTxt"></param>
        /// <param name="opcionesTXT"></param>
        /// <param name="estilos"></param>
        /// <param name="emojiNames">Opcional</param>
        /// <returns></returns>
        public static DiscordMessageBuilder crearMensajeDeBotones(CommandContext ctx, string msgTxt, string[] opcionesIDS, string[] opcionesTXT, DSharpPlus.ButtonStyle[] estilos, string[] emojiNames = null)
        {
            DiscordMessageBuilder builder;
            DiscordButtonComponent[] botones = new DiscordButtonComponent[opcionesIDS.Length];

            try
            {
                if (emojiNames == null)
                {
                    emojiNames = new string[opcionesIDS.Length];
                }

                if (estilos == null)
                {
                    estilos = new DSharpPlus.ButtonStyle[opcionesIDS.Length];
                }

                for (int i=0; i < opcionesIDS.Length; i++)
                {
                    botones[i] = crearBoton(ctx, opcionesIDS[i], opcionesTXT[i], estilos[i] == 0 ? estilos[i] = DSharpPlus.ButtonStyle.Primary : estilos[i], false, emojiNames[i]);
                }

                builder = new DiscordMessageBuilder()
                    .WithContent(msgTxt)
                    .AddComponents(botones);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return builder;
        }

        /// <summary>
        /// Método para crear un solo botón.
        /// Probablemente éste sea mucho más flexible que utilizar uno conjunto.
        /// NOTA: EL ID SERÁ EL TEXTO DESCRIPTIVO SUSTITUYENDO LOS ESPACIOS POR BARRAS BAJAS
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="opcion"></param>
        /// <param name="estilo"></param>
        /// <param name="disabled"></param>
        /// <param name="emojiName"></param>
        /// <returns></returns>
        public static DiscordButtonComponent crearBoton(CommandContext ctx, string customid, string opcion, DSharpPlus.ButtonStyle estilo, bool disabled = false, string emojiName = null)
        {
            DiscordButtonComponent button;

            try
            {
                if (string.IsNullOrEmpty(emojiName))
                {
                    button = new DiscordButtonComponent(estilo, customid, opcion);
                }
                else
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, emojiName);
                    button = new DiscordButtonComponent(estilo, customid, opcion, disabled, new DiscordComponentEmoji(emoji));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return button;
        }
        #endregion
        #endregion
    }
}
