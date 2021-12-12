using Bigotes.Clases;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bigotes.Util;

namespace Bigotes.Commands
{
    public class CreationCommands : BaseCommandModule
    {
        /// <summary>
        /// Método para la creación de distintas utilidades
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="peticion"></param>
        /// <returns></returns>
        [Command("crear")]
        public async Task Create(CommandContext ctx, [RemainingText]string peticion)
        {
            try
            {
                var interactivity = ctx.Client.GetInteractivity();

                switch (peticion.Trim().ToUpper())
                {
                    case "ENCUESTA":
                        await ctx.Channel.SendMessageAsync("`[CREACIÓN DE ENCUESTA ESCOGIDA]` ```Título-de-encuesta-requerido.```").ConfigureAwait(false);
                        var titulo = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                        await Poll(ctx, titulo.Result.Content);
                        break;

                    case "FICHA":
                        await ctx.Channel.SendMessageAsync("`[DESCARGANDO TUTORIAL DE FICHA]` ```De-acuerdo. Procediendo-a-comenzar-ficha-de-personaje-paso-por-paso...```").ConfigureAwait(false);
                        await Ficha(ctx);
                        break;

                    case "EVENTO":
                        string ans = "SÍ";
                        if (Utiles.canalEventos != null)
                        {
                            await ctx.Channel.SendMessageAsync("`[OPCIÓN ESCOGIDA]` ```Canal-de-eventos-escogido: " + Utiles.canalEventos.Name + ". ¿Desea-cambiar-de-canal?```").ConfigureAwait(false);
                            //TODO: Botones
                        }
                        else if (Utiles.canalEventos == null || ans == "SÍ")
                        {
                            await ctx.Channel.SendMessageAsync("```Indicar-canal-de-evento: ```").ConfigureAwait(false);
                            var channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);

                            ulong channelID = 0;
                            while(!ulong.TryParse(channelMSG.Result.Content.Remove(0, 2).Replace('>', ' ').Trim(), out channelID) || channelID == 0)
                            {
                                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Recomendable-citar-canal-en-mensaje-para-facilitar-procesamiento. Inténtelo-de-nuevo:```").ConfigureAwait(false);
                                channelMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                            }

                            Utiles.canalEventos = ctx.Guild.GetChannel(channelID);
                        }

                        await Evento(ctx, Utiles.canalEventos);

                        break;
                }
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Mensaje-de-error: " + ex.Message.Replace(' ', '-') + " ```").ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Método para la realización de encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="titulo"></param>
        /// <returns></returns>
        public async Task Poll(CommandContext ctx, string titulo)
        {
            #region ATRIBUTOS ENCUESTA
            TimeSpan duration = new TimeSpan();
            DiscordMember author = ctx.Member;
            DiscordChannel embedChannel = ctx.Channel;
            string description = String.Empty;
            string numOptions = "0";
            string urlEncuesta = String.Empty;
            List<DiscordEmoji> emojiOptions = new List<DiscordEmoji>();
            #endregion

            await ctx.Channel.SendMessageAsync("`[PROCESANDO PETICIÓN]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            #region Recogida y gestión de la duración de la encuesta
            var durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
            string durationSTR = durationMSG.Result.Content.Trim(), unidadRespuesta;
            int durationINT = 0;

            while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
                durationSTR = durationMSG.Result.Content.Trim();
            }

            if (durationSTR.Contains("m"))
            {
                durationINT = Int32.Parse(durationSTR.Split('m')[0]);
                duration = new TimeSpan(0, durationINT, 0);
                unidadRespuesta = durationINT == 1 ? "minuto" : "minutos";
            }
            else if (durationSTR.Contains("h"))
            {
                durationINT = Int32.Parse(durationSTR.Split('h')[0]);
                duration = new TimeSpan(durationINT, 0, 0);
                unidadRespuesta = durationINT == 1 ? "hora" : "horas";
            }
            else
            {
                durationINT = Int32.Parse(durationSTR.Split('d')[0]);
                duration = new TimeSpan(durationINT, 0, 0, 0);
                unidadRespuesta = durationINT == 1 ? "día" : "días";
            }
            #endregion

            #region Recogida y gestión del canal en el que realizar la encuesta
            await ctx.Channel.SendMessageAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Detallar-nombre-de-canal-de-encuesta.-Libertad-de-tomar-canal-actual-en-caso-de-no-existir-petición.```").ConfigureAwait(false);

            string channelName = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;

            embedChannel = Util.Consultas.GetChannel(ctx, channelName);
            #endregion

            #region Recogida y gestión de la descripción
            await ctx.Channel.SendMessageAsync("`[BUSCANDO CANAL]` ```Canal-de encuesta-seleccionado:-" + embedChannel.Name + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

            description = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Registro de número de opciones
            await ctx.Channel.SendMessageAsync("`[RECOGIENDO DESCRIPCIÓN]` ```¿Número-de-opciones?```");

            numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            while (!Regex.IsMatch(numOptions, @"\d"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-de-numero-entero-positivo.```").ConfigureAwait(false);
                numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            }
            #endregion

            #region Obtención de emojis para las opciones (reacciones)
            var emojiMSG = await ctx.Channel.SendMessageAsync("`[NÚMERO AJUSTADO]` ```Necesarias-reacciones-a-este-mensaje-con-los-emojis-utilizados-en-cada-opción.```");

            while (emojiOptions.Count < Int32.Parse(numOptions))
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == emojiMSG && x.User == author).ConfigureAwait(false);

                emojiOptions.Add(reactionResult.Result.Emoji);

                await ctx.Channel.SendMessageAsync("```Procesada-opción-" + emojiOptions.Count + ":" + reactionResult.Result.Emoji.Name + "```").ConfigureAwait(false);
            }
            #endregion

            #region Creación de encuesta
            await ctx.Channel.SendMessageAsync("`[OPCIONES RECOGIDAS]` ```Se-han-recogido-las-opciones.-Procesando-encuesta...```");

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = titulo,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = author.DisplayName,
                    IconUrl = author.AvatarUrl
                },
                Description = description,
                Color = DiscordColor.Blue
            };

            var pollMessage = await embedChannel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            var msgFinal = "`ENCUESTA " + titulo + " FINALIZADA` ```Resultados:```";

            await pollMessage.RespondAsync(msgFinal + string.Join("\n", results));

            //await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
            #endregion
        }

        /// <summary>
        /// Método para la realización de una ficha de personaje
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public async Task Ficha(CommandContext ctx)
        {
            try
            {
                Ficha nuevaFicha = new Ficha();
                DiscordEmoji emoji = null;
                var interactivity = ctx.Client.GetInteractivity();
                DiscordColor color = DiscordColor.Brown;

                #region DATOS BÁSICOS
                #region Nombre y apellidos
                await ctx.Channel.SendMessageAsync("`[OPCIÓN ESCOGIDA]` ```Ficha-en-blanco-preparada. Comenzando-por-DATOS-BÁSICOS. 1: Nombre.```").ConfigureAwait(false);
                nuevaFicha.nombre_completo = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;

                await ctx.Channel.SendMessageAsync("`[NOMBRE GUARDADO]` ```2: Apellidos.```").ConfigureAwait(false);
                nuevaFicha.apellidos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Género
                var male_signEmoji = DiscordEmoji.FromName(ctx.Client, ":male_sign:");
                var female_signEmoji = DiscordEmoji.FromName(ctx.Client, ":female_sign:");
                var transgender_symbolEmoji = DiscordEmoji.FromName(ctx.Client, ":transgender_symbol:");

                var generoMSG = await ctx.Channel.SendMessageAsync("`[APELLIDOS GUARDADOS]` ```3: Reaccionar-a-este-mensaje-con-icono-de-género:``` Masculino :male_sign:\nFemenino :female_sign:\nNo binario :transgender_symbol:").ConfigureAwait(false);

                await generoMSG.CreateReactionAsync(male_signEmoji).ConfigureAwait(false);
                await generoMSG.CreateReactionAsync(female_signEmoji).ConfigureAwait(false);
                await generoMSG.CreateReactionAsync(transgender_symbolEmoji).ConfigureAwait(false);

                while (emoji == null || !(new[] { male_signEmoji, female_signEmoji, transgender_symbolEmoji }.Contains(emoji)))
                {
                    if (emoji != null) await ctx.Channel.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-tres-iconos:``` :male_sign: :female_sign: :transgender_symbol:").ConfigureAwait(false);
                    emoji = (await interactivity.WaitForReactionAsync(x => x.Message == generoMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
                }
                
                if (emoji == male_signEmoji)
                {
                    nuevaFicha.genero = Util.Utiles.Genero.MASCULINO;
                }
                else if (emoji == female_signEmoji)
                {
                    nuevaFicha.genero = Util.Utiles.Genero.FEMENINO;
                }
                else if (emoji == transgender_symbolEmoji)
                {
                    nuevaFicha.genero = Util.Utiles.Genero.NOBINARIO;
                }

                emoji = null;
                #endregion

                #region Edad
                await ctx.Channel.SendMessageAsync("`[GÉNERO GUARDADO]` ```4: Año-de-nacimiento.```").ConfigureAwait(false);
                int anio;
                while (!Int32.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out anio))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                }
                nuevaFicha.bird_year = anio;
                #endregion

                #region Raza
                var humanoEmoji = DiscordEmoji.FromName(ctx.Client, ":humano:");
                var charrEmoji = DiscordEmoji.FromName(ctx.Client, ":charr:");
                var nornEmoji = DiscordEmoji.FromName(ctx.Client, ":norn:");
                var asuraEmoji = DiscordEmoji.FromName(ctx.Client, ":asura:");
                var sylvariEmoji = DiscordEmoji.FromName(ctx.Client, ":sylvari:");
                var mundoEmoji = DiscordEmoji.FromName(ctx.Client, ":mundo:");

                var raceMSG = await ctx.Channel.SendMessageAsync("`[APELLIDOS GUARDADOS]` ```5: Reaccionar-a-este-mensaje-con-icono-de-raza: ```Humano " + humanoEmoji + "\nCharr " + charrEmoji + "\nNorn " + nornEmoji + "\nAsura " + asuraEmoji + "\nSylvari " + sylvariEmoji + "\nOtros " + mundoEmoji).ConfigureAwait(false);

                await raceMSG.CreateReactionAsync(humanoEmoji).ConfigureAwait(false);
                await raceMSG.CreateReactionAsync(charrEmoji).ConfigureAwait(false);
                await raceMSG.CreateReactionAsync(nornEmoji).ConfigureAwait(false);
                await raceMSG.CreateReactionAsync(asuraEmoji).ConfigureAwait(false);
                await raceMSG.CreateReactionAsync(sylvariEmoji).ConfigureAwait(false);
                await raceMSG.CreateReactionAsync(mundoEmoji).ConfigureAwait(false);

                while (emoji == null || !(new[] { humanoEmoji, charrEmoji, nornEmoji, asuraEmoji, sylvariEmoji, mundoEmoji }.Contains(emoji)))
                {
                    if (emoji != null) await ctx.Channel.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-iconos-nombrados.```").ConfigureAwait(false);
                    emoji = (await interactivity.WaitForReactionAsync(x => x.Message == raceMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
                }

                if (emoji == humanoEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.HUMANO;
                    color = DiscordColor.Gold;
                }
                else if (emoji == charrEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.CHARR;
                    color = DiscordColor.DarkRed;
                }
                else if (emoji == nornEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.NORN;
                    color = DiscordColor.Azure;
                }
                else if (emoji == asuraEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.ASURA;
                    color = DiscordColor.Purple;
                }
                else if (emoji == sylvariEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.SYLVARI;
                    color = DiscordColor.DarkGreen;
                }
                else if (emoji == mundoEmoji)
                {
                    nuevaFicha.raza = Util.Utiles.Raza.OTROS;
                }

                emoji = null;
                #endregion

                #region Ocupación
                await ctx.Channel.SendMessageAsync("`[RAZA GUARDADA]` ```6: Ocupación-actual.```").ConfigureAwait(false);
                nuevaFicha.ocupacion = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion
                #endregion

                #region DATOS FÍSICOS
                #region Descripción física
                await ctx.Channel.SendMessageAsync("`[OCUPACIÓN GUARDADA]` ```Pasando-a-DATOS-FÍSICOS. 7: Descripción-física.```").ConfigureAwait(false);
                nuevaFicha.descripcion_fisica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Altura y peso
                await ctx.Channel.SendMessageAsync("`[DESCRIPCIÓN FÍSICA GUARDADA]` ```8: Altura-aproximada.```").ConfigureAwait(false);
                double altura;
                while (!Double.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out altura))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                }
                nuevaFicha.altura = altura;

                await ctx.Channel.SendMessageAsync("`[ALTURA GUARDADA]` ```9: Peso-aproximado.```").ConfigureAwait(false);
                double peso;
                while (!Double.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out peso))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
                }
                nuevaFicha.peso = peso;
                #endregion

                #region Condición física
                await ctx.Channel.SendMessageAsync("`[PESO GUARDADO]` ```10: Condición-física.```").ConfigureAwait(false);
                nuevaFicha.condicion_fisica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Color de ojos y pelo
                await ctx.Channel.SendMessageAsync("`[CONDICIÓN FÍSICA GUARDADA]` ```11: Color-de-ojos.```").ConfigureAwait(false);
                nuevaFicha.color_ojos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;

                await ctx.Channel.SendMessageAsync("`[COLOR DE OJOS GUARDADO]` ```12: Color-de-pelo.```").ConfigureAwait(false);
                nuevaFicha.color_pelo = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Rasgos característicos
                await ctx.Channel.SendMessageAsync("`[COLOR DE PELO GUARDADO]` ```13: Rasgos-característicos.```").ConfigureAwait(false);
                nuevaFicha.rasgos_caracteristicos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion
                #endregion

                #region DATOS PSICOLÓGICOS
                #region Descripción psicológica
                await ctx.Channel.SendMessageAsync("`[RASGOS GUARDADOS]` ```Pasando-a-DATOS-PSICOLÓGICOS. 14: Descripción-psicológica.```").ConfigureAwait(false);
                nuevaFicha.descripcion_psicologica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Disgustos
                await ctx.Channel.SendMessageAsync("`[DESCRIPCIÓN PSICOLÓGICA GUARDADA]` ```15: Disgustos.```").ConfigureAwait(false);
                nuevaFicha.disgustos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Habilidades
                await ctx.Channel.SendMessageAsync("`[DISGUSTOS GUARDADOS]` ```16: Habilidades.```").ConfigureAwait(false);
                nuevaFicha.habilidades = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Debilidades
                await ctx.Channel.SendMessageAsync("`[HABILIDADES GUARDADAS]` ```17: Debilidades.```").ConfigureAwait(false);
                nuevaFicha.debilidades = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion
                #endregion

                #region HISTORIA PERSONAL
                await ctx.Channel.SendMessageAsync("`[DEBILIDADES GUARDADAS]` ```Insertar-a-continuación-historia-personal. Puede-ser-texto, un-enlace-a-documento-o-dejarlo-en-blanco.```").ConfigureAwait(false);
                nuevaFicha.historia_personal = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region CARACTERÍSTICAS
                await ctx.Channel.SendMessageAsync("`[HISTORIA GUARDADA]` ```Ahora-pasaremos-a-las-características.```").ConfigureAwait(false);

                var muscleEmoji = DiscordEmoji.FromName(ctx.Client, ":muscle:");
                var jugglerEmoji = DiscordEmoji.FromName(ctx.Client, ":juggler:");
                var brainEmoji = DiscordEmoji.FromName(ctx.Client, ":brain:");
                var dancerEmoji = DiscordEmoji.FromName(ctx.Client, ":dancer:");
                var eyeEmoji = DiscordEmoji.FromName(ctx.Client, ":eye:");
                var magic_wandEmoji = DiscordEmoji.FromName(ctx.Client, ":magic_wand:");
                var arrow_down_smallEmoji = DiscordEmoji.FromName(ctx.Client, ":arrow_down_small:");
                var white_check_markEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                var xEmoji = DiscordEmoji.FromName(ctx.Client, ":x:");

                await ctx.Channel.SendMessageAsync("```A-continuación, se-muestran-las-instrucciones:``` Primero, se-contarán-los-puntos-por-características. Éstas-son-**FUERZA** " + muscleEmoji + ", **DESTREZA** " + jugglerEmoji + ", **INTELIGENCIA** " + brainEmoji + ", **CARISMA** " + dancerEmoji + ", **PERCEPCIÓN** " + eyeEmoji + "-y-**MAGIA** " + magic_wandEmoji + ". Los-puntos-máximos-"
                    + "a-repartir-son-10, pudiendo-dar-comó-máximo-4-a-cada-característica, a-excepción-de-**MAGIA**, donde-el-máximo-serán-3.\nPresionar-cada-icono-hasta-alcanzar-cantidad-deseada-o-máxima. Para-**restar**, pulsar " + arrow_down_smallEmoji + "-y-la-característica-concreta. Puede-haber-números-negativos-en-función-"
                    + "de-las-cualidades-del-personaje. Al-**terminar**, pulsar " + white_check_markEmoji + ". En-caso-de-querer-**reiniciar**, pulsar " + xEmoji + ".").ConfigureAwait(false);

                var caracteristicasMSG = await ctx.Channel.SendMessageAsync("```CONTADOR:``` :muscle: FUERZA: " + nuevaFicha.FUERZA + "\n:juggler: DESTREZA: " + nuevaFicha.DESTREZA + "\n:brain: INTELIGENCIA: " + nuevaFicha.INTELIGENCIA + "\n:dancer: CARISMA: "
                    + nuevaFicha.CARISMA + "\n:eye: PERCEPCION: " + nuevaFicha.PERCEPCION + "\n:magic_wand: MAGIA: " + nuevaFicha.MAGIA + "\n**TOTAL**: " + nuevaFicha.TOTALES + "").ConfigureAwait(false);

                await caracteristicasMSG.CreateReactionAsync(muscleEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(jugglerEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(brainEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(dancerEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(eyeEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(magic_wandEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(arrow_down_smallEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(white_check_markEmoji).ConfigureAwait(false);
                await caracteristicasMSG.CreateReactionAsync(xEmoji).ConfigureAwait(false);

                bool finished = false;
                while (!finished)
                {
                    emoji = null;

                    int addValue = 1;

                    while (emoji == null || !(new[] { muscleEmoji, jugglerEmoji, brainEmoji, dancerEmoji, eyeEmoji, magic_wandEmoji, arrow_down_smallEmoji, white_check_markEmoji, xEmoji }.Contains(emoji)))
                    {
                        if (emoji != null) await ctx.Channel.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-iconos-nombrados.```").ConfigureAwait(false);
                        emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
                    }

                    string[] msgError = { "", "", "", "", "", "", "" };

                    if (nuevaFicha.TOTALES == 10 && !(new[] { arrow_down_smallEmoji, white_check_markEmoji, xEmoji }.Contains(emoji)))
                    {
                        msgError[msgError.Count() - 1] = " Total-de-puntos-alcanzado. Imposible-añadir-más-puntos.";
                    }
                    else
                    {
                        if (emoji == arrow_down_smallEmoji)
                        {
                            emoji = (await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
                            addValue = -1;
                        }

                        if (emoji == muscleEmoji)
                        {
                            if (nuevaFicha.FUERZA < 4 || addValue == -1)
                            {
                                nuevaFicha.FUERZA += addValue;
                            }
                            else
                            {
                                msgError[0] = " Límite-de-fuerza-alcanzado.";
                            }
                        }

                        else if (emoji == jugglerEmoji)
                        {
                            if (nuevaFicha.DESTREZA < 4 || addValue == -1)
                            {
                                nuevaFicha.DESTREZA += addValue;
                            }
                            else
                            {
                                msgError[1] = " Límite-de-destreza-alcanzado.";
                            }
                        }

                        else if (emoji == brainEmoji)
                        {
                            if (nuevaFicha.INTELIGENCIA < 4 || addValue == -1)
                            {
                                nuevaFicha.INTELIGENCIA += addValue;
                            }
                            else
                            {
                                msgError[2] = " Límite-de-inteligencia-alcanzado.";
                            }
                        }

                        else if (emoji == dancerEmoji)
                        {
                            if (nuevaFicha.CARISMA < 4 || addValue == -1)
                            {
                                nuevaFicha.CARISMA += addValue;
                            }
                            else
                            {
                                msgError[3] = " Límite-de-carisma-alcanzado.";
                            }
                        }

                        else if (emoji == eyeEmoji)
                        {
                            if (nuevaFicha.PERCEPCION < 4 || addValue == -1)
                            {
                                nuevaFicha.PERCEPCION += addValue;
                            }
                            else
                            {
                                msgError[4] = " Límite-de-percepción-alcanzado.";
                            }
                        }

                        else if (emoji == magic_wandEmoji)
                        {
                            if (nuevaFicha.MAGIA < 3 || addValue == -1)
                            {
                                nuevaFicha.MAGIA += addValue;
                            }
                            else
                            {
                                msgError[5] = " Límite-de-magia-alcanzado.";
                            }
                        }

                        else if (emoji == xEmoji)
                        {
                            nuevaFicha.FUERZA = 0;
                            nuevaFicha.DESTREZA = 0;
                            nuevaFicha.INTELIGENCIA = 0;
                            nuevaFicha.CARISMA = 0;
                            nuevaFicha.PERCEPCION = 0;
                            nuevaFicha.MAGIA = 0;
                        }

                        else if (emoji == white_check_markEmoji)
                        {
                            finished = true;
                        }
                    }

                    await caracteristicasMSG.ModifyAsync("```CONTADOR:``` :muscle: FUERZA: " + nuevaFicha.FUERZA + msgError[0] + "\n:juggler: DESTREZA: " + nuevaFicha.DESTREZA + msgError[1] + "\n:brain: INTELIGENCIA: "
                        + nuevaFicha.INTELIGENCIA + msgError[2] + "\n:dancer: CARISMA: " + nuevaFicha.CARISMA + msgError[3] + "\n:eye: PERCEPCION: " + nuevaFicha.PERCEPCION + msgError[4] + "\n:magic_wand: MAGIA: "
                        + nuevaFicha.MAGIA + msgError[5] + "\n**TOTAL**: " + nuevaFicha.TOTALES + msgError[6] + "");

                    await caracteristicasMSG.DeleteReactionAsync(arrow_down_smallEmoji, ctx.User).ConfigureAwait(false);
                    await caracteristicasMSG.DeleteReactionAsync(emoji, ctx.User).ConfigureAwait(false);
                }

                await ctx.Channel.SendMessageAsync("`[CONTADOR GUARDADO]` ```Necesarios-descriptores-sobre-cada-característica. Por-ejemplo: \"INTELIGENCIA: Erutido\" o \"DESTREZA: Pistolero\"```").ConfigureAwait(false);
                await ctx.Channel.SendMessageAsync("```¿Fuerza?```").ConfigureAwait(false);
                nuevaFicha.descriptorFUERZA = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                await ctx.Channel.SendMessageAsync("```¿Destreza?```").ConfigureAwait(false);
                nuevaFicha.descriptorDESTREZA = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                await ctx.Channel.SendMessageAsync("```¿Inteligencia?```").ConfigureAwait(false);
                nuevaFicha.descriptorINTELIGENCIA = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                await ctx.Channel.SendMessageAsync("```¿Carisma?```").ConfigureAwait(false);
                nuevaFicha.descriptorCARISMA = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                await ctx.Channel.SendMessageAsync("```¿Percepcion?```").ConfigureAwait(false);
                nuevaFicha.descriptorPERCEPCION = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                await ctx.Channel.SendMessageAsync("```¿Magia?```").ConfigureAwait(false);
                nuevaFicha.descriptorMAGIA = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                await ctx.Channel.SendMessageAsync("`[CARACTERÍSTICAS TERMINADAS]` ```Mostrando-resultado-final...```").ConfigureAwait(false);

                var embedFicha = new DiscordEmbedBuilder
                {
                    Title = nuevaFicha.nombre_completo + " " + nuevaFicha.apellidos,
                    Description = nuevaFicha.Mostrar().ToString(),
                    Color = color,
                };

                await ctx.Channel.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Mensaje-de-error: " + ex.Message.Replace(' ', '-') + " ```").ConfigureAwait(false);
            }
        }
    
        /// <summary>
        /// Método para la realización de un evento
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="canalEncuesta"></param>
        /// <returns></returns>
        public async Task Evento(CommandContext ctx, DiscordChannel canalEncuesta)
        {
            try
            {
                Evento nuevoEvento = new Evento();
                var interactivity = ctx.Client.GetInteractivity();

                #region Nombre
                await ctx.Channel.SendMessageAsync("```Preparando... Nombre-del-evento:```").ConfigureAwait(false);
                nuevoEvento.nombre = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Descripción
                await ctx.Channel.SendMessageAsync("```Descripción-del-evento:```").ConfigureAwait(false);
                nuevoEvento.descripcion = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                //REVISAR FECHA Y HORA EN PRUEBAS
                #region Fecha y hora
                await ctx.Channel.SendMessageAsync("```Concretar-fecha-y-hora-de-realización-con-formato-dd/mm/yyyy hh:mm:```").ConfigureAwait(false);
                DateTime fechaAux = new DateTime();

                var dateMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                while (!DateTime.TryParse(dateMSG.Result.Content, out fechaAux))
                {
                    await ctx.Channel.SendMessageAsync("`[ERROR]` ```Formato-incorrecto. Recomendado-formato-coherente-dd/mm/yyyy hh:mm").ConfigureAwait(false);
                }
                
                nuevoEvento.fecha = fechaAux;
                #endregion

                #region Lugar
                await ctx.Channel.SendMessageAsync("```Lugar-del-evento:```").ConfigureAwait(false);
                nuevoEvento.lugar = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
                #endregion

                #region Narradores
                await ctx.Channel.SendMessageAsync("```Mencionar-al-narrador-o-los-múltiples-narradores:```").ConfigureAwait(false);
                string[] lstNarradores = ((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content).Trim().Split(',', ' ');
                nuevoEvento.narradores = lstNarradores.ToList<string>();
                #endregion

                #region Participantes
                await ctx.Channel.SendMessageAsync("```Mencionar-a-los-participantes-o-roles-participantes:```").ConfigureAwait(false);
                string[] lstParticipantes = ((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content).Trim().Split(',', ' ');
                nuevoEvento.participantes = lstParticipantes.ToList<string>();
                #endregion

                await ctx.Channel.SendMessageAsync("`[CARACTERÍSTICAS GUARDADAS]` ```Creando-evento-en-canal-correspondiente...```").ConfigureAwait(false);

                var embedFicha = new DiscordEmbedBuilder
                {
                    Title = nuevoEvento.nombre,
                    Description = nuevoEvento.Mostrar().ToString(),
                    Color = DiscordColor.Red
                };

                await canalEncuesta.SendMessageAsync(embed: embedFicha).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Mensaje-de-error: " + ex.Message.Replace(' ', '-') + " ```").ConfigureAwait(false);
            }
        }
    }
}
